using Npgsql;
using Shared.EnvVarLoader;
using Path = System.IO.Path;

namespace Shared.Database;

/// <summary>
/// Provides database configuration and connection management functionality.
/// This class handles environment setup, connection string building, and database connections.
/// </summary>
public static class DbConfig
{
    /// <summary>
    /// Performs environment checks before database operations.
    /// Includes a delay to ensure proper initialization.
    /// </summary>
    private static async Task DbEnvCheck()
    {
        Console.WriteLine("ATTENTION: running migrations");
        await Task.Delay(4000);
    }

    /// <summary>
    /// Sets up database environment configuration.
    /// Loads environment variables and initializes database settings.
    /// </summary>
    private static void DbEnvSet()
    {
        if (!Env.RUN_MIGRATIONS)
            return;
        DbEnvCheck().Wait();
        var root = Path.Combine(Directory.GetCurrentDirectory(), "../");
        var dotenvLoader = new DotEnvLoader(
            root,
            new[] { $"--env={Environment.GetEnvironmentVariable("STAGE")}" }
        );
        dotenvLoader.Init();
    }

    /// <summary>
    /// Builds a connection string for PostgreSQL database using environment variables.
    /// </summary>
    /// <returns>NpgsqlConnectionStringBuilder with configured connection options</returns>
    public static NpgsqlConnectionStringBuilder Connection()
    {
        DbEnvSet();
        var envLoader = new EnvLoader();
        envLoader.Load();
        var poolSize = () => Env.DB_POOL_SIZE;
        NpgsqlConnectionStringBuilder connectionOptions = new()
        {
            Database = envLoader.DB_DATABASE,
            Host = envLoader.DB_HOST,
            Port = envLoader.DB_PORT,
            Username = envLoader.DB_USER,
            Password = envLoader.DB_PASSWORD,
            MaxPoolSize = poolSize(),
            Timeout = 1024,
            CommandTimeout = 45 * 1000,
        };
        return connectionOptions;
    }

    /// <summary>
    /// Establishes a connection to the PostgreSQL database.
    /// Includes error handling and connection cleanup.
    /// </summary>
    /// <returns>An open NpgsqlConnection instance</returns>
    /// <exception cref="Exception">Thrown if connection fails</exception>
    public static NpgsqlConnection Connect()
    {
        var connectionOptions = Connection();
        var connection = new NpgsqlConnection(connectionOptions.ConnectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            connection.Close();
            Console.WriteLine("Could not connect database");
            throw;
        }
    }
}

/// <summary>
/// Provides a factory method for creating database connections.
/// Acts as a simplified interface to the main DbConfig class.
/// </summary>
public static class DbConfigLoader
{
    /// <summary>
    /// Creates a new database connection using the configured connection options.
    /// </summary>
    /// <returns>A new NpgsqlConnection instance</returns>
    public static NpgsqlConnection Factory()
    {
        var connectionOptions = DbConfig.Connection();
        var connection = new NpgsqlConnection(connectionOptions.ConnectionString);
        return connection;
    }
}
