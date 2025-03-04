using System.Net.Http.Headers;
using System.Text;
using Serilog;
using Shared.EnvVarLoader;

namespace Shared.Logger;

public class AppSerilogLogger : IAppSerilogLogger
{
    // TODO - SUPPORT OTHER DATABASE LIKE MONGO
    private readonly ILogger _logger;
    private readonly IAppSerilogLoggerRepository _repository;
    private readonly List<LogPayload> _list = new();
    private readonly int _transactionThreshold = Env.TRANSACTION_THRESHOLD;
    private DateTime? _lastUpdated = null;

    public AppSerilogLogger(ILogger logger, IAppSerilogLoggerRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    private void Log(LogPayload log)
    {
        switch (log.LogType)
        {
            case LogLevelType.INFORMATION:
                _logger.Information(log.Message);
                break;
            case LogLevelType.DEBUG:
                if (Env.DEBUG_ON)
                {
                    _logger.Debug(log.Message);
                }
                break;
            case LogLevelType.ERROR:
                _logger.Error(log.Message);
                break;
            case LogLevelType.FATAL:
                _logger.Fatal(log.Message);
                break;
            default:
                _logger.Information(log.Message);
                break;
        }
    }

    private bool CheckWindowTime()
    {
        var start = DateTime.Now;
        var oldDate = _lastUpdated;
        if (oldDate == null)
        {
            return false;
        }

        return (start - oldDate).Value.TotalMinutes > 5;
    }

    private async void AddLog(LogPayload log, bool print = true)
    {
        try
        {
            if (print != false)
            {
                Log(log);
            }

            _list.Add(log);

            if (_list.Count <= _transactionThreshold && CheckWindowTime() == false)
            {
                // _logger.Information(
                //     $"Waiting more items to save logs{_transactionThreshold} | current {_list.Count}"
                // );
                return;
            }

            if (FeatureFlags.DB_SAVING_LOGS_ON)
            {
                var result = await _repository.Add(_list).ConfigureAwait(false);
                if (result.Equals(0))
                {
                    return;
                }
            }

            _lastUpdated = DateTime.Now;
            _list.Clear();
            return;
        }
        catch (System.Exception ex)
        {
            _logger.Error($"[AppLogger][AddLog]: {log.Message} | {ex.Message}");
        }
    }

    public void Log(string message, LogLevelType level, bool print = true)
    {
        switch (level)
        {
            case LogLevelType.INFORMATION:
                Information(message);
                break;
            case LogLevelType.ERROR:
                Error(message);
                break;
            case LogLevelType.DEBUG:
                Debug(message);
                break;
            case LogLevelType.FATAL:
                Fatal(message);
                break;
            default:
                Information(message);
                break;
        }
    }

    public void Information(string? message, bool print = true) =>
        AddLog(new LogPayload(message, LogLevelType.INFORMATION), print);

    public void Error(string? message, bool print = true) =>
        AddLog(new LogPayload(message, LogLevelType.ERROR));

    public void Debug(string? message, bool print = true) =>
        AddLog(new LogPayload(message, LogLevelType.DEBUG));

    public void Fatal(string? message, bool print = true) =>
        AddLog(new LogPayload(message, LogLevelType.DEBUG));

    public void Information(
        string className,
        string methodName,
        string? message,
        bool print = true
    ) => Information($"[{className}][{methodName}]: {message}");

    public void Debug(string className, string methodName, string? message, bool print = true) =>
        Debug($"[{className}][{methodName}]: {message}");

    public void Error(string className, string methodName, string? message, bool print = true) =>
        Error($"[{className}][{methodName}]: {message}");

    public void Fatal(string className, string methodName, string? message, bool print = true) =>
        Fatal($"[{className}][{methodName}]: {message}");

    public void Error(Exception ex) => Error($"[{ex.Source ?? ex.TargetSite?.Name}]: {ex.Message}");
}
