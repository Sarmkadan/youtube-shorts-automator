// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Domain models for video processing requests and pipeline configuration
/// Represents the complete request lifecycle for video processing operations
/// </summary>

public class ProcessingJobRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public string VideoFilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string ProcessingProfile { get; set; } = "standard";
    public ProcessingOptions Options { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string RequestedBy { get; set; } = string.Empty;
}

public class ProcessingOptions
{
    public bool EnableWatermark { get; set; }
    public string? WatermarkImagePath { get; set; }
    public bool AutoGenerateThumbnail { get; set; } = true;
    public bool OptimizeForMobile { get; set; } = true;
    public int MaxWidth { get; set; } = 1920;
    public int MaxHeight { get; set; } = 1080;
    public int BitrateKbps { get; set; } = 4000;
    public bool EnableAudioNormalization { get; set; } = true;
}

public class VideoUploadRequest
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string ChannelId { get; set; } = string.Empty;
    public bool MakePublic { get; set; } = true;
    public string? Thumbnail { get; set; }
    public DateTime? ScheduledPublishTimeUtc { get; set; }
    public string License { get; set; } = "creativeCommon";
    public bool AllowEmbedding { get; set; } = true;
}

public class ProcessingResult
{
    public Guid RequestId { get; set; }
    public bool IsSuccessful { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public long OutputFileSizeBytes { get; set; }
    public string VideoResolution { get; set; } = string.Empty;
    public double ProcessingDurationSeconds { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}

public class UploadMetadata
{
    public Guid VideoId { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public double EngagementRate { get; set; }
}

public class BatchProcessingRequest
{
    public Guid BatchId { get; set; } = Guid.NewGuid();
    public List<ProcessingJobRequest> Jobs { get; set; } = new();
    public int ParallelProcessingLimit { get; set; } = 3;
    public bool StopOnFirstError { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class BatchProcessingResult
{
    public Guid BatchId { get; set; }
    public int TotalJobs { get; set; }
    public int SuccessfulJobs { get; set; }
    public int FailedJobs { get; set; }
    public List<ProcessingResult> Results { get; set; } = new();
    public TimeSpan TotalProcessingDuration { get; set; }
    public DateTime CompletedAtUtc { get; set; }

    public double SuccessRate => TotalJobs > 0 ? ((double)SuccessfulJobs / TotalJobs) * 100 : 0;
    public bool IsFullySuccessful => FailedJobs == 0;
}
