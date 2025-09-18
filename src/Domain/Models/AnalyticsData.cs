// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

public class AnalyticsData
{
    public int Id { get; set; }
    public int VideoShortId { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public long ShareCount { get; set; }
    public double AverageViewDuration { get; set; }
    public double EngagementRate { get; set; }
    public double ClickThroughRate { get; set; }
    public int SubscribersGained { get; set; }
    public int SubscribersLost { get; set; }
    public double AudienceRetentionPercentage { get; set; }
    public int TrafficSources { get; set; }
    public int ImpressionCount { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public VideoShort? VideoShort { get; set; }

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

    public void UpdateRetentionData(double retentionPercentage, int impressions)
    {
        // Updates audience retention and impression data
        AudienceRetentionPercentage = Math.Min(retentionPercentage, 100);
        ImpressionCount = impressions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSubscriberMetrics(int gained, int lost)
    {
        // Updates subscriber change metrics
        SubscribersGained = gained;
        SubscribersLost = lost;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValidData()
    {
        // Checks if analytics data has meaningful values
        return ViewCount > 0 || LikeCount > 0 || CommentCount > 0 || ShareCount > 0;
    }

    public double GetNetSubscriberChange()
    {
        // Calculates net change in subscribers
        return SubscribersGained - SubscribersLost;
    }

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
