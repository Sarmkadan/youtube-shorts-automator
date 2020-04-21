// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for VideoUploadStartedEvent providing useful domain operations
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Extension methods for <see cref="VideoUploadStartedEvent"/> providing domain operations
/// </summary>
public static class VideoUploadStartedEventExtensions
{
    /// <summary>
    /// Gets the file size in human-readable format (KB, MB, GB)
    /// </summary>
    /// <param name="event">The video upload started event</param>
    /// <returns>Formatted file size string</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    public static string GetFormattedFileSize(this VideoUploadStartedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return FormatFileSize(@event.FileSizeBytes);
    }

    /// <summary>
    /// Gets the file extension from the file name
    /// </summary>
    /// <param name="event">The video upload started event</param>
    /// <returns>File extension without dot, or empty string if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    public static string GetFileExtension(this VideoUploadStartedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (string.IsNullOrEmpty(@event.FileName))
        {
            return string.Empty;
        }

        var extension = Path.GetExtension(@event.FileName);
        return extension.Length > 1 ? extension[1..] : string.Empty;
    }

    /// <summary>
    /// Determines if the video file size exceeds a specified threshold
    /// </summary>
    /// <param name="event">The video upload started event</param>
    /// <param name="thresholdBytes">The threshold in bytes to compare against</param>
    /// <returns>True if file size exceeds threshold, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    public static bool IsFileSizeExceeding(this VideoUploadStartedEvent @event, long thresholdBytes)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event.FileSizeBytes > thresholdBytes;
    }

    /// <summary>
    /// Gets the video title truncated to a maximum length
    /// </summary>
    /// <param name="event">The video upload started event</param>
    /// <param name="maxLength">Maximum title length (default: 100)</param>
    /// <returns>Truncated title with ellipsis if needed</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxLength is less than 3</exception>
    public static string GetTruncatedTitle(this VideoUploadStartedEvent @event, int maxLength = 100)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);

        if (string.IsNullOrEmpty(@event.Title) || @event.Title.Length <= maxLength)
        {
            return @event.Title;
        }

        return @event.Title[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Gets the file name without directory path
    /// </summary>
    /// <param name="event">The video upload started event</param>
    /// <returns>File name only, without directory path</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    public static string GetFileNameWithoutPath(this VideoUploadStartedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return Path.GetFileName(@event.FileName);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
    }
}