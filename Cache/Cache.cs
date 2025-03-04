using Shared.Extensions;
using StackExchange.Redis;
using ILogger = Shared.Logger.IAppSerilogLogger;

namespace Shared;

public class Cache : ICache
{
    private readonly IDatabase _cache;
    private readonly IConnectionMultiplexer _redisConnectionMultiplexer;
    private readonly ILogger _logger;

    public Cache(IDatabase cache, ILogger logger, IConnectionMultiplexer redisConnectionMultiplexer)
    {
        _logger = logger;
        _cache = cache;
        _redisConnectionMultiplexer = redisConnectionMultiplexer;
    }

    public async Task<List<string>> GetKeysByPrefix(string prefix)
    {
        var server = _redisConnectionMultiplexer.GetServer(
            _redisConnectionMultiplexer.GetEndPoints()[0]
        );
        var keys = new List<string>();

        await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
        {
            keys.Add(key.ToString());
        }
        return keys;
    }

    public async Task<List<string>> GetValuesByPrefixAsync(string prefix)
    {
        var result = new List<string>();

        try
        {
            var server = _redisConnectionMultiplexer.GetServer(
                _redisConnectionMultiplexer.GetEndPoints()[0]
            );
            var keys = new List<RedisKey>();

            // Use SCAN to find keys with the specified prefix
            await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
            {
                keys.Add(key);
            }

            // Fetch values for the keys
            var values = await _cache.StringGetAsync(keys.ToArray());
            foreach (var value in values)
            {
                result.Add(value.ToString());
            }

            return result;
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex.Message);
            return result;
        }
    }

    public async Task<List<(string, string)>> GetKeyAndValuesByPrefixAsync(string prefix)
    {
        var result = new List<(string, string)>();

        try
        {
            var server = _redisConnectionMultiplexer.GetServer(
                _redisConnectionMultiplexer.GetEndPoints()[0]
            );
            var keys = new List<RedisKey>();

            // Use SCAN to find keys with the specified prefix
            await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
            {
                keys.Add(key);
            }

            // Fetch values for the keys
            var values = await _cache.StringGetAsync(keys.ToArray());
            for (int i = 0; i < values.Count(); i++)
            {
                result.Add((keys[i].ToString(), values[i].ToString()));
            }
            foreach (var value in values) { }

            return result;
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex.Message);
            return result;
        }
    }

    public async Task<List<string>> RemoveValuesByPrefixAsync(string prefix)
    {
        var server = _redisConnectionMultiplexer.GetServer(
            _redisConnectionMultiplexer.GetEndPoints()[0]
        );
        var keys = new List<RedisKey>();

        // Use SCAN to find keys with the specified prefix
        var responseList = new List<string>();
        await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
        {
            keys.Add(key);
            responseList.Add(key.ToString());
        }

        foreach (var item in keys)
        {
            _cache.KeyDelete(item);
            responseList.Add(item.ToString());
        }

        return responseList;
    }

    public async Task Set(string key, string value)
    {
        var expiration = TimeSpan.FromSeconds(30);
        await _cache.StringSetAsync(key.WithEnv(), value, expiration);
    }

    public async Task<bool> HasValue(string key, string value)
    {
        try
        {
            var response = await _cache.StringGetAsync(key.WithEnv());
            var payload = response.ToString();
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            return payload == value;
        }
        catch (System.Exception ex)
        {
            _logger.Error($"[Cache][HasValue] - {ex.Message}");
            return false;
        }
    }

    public async Task Set(Guid key, string value)
    {
        var expiration = TimeSpan.FromSeconds(30);
        await _cache.StringSetAsync(key.ToString().WithEnv(), value, expiration);
    }

    public async Task Set(
        string key,
        string value,
        TimeIntervalType intervalType,
        int interval = 30,
        bool isGlobal = false
    )
    {
        try
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }

            var expiration = intervalType switch
            {
                TimeIntervalType.DAYS => TimeSpan.FromDays(interval),
                TimeIntervalType.HOURS => TimeSpan.FromHours(interval),
                TimeIntervalType.MINUTES => TimeSpan.FromMinutes(interval),
                TimeIntervalType.SECONDS => TimeSpan.FromSeconds(interval),
                _ => TimeSpan.FromSeconds(interval),
            };
            var cacheKey = isGlobal ? key : key.WithEnv();
            var exists = await _cache.StringGetAsync(cacheKey);
            if (!string.IsNullOrEmpty(exists.ToString()))
            {
                return;
            }

            await _cache.StringSetAsync(cacheKey, value, expiration);
        }
        catch (Exception e)
        {
            _logger.Error(e);
            return;
        }
    }

    public async Task<bool> HasValue(Guid key, string value)
    {
        try
        {
            var response = await _cache.StringGetAsync(key.ToString().WithEnv());
            var payload = response.ToString();
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            return payload == value;
        }
        catch (System.Exception ex)
        {
            _logger.Error($"[Cache][HasValue] - {ex.Message}");
            return false;
        }
    }

    public async Task Remove(string? key, bool isGlobal = false)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        try
        {
            var cacheKey = isGlobal ? key : key.WithEnv();
            await _cache.KeyDeleteAsync(cacheKey);
        }
        catch (System.Exception ex)
        {
            _logger.Error($"[Cache][Remove] - {ex.Message}");
        }
    }

    public Task<string> Get(string key) => Get(key, false);

    public async Task<string> Get(string key, bool isGlobal = false)
    {
        try
        {
            var cacheKey = isGlobal ? key : key.WithEnv();

            var value = await _cache.StringGetAsync(cacheKey);
            return value.ToString();
        }
        catch (System.Exception ex)
        {
            _logger.Error($"[CacheService]: {ex.Message}");
            _logger.Error(ex.Message);
            return String.Empty;
        }
    }
}
