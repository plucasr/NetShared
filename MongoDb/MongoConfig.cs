using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using Shared.EnvVarLoader;

namespace Shared.Config;

public class DbConnection
{
    public static MongoClient Init()
    {
        var cnString = Env.MONGO_CONNECTION;
        var mongoConnectionUrl = new MongoUrl(cnString);
        var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
        var client = new MongoClient(mongoClientSettings);
        return client;
    }

    public Action<ClusterBuilder> CommandCallBack = cb =>
    {
        cb.Subscribe<CommandStartedEvent>(e =>
        {
            Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
        });
    };
}

public static class MongoConfig
{
    private const string appDatabase = "app_db";
    private static readonly IMongoClient Client = DbConnection.Init();

    public static IServiceCollection AddSingletonMongoConfig(this IServiceCollection services)
    {
        services.AddSingleton(_ => Client.GetDatabase(appDatabase));
        services.AddSingleton(Client);

        return services;
    }

    public static IServiceCollection AddScopedMongoConfig(this IServiceCollection services)
    {
        services.AddScoped(_ => Client.GetDatabase(appDatabase));
        services.AddScoped(_ => Client);

        return services;
    }
}
