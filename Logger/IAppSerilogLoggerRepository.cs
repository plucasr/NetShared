namespace Shared.Logger;

public interface IAppSerilogLoggerRepository
{
    Task<int> Add(IEnumerable<LogPayload> list);
}
