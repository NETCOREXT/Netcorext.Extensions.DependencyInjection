using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static WebApplication ConfigureDependencyInjections(this WebApplication app) => (WebApplication)ConfigureDependencyInjections((IApplicationBuilder)app);
    public static IApplicationBuilder ConfigureDependencyInjections(this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        var configTypes = GetInjectionTypes();

        foreach (var t in configTypes)
        {
            var ctor = t.GetConstructors();
            var ctorParams = ctor[0].GetParameters();

            var args = (from p in ctorParams where p.ParameterType == typeof(IApplicationBuilder) || p.ParameterType == typeof(WebApplication) select app).Cast<object>().ToList();

            if (args.Count == 0) continue;

            Activator.CreateInstance(t, args.ToArray());
        }

        return app;
    }

    private static IEnumerable<Type> GetInjectionTypes()
    {
        var assembly = Assembly.GetEntryAssembly();

        if (assembly == null) throw new NullReferenceException("EntryAssembly not found.");

        var types = assembly.GetTypes()
                            .Where(t => t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 1
                                     && t.CustomAttributes.Any(a => a.AttributeType == typeof(InjectionAttribute)))
                            .OrderByDescending(t => t.GetCustomAttribute<InjectionAttribute>()?.Index ?? 0)
                            .ThenBy(t => t.Name);

        foreach (var type in types)
        {
            var ctor = type.GetConstructors();
            var firstParam = ctor[0].GetParameters().FirstOrDefault();

            if (firstParam == null) continue;

            if (firstParam.ParameterType != typeof(WebApplication) && firstParam.ParameterType != typeof(IApplicationBuilder))
                continue;

            yield return type;
        }
    }
}