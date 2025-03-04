using System.Timers;
using Dapper;
using Shared.Json;
using Npgsql;
using Serilog;

namespace Shared.Database;

public interface IHealthQueriesRunner
{
    void Init();
}

public class HealthQueriesRunner : IHealthQueriesRunner
{
    private readonly ILogger _logger;
    private readonly PgConnectionManager _pgConnectionManager;

    public HealthQueriesRunner(
        ILogger logger,
        PgConnectionManager pgConnectionManager
    )
    {
        _logger = logger;
        _pgConnectionManager = pgConnectionManager;
    }

    public void Init()
    {
        _logger.Information("INITIALIZING DB SANITY CHECK");
        const int interval = (10 * 6000) * 4; // EVERY 4 minutes
        _ = Task.Run(async () =>
            {
                while (true)
                {
                    await KillLongRunning().ConfigureAwait(false);
                    await Task.Delay(interval);
                }
            })
            .ConfigureAwait(false);
    }

    private async Task LogActiveQueries(NpgsqlConnection conn)
    {
        try
        {
            var logRunningQueries = await conn.QueryAsync<LogRunningQuery>(
                "SELECT pid, cast(now() - pg_stat_activity.query_start as text), query, state "
                    + "FROM pg_stat_activity WHERE pg_stat_activity. query <> '' "
                    + "and pg_stat_activity.query <> '<insufficient privilege>' "
                    + "and pg_stat_activity.query <> 'LISTEN datachange' "
                    + "and (now() - pg_stat_activity.query_start > interval '1 minute');"
            );
            var message = JsonConverter.ToJson(new { logRunningQueries });
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.Information(message);
        }
        catch (System.Exception ex)
        {
            _logger.Error("[LogActiveQueries]: {ExMessage}", ex.Message);
            return;
        }
    }

    private async Task KillLongRunning()
    {
        var conn = await _pgConnectionManager.GetConnectionAsync();
        _logger.Information("DB SANITY CHECK");

        try
        {
            await LogActiveQueries(conn.NpgsqlConnection);
            await using var command = new NpgsqlCommand(
                @"DO $$
DECLARE 
    rec RECORD;
BEGIN 
    FOR rec IN (
        SELECT
    pid
FROM
    pg_stat_activity
WHERE
    pg_stat_activity.query <> ''
    and pg_stat_activity.query <> '<insufficient privilege>'
    and pg_stat_activity.query <> 'LISTEN datachange'
    and (now() - pg_stat_activity.query_start > interval '1 minute')
    )
    LOOP
        EXECUTE 'SELECT pg_terminate_backend(' || rec.pid || ');';
    END LOOP;
END $$;",
                conn.NpgsqlConnection
            );
            await command.ExecuteNonQueryAsync();
        }
        catch (System.Exception ex)
        {
            _logger.Error(
                "Killing long running processes error: {ExMessage}",
                ex.Message
            );
            return;
        }

        _pgConnectionManager.ReturnConnection(conn);
    }
}

public class LogRunningQuery
{
    public int? Pid { get; set; }
    public string? Duration { get; set; }
    public string? Query { get; set; }
    public string? State { get; set; }
}
