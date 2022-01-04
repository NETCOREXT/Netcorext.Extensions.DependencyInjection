using System.Reflection;
using Microsoft.Extensions.Hosting;
using Netcorext.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDependencyInjections(this IHostBuilder hostBuilder, params object[] initObjects)
    {
        return ConfigureDependencyInjections(hostBuilder, GetInjectionTypes().ToArray(), initObjects);
    }

    private static IHostBuilder ConfigureDependencyInjections(this IHostBuilder hostBuilder, Type[] configTypes, params object[] initObjects)
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        if (configTypes == null) throw new ArgumentNullException(nameof(configTypes));

        if (!configTypes.Any()) return hostBuilder;

        foreach (var t in configTypes)
        {
            var args = new List<object>();
            var ctor = t.GetConstructors();
            var ctorParams = ctor[0].GetParameters();

            foreach (var p in ctorParams)
            {
                if (p.ParameterType == typeof(IHostBuilder))
                {
                    args.Add(hostBuilder);
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

            if (args.Count == 0) continue;

            Activator.CreateInstance(t, args.ToArray());
        }

        return hostBuilder;
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

            if (firstParam.ParameterType != typeof(IHostBuilder))
                continue;

            yield return type;
        }
    }
}