// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Formatters;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for analytics and metrics
/// Provides video performance data, engagement metrics, and trend analysis
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly JsonResponseFormatter _responseFormatter;
    private readonly ICacheService _cacheService;

    public AnalyticsController(
        ILogger<AnalyticsController> logger,
        JsonResponseFormatter responseFormatter,
        ICacheService cacheService)
    {
        _logger = logger;
        _responseFormatter = responseFormatter;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Get analytics for a specific video
    /// </summary>
    [HttpGet("videos/{videoId}")]
    [ProducesResponseType(typeof(VideoAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideoAnalyticsAsync(Guid videoId)
    {
        try
        {
            // Try to get from cache first
            var cachedData = _cacheService.Get<VideoAnalyticsResponse>($"analytics:video:{videoId}");
            if (cachedData != null)
            {
                return Ok(cachedData);
            }

            // Simulate fetching from database
            var response = new VideoAnalyticsResponse
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

            // Cache for 5 minutes
            _cacheService.Set($"analytics:video:{videoId}", response, TimeSpan.FromMinutes(5));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for video: {VideoId}", videoId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get analytics summary for all videos
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsSummaryAsync([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest("Days must be between 1 and 365");

            var cachedData = _cacheService.Get<AnalyticsSummaryResponse>($"analytics:summary:{days}days");
            if (cachedData != null)
            {
                return Ok(cachedData);
            }

            var response = new AnalyticsSummaryResponse
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

            _cacheService.Set($"analytics:summary:{days}days", response, TimeSpan.FromHours(1));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics summary");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Export analytics data as CSV
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAnalyticsAsync([FromQuery] string format = "csv")
    {
        try
        {
            if (!new[] { "csv", "json" }.Contains(format.ToLowerInvariant()))
                return BadRequest("Format must be 'csv' or 'json'");

            // Simulate analytics data
            var data = new[]
            {
                new { VideoId = "video1", Title = "Shorts 1", Views = 1500, Likes = 120 },
                new { VideoId = "video2", Title = "Shorts 2", Views = 2500, Likes = 200 },
                new { VideoId = "video3", Title = "Shorts 3", Views = 1200, Likes = 95 }
            };

            if (format.ToLowerInvariant() == "csv")
            {
                var csvFormatter = new CsvExportFormatter();
                var csvData = csvFormatter.ExportToCsvWithAllProperties(data);
                return File(csvData, "text/csv", $"analytics-{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            else
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"analytics-{DateTime.UtcNow:yyyyMMdd}.json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get engagement trends over time
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(EngagementTrendResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEngagementTrendsAsync([FromQuery] int days = 30)
    {
        try
        {
            var trendPoints = Enumerable.Range(0, days)
                .Select(i => new TrendPoint
                {
                    Date = DateTime.UtcNow.AddDays(-days + i).Date,
                    Views = 1000 + Random.Shared.Next(500),
                    Engagement = Random.Shared.Next(100)
                })
                .ToArray();

            var response = new EngagementTrendResponse
            {
                TrendPoints = trendPoints,
                DateRangeStartUtc = DateTime.UtcNow.AddDays(-days),
                DateRangeEndUtc = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving engagement trends");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

#region Response Models

public class VideoAnalyticsResponse
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int WatchTimeMinutes { get; set; }
    public double EngagementRate { get; set; }
    public int AverageWatchDurationSeconds { get; set; }
    public double CtrPercent { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class AnalyticsSummaryResponse
{
    public int TotalVideos { get; set; }
    public int TotalViews { get; set; }
    public int TotalEngagement { get; set; }
    public int AverageViewsPerVideo { get; set; }
    public double AverageEngagementRate { get; set; }
    public VideoSummary? TopPerformingVideo { get; set; }
    public DateTime DateRangeStartUtc { get; set; }
    public DateTime DateRangeEndUtc { get; set; }
}

public class VideoSummary
{
    public Guid VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public double EngagementRate { get; set; }
}

public class EngagementTrendResponse
{
    public TrendPoint[]? TrendPoints { get; set; }
    public DateTime DateRangeStartUtc { get; set; }
    public DateTime DateRangeEndUtc { get; set; }
}

public class TrendPoint
{
    public DateTime Date { get; set; }
    public int Views { get; set; }
    public int Engagement { get; set; }
}

#endregion
