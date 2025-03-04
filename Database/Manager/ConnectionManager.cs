using System.Data;
using Npgsql;
using Serilog;
using Shared.EnvVarLoader;
using Shared.Json;

namespace Shared.Database;

/// <summary>
/// This is a helper to help debugging db connection management, depending or your context
/// is better to just rely at the framework connection management, just make sure you the proper
/// dependency injection strategy.
/// </summary>
public class PgConnectionManager(
    ILogger logger,
    ConnectionErrorNotification connectionErrorNotification
) : IDisposable
{
    private List<ConnectionWrapper> _connections = new();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(
        Env.CONNECTION_POOL_THRELSHOLD,
        Env.CONNECTION_POOL_THRELSHOLD
    );
    private readonly ILogger _logger = logger;
    private readonly ConnectionErrorNotification _connectionErrorNotification =
        connectionErrorNotification;
    private static readonly int _connectionLifeTimeDelta = Env.CONNECTION_LIFETIME_DELTA;
    private bool _isRestarting = false;

    private async Task NotifyRestartingApp(Exception ex)
    {
        try
        {
            if (!string.IsNullOrEmpty(Env.APP_RESTART_HOOK) && !_isRestarting)
            {
                _isRestarting = true;
                Console.WriteLine("_______________");
                Console.WriteLine("\n");
                Console.WriteLine($"RESTARTING APP: {Env.APP_RESTART_HOOK}");
                Console.WriteLine("\n");
                Console.WriteLine("______________");
                var client = new HttpClient();
                // _connectionErrorNotification.Notify(
                //     $"[RESTARTING][APP]: {ex.Message}",
                //     ex.StackTrace?.Substring(0, 20)
                // );

                await client.GetAsync(Env.APP_RESTART_HOOK);
            }
        }
        catch (System.Exception e)
        {
            _logger.Error(e.Message);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a database connection.
    /// </summary>
    /// <param name="isRecursive">Indicates whether the operation is recursive.</param>
    /// <returns>A <see cref="Task{ConnectionWrapper}"/> representing the asynchronous operation. The result of the task is a <see cref="ConnectionWrapper"/> object representing the retrieved connection.</returns>
    /// <remarks>
    /// This method first attempts to release any unused connections. It then logs the total number of connections currently being managed.
    /// It waits for the semaphore to be available, which is a synchronization mechanism used to limit the number of threads that can access a resource or pool of resources concurrently.
    /// It then tries to get an existing connection that is not in use. If it finds one, it sets the connection as in use and returns it.
    /// If no existing connection is found, it creates a new connection, sets it as in use, adds it to the list of connections, and returns it.
    /// If an exception occurs during the process, it checks if the operation was recursive. If it was, it notifies the application to restart and throws a new exception with the error message. If the operation was not recursive, it logs a message indicating that it's waiting for a connection to be released, waits for the semaphore again, and recursively calls itself.
    /// Finally, it releases the semaphore to allow other threads to proceed.
    /// </remarks>
    public async Task<ConnectionWrapper> GetConnectionAsync(bool isRecursive = false)
    {
        try
        {
            await ReleaseUnusedConnections();
            _logger.Debug($"Starting to get connections of TOTAL: {_connections.Count}");
            await _semaphore.WaitAsync();

            var connectionWrapper = await TryGetExistingConnection();

            if (connectionWrapper != null)
            {
                connectionWrapper.SetConnectionInUse(_connectionLifeTimeDelta);
                return connectionWrapper;
            }
            var response = await CreateNewConnection();
            response.SetConnectionInUse(_connectionLifeTimeDelta);
            _connections.Add(response);
            return response;
        }
        catch (System.Exception ex)
        {
            if (isRecursive)
            {
                _ = NotifyRestartingApp(ex).ConfigureAwait(false);
                // _connectionErrorNotification.Notify(
                //     $"[ConnectionManager][Error]: {ex.Message}",
                //     null
                // );
                throw new Exception($"ERROR - {ex.Message}");
            }
            _connections = new();

            _logger.Debug("Waiting until connection gets released");

            await _semaphore.WaitAsync();
            return await GetConnectionAsync(true);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public List<ConnectionWrapperView> ListConnections() =>
        _connections.ConvertAll(x => x.ToView());

    private async Task<ConnectionWrapper?> TryGetExistingConnection()
    {
        if (!_connections.Any())
        {
            return null;
        }

        var connectionWrapper = _connections.FirstOrDefault(cw => !cw.IsInUse);

        if (connectionWrapper == null)
        {
            return null;
        }

        if (connectionWrapper.TimeoutTokenSource?.Token.IsCancellationRequested == true)
        {
            // The timeout has been triggered, close the connection
            _logger.Debug("REMOVING CONNECTION AFTER TOKEN EXPIRES");
            await CloseConnection(connectionWrapper);
            _connections.Remove(connectionWrapper);
            connectionWrapper.NpgsqlConnection.Dispose();
            return null;
        }

        if (connectionWrapper.NpgsqlConnection.State != ConnectionState.Open)
        {
            await CloseConnection(connectionWrapper);
            _connections.Remove(connectionWrapper);
            connectionWrapper.NpgsqlConnection.Dispose();
            return null;
        }

        if (await CheckCurrentOpenConnection(connectionWrapper))
        {
            return connectionWrapper;
        }

        await CloseConnection(connectionWrapper);
        _connections.Remove(connectionWrapper);
        connectionWrapper.NpgsqlConnection.Dispose();
        return null;
    }

    private void NotifyConnectionStates()
    {
        var copyList = _connections
            .ToList()
            .ConvertAll(x => new
            {
                x.AddedAt,
                x.Id,
                x.IsInUse,
                x.LastContextInUse,
                x.NpgsqlConnection.State,
            });
        var json = JsonConverter.ToJson(copyList);
        _logger.Debug(
            $"[{Env.STAGE}][ConnectionManager][Warning]: near limit of available connections: (ON LIST: {_connections.Count} / POOL SIZE: {Env.DB_POOL_SIZE})"
        );
        _connectionErrorNotification.Notify(
            $"[{Env.STAGE}][ConnectionManager][Warning]: near limit of available connections: (ON LIST: {_connections.Count} / POOL SIZE: {Env.DB_POOL_SIZE})",
            null,
            "WARN"
        );
        _connectionErrorNotification.Notify(
            $"[{Env.STAGE}][ConnectionManager][Warning][statusList]: {json}",
            null,
            "WARN"
        );
    }

    private async Task<ConnectionWrapper> CreateNewConnection()
    {
        _logger.Debug($"Creating new connection: {_connections.Count}");
        if (_connections.Count >= (Env.DB_POOL_SIZE * 0.95))
        {
            NotifyConnectionStates();
            await Task.Delay(200);
            if (_connections.Count > Env.DB_POOL_SIZE && !_isRestarting)
            {
                _logger.Debug(
                    $"[{Env.STAGE}][ConnectionManager][Reseting]: reseting connection list ({_connections.Count} / POOL SIZE: {Env.DB_POOL_SIZE})"
                );
                // _connections = new();
            }
        }

        var connectionWrapper = GetConnection();
        await connectionWrapper.NpgsqlConnection.OpenAsync();
        _logger.Debug("Providing connection: {S}", connectionWrapper.Id.ToString());
        return connectionWrapper;
    }

    private ConnectionWrapper GetConnection() =>
        new() { NpgsqlConnection = DbConfigLoader.Factory(), AddedAt = DateTime.Now };

    private async Task<bool> CheckCurrentOpenConnection(ConnectionWrapper conn)
    {
        try
        {
            await using var command = new NpgsqlCommand("SELECT 1", conn.NpgsqlConnection);
            await command.ExecuteScalarAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(
                "[GetConnectionAsync]: {ExMessage} - no worries, a new connection will be returned for this request",
                ex.Message
            );
            await CloseConnection(conn);
            conn.NpgsqlConnection.Dispose();
            return false;
        }
    }

    private async Task ReleaseUnusedConnections()
    {
        List<ConnectionWrapper> connectionsToRemove = new();
        List<string> releasedConnections = new();
        try
        {
            if (!_connections.Any())
            {
                return;
            }

            foreach (var connection in _connections.ToList())
            {
                _logger.Debug(
                    "CONNECTION STATE: {NpgsqlConnectionState} | context: {Context}",
                    connection.NpgsqlConnection.State,
                    connection.LastContextInUse
                );

                if (
                    connection.NpgsqlConnection.State == ConnectionState.Open
                    && !connection.IsInUse
                )
                {
                    continue;
                }

                if (
                    connection.NpgsqlConnection.State != ConnectionState.Closed
                    || connection.IsInUse
                )
                {
                    continue;
                }

                connectionsToRemove.Add(connection);
                releasedConnections.Add(connection.Id.ToString());
            }
            if (!connectionsToRemove.Any())
            {
                return;
            }
            // Close and remove outside of the iteration loop
            foreach (var connection in connectionsToRemove)
            {
                await CloseConnection(connection);
                _connections.Remove(connection);
            }

            _logger.Debug(
                "[ReleaseUnusedConnections]: released a total of {Total}",
                releasedConnections.Count
            );
        }
        catch (System.Exception ex)
        {
            _logger.Error("[ReleaseUnusedConnections]: {ExMessage}", ex.Message);
        }
    }

    private async Task CloseConnection(ConnectionWrapper conn)
    {
        try
        {
            await conn.NpgsqlConnection.DisposeAsync();
            await conn.NpgsqlConnection.CloseAsync();
        }
        catch (Exception e)
        {
            _logger.Debug("[CloseConnection]: {EMessage}", e.Message);
            return;
        }
    }

    public void ReturnConnection(ConnectionWrapper conn)
    {
        try
        {
            _logger.Debug("Returning connection: {S}", conn.Id.ToString());
            if (conn.NpgsqlConnection.State == ConnectionState.Open)
            {
                conn.TimeoutTokenSource?.Cancel();
                conn.IsInUse = false;
            }
            else
            {
                conn.NpgsqlConnection?.DisposeAsync();
                conn.NpgsqlConnection?.CloseAsync();
            }
        }
        catch (Exception e)
        {
            _logger.Error("[ReturnConnection]: {ExMessage}", e.Message);
        }
    }

    public void Dispose()
    {
        foreach (var conn in _connections)
        {
            conn.NpgsqlConnection?.Dispose();
        }

        _semaphore.Dispose();
    }
}
