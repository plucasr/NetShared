using Shared.EnvVarLoader;
using StackExchange.Redis;

namespace Shared.RedisProvider;

public class RedisInstances
{
    public readonly IConnectionMultiplexer RedisCacheClient;

    public readonly IConnectionMultiplexer RedisInstance;

    public RedisInstances()
    {
        if (Env.IsCloud() || Env.MOCK_CLOUD)
        {
            string cacheHost = Env.REDIS_CACHE_HOST;
            string cachePassword = Env.REDIS_CACHE_PASSWORD;
            int cachePort = int.Parse(Env.REDIS_CACHE_PORT);
            string cacheUser = Env.REDIS_CACHE_USER;
            RedisCacheClient = new RedisClientProvider(
                cacheHost,
                cachePassword,
                cachePort,
                cacheUser
            ).Instance;
            RedisInstance = new RedisClientProvider(
                cacheHost,
                cachePassword,
                cachePort,
                cacheUser
            ).Instance;
        }
        else
        {
            RedisCacheClient = new RedisClientProvider(Env.REDIS_CACHE_URL).Instance;
            RedisInstance = new RedisClientProvider(Env.REDIS_CACHE_URL).Instance;
        }
    }
}
