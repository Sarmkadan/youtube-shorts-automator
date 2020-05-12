// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a job to upload a video to YouTube.
/// </summary>
public class UploadJob
{
    /// <summary>
    /// Gets the unique identifier of the upload job.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the identifier of the video short associated with this upload job.
    /// </summary>
    public int VideoShortId { get; set; }

    /// <summary>
    /// Gets the YouTube video ID associated with this upload job.
    /// </summary>
    public string YouTubeVideoId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the status of the upload job.
    /// </summary>
    public UploadStatus Status { get; set; }

    /// <summary>
    /// Gets the scheduled date and time of the upload job.
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Gets the date and time when the upload job was completed.
    /// </summary>
    public DateTime? UploadedAt { get; set; }

    /// <summary>
    /// Gets the number of attempts made to upload the video.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Gets the maximum number of retries allowed for the upload job.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets any error message associated with the upload job.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the total number of bytes uploaded.
    /// </summary>
    public long UploadedBytes { get; set; }

    /// <summary>
    /// Gets the progress percentage of the upload job.
    /// </summary>
    public double UploadProgressPercentage { get; set; }

    /// <summary>
    /// Gets the estimated time remaining to complete the upload job.
    /// </summary>
    public TimeSpan EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Gets the date and time when the upload job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time when the upload job was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets the video short associated with this upload job.
    /// </summary>
    public VideoShort? VideoShort { get; set; }

    /// <summary>
    /// Checks if the upload job can be retried.
    /// </summary>
    /// <returns>true if the upload job can be retried; otherwise, false.</returns>
    public bool CanRetry()
    {
        // Check if the job can be retried
        return AttemptCount < MaxRetries && Status == UploadStatus.Failed;
    }

    /// <summary>
    /// Increments the attempt count of the upload job.
    /// </summary>
    public void IncrementAttempt()
    {
        AttemptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the upload job as queued.
    /// </summary>
    public void MarkAsQueued()
    {
        Status = UploadStatus.Queued;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the upload job as uploading.
    /// </summary>
    public void MarkAsUploading()
    {
        Status = UploadStatus.Uploading;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the upload job as completed.
    /// </summary>
    /// <param name="videoId">The YouTube video ID associated with the completed upload job.</param>
    public void MarkAsCompleted(string videoId)
    {
        Status = UploadStatus.Completed;
        YouTubeVideoId = videoId;
        UploadedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UploadProgressPercentage = 100.0;
    }

    /// <summary>
    /// Marks the upload job as failed.
    /// </summary>
    /// <param name="error">The error message associated with the failed upload job.</param>
    public void MarkAsFailed(string error)
    {
        Status = UploadStatus.Failed;
        ErrorMessage = error;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the progress of the upload job.
    /// </summary>
    /// <param name="uploadedBytes">The total number of bytes uploaded.</param>
    /// <param name="totalBytes">The total number of bytes to be uploaded.</param>
    public void UpdateProgress(long uploadedBytes, long totalBytes)
    {
        UploadedBytes = uploadedBytes;
        UploadProgressPercentage = (double)uploadedBytes / totalBytes * 100;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the upload job metadata.
    /// </summary>
    /// <returns>true if the upload job metadata is valid; otherwise, false.</returns>
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
