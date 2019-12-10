// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Application.Services;

namespace YouTubeShortsAutomator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(AnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets top performing videos for a user
    /// </summary>
    [HttpGet("user/{userId}/top-videos")]
    public async Task<IActionResult> GetTopPerformingVideos(Guid userId, [FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation($"Fetching top {limit} videos for user {userId}");

            var topVideos = await _analyticsService.GetTopPerformingVideosAsync(userId, limit);

            return Ok(new
            {
                success = true,
                data = topVideos,
                count = topVideos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top videos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets trending videos for a user
    /// </summary>
    [HttpGet("user/{userId}/trending")]
    public async Task<IActionResult> GetTrendingVideos(Guid userId, [FromQuery] int daysBack = 7)
    {
        try
        {
            _logger.LogInformation($"Fetching trending videos for user {userId} from last {daysBack} days");

            var trendingVideos = await _analyticsService.GetTrendingVideosAsync(userId, daysBack);

            return Ok(new
            {
                success = true,
                data = trendingVideos,
                count = trendingVideos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trending videos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets average metrics for a user
    /// </summary>
    [HttpGet("user/{userId}/average")]
    public async Task<IActionResult> GetAverageMetrics(Guid userId)
    {
        try
        {
            _logger.LogInformation($"Calculating average metrics for user {userId}");

            var (avgViews, avgEngagement, avgRetention) = await _analyticsService.GetAverageMetricsAsync(userId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    averageViews = avgViews,
                    averageEngagementRate = avgEngagement,
                    averageRetentionRate = avgRetention
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average metrics");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Generates analytics report for a user
    /// </summary>
    [HttpGet("user/{userId}/report")]
    public async Task<IActionResult> GenerateReport(Guid userId, [FromQuery] int daysBack = 30)
    {
        try
        {
            _logger.LogInformation($"Generating analytics report for user {userId}");

            var report = await _analyticsService.GenerateReportAsync(userId, daysBack);

            return Ok(new
            {
                success = true,
                data = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Checks if a video is performing well
    /// </summary>
    [HttpGet("video/{videoId}/performance")]
    public async Task<IActionResult> CheckVideoPerformance(Guid videoId, [FromQuery] Guid userId)
    {
        try
        {
            _logger.LogInformation($"Checking performance of video {videoId}");

            var isHighPerforming = await _analyticsService.IsVideoHighPerformingAsync(videoId, userId);

            return Ok(new
            {
                success = true,
                data = new { isHighPerforming }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video performance");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Records metrics for a video
    /// </summary>
    [HttpPost("video/{videoId}/metrics")]
    public async Task<IActionResult> RecordMetrics(Guid videoId, [FromBody] RecordMetricsRequest request)
    {
        try
        {
            _logger.LogInformation($"Recording metrics for video {videoId}");

            var metric = await _analyticsService.RecordMetricsAsync(
                videoId,
                request.ViewCount,
                request.LikeCount,
                request.CommentCount,
                request.ShareCount);

            return Created(string.Empty, new { success = true, data = metric });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metrics");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class RecordMetricsRequest
{
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public long ShareCount { get; set; }
}
