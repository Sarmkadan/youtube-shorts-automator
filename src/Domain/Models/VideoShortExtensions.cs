using System;
using System.Collections.Generic;
using System.Globalization;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="VideoShort"/> class.
/// </summary>
public static class VideoShortExtensions
{
    /// <summary>
    /// Gets the file size of the video in megabytes.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns>The file size in megabytes, or 0 if the file size is not set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static double GetFileSizeMB(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        return videoShort.FileSizeBytes > 0
            ? Math.Round(videoShort.FileSizeBytes / (1024.0 * 1024.0), 2, MidpointRounding.AwayFromZero)
            : 0;
    }

    /// <summary>
    /// Gets a human-readable string representation of the video duration.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns>A formatted duration string in the format "mm:ss".</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static string GetDurationString(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        return $"{videoShort.Duration.TotalMinutes:0}:{videoShort.Duration.Seconds:00}";
    }

    /// <summary>
    /// Determines whether the video short has the specified tag.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <param name="tag">The tag to check for.</param>
    /// <returns><see langword="true"/> if the video short has the specified tag; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="tag"/> is <see langword="null"/> or whitespace.</exception>
    public static bool HasTag(this VideoShort videoShort, string tag)
    {
        ArgumentNullException.ThrowIfNull(videoShort);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        if (videoShort.Tags == null || videoShort.Tags.Length == 0)
        {
            return false;
        }

        var comparison = StringComparison.OrdinalIgnoreCase;
        foreach (var videoTag in videoShort.Tags)
        {
            if (tag.Equals(videoTag, comparison))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a user-friendly display name for the video quality.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns>A user-friendly quality name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static string GetQualityDisplayName(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        return videoShort.Quality switch
        {
            VideoQuality.Low => "360p",
            VideoQuality.Medium => "720p",
            VideoQuality.High => "1080p",
            VideoQuality.UltraHD => "1440p",
            _ => videoShort.Quality.ToString()
        };
    }

    /// <summary>
    /// Gets a collection of all tags associated with the video short.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns>A read-only collection of tags.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> GetTags(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        return videoShort.Tags ?? Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the video short is in a terminal state (Completed, Failed, Uploaded, or Cancelled).
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns><see langword="true"/> if the video short is in a terminal state; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static bool IsTerminalState(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        return videoShort.Status is ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Uploaded or ProcessingStatus.Cancelled;
    }

    /// <summary>
    /// Gets the estimated processing time in seconds based on file size and quality.
    /// </summary>
    /// <param name="videoShort">The video short instance.</param>
    /// <returns>The estimated processing time in seconds, or 0 if not estimable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="videoShort"/> is <see langword="null"/></exception>
    public static int GetEstimatedProcessingTimeSeconds(this VideoShort videoShort)
    {
        ArgumentNullException.ThrowIfNull(videoShort);

        if (videoShort.FileSizeBytes <= 0 || videoShort.Duration.TotalSeconds <= 0)
        {
            return 0;
        }

        // Base processing time is duration * 2 (real-time processing)
        // Add additional time based on file size (1 second per 5MB)
        var baseTime = (int)Math.Ceiling(videoShort.Duration.TotalSeconds * 2);
        var sizeFactor = videoShort.FileSizeBytes / (5.0 * 1024 * 1024);
        var totalTime = baseTime + (int)Math.Ceiling(sizeFactor);

        return Math.Max(1, totalTime);
    }
}
