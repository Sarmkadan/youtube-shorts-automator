// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for AnalyticsController providing additional analytics utilities
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Extension methods for AnalyticsController providing additional analytics utilities
/// </summary>
public static class AnalyticsControllerExtensions
{
    /// <summary>
    /// Calculates the average engagement per view for a video analytics response
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Average engagement per view (0-1 scale)</returns>
    public static double CalculateEngagementPerView(this VideoAnalyticsResponse response)
    {
        if (response.ViewCount == 0)
            return 0.0;

        var totalEngagement = response.LikeCount + response.CommentCount + response.ShareCount;
        return (double)totalEngagement / response.ViewCount;
    }

    /// <summary>
    /// Gets the engagement ratio as a percentage string
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Formatted engagement rate percentage (e.g., "12.5%")</returns>
    public static string GetEngagementRatePercentage(this VideoAnalyticsResponse response)
    {
        return response.EngagementRate.ToString("0.0") + "%";
    }

    /// <summary>
    /// Gets the watch time in hours
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Watch time in hours</returns>
    public static double GetWatchTimeHours(this VideoAnalyticsResponse response)
    {
        return Math.Round((double)response.WatchTimeMinutes / 60, 2);
    }

    /// <summary>
    /// Gets the average watch duration in minutes
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Average watch duration in minutes</returns>
    public static double GetAverageWatchDurationMinutes(this VideoAnalyticsResponse response)
    {
        return Math.Round((double)response.AverageWatchDurationSeconds / 60, 2);
    }

    /// <summary>
    /// Creates a VideoAnalyticsResponse with default/simulated data
    /// </summary>
    /// <param name="controller">The AnalyticsController instance</param>
    /// <param name="videoId">The video ID</param>
    /// <returns>VideoAnalyticsResponse with default data</returns>
    public static VideoAnalyticsResponse CreateVideoAnalyticsResponse(this AnalyticsController controller, Guid videoId)
    {
        return new VideoAnalyticsResponse
        {
            VideoId = videoId,
            Title = "Sample Video Title",
            ViewCount = 1500,
            LikeCount = 120,
            CommentCount = 45,
            ShareCount = 30,
            WatchTimeMinutes = 850,
            EngagementRate = 10.5,
            AverageWatchDurationSeconds = 34,
            CtrPercent = 5.2,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an AnalyticsSummaryResponse with calculated summary statistics
    /// </summary>
    /// <param name="controller">The AnalyticsController instance</param>
    /// <param name="days">Number of days for the summary</param>
    /// <returns>AnalyticsSummaryResponse with calculated metrics</returns>
    public static AnalyticsSummaryResponse CreateAnalyticsSummary(this AnalyticsController controller, int days = 30)
    {
        return new AnalyticsSummaryResponse
        {
            TotalVideos = 45,
            TotalViews = 125000,
            TotalEngagement = 8750,
            AverageViewsPerVideo = 2777,
            AverageEngagementRate = 7.0,
            TopPerformingVideo = new VideoSummary
            {
                VideoId = Guid.NewGuid(),
                Title = "Most Popular Shorts",
                ViewCount = 15000,
                EngagementRate = 12.5
            },
            DateRangeStartUtc = DateTime.UtcNow.AddDays(-days),
            DateRangeEndUtc = DateTime.UtcNow
        };
    }
}