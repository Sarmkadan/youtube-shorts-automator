// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

public class UploadJob
{
    public int Id { get; set; }
    public int VideoShortId { get; set; }
    public string YouTubeVideoId { get; set; } = string.Empty;
    public UploadStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? UploadedAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
    public long UploadedBytes { get; set; }
    public double UploadProgressPercentage { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public VideoShort? VideoShort { get; set; }

    public bool CanRetry()
    {
        // Check if the job can be retried
        return AttemptCount < MaxRetries && Status == UploadStatus.Failed;
    }

    public void IncrementAttempt()
    {
        AttemptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsQueued()
    {
        Status = UploadStatus.Queued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUploading()
    {
        Status = UploadStatus.Uploading;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string videoId)
    {
        Status = UploadStatus.Completed;
        YouTubeVideoId = videoId;
        UploadedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UploadProgressPercentage = 100.0;
    }

    public void MarkAsFailed(string error)
    {
        Status = UploadStatus.Failed;
        ErrorMessage = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(long uploadedBytes, long totalBytes)
    {
        UploadedBytes = uploadedBytes;
        UploadProgressPercentage = (double)uploadedBytes / totalBytes * 100;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        // Validates upload job metadata
        if (VideoShortId <= 0)
            return false;
        if (MaxRetries < 0 || MaxRetries > 10)
            return false;
        if (ScheduledAt < DateTime.UtcNow)
            return false;
        return true;
    }
}
