// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="YouTubeChannel"/> class.
/// </summary>
public static class YouTubeChannelExtensions
{
    /// <summary>
    /// Gets the formatted subscriber count for display purposes.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>A formatted string representing the subscriber count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetFormattedSubscriberCount(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return FormatNumber(channel.SubscriberCount);
    }

    /// <summary>
    /// Gets the formatted view count for display purposes.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>A formatted string representing the view count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetFormattedViewCount(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return FormatNumber(channel.ViewCount);
    }

    /// <summary>
    /// Gets the formatted video count for display purposes.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>A formatted string representing the video count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetFormattedVideoCount(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return FormatNumber(channel.VideoCount);
    }

    /// <summary>
    /// Gets the engagement rate percentage based on views and subscribers.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>The engagement rate as a percentage, or 0 if subscribers is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static double GetEngagementRate(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (channel.SubscriberCount == 0)
        {
            return 0;
        }

        // Engagement rate = (views / subscribers) * 100
        return Math.Round((double)channel.ViewCount / channel.SubscriberCount * 100, 2);
    }

    /// <summary>
    /// Gets the video upload frequency in videos per day.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>The average number of videos uploaded per day, or 0 if channel is new.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static double GetVideoUploadFrequency(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (channel.VideoCount == 0 || channel.CreatedAt == default)
        {
            return 0;
        }

        var daysSinceCreation = (DateTime.UtcNow - channel.CreatedAt).TotalDays;
        return Math.Round(channel.VideoCount / daysSinceCreation, 2);
    }

    /// <summary>
    /// Checks if the channel is considered "small" based on subscriber count.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <param name="threshold">The subscriber threshold for a small channel (default: 10000).</param>
    /// <returns>true if the channel has fewer subscribers than the threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static bool IsSmallChannel(this YouTubeChannel channel, long threshold = 10_000)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return channel.SubscriberCount < threshold;
    }

    /// <summary>
    /// Gets the channel's profile image URL with a fallback to a default image if not set.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>The profile image URL or a default YouTube avatar URL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetProfileImageUrlOrDefault(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return string.IsNullOrWhiteSpace(channel.ProfileImageUrl)
            ? "https://yt3.ggpht.com/ytc/default.jpg"
            : channel.ProfileImageUrl;
    }

    /// <summary>
    /// Gets the channel's description truncated to a maximum length.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <param name="maxLength">The maximum length of the description (default: 200).</param>
    /// <returns>The truncated description.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetTruncatedDescription(this YouTubeChannel channel, int maxLength = 200)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (string.IsNullOrWhiteSpace(channel.Description))
        {
            return string.Empty;
        }

        return channel.Description.Length <= maxLength
            ? channel.Description
            : channel.Description[..maxLength] + "...";
    }

    /// <summary>
    /// Gets the channel's age in days.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>The number of days since the channel was created.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static int GetChannelAgeInDays(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (channel.CreatedAt == default)
        {
            return 0;
        }

        return (int)(DateTime.UtcNow - channel.CreatedAt).TotalDays;
    }

    /// <summary>
    /// Gets all video shorts that were uploaded within the last N days.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <param name="days">The number of days to look back (default: 7).</param>
    /// <returns>A read-only list of video shorts uploaded within the specified time period.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static IReadOnlyList<VideoShort> GetRecentVideoShorts(this YouTubeChannel channel, int days = 7)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (channel.VideoShorts == null || channel.VideoShorts.Count == 0)
        {
            return Array.Empty<VideoShort>();
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return channel.VideoShorts
            .Where(vs => vs.CreatedAt >= cutoffDate)
            .OrderByDescending(vs => vs.CreatedAt)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the total number of video shorts uploaded by the channel.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>The count of video shorts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static int GetVideoShortsCount(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return channel.VideoShorts?.Count ?? 0;
    }

    /// <summary>
    /// Checks if the channel is monetized based on subscriber count and view count.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <param name="minSubscribers">Minimum subscribers required (default: 1000).</param>
    /// <param name="minViews">Minimum views required (default: 4000).</param>
    /// <returns>true if the channel meets monetization requirements; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static bool IsMonetized(this YouTubeChannel channel, long minSubscribers = 1_000, long minViews = 4_000)
    {
        ArgumentNullException.ThrowIfNull(channel);

        return channel.SubscriberCount >= minSubscribers && channel.ViewCount >= minViews;
    }

    /// <summary>
    /// Gets the channel's engagement level based on subscriber count.
    /// </summary>
    /// <param name="channel">The YouTube channel.</param>
    /// <returns>A string representing the engagement level: "Low", "Medium", "High", or "Very High".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channel"/> is null.</exception>
    public static string GetEngagementLevel(this YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        var engagementRate = channel.GetEngagementRate();

        return engagementRate switch
        {
            <= 5 => "Low",
            <= 15 => "Medium",
            <= 30 => "High",
            _ => "Very High"
        };
    }

    /// <summary>
    /// Formats a number with appropriate suffix (K, M, B) for display.
    /// </summary>
    /// <param name="number">The number to format.</param>
    /// <returns>A formatted string with suffix.</returns>
    private static string FormatNumber(long number)
    {
        if (number < 1_000)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        if (number < 1_000_000)
        {
            return $"{number / 1_000.0:0.#}K";
        }

        if (number < 1_000_000_000)
        {
            return $"{number / 1_000_000.0:0.#}M";
        }

        return $"{number / 1_000_000_000.0:0.#}B";
    }
}