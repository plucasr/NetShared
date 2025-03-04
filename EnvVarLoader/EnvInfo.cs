using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.EnvVarLoader;

public class EnvInfo
{
    private readonly ILogger _logger;
    public string SessionLiftedAt { get; set; } = String.Empty;
    public string EnvLabel { get; set; } = Env.STAGE;
    public Guid SessionId { get; set; }
    public string ProcessId { get; set; } = String.Empty;
    public string SessionHash { get; set; } = String.Empty;
    public string InstanceHash { get; set; } = String.Empty;
    public int NodeId { get; set; }

    public EnvInfo(ILogger logger)
    {
        _logger = logger;
        Init();
        // Env.STAGE;
    }

    static Random random = new Random();

    public static string GetRandomHexNumber(int digits)
    {
        byte[] buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
            return result;
        return result + random.Next(16).ToString("X");
    }

    private void Init()
    {
        SessionLiftedAt = Regex.Replace(
            $"BRT {DateTime.UtcNow.AddHours(-3).ToString()}",
            @"\s",
            "||"
        );
        Process currentProcess = Process.GetCurrentProcess();
        ProcessId = currentProcess.Id.ToString();
        SessionId = Guid.NewGuid();

        SessionHash = Regex.Replace(
            $"[{ProcessId}][{SessionId.ToString()}][{SessionLiftedAt}]",
            @"\s",
            String.Empty
        );
        InstanceHash = GetRandomHexNumber(6);
    }

    public void Info()
    {
        _logger.Information($"Server env started: {SessionHash}");
        _logger.Information($"Server env started: {InstanceHash}");
    }

    public void Info(string trace)
    {
        _logger.Information($"{trace}: Server env started: {SessionHash}");
        _logger.Information($"{trace}: Server env Hash: {InstanceHash}");
    }
}

public static class EnvInfoConfigExtensions
{
    public static IServiceCollection AddEnvInfo(this IServiceCollection services, ILogger logger)
    {
        services.AddSingleton<EnvInfo>();
        return services;
    }

    public static IApplicationBuilder UseEnvInfo(this IApplicationBuilder builder)
    {
        var envInfo = builder.ApplicationServices.GetRequiredService<EnvInfo>();
        envInfo.Info("[UseEnvInfo]");
        return builder;
    }
}
