using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Shared.RedisProvider;

public static class RedisServicesExtensions
{
    public static IServiceCollection AddRedisService(this IServiceCollection services)
    {
        var redisInstances = new RedisInstances();
        services.AddSingleton<IConnectionMultiplexer>(_ => redisInstances.RedisInstance);
        services.AddSingleton<IDatabase>(_ => redisInstances.RedisCacheClient.GetDatabase());

        return services;
    }

    public static IApplicationBuilder UseRedisManagement(
        this IApplicationBuilder builder,
        IHostApplicationLifetime applicationLifetime
    )
    {
        return builder;
    }
}
