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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null</exception>
    public static double CalculateEngagementPerView(this VideoAnalyticsResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null</exception>
    public static string GetEngagementRatePercentage(this VideoAnalyticsResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.EngagementRate.ToString("0.0", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Gets the watch time in hours
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Watch time in hours</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null</exception>
    public static double GetWatchTimeHours(this VideoAnalyticsResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return Math.Round((double)response.WatchTimeMinutes / 60, 2);
    }

    /// <summary>
    /// Gets the average watch duration in minutes
    /// </summary>
    /// <param name="response">The VideoAnalyticsResponse instance</param>
    /// <returns>Average watch duration in minutes</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null</exception>
    public static double GetAverageWatchDurationMinutes(this VideoAnalyticsResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return Math.Round((double)response.AverageWatchDurationSeconds / 60, 2);
    }

    /// <summary>
    /// Creates a VideoAnalyticsResponse with default/simulated data
    /// </summary>
    /// <param name="controller">The AnalyticsController instance</param>
    /// <param name="videoId">The video ID</param>
    /// <returns>VideoAnalyticsResponse with default data</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    public static VideoAnalyticsResponse CreateVideoAnalyticsResponse(this AnalyticsController controller, Guid videoId)
    {
        ArgumentNullException.ThrowIfNull(controller);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is less than 1 or greater than 365</exception>
    public static AnalyticsSummaryResponse CreateAnalyticsSummary(this AnalyticsController controller, int days = 30)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfLessThan(days, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(days, 365);

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