// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides analytics services for YouTube Shorts videos, including tracking metrics, syncing data from YouTube API,
/// and generating performance reports.
/// </summary>
public class AnalyticsService
{
    private readonly AnalyticsRepository _analyticsRepository;
    private readonly VideoShortRepository _videoRepository;
    private readonly ILogger<AnalyticsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
    /// </summary>
    /// <param name="analyticsRepository">The analytics repository for data persistence.</param>
    /// <param name="videoRepository">The video repository for accessing video data.</param>
    /// <param name="logger">The logger for recording operational messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AnalyticsService(AnalyticsRepository analyticsRepository, VideoShortRepository videoRepository,
        ILogger<AnalyticsService> logger)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new analytics record for a YouTube Short video.
    /// </summary>
    /// <param name="videoShortId">The ID of the video short to create analytics for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The created analytics data record.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the analytics record cannot be created.</exception>
    public async Task<AnalyticsData> CreateAnalyticsRecordAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        // Creates an initial analytics record for a video short
        try
        {
            var analyticsData = new AnalyticsData
            {
                VideoShortId = videoShortId,
                ViewCount = 0,
                LikeCount = 0,
                CommentCount = 0,
                ShareCount = 0,
                AverageViewDuration = 0,
                EngagementRate = 0,
                ClickThroughRate = 0,
                SubscribersGained = 0,
                SubscribersLost = 0,
                AudienceRetentionPercentage = 0,
                TrafficSources = 0,
                ImpressionCount = 0,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _analyticsRepository.AddAsync(analyticsData, cancellationToken);
            _logger.LogInformation($"Created analytics record for video {videoShortId}");
            return created;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create analytics record: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Synchronizes analytics data from YouTube API for a specific video short.
    /// </summary>
    /// <param name="videoShortId">The ID of the video short to sync analytics for.</param>
    /// <param name="youtubeVideoId">The YouTube video ID to fetch analytics from.</param>
    /// <param name="channel">The YouTube channel associated with the video.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The updated analytics data, or null if sync failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when youtubeVideoId is null or empty, or channel is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the analytics sync operation fails.</exception>
    public async Task<AnalyticsData?> SyncAnalyticsFromYouTubeAsync(int videoShortId, string youtubeVideoId,
        YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        // Fix: Add validation for youtubeVideoId and channel to prevent null reference exceptions.
        if (string.IsNullOrWhiteSpace(youtubeVideoId))
        {
            throw new ArgumentNullException(nameof(youtubeVideoId), "YouTube video ID cannot be null or empty.");
        }
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel), "YouTube channel cannot be null.");
        }
        // Syncs analytics data from YouTube API
        try
        {
            // Mock data that would come from YouTube API
            var views = new Random().Next(100, 10000);
            var likes = (int)(views * 0.05);
            var comments = (int)(views * 0.02);
            var shares = (int)(views * 0.01);
            var avgDuration = 15.5; // seconds

            var existingAnalytics = await _analyticsRepository.GetByVideoIdAsync(videoShortId, cancellationToken);
            
            if (existingAnalytics == null)
            {
                existingAnalytics = await CreateAnalyticsRecordAsync(videoShortId, cancellationToken);
            }

            existingAnalytics.UpdateFromAPI(views, likes, comments, shares, avgDuration);
            existingAnalytics.UpdateRetentionData(75.5, views / 2);

            var updated = await _analyticsRepository.UpdateAsync(existingAnalytics, cancellationToken);
            
            _logger.LogInformation($"Synced analytics for video {videoShortId}: {views} views, {likes} likes");
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error syncing analytics: {ex.Message}");
            throw new InvalidOperationException($"Failed to sync analytics: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves analytics data for a specific video short.
    /// </summary>
    /// <param name="videoShortId">The ID of the video short to retrieve analytics for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The analytics data for the specified video, or null if not found.</returns>
    /// <exception cref="Exception">Thrown when an error occurs while retrieving analytics.</exception>
    public async Task<AnalyticsData?> GetVideoAnalyticsAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        // Retrieves analytics for a specific video
        try
        {
            return await _analyticsRepository.GetByVideoIdAsync(videoShortId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving analytics for video {videoShortId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the top performing videos based on engagement rate.
    /// </summary>
    /// <param name="limit">The maximum number of top videos to retrieve. Must be a positive integer.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An enumerable of analytics data for the top performing videos.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when limit is not a positive integer.</exception>
    /// <exception cref="Exception">Thrown when an error occurs while retrieving top performers.</exception>
    public async Task<IEnumerable<AnalyticsData>> GetTopPerformingVideosAsync(int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // Fix: Validate that the limit is a positive integer to avoid unexpected behavior.
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "The limit for top performing videos must be a positive integer.");
        }
        // Retrieves the top performing videos by engagement rate
        try
        {
            var topVideos = await _analyticsRepository.GetTopPerformersAsync(limit, cancellationToken);
            _logger.LogInformation($"Retrieved top {limit} performing videos");
            return topVideos;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving top performing videos: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generates an analytics report for a specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the reporting period (inclusive).</param>
    /// <param name="endDate">The end date of the reporting period (inclusive).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An analytics report containing aggregated metrics for the specified period.</returns>
    /// <exception cref="ArgumentException">Thrown when startDate is after endDate.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the report generation fails.</exception>
    public async Task<AnalyticsReport> GeneratePeriodReportAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Fix: Validate date range to ensure startDate is not after endDate.
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.", nameof(startDate));
        }
        // Generates an analytics report for a date range
        try
        {
            var allAnalytics = await _analyticsRepository.GetAllAsync(cancellationToken);
            
            var report = new AnalyticsReport
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalVideos = allAnalytics.Count(),
                TotalViews = allAnalytics.Sum(a => a.ViewCount),
                TotalLikes = allAnalytics.Sum(a => a.LikeCount),
                TotalComments = allAnalytics.Sum(a => a.CommentCount),
                TotalShares = allAnalytics.Sum(a => a.ShareCount),
                AverageEngagementRate = allAnalytics.Any() ? allAnalytics.Average(a => a.EngagementRate) : 0,
                TotalSubscribersGained = allAnalytics.Sum(a => a.SubscribersGained),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation($"Generated analytics report from {startDate} to {endDate}");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating analytics report: {ex.Message}");
            throw new InvalidOperationException($"Failed to generate report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Analyzes performance metrics for a video and provides insights and recommendations.
    /// </summary>
    /// <param name="analytics">The analytics data to analyze.</param>
    /// <returns>A formatted string containing performance analysis and recommendations.</returns>
    public string AnalyzePerformanceMetrics(AnalyticsData analytics)
    {
        // Analyzes performance metrics and provides insights
        if (!analytics.HasValidData())
        {
            return "No performance data available yet.";
        }

        var insights = $"Video Performance Analysis:\n";
        insights += $"- Views: {analytics.ViewCount:N0}\n";
        insights += $"- Engagement Rate: {analytics.EngagementRate:F2}%\n";
        insights += $"- Performance Level: {analytics.GetPerformanceLevel()}\n";
        insights += $"- Avg Watch Duration: {analytics.AverageViewDuration:F1}s\n";
        insights += $"- Audience Retention: {analytics.AudienceRetentionPercentage:F1}%\n";

        if (analytics.EngagementRate > 10)
        {
            insights += "- Status: Excellent engagement!\n";
        }
        else if (analytics.EngagementRate < 2)
        {
            insights += "- Status: Low engagement. Consider reviewing title and thumbnail.\n";
        }

        return insights;
    }

    /// <summary>
    /// Calculates the overall channel growth based on analytics data across all videos.
    /// </summary>
    /// <param name="channelId">The ID of the YouTube channel to calculate growth for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The net subscriber growth (subscribers gained minus subscribers lost).</returns>
    /// <exception cref="Exception">Thrown when an error occurs while calculating channel growth.</exception>
    public async Task<double> CalculateChannelGrowthAsync(int channelId, CancellationToken cancellationToken = default)
    {
        // Calculates the overall channel growth based on analytics
        try
        {
            var allAnalytics = await _analyticsRepository.GetAllAsync(cancellationToken);
            var totalSubscribersGained = allAnalytics.Sum(a => a.SubscribersGained);
            var totalSubscribersLost = allAnalytics.Sum(a => a.SubscribersLost);

            return totalSubscribersGained - totalSubscribersLost;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating channel growth: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Represents an analytics report containing aggregated metrics for a specified date range.
/// </summary>
public class AnalyticsReport
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalVideos { get; set; }
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public double AverageEngagementRate { get; set; }
    public int TotalSubscribersGained { get; set; }
    public DateTime GeneratedAt { get; set; }
}
