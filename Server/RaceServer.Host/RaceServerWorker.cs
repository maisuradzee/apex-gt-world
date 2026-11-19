using ApexGTWorld.RaceServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApexGTWorld.RaceServer.Host;

/// <summary>
/// Windows Service process host. This currently verifies the protected
/// Supabase control configuration and holds the service lifecycle open.
/// The authoritative physics/network simulation adapters are separate work.
/// </summary>
public sealed class RaceServerWorker : BackgroundService
{
    private readonly ILogger<RaceServerWorker> _logger;

    public RaceServerWorker(ILogger<RaceServerWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var controlClient = RaceControlClient.FromEnvironment();
        var endpoint = Environment.GetEnvironmentVariable("APEX_RACE_CONTROL_URL");

        _logger.LogInformation(
            "APEX GT WORLD Race Server host started. Protected control endpoint configured: {Endpoint}",
            endpoint);

        _logger.LogWarning(
            "Control-plane host is running, but authoritative vehicle physics, player networking and race simulation adapters are not yet attached.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("APEX GT WORLD Race Server host is stopping.");
        }
    }
}
