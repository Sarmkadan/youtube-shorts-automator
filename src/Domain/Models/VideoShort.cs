// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

public class VideoShort
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }
    public VideoQuality Quality { get; set; }
    public ProcessingStatus Status { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int ProcessingProfileId { get; set; }
    public int YouTubeChannelId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public ProcessingProfile? ProcessingProfile { get; set; }
    public YouTubeChannel? YouTubeChannel { get; set; }
    public ICollection<UploadJob> UploadJobs { get; set; } = new List<UploadJob>();
    public ICollection<ProcessingTask> ProcessingTasks { get; set; } = new List<ProcessingTask>();
    public AnalyticsData? Analytics { get; set; }

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

    public void MarkAsProcessing()
    {
        Status = ProcessingStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

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

    public void MarkAsUploaded()
    {
        Status = ProcessingStatus.Uploaded;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeProcessed()
    {
        return Status == ProcessingStatus.Pending && IsValid();
    }
}
