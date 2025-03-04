using Shared.Json;

namespace Shared.EnvVarLoader;

public static class EnvStageVariables
{
    public static IEnumerable<string> Local = new List<string>()
    {
        "DB_DATABASE",
        "DB_HOST",
        "DB_PASSWORD",
        "DB_PORT",
        "DB_USER",
        "JWT_SECRET",
        "MONGO_DATABASE_NAME",
        "STAGE",
        "DEBUG_STAGE",
        "AWS_ACCESS_KEY",
        "AWS_ACCESS_SECRET",
        "AWS_REGION",
    };

    public static IEnumerable<string> Cloud = new List<string>()
    {
        "DB_DATABASE",
        "DB_HOST",
        "DB_PASSWORD",
        "DB_PORT",
        "DB_USER",
        "JWT_SECRET",
        "MONGO_DATABASE_NAME",
        "STAGE",
        "DEBUG_STAGE",
        "AWS_ACCESS_KEY",
        "AWS_ACCESS_SECRET",
        "AWS_REGION",
    };
}

public class EnvTools
{
    private bool _hasNeededEnvVars = true;

    public EnvTools() { }

    private string ToJson(List<string> source)
    {
        string defaultValue = "[]";
        try
        {
            return JsonConverter.ToJson(source) ?? defaultValue;
        }
        catch (System.Exception)
        {
            return defaultValue;
        }
    }

    public void Check()
    {
        List<string> missing = new();
        if (Env.RUN_MIGRATIONS)
        {
            return;
        }

        var musHave = Env.IsCloud() == true ? EnvStageVariables.Cloud : EnvStageVariables.Local;

        foreach (var item in musHave)
        {
            var current = Environment.GetEnvironmentVariable(item);
            if (string.IsNullOrEmpty(current))
            {
                missing.Add(item);
                _hasNeededEnvVars = false;
            }
        }

        if (!_hasNeededEnvVars)
        {
            Console.WriteLine("[EnvTools][Check] Missing Environment Variables");
            throw new ApplicationException($"Missing the following env vars: {ToJson(missing)}");
        }

        return;
    }
}
