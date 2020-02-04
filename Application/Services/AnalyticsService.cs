// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Handles video analytics collection, aggregation, and reporting
/// </summary>
public class AnalyticsService
{
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IVideoRepository _videoRepository;

    public AnalyticsService(
        ILogger<AnalyticsService> logger,
        IAnalyticsRepository analyticsRepository,
        IVideoRepository videoRepository)
    {
        _logger = logger;
        _analyticsRepository = analyticsRepository;
        _videoRepository = videoRepository;
    }

    /// <summary>
    /// Records analytics metrics for a video
    /// </summary>
    public async Task<AnalyticsMetric> RecordMetricsAsync(
        Guid videoId,
        long viewCount,
        long likeCount,
        long commentCount,
        long shareCount)
    {
        _logger.LogInformation($"Recording analytics for video {videoId}");

        var video = await _videoRepository.GetByIdAsync(videoId)
            ?? throw new ResourceNotFoundException("Video not found", videoId, "Video");

        var metric = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            ViewCount = viewCount,
            LikeCount = likeCount,
            CommentCount = commentCount,
            ShareCount = shareCount,
            Period = MetricsPeriod.Daily,
            CollectedAt = DateTime.UtcNow
        };

        video.Metrics.Add(metric);

        await _analyticsRepository.AddAsync(metric);
        await _videoRepository.UpdateAsync(video);

        _logger.LogInformation($"Metrics recorded: Views={viewCount}, Likes={likeCount}, Comments={commentCount}");

