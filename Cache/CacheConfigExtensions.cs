using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class CacheConfig
{
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddSingleton<ICache, Cache>();
        return services;
    }
}
