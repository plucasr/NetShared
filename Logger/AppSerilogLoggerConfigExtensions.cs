using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.EnvVarLoader;

namespace Shared.Logger;

public static class AppSerilogLoggerConfigExtensions
{
    public static IServiceCollection AddAppSerilogLogger(this IServiceCollection services)
    {
        services.AddSingleton<IAppSerilogLoggerRepository, AppSerilogLoggerRepository>();
        services.AddSingleton<IAppSerilogLogger, AppSerilogLogger>();
        return services;
    }
}
