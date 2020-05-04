// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents the result of an upload operation.
/// </summary>
public class UploadResult
{
    /// <summary>
    /// Gets the unique identifier of the upload result.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the identifier of the upload job associated with this result.
    /// </summary>
    public int UploadJobId { get; set; }

    /// <summary>
    /// Gets the ID of the video uploaded.
    /// </summary>
    public string VideoId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the URL of the uploaded video.
    /// </summary>
    public string VideoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the upload was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets the error details if the upload failed.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Gets the total number of bytes uploaded.
    /// </summary>
    public long UploadedBytes { get; set; }

    /// <summary>
    /// Gets the total number of bytes in the uploaded file.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Gets the duration of the upload operation.
    /// </summary>
    public TimeSpan UploadDuration { get; set; }

    /// <summary>
    /// Gets the average upload speed in MB/s.
    /// </summary>
    public double AverageUploadSpeed { get; set; }

    /// <summary>
    /// Gets the date and time when the upload operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Gets the upload job associated with this result.
    /// </summary>
    public UploadJob? UploadJob { get; set; }

    /// <summary>
    /// Marks the upload as successful with the specified video ID, URL, uploaded bytes, total bytes, and duration.
    /// </summary>
    /// <param name="videoId">The ID of the uploaded video.</param>
    /// <param name="videoUrl">The URL of the uploaded video.</param>
    /// <param name="uploadedBytes">The total number of bytes uploaded.</param>
    /// <param name="totalBytes">The total number of bytes in the uploaded file.</param>
    /// <param name="duration">The duration of the upload operation.</param>
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

    /// <summary>
    /// Marks the upload as failed with the specified error details.
    /// </summary>
    /// <param name="errorDetails">The error details.</param>
    public void MarkAsFailed(string errorDetails)
    {
        // Records a failed upload
        IsSuccessful = false;
        ErrorDetails = errorDetails;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns the upload speed in MB/s, formatted as a string.
    /// </summary>
    /// <returns>The upload speed in MB/s, formatted as a string.</returns>
    public string GetUploadSpeedFormatted()
    {
        // Returns formatted upload speed in MB/s
        return $"{AverageUploadSpeed:F2} MB/s";
    }

    /// <summary>
    /// Returns the upload duration, formatted as a string.
    /// </summary>
    /// <returns>The upload duration, formatted as a string.</returns>
    public string GetDurationFormatted()
    {
        // Returns formatted upload duration
        return $"{UploadDuration.Hours:D2}:{UploadDuration.Minutes:D2}:{UploadDuration.Seconds:D2}";
    }
}
