// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

public class UploadResult
{
    public int Id { get; set; }
    public int UploadJobId { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorDetails { get; set; }
    public long UploadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public TimeSpan UploadDuration { get; set; }
    public double AverageUploadSpeed { get; set; }
    public DateTime CompletedAt { get; set; }

    // Navigation property
    public UploadJob? UploadJob { get; set; }

    public void MarkAsSuccessful(string videoId, string videoUrl, long uploadedBytes, long totalBytes, TimeSpan duration)
    {
        // Records a successful upload
        IsSuccessful = true;
        VideoId = videoId;
        VideoUrl = videoUrl;
        UploadedBytes = uploadedBytes;
        TotalBytes = totalBytes;
        UploadDuration = duration;
        CompletedAt = DateTime.UtcNow;
        
        if (duration.TotalSeconds > 0)
        {
            AverageUploadSpeed = (double)uploadedBytes / duration.TotalSeconds / (1024 * 1024);
        }
    }

    public void MarkAsFailed(string errorDetails)
    {
        // Records a failed upload
        IsSuccessful = false;
        ErrorDetails = errorDetails;
        CompletedAt = DateTime.UtcNow;
    }

    public string GetUploadSpeedFormatted()
    {
        // Returns formatted upload speed in MB/s
        return $"{AverageUploadSpeed:F2} MB/s";
    }

    public string GetDurationFormatted()
    {
        // Returns formatted upload duration
        return $"{UploadDuration.Hours:D2}:{UploadDuration.Minutes:D2}:{UploadDuration.Seconds:D2}";
    }
}
