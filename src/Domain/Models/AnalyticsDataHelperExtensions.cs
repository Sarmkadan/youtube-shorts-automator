// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides side-effect-free calculations for <see cref="AnalyticsData"/>.
/// </summary>
public static class AnalyticsDataHelperExtensions
{
    /// <summary>
    /// Calculates the total number of likes, comments, and shares.
    /// </summary>
    /// <param name="analyticsData">The analytics data to evaluate.</param>
    /// <returns>The total number of engagements.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="analyticsData"/> is <see langword="null"/>.
    /// </exception>
    public static long CalculateTotalEngagements(this AnalyticsData analyticsData)
    {
        ArgumentNullException.ThrowIfNull(analyticsData);

        return analyticsData.LikeCount + analyticsData.CommentCount + analyticsData.ShareCount;
    }

    /// <summary>
    /// Calculates the engagement rate as a percentage of views.
    /// </summary>
    /// <param name="analyticsData">The analytics data to evaluate.</param>
    /// <returns>
    /// The percentage of views that resulted in a like, comment, or share; or
    /// <c>0</c> when the view count is not positive.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="analyticsData"/> is <see langword="null"/>.
    /// </exception>
    public static double CalculateEngagementRate(this AnalyticsData analyticsData)
    {
        ArgumentNullException.ThrowIfNull(analyticsData);

        if (analyticsData.ViewCount <= 0)
        {
            return 0;
        }

        return (double)analyticsData.CalculateTotalEngagements() / analyticsData.ViewCount * 100;
    }

    /// <summary>
    /// Calculates the net subscriber change from subscribers gained and lost.
    /// </summary>
    /// <param name="analyticsData">The analytics data to evaluate.</param>
    /// <returns>The number of subscribers gained minus the number lost.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="analyticsData"/> is <see langword="null"/>.
    /// </exception>
    public static int CalculateNetSubscriberChange(this AnalyticsData analyticsData)
    {
        ArgumentNullException.ThrowIfNull(analyticsData);

        return analyticsData.SubscribersGained - analyticsData.SubscribersLost;
    }
}
