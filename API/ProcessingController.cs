// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Utilities;
using YouTubeShortsAutomator.Formatters;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for video processing operations
/// Handles video submission, status checks, and processing configuration
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProcessingController : ControllerBase
{
    private readonly ILogger<ProcessingController> _logger;
    private readonly JsonResponseFormatter _responseFormatter;
    private readonly ICacheService _cacheService;

    public ProcessingController(
        ILogger<ProcessingController> logger,
        JsonResponseFormatter responseFormatter,
        ICacheService cacheService)
    {
        _logger = logger;
        _responseFormatter = responseFormatter;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Submit a video file for processing
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(ProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitVideoAsync([FromForm] SubmitVideoRequest request)
    {
        try
        {
            // Validate request
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file provided");

            var (isValid, error) = ValidationUtility.ValidateVideoTitle(request.Title);
            if (!isValid)
                return BadRequest(new { message = error });

            (isValid, error) = ValidationUtility.ValidateVideoDescription(request.Description);
            if (!isValid)
                return BadRequest(new { message = error });

            var processingId = Guid.NewGuid();

            // Store processing metadata in cache
            _cacheService.Set($"processing:{processingId}", new
            {
                ProcessingId = processingId,
                FileName = request.File.FileName,
                Title = request.Title,
                Description = request.Description,
                Status = "Queued",
                CreatedAtUtc = DateTime.UtcNow
            }, TimeSpan.FromHours(24));

            _logger.LogInformation("Video submission queued. ProcessingId: {ProcessingId}, FileName: {FileName}",
                processingId, request.File.FileName);

            var response = new ProcessingResponse
            {
                ProcessingId = processingId,
                Status = "Queued",
                Message = "Video submitted for processing"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting video for processing");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while processing the request"
            });
        }
    }

    /// <summary>
    /// Get the status of a processing job
    /// </summary>
    [HttpGet("{processingId}/status")]
    [ProducesResponseType(typeof(ProcessingStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProcessingStatusAsync(Guid processingId)
    {
        try
        {
            var cacheKey = $"processing:{processingId}";
            var processingData = _cacheService.Get<dynamic>(cacheKey);

            if (processingData == null)
            {
                _logger.LogWarning("Processing job not found. ProcessingId: {ProcessingId}", processingId);
                return NotFound(new { message = "Processing job not found" });
            }

            var response = new ProcessingStatusResponse
            {
                ProcessingId = processingId,
                Status = processingData.Status,
                Progress = 0,
                CreatedAtUtc = processingData.CreatedAtUtc
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing status. ProcessingId: {ProcessingId}", processingId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get available processing profiles
    /// </summary>
    [HttpGet("profiles")]
    [ProducesResponseType(typeof(ProfilesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableProfilesAsync()
    {
        try
        {
            var profiles = new[]
            {
                new ProfileInfo
                {
                    Name = "High Quality",
                    Code = "hq",
                    Resolution = "1080p",
                    Bitrate = "8000k",
                    EstimatedProcessingTimeMinutes = 15
                },
                new ProfileInfo
                {
                    Name = "Standard Quality",
                    Code = "standard",
                    Resolution = "720p",
                    Bitrate = "4000k",
                    EstimatedProcessingTimeMinutes = 8
                },
                new ProfileInfo
                {
                    Name = "Mobile Optimized",
                    Code = "mobile",
                    Resolution = "480p",
                    Bitrate = "2000k",
                    EstimatedProcessingTimeMinutes = 4
                }
            };

            var response = new ProfilesResponse
            {
                Profiles = profiles
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving processing profiles");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Cancel a processing job
    /// </summary>
    [HttpDelete("{processingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelProcessingAsync(Guid processingId)
    {
        try
        {
            var cacheKey = $"processing:{processingId}";
            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Processing job cancelled. ProcessingId: {ProcessingId}", processingId);

            return Ok(new { message = "Processing job cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling processing job. ProcessingId: {ProcessingId}", processingId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

#region Request/Response Models

public class SubmitVideoRequest
{
    public IFormFile? File { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProcessingProfile { get; set; } = "standard";
}

public class ProcessingResponse
{
    public Guid ProcessingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ProcessingStatusResponse
{
    public Guid ProcessingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class ProfileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string Bitrate { get; set; } = string.Empty;
    public int EstimatedProcessingTimeMinutes { get; set; }
}

public class ProfilesResponse
{
    public ProfileInfo[]? Profiles { get; set; }
}

#endregion
