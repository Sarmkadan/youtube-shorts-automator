// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class Video
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string ThumbnailPath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Pending;
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? YouTubeVideoId { get; set; }
    public List<ProcessingJob> ProcessingJobs { get; set; } = new();
    public List<AnalyticsMetric> Metrics { get; set; } = new();
    public UploadResult? UploadResult { get; set; }

    /// <summary>
    /// Validates the video data for processing
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 100)
            errors.Add("Title must be between 1 and 100 characters");

        if (Description.Length > 5000)
            errors.Add("Description must not exceed 5000 characters");

        if (string.IsNullOrWhiteSpace(FilePath))
            errors.Add("File path is required");

        if (FileSizeBytes <= 0)
            errors.Add("File size must be greater than zero");

        if (FileSizeBytes > 4_294_967_296) // 4GB limit
            errors.Add("File size exceeds maximum limit of 4GB");

        if (DurationSeconds <= 0 || DurationSeconds > 3600)
            errors.Add("Duration must be between 1 second and 60 minutes");

        if (Tags.Length > 500)
            errors.Add("Cannot have more than 500 tags");

        if (Tags.Any(t => t.Length > 30))
            errors.Add("Individual tags cannot exceed 30 characters");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Marks the video as processed
    /// </summary>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Status = VideoStatus.Processed;
    }

    /// <summary>
    /// Marks the video with upload information
    /// </summary>
    public void MarkAsUploaded(string youtubeVideoId)
    {
        YouTubeVideoId = youtubeVideoId;
        Status = VideoStatus.Uploaded;
    }

    /// <summary>
    /// Sets error status for the video
    /// </summary>
    public void MarkAsError(string errorMessage)
    {
        Status = VideoStatus.Error;
    }
}

public enum VideoStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Uploading = 3,
    Uploaded = 4,
    Published = 5,
    Error = 99
}
