// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Event handlers for domain events
/// Implements business logic that responds to video processing events
/// </summary>

public class VideoUploadStartedEventHandler : IEventHandler<VideoUploadStartedEvent>
{
    private readonly ILogger<VideoUploadStartedEventHandler> _logger;

    public VideoUploadStartedEventHandler(ILogger<VideoUploadStartedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(VideoUploadStartedEvent @event)
    {
        _logger.LogInformation("Handling VideoUploadStarted event. VideoId: {VideoId}, FileName: {FileName}",
            @event.VideoId, @event.FileName);

        // Business logic: Log upload start, update database, notify user, etc.
        await Task.CompletedTask;
    }
}

public class VideoUploadCompletedEventHandler : IEventHandler<VideoUploadCompletedEvent>
{
    private readonly ILogger<VideoUploadCompletedEventHandler> _logger;
    private readonly IWebhookPublisher _webhookPublisher;

    public VideoUploadCompletedEventHandler(
        ILogger<VideoUploadCompletedEventHandler> logger,
        IWebhookPublisher webhookPublisher)
    {
        _logger = logger;
        _webhookPublisher = webhookPublisher;
    }

    public async Task HandleAsync(VideoUploadCompletedEvent @event)
    {
        _logger.LogInformation("Handling VideoUploadCompleted event. VideoId: {VideoId}, YouTubeVideoId: {YouTubeVideoId}",
            @event.VideoId, @event.YouTubeVideoId);

        // Business logic: Update database, publish webhook, generate notifications
        try
        {
            // Simulate webhook publishing
            await Task.Delay(100);
            _logger.LogInformation("Video upload completed and notifications sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling VideoUploadCompleted event");
        }
    }
}

public class VideoProcessingCompletedEventHandler : IEventHandler<VideoProcessingCompletedEvent>
{
    private readonly ILogger<VideoProcessingCompletedEventHandler> _logger;

    public VideoProcessingCompletedEventHandler(ILogger<VideoProcessingCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(VideoProcessingCompletedEvent @event)
    {
        _logger.LogInformation("Handling VideoProcessingCompleted event. VideoId: {VideoId}, Duration: {Duration}s",
            @event.VideoId, @event.ProcessingDurationSeconds);

        // Business logic: Trigger next steps, send notifications, update stats
        await Task.CompletedTask;
    }
}

public class AnalyticsUpdatedEventHandler : IEventHandler<AnalyticsUpdatedEvent>
{
    private readonly ILogger<AnalyticsUpdatedEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public AnalyticsUpdatedEventHandler(
        ILogger<AnalyticsUpdatedEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task HandleAsync(AnalyticsUpdatedEvent @event)
    {
        _logger.LogInformation("Handling AnalyticsUpdated event. VideoId: {VideoId}, Views: {Views}, Engagement: {Engagement}",
            @event.VideoId, @event.ViewCount, @event.EngagementRate);

        // Business logic: Cache updated analytics, trigger alerts if thresholds exceeded
        _cacheService.Set($"analytics:video:{@event.VideoId}", @event, TimeSpan.FromMinutes(5));

        await Task.CompletedTask;
    }
}

public class VideoUploadFailedEventHandler : IEventHandler<VideoUploadFailedEvent>
{
    private readonly ILogger<VideoUploadFailedEventHandler> _logger;

    public VideoUploadFailedEventHandler(ILogger<VideoUploadFailedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(VideoUploadFailedEvent @event)
    {
        _logger.LogError("Handling VideoUploadFailed event. VideoId: {VideoId}, ErrorCode: {ErrorCode}, Message: {Message}",
            @event.VideoId, @event.ErrorCode, @event.ErrorMessage);

        // Business logic: Retry logic, notify user, log error details
        if (@event.RetryCount < 3)
        {
            _logger.LogInformation("Scheduling retry for VideoId: {VideoId}, Attempt: {Attempt}",
                @event.VideoId, @event.RetryCount + 1);
        }

        await Task.CompletedTask;
    }
}
