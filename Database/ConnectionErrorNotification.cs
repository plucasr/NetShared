namespace Shared.Database;

public class ConnectionErrorNotification
{
    private readonly Action<string, string?, string?> _notifyError;

    public ConnectionErrorNotification(Action<string, string?, string?> notifyError)
    {
        _notifyError = notifyError;
    }

    public void Notify(string message, string? stackTrace, string? logType)
    {
        _notifyError(message, stackTrace, logType);
    }
}
