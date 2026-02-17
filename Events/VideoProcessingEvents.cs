// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Domain events for video processing pipeline
/// These events are published when key video processing milestones occur
/// </summary>

public class VideoUploadStartedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Title { get; set; } = string.Empty;

    public override string EventType => nameof(VideoUploadStartedEvent);
}

public class VideoUploadCompletedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public long FileSizeBytes { get; set; }

    public override string EventType => nameof(VideoUploadCompletedEvent);
}

public class VideoUploadFailedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public int RetryCount { get; set; }

    public override string EventType => nameof(VideoUploadFailedEvent);
}

public class VideoProcessingStartedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string InputFilePath { get; set; } = string.Empty;
    public string ProcessingProfile { get; set; } = string.Empty;

    public override string EventType => nameof(VideoProcessingStartedEvent);
}

public class VideoProcessingCompletedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public long OutputFileSizeBytes { get; set; }
    public string VideoResolution { get; set; } = string.Empty;
    public double ProcessingDurationSeconds { get; set; }

    public override string EventType => nameof(VideoProcessingCompletedEvent);
}

public class VideoProcessingFailedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ProcessingProfile { get; set; } = string.Empty;

    public override string EventType => nameof(VideoProcessingFailedEvent);
}

public class AnalyticsUpdatedEvent : DomainEvent
{
    public Guid VideoId { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public double EngagementRate { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public override string EventType => nameof(AnalyticsUpdatedEvent);
}

public class ScheduledUploadEvent : DomainEvent
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;

    public override string EventType => nameof(ScheduledUploadEvent);
}

public abstract class DomainEvent : IEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
