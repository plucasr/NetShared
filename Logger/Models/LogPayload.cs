using Shared.EnvVarLoader;

namespace Shared.Logger;

public enum LogLevelType
{
    INFORMATION,
    ERROR,
    FATAL,
    DEBUG
}

public class LogPayload
{

    public LogPayload(string? message, LogLevelType logType = LogLevelType.INFORMATION)
    {
        Message = message ?? String.Empty;
        EnvName = Env.STAGE;
        OcurredAt = DateTime.UtcNow.AddHours(-3);
        LogType = logType;
        EnvLabel = Env.ENV_LABEL;
    }

    public Guid Id { get; set; }
    public string Message { get; set; } = String.Empty;
    public string EnvName { get; set; } = String.Empty;
    public string EnvLabel { get; set; } = Env.ENV_LABEL;
    public DateTime OcurredAt { get; set; }
    public LogLevelType LogType { get; set; } = LogLevelType.INFORMATION;
}
