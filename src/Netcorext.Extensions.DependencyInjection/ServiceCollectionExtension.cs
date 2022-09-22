using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureDependencyInjections(this IServiceCollection services, params object[] initObjects)
    {
        return ConfigureDependencyInjections(services, GetInjectionTypes().ToArray(), initObjects);
    }

    private static IServiceCollection ConfigureDependencyInjections(this IServiceCollection services, Type[] configTypes, params object[] initObjects)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        if (configTypes == null || !configTypes.Any()) throw new ArgumentNullException(nameof(configTypes));

        foreach (var t in configTypes)
        {
            var ctor = t.GetConstructors().FirstOrDefault();

            if (ctor == null) continue;

            var ctorParams = ctor.GetParameters();

            if (!ctorParams.Any())
            {
                Activator.CreateInstance(t);

                continue;
            }

            var args = new List<object>();

            foreach (var p in ctorParams)
            {
                if (p.ParameterType == typeof(IServiceCollection))
                {
                    args.Add(services);
                }
                else
                {
                    var svc = initObjects.FirstOrDefault(t => t.GetType() == p.ParameterType || p.ParameterType.IsInstanceOfType(t));

                    if (svc != null)
                    {
                        args.Add(svc);
                    }
                }
            }

            if (args.Count != ctorParams.Length) continue;

            Activator.CreateInstance(t, args.ToArray());
        }

        return services;
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
            var ctor = type.GetConstructors().FirstOrDefault();

            if (ctor == null) continue;

            var firstParam = ctor.GetParameters().FirstOrDefault();

            if (firstParam == null || firstParam.ParameterType == typeof(IServiceCollection))
                yield return type;
        }
    }
}