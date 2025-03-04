namespace Shared.Database;

public class ConnectionErrorNotification(Action<string, string?, string?> notifyError)
{
    private readonly Action<string, string?, string?> _notifyError = notifyError;

    public void Notify(string message, string? stackTrace, string? logType)
    {
        _notifyError(message, stackTrace, logType);
    }
}
