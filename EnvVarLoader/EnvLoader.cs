namespace Shared.EnvVarLoader;

public class EnvLoader
{
    public string DB_HOST { get; private set; } = String.Empty;
    public string DB_DATABASE { get; private set; } = String.Empty;
    public string DB_PASSWORD { get; private set; } = String.Empty;
    public string DB_USER { get; private set; } = String.Empty;
    public string DB_URL { get; private set; } = String.Empty;
    public string REDIS_HOST { get; private set; } = String.Empty;
    public string REDIS_USER { get; private set; } = String.Empty;
    public string REDIS_PORT { get; private set; } = String.Empty;
    public string REDIS_PASSWORD { get; private set; } = String.Empty;
    public string REDIS_URL { get; private set; } = String.Empty;
    public string STAGE { get; private set; } = String.Empty;
    public string JWT_SECRET { get; private set; } = String.Empty;
    public string PWD_SECRET { get; private set; } = String.Empty;
    public string MONGO_CONNECTION { get; private set; } = String.Empty;
    public string COLLECTION_NAME { get; private set; } = String.Empty;
    public string AppDatabase { get; private set; } = String.Empty;
    public int DB_PORT { get; set; } = 5432;
    public bool RUN_MIGRATIONS { get; private set; }

    private int GetDbPort()
    {
        int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out int dbPort);
        int finalPort = dbPort != 0 ? dbPort : 5432;
        return finalPort;
    }

    public bool IsCloud() =>
        (
            Environment.GetEnvironmentVariable("ASPNETCORE_ASPNETCORE_ENVIRONMENT") ?? String.Empty
        ).ToLower() == "production";

    public void Load()
    {
        DB_HOST = Environment.GetEnvironmentVariable("DB_HOST") ?? String.Empty;
        DB_DATABASE = Environment.GetEnvironmentVariable("DB_DATABASE") ?? String.Empty;
        DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? String.Empty;
        DB_USER = Environment.GetEnvironmentVariable("DB_USER") ?? String.Empty;
        DB_PORT = GetDbPort();
        DB_URL = Environment.GetEnvironmentVariable("DB_URL") ?? String.Empty;
        REDIS_HOST = Environment.GetEnvironmentVariable("REDIS_HOST") ?? String.Empty;
        REDIS_USER = Environment.GetEnvironmentVariable("REDIS_USER") ?? String.Empty;
        REDIS_PORT = Environment.GetEnvironmentVariable("REDIS_PORT") ?? String.Empty;
        REDIS_PASSWORD = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? String.Empty;
        REDIS_URL = Environment.GetEnvironmentVariable("REDIS_URL") ?? String.Empty;
        STAGE = Environment.GetEnvironmentVariable("STAGE") ?? String.Empty;
        JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET") ?? String.Empty;
        PWD_SECRET = Environment.GetEnvironmentVariable("PWD_SECRET") ?? String.Empty;
        MONGO_CONNECTION = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? String.Empty;
        COLLECTION_NAME = Environment.GetEnvironmentVariable("COLLECTION_NAME") ?? String.Empty;
        AppDatabase = Environment.GetEnvironmentVariable("AppDatabase") ?? String.Empty;
        RUN_MIGRATIONS = Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true";
    }
}
