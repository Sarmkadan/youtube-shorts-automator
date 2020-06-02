// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Extension methods for the Video class providing common video operations
/// </summary>
public static class VideoExtensions
{
    /// <summary>
    /// Gets the formatted duration of the video in HH:MM:SS format
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Formatted duration string</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetFormattedDuration(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var timeSpan = TimeSpan.FromSeconds(video.DurationSeconds);
        return timeSpan.ToString("hh\\:mm\\:ss");
    }

    /// <summary>
    /// Gets the formatted file size in human-readable format (KB, MB, GB)
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Formatted file size string</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetFormattedFileSize(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return FormatFileSize(video.FileSizeBytes);
    }

    /// <summary>
    /// Gets the formatted creation date
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <param name="format">Optional format string (default: "yyyy-MM-dd HH:mm")</param>
    /// <returns>Formatted date string</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetFormattedCreatedAt(this Video video, string format = "yyyy-MM-dd HH:mm")
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.CreatedAt.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the total processing time in seconds from all completed processing jobs
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Total processing time in seconds, or 0 if no completed jobs</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static int GetTotalProcessingTimeSeconds(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (video.ProcessingJobs == null || video.ProcessingJobs.Count == 0)
            return 0;

        return video.ProcessingJobs
            .Where(j => j.Status == ProcessingJobStatus.Completed && j.CompletedAt.HasValue && j.StartedAt.HasValue)
            .Sum(j => (int)(j.CompletedAt!.Value - j.StartedAt!.Value).TotalSeconds);
    }

    /// <summary>
    /// Gets the total view count from all analytics metrics
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Total view count across all metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static long GetTotalViews(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (video.Metrics == null || video.Metrics.Count == 0)
            return 0;

        return video.Metrics.Sum(m => m.ViewCount);
    }

    /// <summary>
    /// Gets the total engagement score (0-100) from all analytics metrics
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Average engagement score across all metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static double GetAverageEngagementScore(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (video.Metrics == null || video.Metrics.Count == 0)
            return 0;

        return video.Metrics.Average(m => m.CalculateEngagementScore());
    }

    /// <summary>
    /// Gets the primary tag from the video's tags array
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <param name="fallback">Optional fallback value if no tags exist</param>
    /// <returns>The first non-empty tag, or fallback if no tags exist</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetPrimaryTag(this Video video, string fallback = "general")
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.Tags.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? fallback;
    }

    /// <summary>
    /// Checks if the video has any processing jobs with the specified status
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <param name="status">The processing job status to check for</param>
    /// <returns>True if any processing job has the specified status</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static bool HasProcessingJobStatus(this Video video, ProcessingJobStatus status)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.ProcessingJobs?.Any(j => j.Status == status) ?? false;
    }

    /// <summary>
    /// Gets the video's file extension
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>File extension including dot, or empty string if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetFileExtension(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (string.IsNullOrWhiteSpace(video.FilePath))
            return string.Empty;

        var extension = Path.GetExtension(video.FilePath);
        return extension ?? string.Empty;
    }

    /// <summary>
    /// Gets the video's file name without path
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>File name without directory path</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetFileName(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (string.IsNullOrWhiteSpace(video.FilePath))
            return string.Empty;

        return Path.GetFileName(video.FilePath);
    }

    /// <summary>
    /// Gets the video's directory path
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Directory path, or empty string if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetDirectoryPath(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        if (string.IsNullOrWhiteSpace(video.FilePath))
            return string.Empty;

        return Path.GetDirectoryName(video.FilePath) ?? string.Empty;
    }

    /// <summary>
    /// Gets the video's upload status description
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>Human-readable status description</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetStatusDescription(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.Status switch
        {
            VideoStatus.Pending => "Pending processing",
            VideoStatus.Processing => "Currently being processed",
            VideoStatus.Processed => "Processing complete, ready for upload",
            VideoStatus.Uploading => "Uploading to YouTube",
            VideoStatus.Uploaded => "Uploaded to YouTube",
            VideoStatus.Published => "Published on YouTube",
            VideoStatus.Error => "Error occurred during processing",
            _ => "Unknown status"
        };
    }

    /// <summary>
    /// Gets the video's storage usage in bytes
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <returns>File size in bytes</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static long GetStorageUsage(this Video video)
    {
        ArgumentNullException.ThrowIfNull(video);

        return video.FileSizeBytes;
    }

    /// <summary>
    /// Gets the video's title with fallback for empty titles
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <param name="fallback">Fallback title if original is empty</param>
    /// <returns>Video title or fallback</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetTitleOrFallback(this Video video, string fallback = "Untitled Video")
    {
        ArgumentNullException.ThrowIfNull(video);

        return string.IsNullOrWhiteSpace(video.Title) ? fallback : video.Title;
    }

    /// <summary>
    /// Gets the video's description with fallback for empty descriptions
    /// </summary>
    /// <param name="video">The video instance</param>
    /// <param name="fallback">Fallback description if original is empty</param>
    /// <returns>Video description or fallback</returns>
    /// <exception cref="ArgumentNullException">Thrown when video is null</exception>
    public static string GetDescriptionOrFallback(this Video video, string fallback = "No description provided")
    {
        ArgumentNullException.ThrowIfNull(video);

        return string.IsNullOrWhiteSpace(video.Description) ? fallback : video.Description;
    }

    /// <summary>
    /// Formats file size in bytes to human-readable format (KB, MB, GB)
    /// </summary>
    /// <param name="bytes">File size in bytes</param>
    /// <returns>Formatted file size string</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double len = bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}