// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;

namespace YouTubeShortsAutomator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly IVideoRepository _videoRepository;
    private readonly VideoProcessingService _processingService;
    private readonly YouTubeUploadService _uploadService;
    private readonly AnalyticsService _analyticsService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(
        IVideoRepository videoRepository,
        VideoProcessingService processingService,
        YouTubeUploadService uploadService,
        AnalyticsService analyticsService,
        ILogger<VideoController> logger)
    {
        _videoRepository = videoRepository;
        _processingService = processingService;
        _uploadService = uploadService;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all videos for the authenticated user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserVideos(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation($"Fetching videos for user {userId}");
            var (videos, total) = await _videoRepository.GetUserVideosPaginatedAsync(userId, pageNumber, pageSize);

            return Ok(new
            {
                success = true,
                data = videos,
                pagination = new { pageNumber, pageSize, total }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user videos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets details of a specific video
    /// </summary>
    [HttpGet("{videoId}")]
    public async Task<IActionResult> GetVideo(Guid videoId)
    {
        try
        {
            var video = await _videoRepository.GetByIdAsync(videoId);
            if (video == null)
                return NotFound(new { success = false, message = "Video not found" });

            return Ok(new { success = true, data = video });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching video {videoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Starts processing a video
    /// </summary>
    [HttpPost("{videoId}/process")]
    public async Task<IActionResult> ProcessVideo(Guid videoId)
    {
        try
        {
            _logger.LogInformation($"Starting video processing for {videoId}");

            var job = await _processingService.CreateProcessingJobAsync(videoId, Domain.Models.ProcessingJobType.FullProcessing);
            var processedJob = await _processingService.ProcessVideoAsync(job);

            return Accepted(new { success = true, data = processedJob });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (VideoValidationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, errors = ex.ValidationErrors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing video {videoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Uploads a processed video to YouTube
    /// </summary>
    [HttpPost("{videoId}/upload")]
    public async Task<IActionResult> UploadVideo(Guid videoId, [FromQuery] Guid userId)
    {
        try
        {
            _logger.LogInformation($"Uploading video {videoId} to YouTube");

            var uploadResult = await _uploadService.UploadVideoAsync(videoId, userId);

            return Accepted(new { success = true, data = uploadResult });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (CredentialException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (UploadException ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading video {videoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets analytics for a video
    /// </summary>
    [HttpGet("{videoId}/analytics")]
    public async Task<IActionResult> GetAnalytics(Guid videoId)
    {
        try
        {
            _logger.LogInformation($"Fetching analytics for video {videoId}");

            var metric = await _analyticsService.GetVideoAnalyticsAsync(videoId, Domain.Models.MetricsPeriod.Daily);

            if (metric == null)
                return NotFound(new { success = false, message = "No analytics data found" });

            return Ok(new { success = true, data = metric });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching analytics for video {videoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets processing job status
    /// </summary>
    [HttpGet("job/{jobId}/status")]
    public async Task<IActionResult> GetJobStatus(Guid jobId)
    {
        try
        {
            var job = await _processingService.GetJobStatusAsync(jobId);
            return Ok(new { success = true, data = job });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting job status {jobId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Publishes an uploaded video
    /// </summary>
    [HttpPost("{videoId}/publish")]
    public async Task<IActionResult> PublishVideo(Guid videoId)
    {
        try
        {
            _logger.LogInformation($"Publishing video {videoId}");

            var video = await _videoRepository.GetByIdAsync(videoId);
            if (video?.UploadResult == null)
                return BadRequest(new { success = false, message = "Video has not been uploaded yet" });

            await _uploadService.PublishVideoAsync(video.UploadResult.Id);

            return Ok(new { success = true, message = "Video published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing video {videoId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
