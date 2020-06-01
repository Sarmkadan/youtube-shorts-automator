using System.Globalization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="AnalyticsMetric"/>.
/// </summary>
public static class AnalyticsMetricExtensions
{
    /// <summary>
    /// Calculates the net subscriber gain for the metric.
    /// </summary>
    /// <param name="metric">The analytics metric.</param>
    /// <returns>The net subscriber gain.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    public static long GetNetSubscriberGain(this AnalyticsMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);
        return metric.SubscriberGainedCount - metric.SubscriberLostCount;
    }

    /// <summary>
    /// Calculates the total number of interactions (likes, comments, and shares).
    /// </summary>
    /// <param name="metric">The analytics metric.</param>
    /// <returns>The sum of likes, comments, and shares.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    public static long GetTotalInteractionCount(this AnalyticsMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);
        return metric.LikeCount + metric.CommentCount + metric.ShareCount;
    }

    /// <summary>
    /// Generates a human-readable summary string of the metric.
    /// </summary>
    /// <param name="metric">The analytics metric.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    public static string ToSummaryString(this AnalyticsMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);
        
        return string.Create(CultureInfo.InvariantCulture, 
            $"Metric for Video {metric.VideoId}: Views={metric.ViewCount}, " +
            $"EngagementRate={metric.EngagementRatePercent:F2}%, " +
            $"NetSubs={metric.GetNetSubscriberGain()}, " +
            $"HighPerforming={metric.IsHighPerforming()}");
    }
}
