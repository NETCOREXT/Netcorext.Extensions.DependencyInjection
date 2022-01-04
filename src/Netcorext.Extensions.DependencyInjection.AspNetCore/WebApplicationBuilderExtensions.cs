namespace Microsoft.Extensions.DependencyInjection;

public static class WebApplicationBuilderExtensions
{

    public static WebApplicationBuilder ConfigureDependencyInjections(this WebApplicationBuilder builder)
    {
        return ConfigureDependencyInjections(builder, builder.Configuration, builder.Environment);
    }

    public static WebApplicationBuilder ConfigureDependencyInjections(this WebApplicationBuilder builder, params object[] initObjects)
    {
        builder.Host.ConfigureDependencyInjections(initObjects);
        builder.Services.ConfigureDependencyInjections(initObjects);
        builder.Build().ConfigureDependencyInjections();

        return builder;
    }
}