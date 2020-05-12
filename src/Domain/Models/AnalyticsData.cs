// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents analytics data for a video short.
/// </summary>
public class AnalyticsData
{
    /// <summary>
    /// Gets the unique identifier for the analytics data.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the identifier of the video short associated with the analytics data.
    /// </summary>
    public int VideoShortId { get; set; }

    /// <summary>
    /// Gets the total number of views for the video short.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Gets the total number of likes for the video short.
    /// </summary>
    public long LikeCount { get; set; }

    /// <summary>
    /// Gets the total number of comments for the video short.
    /// </summary>
    public long CommentCount { get; set; }

    /// <summary>
    /// Gets the total number of shares for the video short.
    /// </summary>
    public long ShareCount { get; set; }

    /// <summary>
    /// Gets the average view duration for the video short in seconds.
    /// </summary>
    public double AverageViewDuration { get; set; }

    /// <summary>
    /// Gets the engagement rate for the video short as a percentage.
    /// </summary>
    public double EngagementRate { get; set; }

    /// <summary>
    /// Gets the click-through rate for the video short as a percentage.
    /// </summary>
    public double ClickThroughRate { get; set; }

    /// <summary>
    /// Gets the number of subscribers gained for the video short.
    /// </summary>
    public int SubscribersGained { get; set; }

    /// <summary>
    /// Gets the number of subscribers lost for the video short.
    /// </summary>
    public int SubscribersLost { get; set; }

    /// <summary>
    /// Gets the audience retention percentage for the video short.
    /// </summary>
    public double AudienceRetentionPercentage { get; set; }

    /// <summary>
    /// Gets the number of traffic sources for the video short.
    /// </summary>
    public int TrafficSources { get; set; }

    /// <summary>
    /// Gets the total number of impressions for the video short.
    /// </summary>
    public int ImpressionCount { get; set; }

    /// <summary>
    /// Gets the date and time when the analytics data was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets the video short associated with the analytics data.
    /// </summary>
    public VideoShort? VideoShort { get; set; }

    /// <summary>
    /// Updates the analytics data from a YouTube API response.
    /// </summary>
    /// <param name="views">The total number of views for the video short.</param>
    /// <param name="likes">The total number of likes for the video short.</param>
    /// <param name="comments">The total number of comments for the video short.</param>
    /// <param name="shares">The total number of shares for the video short.</param>
    /// <param name="avgDuration">The average view duration for the video short in seconds.</param>
    public void UpdateFromAPI(long views, long likes, long comments, long shares, double avgDuration)
    {
        // Updates analytics data from YouTube API response
        ViewCount = views;
        LikeCount = likes;
        CommentCount = comments;
        ShareCount = shares;
        AverageViewDuration = avgDuration;
        UpdatedAt = DateTime.UtcNow;
        RecalculateEngagementMetrics();
    }

    /// <summary>
    /// Recalculates the engagement metrics for the video short.
    /// </summary>
    public void RecalculateEngagementMetrics()
    {
        // Recalculates engagement and retention metrics
        if (ViewCount == 0)
        {
            EngagementRate = 0;
            return;
        }

        long totalEngagements = LikeCount + CommentCount + ShareCount;
        EngagementRate = (double)totalEngagements / ViewCount * 100;
    }

    /// <summary>
    /// Updates the audience retention and impression data for the video short.
    /// </summary>
    /// <param name="retentionPercentage">The audience retention percentage for the video short.</param>
    /// <param name="impressions">The total number of impressions for the video short.</param>
    public void UpdateRetentionData(double retentionPercentage, int impressions)
    {
        // Updates audience retention and impression data
        AudienceRetentionPercentage = Math.Min(retentionPercentage, 100);
        ImpressionCount = impressions;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the subscriber change metrics for the video short.
    /// </summary>
    /// <param name="gained">The number of subscribers gained for the video short.</param>
    /// <param name="lost">The number of subscribers lost for the video short.</param>
    public void UpdateSubscriberMetrics(int gained, int lost)
    {
        // Updates subscriber change metrics
        SubscribersGained = gained;
        SubscribersLost = lost;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the analytics data has meaningful values.
    /// </summary>
    /// <returns>true if the analytics data has meaningful values; otherwise, false.</returns>
    public bool HasValidData()
    {
        // Checks if analytics data has meaningful values
        return ViewCount > 0 || LikeCount > 0 || CommentCount > 0 || ShareCount > 0;
    }

    /// <summary>
    /// Calculates the net change in subscribers for the video short.
    /// </summary>
    /// <returns>The net change in subscribers for the video short.</returns>
    public double GetNetSubscriberChange()
    {
        // Calculates net change in subscribers
        return SubscribersGained - SubscribersLost;
    }

    /// <summary>
    /// Returns the performance classification for the video short based on the engagement rate.
    /// </summary>
    /// <returns>The performance classification for the video short.</returns>
    public string GetPerformanceLevel()
    {
        // Returns performance classification based on engagement rate
        return EngagementRate switch
        {
            >= 10 => "Excellent",
            >= 5 => "Good",
            >= 2 => "Average",
            _ => "Low"
        };
    }
}
