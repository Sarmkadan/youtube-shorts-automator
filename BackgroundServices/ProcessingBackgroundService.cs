// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.BackgroundServices;

/// <summary>
/// Background service for processing queued video jobs
/// Monitors processing queue and orchestrates video processing pipeline
/// </summary>
public class ProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<ProcessingBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public ProcessingBackgroundService(
        ILogger<ProcessingBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    await ProcessQueuedJobsAsync(scope.ServiceProvider, stoppingToken);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing Background Service is shutting down");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in processing background service");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ProcessQueuedJobsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            // This would typically fetch pending jobs from database
            // and orchestrate their processing
            _logger.LogDebug("Checking for queued processing jobs");

            // Placeholder for actual processing logic
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queued jobs");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
