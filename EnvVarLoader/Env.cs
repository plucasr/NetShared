namespace Shared.EnvVarLoader;

public static class Env
{
    private static string GetEnvVar(string key, string defaultValue = "") =>
        Environment.GetEnvironmentVariable(key) ?? defaultValue;

    public static bool CHECK_TOKEN = GetEnvVar("CHECK_TOKEN") == "true";
    public static bool SEND_EMAILS = GetEnvVar("SEND_EMAILS") == "true";
    public static bool DEV_EMAIL_TARGET = GetEnvVar("DEV_EMAIL_TARGET") == "true";
    public static int DB_PORT = GetDbPort();
    public static string DB_HOST = GetEnvVar("DB_HOST");
    public static string DB_DATABASE = GetEnvVar("DB_DATABASE");
    public static string DB_PASSWORD = GetEnvVar("DB_PASSWORD");
    public static string DB_USER = GetEnvVar("DB_USER");
    public static string DB_URL = GetEnvVar("DB_URL", "");

    public static readonly bool RUN_MIGRATIONS = GetEnvVar("RUN_MIGRATIONS") == "true";
    public static readonly bool DEBUG_ON = Env.STAGE == "DEBUG";

    public static readonly string REDIS_CACHE_HOST = GetEnvVar("REDIS_CACHE_HOST");
    public static readonly string REDIS_CACHE_USER = GetEnvVar("REDIS_CACHE_USER");
    public static readonly string REDIS_CACHE_PORT = GetEnvVar("REDIS_CACHE_PORT");
    public static readonly string REDIS_CACHE_PASSWORD = GetEnvVar("REDIS_CACHE_PASSWORD");
    public static readonly string REDIS_CACHE_URL = GetEnvVar("REDIS_CACHE_URL");
    public static readonly string STAGE = GetEnvVar("STAGE");
    public static readonly string JWT_SECRET = GetEnvVar("JWT_SECRET") ?? String.Empty;
    public static readonly string PWD_SECRET = GetEnvVar("PWD_SECRET") ?? String.Empty;
    public static readonly string MONGO_CONNECTION = GetEnvVar("MONGO_CONNECTION");
    public static readonly string APP_HEADER_SECRET = GetEnvVar("APP_HEADER_SECRET");
    public static string SENDIN_BLUE_API_KEY = GetEnvVar("SENDIN_BLUE_API_KEY");
    public static string ENV_LABEL = GetEnvVar("ENV_LABEL", "LOCAL");

    public static bool DEBUG_NOTIFICATIONS = GetEnvVar("DEBUG_NOTIFICATIONS") == "true";
    public static bool DISPATCH_EMAILS = GetEnvVar("DISPATCH_EMAILS") == "true";
    public static bool MOCK_CLOUD = GetEnvVar("MOCK_CLOUD") == "true";

    public static int TRANSACTION_THRESHOLD = GetIntEnvVar("TRANSACTION_THRESHOLD", 30);
    public static string LOGGER_DB_DATABASE = GetEnvVar("LOGGER_DB_DATABASE");
    public static string LOGGER_DB_HOST = GetEnvVar("LOGGER_DB_HOST");
    public static int LOGGER_DB_PORT = GetIntEnvVar("LOGGER_DB_PORT", 5432);
    public static string LOGGER_DB_USER = GetEnvVar("LOGGER_DB_USER");
    public static string LOGGER_DB_PASSWORD = GetEnvVar("LOGGER_DB_PASSWORD");
    public static string GATEWAY_HOOK_URL = GetEnvVar("GATEWAY_HOOK_URL");
    public static string STATIC_LOGGER_URL = GetEnvVar("STATIC_LOGGER_URL");

    public static string LOGS_PATH = GetEnvVar("LOGS_PATH");
    public static string LOGGER_DB = GetEnvVar("LOGGER_DB");
    public static string COLLECTION_NAME = $"{STAGE}_appApiLogs";

    public static string AWS_ACCESS_KEY = GetEnvVar("AWS_ACCESS_KEY");
    public static string AWS_ACCESS_SECRET = GetEnvVar("AWS_ACCESS_SECRET");
    public static string AWS_REGION = GetEnvVar("AWS_REGION");
    public static string AWS_BUCKET_NAME = GetEnvVar("AWS_BUCKET_NAME");

    public static string APPLE_CLIENT_ID = GetEnvVar("APPLE_CLIENT_ID");
    public static string APPLE_CLIENT_SECRET = GetEnvVar("APPLE_CLIENT_SECRET");
    public static string APPLE_MOBILE_SECRET = GetEnvVar("APPLE_MOBILE_SECRET");
    public static string APPLE_MOBILE_CLIENT_ID = GetEnvVar("APPLE_MOBILE_CLIENT_ID");
    public static string APPLE_TEAM_ID = GetEnvVar("APPLE_TEAM_ID");
    public static string PUB_SUB_URL = GetEnvVar("PUB_SUB_URL");
    public static string FIREBASE_SECRET = GetEnvVar("FIREBASE_SECRET");
    public static string DEVICES_CLOUD_HUB_URL = GetEnvVar("DEVICES_CLOUD_HUB_URL");
    public static int NOTIFICATION_INTERVAL = GetIntEnvVar("NOTIFICATION_INTERVAL", 10 * 1000);
    public static bool DISABLE_NOTIFICATION_ENGINE =
        GetEnvVar("DISABLE_NOTIFICATION_ENGINE") == "true";
    public static int DB_POOL_SIZE = GetIntEnvVar("DB_POOL_SIZE", 30);
    public static int CONNECTION_LIFETIME_DELTA = GetIntEnvVar("CONNECTION_LIFETIME_DELTA", 15);
    public static int CONNECTION_POOL_THRELSHOLD = GetIntEnvVar("CONNECTION_POOL_THRELSHOLD", 2);

    public static int MENU_ENQUEUE_DELTA = GetIntEnvVar("MENU_ENQUEUE_DELTA", 5);

    public static string APP_RESTART_HOOK = GetEnvVar("APP_RESTART_HOOK");

    public static bool GetBoolEnvVar(string key)
    {
        return Environment.GetEnvironmentVariable(key) == "true";
    }

    public static bool IsNotProd() => STAGE != "prod";

    private static int GetIntEnvVar(string key, int defaultValue = 0)
    {
        _ = int.TryParse(GetEnvVar(key), out int intEnvVar);
        if (intEnvVar.Equals(0) && defaultValue.Equals(0))
        {
            return intEnvVar;
        }

        if (intEnvVar.Equals(0) && defaultValue >= 0)
        {
            return defaultValue;
        }

        return intEnvVar;
    }

    private static int GetDbPort()
    {
        int.TryParse(GetEnvVar("DB_PORT"), out int dbPort);
        int finalPort = dbPort != 0 ? dbPort : 5432;
        return finalPort;
    }

    public static bool IsCloud() => GetEnvVar("ASPNETCORE_ENVIRONMENT").ToLower() == "production";
}
