using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a short video that can be processed and uploaded to YouTube.
/// </summary>
public class VideoShort
{
    /// <summary>
    /// Gets or sets the unique identifier of the video short.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the video short. Must be non‑empty and up to 100 characters.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the video short. Maximum 5000 characters.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file system path to the video file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file system path to the thumbnail image.
    /// </summary>
    public string ThumbnailPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the video.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the size of the video file in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the video quality.
    /// </summary>
    public VideoQuality Quality { get; set; }

    /// <summary>
    /// Gets or sets the current processing status of the video short.
    /// </summary>
    public ProcessingStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the video short.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the identifier of the processing profile used for this video short.
    /// </summary>
    public int ProcessingProfileId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the YouTube channel to which this video short will be uploaded.
    /// </summary>
    public int YouTubeChannelId { get; set; }

    /// <summary>
    /// Gets or sets the error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the video short was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the video short was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the video short finished processing.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Gets or sets the processing profile associated with this video short.
    /// </summary>
    public ProcessingProfile? ProcessingProfile { get; set; }

    /// <summary>
    /// Gets or sets the YouTube channel associated with this video short.
    /// </summary>
    public YouTubeChannel? YouTubeChannel { get; set; }

    /// <summary>
    /// Gets or sets the collection of upload jobs related to this video short.
    /// </summary>
    public ICollection<UploadJob> UploadJobs { get; set; } = new List<UploadJob>();

    /// <summary>
    /// Gets or sets the collection of processing tasks related to this video short.
    /// </summary>
    public ICollection<ProcessingTask> ProcessingTasks { get; set; } = new List<ProcessingTask>();

    /// <summary>
    /// Gets or sets the analytics data for this video short.
    /// </summary>
    public AnalyticsData? Analytics { get; set; }

    /// <summary>
    /// Determines whether the video short has valid metadata.
    /// </summary>
    /// <returns><c>true</c> if the metadata is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        // Validates the video short metadata
        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 100)
            return false;
        if (Description.Length > 5000)
            return false;
        if (string.IsNullOrWhiteSpace(FilePath))
            return false;
        if (Duration.TotalSeconds < 1 || Duration.TotalSeconds > 60)
            return false;
        if (ProcessingProfileId <= 0 || YouTubeChannelId <= 0)
            return false;
        return true;
    }

    /// <summary>
    /// Marks the video short as processing and updates the <see cref="UpdatedAt"/> timestamp.
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = ProcessingStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the video short as processed. If an error message is provided, the status is set to <see cref="ProcessingStatus.Failed"/>; otherwise, the status is set to <see cref="ProcessingStatus.Completed"/> and the <see cref="ProcessedAt"/> timestamp is recorded.
    /// </summary>
    /// <param name="error">An optional error message indicating why processing failed.</param>
    public void MarkAsProcessed(string? error = null)
    {
        if (error != null)
        {
            Status = ProcessingStatus.Failed;
            ErrorMessage = error;
        }
        else
        {
            Status = ProcessingStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
            ErrorMessage = null;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the video short as uploaded and updates the <see cref="UpdatedAt"/> timestamp.
    /// </summary>
    public void MarkAsUploaded()
    {
        Status = ProcessingStatus.Uploaded;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the video short can be processed, i.e., its status is <see cref="ProcessingStatus.Pending"/> and its metadata is valid.
    /// </summary>
    /// <returns><c>true</c> if the video short can be processed; otherwise, <c>false</c>.</returns>
    public bool CanBeProcessed()
    {
        return Status == ProcessingStatus.Pending && IsValid();
    }
}
