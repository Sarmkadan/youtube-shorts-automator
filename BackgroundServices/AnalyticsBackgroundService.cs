// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.BackgroundServices;

/// <summary>
/// Background service for updating analytics from YouTube API
/// Periodically fetches metrics for uploaded videos
/// </summary>
public class AnalyticsBackgroundService : BackgroundService
{
    private readonly ILogger<AnalyticsBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private TimeSpan _updateInterval = TimeSpan.FromHours(1);

    public AnalyticsBackgroundService(
        ILogger<AnalyticsBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics Background Service started");

        // Allow service to start before first update
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    await UpdateAnalyticsAsync(stoppingToken);
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Analytics Background Service is shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task UpdateAnalyticsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting analytics update");

        try
        {
            // Placeholder for actual analytics fetching logic
            // This would typically:
            // 1. Fetch list of uploaded videos
            // 2. Call YouTube API to get current metrics
            // 3. Update database with latest analytics
            // 4. Publish analytics updated events

            _logger.LogDebug("Analytics update completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating analytics");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
