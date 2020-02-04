// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class UploadResult
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public UploadStatus Status { get; set; } = UploadStatus.Pending;
    public DateTime UploadedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
    public int FailureRetries { get; set; }
    public string? FailureReason { get; set; }
    public long UploadedFileSizeBytes { get; set; }
    public TimeSpan UploadDuration { get; set; }
    public UploadQuality Quality { get; set; } = UploadQuality.Optimal;
    public List<UploadMetadata> UploadMetadata { get; set; } = new();

    /// <summary>
    /// Marks the upload as successfully completed
    /// </summary>
    public void MarkAsCompleted(string youtubeUrl)
    {
        Status = UploadStatus.Completed;
        YouTubeUrl = youtubeUrl;
        UploadedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the upload as published on YouTube
    /// </summary>
    public void MarkAsPublished()
    {
        Status = UploadStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records an upload failure
    /// </summary>
    public void RecordFailure(string reason)
    {
        FailureRetries++;
        FailureReason = reason;
        Status = UploadStatus.Failed;

        if (FailureRetries >= 3)
        {
            Status = UploadStatus.PermanentlyFailed;
        }
    }

    /// <summary>
    /// Calculates upload speed in Mbps
    /// </summary>
    public double GetUploadSpeedMbps()
    {
        if (UploadDuration.TotalSeconds == 0)
            return 0;

        var totalBits = UploadedFileSizeBytes * 8;
        return totalBits / (1_000_000 * UploadDuration.TotalSeconds);
    }

    /// <summary>
    /// Validates upload result completeness
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(YouTubeVideoId))
            errors.Add("YouTube video ID is required");

        if (string.IsNullOrWhiteSpace(YouTubeUrl))
            errors.Add("YouTube URL is required");

        if (UploadedFileSizeBytes <= 0)
            errors.Add("Uploaded file size must be greater than zero");

        if (Status == UploadStatus.Completed && UploadedAt == default)
            errors.Add("Upload timestamp is required for completed uploads");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Adds metadata to the upload
    /// </summary>
    public void AddMetadata(string key, string value)
    {
        UploadMetadata.Add(new UploadMetadata
        {
            Id = Guid.NewGuid(),
            UploadResultId = Id,
            MetadataKey = key,
            MetadataValue = value,
            AddedAt = DateTime.UtcNow
        });
    }
}

public enum UploadStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Published = 3,
    Failed = 4,
    PermanentlyFailed = 5,
    Cancelled = 6
}

public enum UploadQuality
{
    Low = 0,
    Medium = 1,
    High = 2,
    Optimal = 3
}

public class UploadMetadata
{
    public Guid Id { get; set; }
    public Guid UploadResultId { get; set; }
    public string MetadataKey { get; set; } = string.Empty;
    public string MetadataValue { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
