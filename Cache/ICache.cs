namespace Shared;

public interface ICache
{
    Task Set(string key, string value);
    Task<bool> HasValue(string key, string value);
    Task Set(Guid key, string value);
    Task<bool> HasValue(Guid key, string value);
    Task<string> Get(string key);
    Task<string> Get(string key, bool isGlobal = false);
    Task Set(
        string key,
        string value,
        TimeIntervalType intervalType,
        int interval = 30,
        bool isGlobal = false
    );
    Task Remove(string? key, bool isGlobal = false);
    Task<List<string>> GetValuesByPrefixAsync(string prefix);
    Task<List<(string, string)>> GetKeyAndValuesByPrefixAsync(string prefix);
    Task<List<string>> RemoveValuesByPrefixAsync(string prefix);
    Task<List<string>> GetKeysByPrefix(string prefix);
}