        return metric;
    }

    /// <summary>
    /// Updates existing analytics metrics
    /// </summary>
    public async Task<AnalyticsMetric> UpdateMetricsAsync(
        Guid metricsId,
        long viewCount,
        long likeCount,
        long commentCount,
        long shareCount,
        double avgDurationSeconds = 0)
    {
        _logger.LogInformation($"Updating metrics {metricsId}");

        var metric = await _analyticsRepository.GetByIdAsync(metricsId)
            ?? throw new ResourceNotFoundException("Metric not found", metricsId, "AnalyticsMetric");

        metric.ViewCount = viewCount;
        metric.LikeCount = likeCount;
        metric.CommentCount = commentCount;
        metric.ShareCount = shareCount;
        metric.AverageViewDurationSeconds = avgDurationSeconds;
        metric.UpdatedAt = DateTime.UtcNow;

        // Calculate engagement rate
        var totalInteractions = likeCount + commentCount + shareCount;
        metric.EngagementRatePercent = viewCount > 0
            ? (totalInteractions / (double)viewCount) * 100
            : 0;

        await _analyticsRepository.UpdateAsync(metric);

        _logger.LogInformation($"Metrics updated successfully. Engagement: {metric.EngagementRatePercent:F2}%");

        return metric;
    }

    /// <summary>
    /// Gets analytics for a specific video
    /// </summary>
    public async Task<AnalyticsMetric?> GetVideoAnalyticsAsync(Guid videoId, MetricsPeriod period)
    {
        _logger.LogInformation($"Retrieving {period} analytics for video {videoId}");

        var metrics = await _analyticsRepository.GetByVideoIdAsync(videoId);
        return metrics.FirstOrDefault(m => m.Period == period);
    }

    /// <summary>
    /// Gets cumulative analytics for a video
    /// </summary>
    public async Task<AnalyticsMetric?> GetCumulativeAnalyticsAsync(Guid videoId)
    {
        _logger.LogInformation($"Retrieving cumulative analytics for video {videoId}");

        var metrics = await _analyticsRepository.GetByVideoIdAsync(videoId);
        var cumulative = metrics.FirstOrDefault(m => m.Period == MetricsPeriod.Cumulative);

        if (cumulative == null)
        {
            cumulative = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                VideoId = videoId,
                Period = MetricsPeriod.Cumulative,
                CollectedAt = DateTime.UtcNow
            };

            foreach (var metric in metrics)
            {
                cumulative.ViewCount += metric.ViewCount;
                cumulative.LikeCount += metric.LikeCount;
                cumulative.CommentCount += metric.CommentCount;
                cumulative.ShareCount += metric.ShareCount;
            }

            await _analyticsRepository.AddAsync(cumulative);
        }

        return cumulative;
    }

    /// <summary>
    /// Gets top performing videos for a user
    /// </summary>
    public async Task<List<Video>> GetTopPerformingVideosAsync(Guid userId, int limit = 10)
    {
        _logger.LogInformation($"Retrieving top {limit} performing videos for user {userId}");

        var videos = await _videoRepository.GetByUserIdAsync(userId);

        var topVideos = videos
            .Where(v => v.UploadResult != null && v.Metrics.Any())
            .OrderByDescending(v => v.Metrics.Sum(m => m.ViewCount))
            .Take(limit)
            .ToList();

        return topVideos;
    }

    /// <summary>
    /// Calculates average metrics across multiple videos
    /// </summary>
    public async Task<(long AvgViews, double AvgEngagement, double AvgRetention)> GetAverageMetricsAsync(Guid userId)
    {
        _logger.LogInformation($"Calculating average metrics for user {userId}");

        var videos = await _videoRepository.GetByUserIdAsync(userId);
        var allMetrics = videos.SelectMany(v => v.Metrics).ToList();

        if (!allMetrics.Any())
            return (0, 0, 0);

        var avgViews = (long)allMetrics.Average(m => m.ViewCount);
        var avgEngagement = allMetrics.Average(m => m.EngagementRatePercent);
        var avgRetention = allMetrics.Average(m => m.CalculateRetentionRate());

        return (avgViews, avgEngagement, avgRetention);
    }

    /// <summary>
    /// Identifies trending videos
    /// </summary>
    public async Task<List<Video>> GetTrendingVideosAsync(Guid userId, int daysBack = 7)
    {
        _logger.LogInformation($"Retrieving trending videos for user {userId} from last {daysBack} days");

        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
        var videos = await _videoRepository.GetByUserIdAsync(userId);

        var trendingVideos = videos
            .Where(v => v.UploadResult?.PublishedAt > cutoffDate && v.Metrics.Any())
            .Where(v => v.Metrics.LastOrDefault()?.CalculateEngagementScore() >= ApplicationConstants.Analytics.MinEngagementScoreThreshold)
            .OrderByDescending(v => v.Metrics.Last().CalculateEngagementScore())
            .ToList();

        return trendingVideos;
    }

    /// <summary>
    /// Generates analytics report for a user
    /// </summary>
    public async Task<AnalyticsReport> GenerateReportAsync(Guid userId, int daysBack = 30)
    {
        _logger.LogInformation($"Generating analytics report for user {userId}");

        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
        var videos = await _videoRepository.GetByUserIdAsync(userId);
        var recentVideos = videos.Where(v => v.CreatedAt > cutoffDate).ToList();

        var report = new AnalyticsReport
        {
            GeneratedAt = DateTime.UtcNow,
            UserId = userId,
            TotalVideos = recentVideos.Count,
            TotalViews = recentVideos.SelectMany(v => v.Metrics).Sum(m => m.ViewCount),
            TotalEngagement = recentVideos.SelectMany(v => v.Metrics).Sum(m => m.LikeCount + m.CommentCount + m.ShareCount),
            TopPerformingVideo = recentVideos.OrderByDescending(v => v.Metrics.Sum(m => m.ViewCount)).FirstOrDefault(),
            AverageEngagementRate = recentVideos.SelectMany(v => v.Metrics).Any()
                ? recentVideos.SelectMany(v => v.Metrics).Average(m => m.EngagementRatePercent)
                : 0
        };

        return report;
    }

    /// <summary>
    /// Adds demographic data to metrics
    /// </summary>
    public async Task AddDemographicDataAsync(Guid metricsId, string ageGroup, string gender, long viewCount)
    {
        _logger.LogInformation($"Adding demographic data to metrics {metricsId}");

        var metric = await _analyticsRepository.GetByIdAsync(metricsId)
            ?? throw new ResourceNotFoundException("Metric not found", metricsId, "AnalyticsMetric");

        metric.AddDemographic(ageGroup, gender, viewCount);
        await _analyticsRepository.UpdateAsync(metric);

        _logger.LogInformation($"Demographic data added: {ageGroup}, {gender}");
    }

    /// <summary>
    /// Checks if video is performing well compared to average
    /// </summary>
    public async Task<bool> IsVideoHighPerformingAsync(Guid videoId, Guid userId)
    {
        _logger.LogInformation($"Checking if video {videoId} is high performing");

        var (avgViews, _, _) = await GetAverageMetricsAsync(userId);
        var video = await _videoRepository.GetByIdAsync(videoId)
            ?? throw new ResourceNotFoundException("Video not found", videoId, "Video");

        var currentMetric = video.Metrics.LastOrDefault();
        if (currentMetric == null)
            return false;

        return currentMetric.ViewCount > (avgViews * 1.5);
    }
}

/// <summary>
/// Analytics report data model
/// </summary>
public class AnalyticsReport
{
    public DateTime GeneratedAt { get; set; }
    public Guid UserId { get; set; }
    public int TotalVideos { get; set; }
    public long TotalViews { get; set; }
    public long TotalEngagement { get; set; }
    public double AverageEngagementRate { get; set; }
    public Video? TopPerformingVideo { get; set; }
}
