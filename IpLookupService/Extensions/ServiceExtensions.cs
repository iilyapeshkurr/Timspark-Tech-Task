using IpLookupService.Interfaces;
using IpLookupService.Services;
using IpStack.Extensions;
using Serilog;

public static class ServiceExtensions
{

    public static void ConfigureIpStack(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIpStack(configuration["IpStack:ApiKey"]);
        services.AddScoped<IIpServiceWrapper, IpServiceWrapper>();
    }

    public static void ConfigureSerilog(this IHostBuilder host)
    {
        host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));
    }

}