using System.Net;
using Shared.Extensions;
using StackExchange.Redis;

namespace Shared.RedisProvider;

public class RedisClientProvider
{
    // private readonly string host = Env.REDIS_HOST;
    // private readonly string password = Env.REDIS_PASSWORD;
    // private readonly int port = int.Parse(Env.REDIS_PORT);
    // private readonly string user = Env.REDIS_USER;
    private readonly string Host = String.Empty;
    private readonly string Password = String.Empty;
    private readonly int Port;
    private readonly string User = String.Empty;

    public readonly IConnectionMultiplexer Instance;

    public RedisClientProvider(string host, string password, int port, string user)
    {
        Host = host;
        Password = password;
        Port = port;
        User = user;
        Instance = Init();
    }

    public RedisClientProvider(string connectionString)
    {
        Instance = Init(connectionString);
    }

    public void Stop()
    {
        _ = Instance?.CloseAsync();
    }

    private IConnectionMultiplexer Init(string connectionString)
    {
        return ConnectionMultiplexer.Connect(connectionString);
    }

    private IConnectionMultiplexer Init()
    {
        try
        {
            ConfigurationOptions redisClientConfig =
                new()
                {
                    AbortOnConnectFail = false,
                    Ssl = true,
                    SyncTimeout = 5000,
                    User = User,
                    Password = Password,
                    CheckCertificateRevocation = false,
                    ConnectRetry = 5
                };

            DnsEndPoint endPoint = new(Host, Port);

            redisClientConfig.EndPoints.Add(endPoint);
            return ConnectionMultiplexer.Connect(redisClientConfig);
        }
        catch (Exception e)
        {
            Console.WriteLine("RedisConfiguration:ConnectionMultiplexer:Start-> " + e.Message);
            throw;
        }
    }

    public ISubscriber? GetRedisSubscriber()
    {
        if (Instance.IsNotNull())
        {
            var sub = Instance.GetSubscriber();
            return sub;
        }
        return null;
    }
}
