// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

public static class AnalyticsServiceExtensions
{
    /// <summary>
    /// Calculates the engagement ratio (likes to views ratio) for a video
    /// </summary>
    /// <param name="service">The analytics service instance</param>
    /// <param name="videoShortId">The video short ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Engagement ratio as a decimal percentage, or 0 if no views or likes</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    public static async Task<decimal> CalculateEngagementRatioAsync(this AnalyticsService service, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var analytics = await service.GetVideoAnalyticsAsync(videoShortId, cancellationToken);

        if (analytics == null || analytics.ViewCount == 0)
        {
            return 0;
        }

        if (analytics.LikeCount == 0)
        {
            return 0;
        }

        return (decimal)analytics.LikeCount / analytics.ViewCount * 100;
    }

    /// <summary>
    /// Gets the top performing videos by a specific metric
    /// </summary>
    /// <param name="service">The analytics service instance</param>
    /// <param name="limit">Number of videos to return</param>
    /// <param name="sortBy">Metric to sort by (views, likes, comments, shares, engagement)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top performing videos sorted by the specified metric</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sortBy"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="sortBy"/> is empty or whitespace</exception>
    public static async Task<IEnumerable<AnalyticsData>> GetTopPerformingVideosAsync(this AnalyticsService service, int limit, string sortBy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(sortBy);

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            throw new ArgumentException("Sort metric cannot be empty or whitespace.", nameof(sortBy));
        }

        var topVideos = await service.GetTopPerformingVideosAsync(limit, cancellationToken);

        return sortBy.ToLowerInvariant() switch
        {
            "views" => topVideos.OrderByDescending(v => v.ViewCount),
            "likes" => topVideos.OrderByDescending(v => v.LikeCount),
            "comments" => topVideos.OrderByDescending(v => v.CommentCount),
            "shares" => topVideos.OrderByDescending(v => v.ShareCount),
            "engagement" => topVideos.OrderByDescending(v => v.EngagementRate),
            _ => topVideos.OrderByDescending(v => v.ViewCount) // default to views
        };
    }

    /// <summary>
    /// Calculates the average view duration across all videos in hours
    /// </summary>
    /// <param name="service">The analytics service instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Average view duration in hours</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    public static async Task<double> CalculateAverageViewDurationHoursAsync(this AnalyticsService service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var allAnalytics = await service.GetTopPerformingVideosAsync(int.MaxValue, cancellationToken);

        if (!allAnalytics.Any())
        {
            return 0;
        }

        var totalSeconds = allAnalytics.Sum(a => a.AverageViewDuration);
        return totalSeconds / 3600.0; // Convert seconds to hours
    }

    /// <summary>
    /// Generates a performance comparison between two videos
    /// </summary>
    /// <param name="service">The analytics service instance</param>
    /// <param name="videoShortId1">First video ID</param>
    /// <param name="videoShortId2">Second video ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comparison string with performance metrics</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null</exception>
    public static async Task<string> CompareVideoPerformanceAsync(this AnalyticsService service, int videoShortId1, int videoShortId2, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var video1 = await service.GetVideoAnalyticsAsync(videoShortId1, cancellationToken);
        var video2 = await service.GetVideoAnalyticsAsync(videoShortId2, cancellationToken);

        if (video1 == null && video2 == null)
        {
            return "Both videos have no analytics data.";
        }

        if (video1 == null)
        {
            return $"Video {videoShortId2} has data, Video {videoShortId1} does not.";
        }

        if (video2 == null)
        {
            return $"Video {videoShortId1} has data, Video {videoShortId2} does not.";
        }

        var comparison = $"Video Performance Comparison:\n";
        comparison += $"Video {videoShortId1}: {video1.ViewCount:N0} views, {video1.LikeCount:N0} likes, {video1.EngagementRate:F2}% engagement\n";
        comparison += $"Video {videoShortId2}: {video2.ViewCount:N0} views, {video2.LikeCount:N0} likes, {video2.EngagementRate:F2}% engagement\n";

        var viewsDiff = video1.ViewCount - video2.ViewCount;
        var likesDiff = video1.LikeCount - video2.LikeCount;
        var engagementDiff = video1.EngagementRate - video2.EngagementRate;

        comparison += "Differences:\n";
        comparison += $"- Views: {viewsDiff:+#;-#;0} ({Math.Abs(viewsDiff):N0})\n";
        comparison += $"- Likes: {likesDiff:+#;-#;0} ({Math.Abs(likesDiff):N0})\n";
        comparison += $"- Engagement Rate: {engagementDiff:+#.##;-#.##;0}%";

        return comparison;
    }
}