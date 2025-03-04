namespace Shared.Logger;

public interface IAppSerilogLogger
{
    void Log(string message, LogLevelType level, bool print = true);
    void Information(string? message, bool print = true);
    void Error(string? message, bool print = true);
    void Debug(string? message, bool print = true);
    void Fatal(string? message, bool print = true);
    void Information(string className, string methodName, string? message, bool print = true);
    void Debug(string className, string methodName, string? message, bool print = true);
    void Error(string className, string methodName, string? message, bool print = true);
    void Fatal(string className, string methodName, string? message, bool print = true);

    void Error(Exception ex);
}
