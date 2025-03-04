using System.Data;
using Npgsql;

namespace Shared.Database;

public class ConnectionWrapper
{
    public Guid Id { get; } = Guid.NewGuid();
    public NpgsqlConnection NpgsqlConnection { get; set; } = null!;
    public DateTime AddedAt { get; set; }
    public bool IsInUse { get; set; }
    public CancellationTokenSource? TimeoutTokenSource { get; set; }
    public string? LastContextInUse { get; set; }

    public void SetConnectionInUse(int lifetime = 60)
    {
        IsInUse = true;
        TimeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(lifetime));
        TimeoutTokenSource.Token.Register(() =>
        {
            if (IsInUse == true)
            {
                try
                {
                    IsInUse = false;
                    NpgsqlConnection.Dispose();
                    return;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        });
    }
}

public class ConnectionWrapperView
{
    public Guid Id { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsInUse { get; set; }
    public ConnectionState State { get; set; }
}

public static class ConnectionWrapperExtensions
{
    public static ConnectionWrapperView ToView(this ConnectionWrapper wrapper)
    {
        return new()
        {
            Id = wrapper.Id,
            AddedAt = wrapper.AddedAt,
            IsInUse = wrapper.IsInUse,
            State = wrapper.NpgsqlConnection.State,
        };
    }
}
