// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for scheduling uploads
/// Manages scheduled video uploads with timezone support and recurrence options
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly ICacheService _cacheService;

    public ScheduleController(
        ILogger<ScheduleController> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new scheduled upload
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScheduleAsync([FromBody] CreateScheduleRequest request)
    {
        try
        {
            // Validate request
            var (isValid, error) = ValidationUtility.ValidateScheduleTime(request.ScheduledUploadTimeUtc.ToString("O"));
            if (!isValid)
                return BadRequest(new { message = error });

            var scheduleId = Guid.NewGuid();

            var schedule = new ScheduledUpload
            {
                ScheduleId = scheduleId,
                VideoId = request.VideoId,
                ScheduledUploadTimeUtc = request.ScheduledUploadTimeUtc,
                Status = "Scheduled",
                CreatedAtUtc = DateTime.UtcNow,
                RecurrencePattern = request.RecurrencePattern,
                TimeZone = request.TimeZone ?? "UTC"
            };

            _cacheService.Set($"schedule:{scheduleId}", schedule, TimeSpan.FromDays(30));

            _logger.LogInformation("Upload scheduled. ScheduleId: {ScheduleId}, UploadTime: {UploadTime}",
                scheduleId, request.ScheduledUploadTimeUtc);

            return CreatedAtAction(nameof(GetScheduleAsync), new { scheduleId },
                new ScheduleResponse { ScheduleId = scheduleId, Status = "Scheduled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get details of a scheduled upload
    /// </summary>
    [HttpGet("{scheduleId}")]
    [ProducesResponseType(typeof(ScheduledUploadDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScheduleAsync(Guid scheduleId)
    {
        try
        {
            var schedule = _cacheService.Get<ScheduledUpload>($"schedule:{scheduleId}");
            if (schedule == null)
                return NotFound();

            return Ok(new ScheduledUploadDetails
            {
                ScheduleId = schedule.ScheduleId,
                VideoId = schedule.VideoId,
                ScheduledUploadTimeUtc = schedule.ScheduledUploadTimeUtc,
                Status = schedule.Status,
                RecurrencePattern = schedule.RecurrencePattern,
                TimeZone = schedule.TimeZone,
                CreatedAtUtc = schedule.CreatedAtUtc,
                LastExecutedUtc = schedule.LastExecutedUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// List all scheduled uploads
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ScheduleListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSchedulesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters");

            // Simulate fetching from database
            var schedules = new List<ScheduleListItem>
            {
                new() { ScheduleId = Guid.NewGuid(), VideoId = Guid.NewGuid(), Status = "Scheduled", ScheduledUploadTimeUtc = DateTime.UtcNow.AddDays(1) },
                new() { ScheduleId = Guid.NewGuid(), VideoId = Guid.NewGuid(), Status = "Pending", ScheduledUploadTimeUtc = DateTime.UtcNow.AddHours(2) }
            };

            var response = new ScheduleListResponse
            {
                Schedules = schedules,
                TotalCount = schedules.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing schedules");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update a scheduled upload
    /// </summary>
    [HttpPut("{scheduleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScheduleAsync(Guid scheduleId, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            var schedule = _cacheService.Get<ScheduledUpload>($"schedule:{scheduleId}");
            if (schedule == null)
                return NotFound();

            if (request.ScheduledUploadTimeUtc.HasValue)
            {
                schedule.ScheduledUploadTimeUtc = request.ScheduledUploadTimeUtc.Value;
            }

            if (!string.IsNullOrEmpty(request.RecurrencePattern))
            {
                schedule.RecurrencePattern = request.RecurrencePattern;
            }

            _cacheService.Set($"schedule:{scheduleId}", schedule, TimeSpan.FromDays(30));

            _logger.LogInformation("Schedule updated. ScheduleId: {ScheduleId}", scheduleId);

            return Ok(new { message = "Schedule updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete a scheduled upload
    /// </summary>
    [HttpDelete("{scheduleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduleAsync(Guid scheduleId)
    {
        try
        {
            await _cacheService.RemoveAsync($"schedule:{scheduleId}");
            _logger.LogInformation("Schedule deleted. ScheduleId: {ScheduleId}", scheduleId);
            return Ok(new { message = "Schedule deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

#region Schedule Models

public class CreateScheduleRequest
{
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string? RecurrencePattern { get; set; }
    public string? TimeZone { get; set; }
}

public class UpdateScheduleRequest
{
    public DateTime? ScheduledUploadTimeUtc { get; set; }
    public string? RecurrencePattern { get; set; }
}

public class ScheduleResponse
{
    public Guid ScheduleId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScheduledUpload
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RecurrencePattern { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastExecutedUtc { get; set; }
}

public class ScheduledUploadDetails
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RecurrencePattern { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastExecutedUtc { get; set; }
}

public class ScheduleListItem
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledUploadTimeUtc { get; set; }
}

public class ScheduleListResponse
{
    public List<ScheduleListItem> Schedules { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

#endregion
