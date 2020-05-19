using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="AnalyticsData"/> to enhance analytics operations and calculations.
/// </summary>
public static class AnalyticsDataExtensions
{
    /// <summary>
    /// Calculates the total engagement count (likes + comments + shares) for the video short.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>The total engagement count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static long GetTotalEngagementCount(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);
        return analytics.LikeCount + analytics.CommentCount + analytics.ShareCount;
    }

    /// <summary>
    /// Calculates the view-to-engagement ratio (engagements per 100 views) for the video short.
    /// This provides a normalized metric for comparing engagement across videos of different sizes.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>The view-to-engagement ratio per 100 views, or 0 if view count is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static double GetViewToEngagementRatio(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        if (analytics.ViewCount == 0)
        {
            return 0;
        }

        long totalEngagements = analytics.GetTotalEngagementCount();
        return (double)totalEngagements / analytics.ViewCount * 100;
    }

    /// <summary>
    /// Gets the top traffic sources as a formatted string for display purposes.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>A formatted string representing the traffic sources, or "Unknown" if TrafficSources is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static string GetTrafficSourcesDisplay(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        return analytics.TrafficSources switch
        {
            0 => "Unknown",
            1 => "Search",
            2 => "Suggested Videos",
            3 => "Channel Page",
            4 => "Hashtags",
            5 => "External",
            _ => $"Multiple ({analytics.TrafficSources} sources)"
        };
    }

    /// <summary>
    /// Calculates the average view duration in minutes and seconds for display purposes.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>A formatted string representing the average view duration as "MM:SS".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static string GetAverageViewDurationFormatted(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        int totalSeconds = (int)Math.Round(analytics.AverageViewDuration);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// Determines if the video short has reached the minimum threshold for meaningful analytics.
    /// A video is considered "meaningful" if it has at least 100 views and at least 1 engagement.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>true if the video has meaningful analytics; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static bool HasMeaningfulAnalytics(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        return analytics.ViewCount >= 100 && analytics.GetTotalEngagementCount() >= 1;
    }

    /// <summary>
    /// Calculates the impression-to-view ratio (views per 1000 impressions) for the video short.
    /// This helps measure how effectively impressions are converting to views.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>The impression-to-view ratio per 1000 impressions, or 0 if impression count is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static double GetImpressionToViewRatio(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        if (analytics.ImpressionCount == 0)
        {
            return 0;
        }

        return (double)analytics.ViewCount / analytics.ImpressionCount * 1000;
    }

    /// <summary>
    /// Gets the engagement quality score (0-100) based on multiple metrics.
    /// This composite score considers engagement rate, audience retention, and click-through rate.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <returns>The engagement quality score (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static double GetEngagementQualityScore(this AnalyticsData analytics)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        // Normalize metrics to 0-100 scale and combine with weights
        double engagementScore = Math.Min(analytics.EngagementRate, 100);
        double retentionScore = analytics.AudienceRetentionPercentage;
        double ctrScore = analytics.ClickThroughRate;

        // Weighted average: engagement (50%), retention (30%), CTR (20%)
        return engagementScore * 0.5 + retentionScore * 0.3 + ctrScore * 0.2;
    }

    /// <summary>
    /// Gets the performance trend indicator based on recent changes in key metrics.
    /// </summary>
    /// <param name="analytics">The analytics data instance.</param>
    /// <param name="previousViewCount">The previous view count for comparison.</param>
    /// <param name="previousEngagementRate">The previous engagement rate for comparison.</param>
    /// <returns>A performance trend indicator: "Improving", "Declining", "Stable", or "New".</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analytics"/> is null.</exception>
    public static string GetPerformanceTrend(this AnalyticsData analytics, long previousViewCount, double previousEngagementRate)
    {
        ArgumentNullException.ThrowIfNull(analytics);

        if (previousViewCount == 0 && previousEngagementRate == 0)
        {
            return "New";
        }

        bool viewsImproving = analytics.ViewCount > previousViewCount * 1.1; // 10% improvement
        bool engagementImproving = analytics.EngagementRate > previousEngagementRate * 1.05; // 5% improvement

        if (viewsImproving && engagementImproving)
        {
            return "Improving";
        }

        if (analytics.ViewCount < previousViewCount * 0.95 || analytics.EngagementRate < previousEngagementRate * 0.95)
        {
            return "Declining";
        }

        return "Stable";
    }

    /// <summary>
    /// Gets the top performing videos in a collection based on engagement metrics.
    /// </summary>
    /// <param name="analyticsCollection">The collection of analytics data.</param>
    /// <param name="topN">The number of top videos to return.</param>
    /// <returns>A read-only list of the top performing videos sorted by engagement quality score.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="analyticsCollection"/> is null.</exception>
    public static IReadOnlyList<AnalyticsData> GetTopPerformingVideos(this IEnumerable<AnalyticsData> analyticsCollection, int topN = 5)
    {
        ArgumentNullException.ThrowIfNull(analyticsCollection);

        if (topN <= 0)
        {
            return Array.Empty<AnalyticsData>();
        }

        return analyticsCollection
            .Where(a => a.HasMeaningfulAnalytics())
            .OrderByDescending(a => a.GetEngagementQualityScore())
            .Take(topN)
            .ToList()
            .AsReadOnly();
    }
}