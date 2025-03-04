namespace Shared.EnvVarLoader;

public class DotEnvLoader
{
    private string FilePath { get; }
    private string[] Args { get; }

    private bool IsCloud { get; }
    private readonly EnvTools _envTools = new();

    public DotEnvLoader(string path, string[]? args = null)
    {
        FilePath = path;
        Args = args ?? new string[] { };
        IsCloud = Env.IsCloud();
    }

    public void Init()
    {
        string file = FileSetup(FilePath, Args);
        Load(file);
    }

    private void Load(string filePath)
    {
        if (IsCloud)
        {
            filePath = "/etc/secrets/config.prod.env";
        }

        Console.WriteLine(filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("production - no env file!");
            return;
        }

        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            int firstOccurrenceIndex = line.IndexOf("=", StringComparison.Ordinal);
            if (firstOccurrenceIndex == -1)
            {
                continue;
            }

            string keyName = line.Substring(0, firstOccurrenceIndex);
            string keyValue = line.Substring(firstOccurrenceIndex + 1);

            if (string.IsNullOrEmpty(keyName) && string.IsNullOrEmpty(keyValue))
            {
                continue;
            }

            Console.WriteLine($"SETTING ENV VARIABLE: key = {keyName} | value = {keyValue}");
            Environment.SetEnvironmentVariable(keyName, keyValue);
        }

        _envTools.Check();
    }

    private string FileSetup(string filePath, string[]? args = null)
    {
        string defaultEnvFile = System.IO.Path.Combine(filePath, "config.local.env");
        if (args != null)
        {
            var envType = String.Empty;
            foreach (string arg in args)
            {
                string[] option = arg.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (option[0] == "--env")
                {
                    envType = option[1];
                }
            }

            if (string.IsNullOrEmpty(envType))
            {
                return defaultEnvFile;
            }

            if (envType.Equals("production"))
            {
                return String.Empty;
            }

            var envFileType = $"config.{envType}.env";
            string envFile = System.IO.Path.Combine(filePath, envFileType);
            return envFile;
        }

        return defaultEnvFile;
    }
}
