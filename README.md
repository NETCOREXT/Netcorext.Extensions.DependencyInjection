# Netcorext.Extensions.DependencyInjection
[![Nuget](https://img.shields.io/nuget/v/Netcorext.Extensions.DependencyInjection)](https://www.nuget.org/packages/Netcorext.Extensions.DependencyInjection)

Extended for Microsoft.Extensions.DependencyInjection.

---

## Example for ASP.NET 6
### Step 1
Program.cs
```
WebApplication.CreateBuilder(args)
              .ConfigureDependencyInjections();
```
### Step 2
Create IHostBuilder Config
```
[Injection]
public class HostConfig
{
    public HostConfig(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((w, s) =>
                                          {
                                              var host = w.HostingEnvironment;

                                              s.SetBasePath(host.ContentRootPath)
                                               .AddJsonFile("appsettings.json", false, true)
                                               .AddJsonFile($"appsettings.{host.EnvironmentName}.json", true, true)
                                               .AddEnvironmentVariables();
                                          });
    }
}
```
### Step 3
Create WebApplication Config
```
[Injection]
public class AppConfig
{
    public AppConfig(WebApplication app)
    {
        app.MapControllers();
        app.Run();
    }
}
```
### Step 4
Create Service Config
```
// Sorting by desc
[Injection(int.MaxValue)]
public class CommonSetting
{
    public CommonSetting(IServiceCollection services, IConfiguration configuration)
    {
        // Add Services
    }
}
```