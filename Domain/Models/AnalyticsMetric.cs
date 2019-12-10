// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class AnalyticsMetric
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public long ShareCount { get; set; }
    public long SubscriberGainedCount { get; set; }
    public long SubscriberLostCount { get; set; }
    public double AverageViewDurationSeconds { get; set; }
    public double EngagementRatePercent { get; set; }
    public double ClickThroughRatePercent { get; set; }
    public MetricsPeriod Period { get; set; } = MetricsPeriod.Daily;
    public DateTime CollectedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? TrafficSource { get; set; }
    public string? DeviceType { get; set; }
    public List<DemographicMetric> Demographics { get; set; } = new();

    /// <summary>
    /// Calculates the engagement score (0-100)
    /// </summary>
    public double CalculateEngagementScore()
    {
        var totalInteractions = LikeCount + CommentCount + ShareCount;
        if (ViewCount == 0)
            return 0;

        var engagementRate = (totalInteractions / (double)ViewCount) * 100;
        return Math.Min(engagementRate * 10, 100);
    }

    /// <summary>
    /// Determines if the metric shows strong performance
    /// </summary>
    public bool IsHighPerforming(AnalyticsMetric? averageMetric = null)
    {
        if (ViewCount == 0)
            return false;

        var engagementScore = CalculateEngagementScore();
        var isHighEngagement = engagementScore >= 5.0;
        var isGoodRetention = AverageViewDurationSeconds >= 30;

        return isHighEngagement && isGoodRetention;
    }

    /// <summary>
    /// Compares with another metric to determine trend
    /// </summary>
    public MetricsTrend CompareTo(AnalyticsMetric? previous)
    {
        if (previous == null)
            return MetricsTrend.Neutral;

        var viewChange = ((double)ViewCount - previous.ViewCount) / Math.Max(previous.ViewCount, 1);

        if (viewChange > 0.1)
            return MetricsTrend.Improving;
        if (viewChange < -0.1)
            return MetricsTrend.Declining;

        return MetricsTrend.Neutral;
    }

    /// <summary>
    /// Gets the primary demographic
    /// </summary>
    public DemographicMetric? GetPrimaryDemographic()
    {
        return Demographics.OrderByDescending(d => d.ViewCount).FirstOrDefault();
    }

    /// <summary>
    /// Calculates retention rate
    /// </summary>
    public double CalculateRetentionRate()
    {
        if (ViewCount == 0)
            return 0;

        var avgDurationPercent = (AverageViewDurationSeconds / 60.0) * 100; // Assuming 60s shorts
        return Math.Min(avgDurationPercent, 100);
    }

    /// <summary>
    /// Adds demographic data
    /// </summary>
    public void AddDemographic(string ageGroup, string gender, long viewCount)
    {
        Demographics.Add(new DemographicMetric
        {
            Id = Guid.NewGuid(),
            MetricId = Id,
            AgeGroup = ageGroup,
            Gender = gender,
            ViewCount = viewCount,
            RecordedAt = DateTime.UtcNow
        });
    }
}

public enum MetricsPeriod
{
    Hourly = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Cumulative = 4
}

public enum MetricsTrend
{
    Improving = 1,
    Neutral = 0,
    Declining = -1
}

public class DemographicMetric
{
    public Guid Id { get; set; }
    public Guid MetricId { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public long ViewCount { get; set; }
    public DateTime RecordedAt { get; set; }
}
