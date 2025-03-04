using Shared.EnvVarLoader;
using Serilog;
using Serilog.Events;

namespace Shared;

/// <summary>
/// WARNING: THIS SETUP OVERRIDES THE MICROSOFT LOGGER
/// SERILOG SETUP
/// </summary>
public static class LoggerConfig
{
    public static ILogger GetLoggerConfig()
    {
        LogEventLevel printLevel = LogEventLevel.Warning;
        if (Env.DEBUG_ON)
        {
            printLevel = LogEventLevel.Debug;
        }
        string logFilesPath = "logs/applogs.log";
        string overrideLibPath = "Microsoft.EntityFrameworkCore.Database.Command";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override(overrideLibPath, printLevel)
            .WriteTo.File(logFilesPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        return Log.Logger;
    }
}
