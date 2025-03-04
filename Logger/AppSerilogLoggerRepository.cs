using Dapper;
using Npgsql;
using Serilog;
using Shared.EnvVarLoader;

namespace Shared.Logger;

public static class DbLoggerConfig
{
    public static NpgsqlConnectionStringBuilder LoggerDBConnection()
    {
        NpgsqlConnectionStringBuilder connectionOptions = new();
        connectionOptions.Database = Env.LOGGER_DB_DATABASE;
        connectionOptions.Host = Env.LOGGER_DB_HOST;
        connectionOptions.Port = Env.LOGGER_DB_PORT;
        connectionOptions.Username = Env.LOGGER_DB_USER;
        connectionOptions.Password = Env.LOGGER_DB_PASSWORD;
        connectionOptions.MaxPoolSize = 40;
        return connectionOptions;
    }
}

public class AppSerilogLoggerRepository(ILogger logger) : IAppSerilogLoggerRepository
{
    private readonly ILogger _logger = logger;
    private readonly string? _connectionOptions = DbLoggerConfig
        .LoggerDBConnection()
        .ConnectionString;

    public async Task<int> Add(IEnumerable<LogPayload> list)
    {
        var currentList = list.ToArray();
        _logger.Information("[AppLogger][DbTransaction]");

        var conn = new NpgsqlConnection(_connectionOptions);

        await conn.OpenAsync();
        try
        {
            const string sql =
                "INSERT INTO public.log (id, message, env_name, occurred_at, log_level, env_label) VALUES (DEFAULT, @Message, @EnvName, @OccurredAt::timestamp, @LogType, @EnvLabel);";

            var result = 0;
            var trans = await conn.BeginTransactionAsync();

            foreach (var item in currentList)
            {
                var param = new
                {
                    Message = item.Message,
                    EnvName = item.EnvName,
                    LogType = item.LogType.ToString(),
                    EnvLabel = item.EnvLabel,
                    OccurredAt = item.OcurredAt,
                };

                result += await conn.ExecuteAsync(sql, param, trans).ConfigureAwait(false);
            }

            await trans.CommitAsync();
            await conn.CloseAsync();

            return result;
        }
        catch (System.Exception ex)
        {
            await conn.CloseAsync();
            _logger.Error($"[AppLogger][DbTransaction]: {ex.Message}");
            return 0;
        }
    }
}
