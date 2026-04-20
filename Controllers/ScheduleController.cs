// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly SchedulingService _schedulingService;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(SchedulingService schedulingService, ILogger<ScheduleController> logger)
    {
        _schedulingService = schedulingService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new upload schedule
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
    {
        try
        {
            _logger.LogInformation($"Creating schedule for user {request.UserId}");

            var schedule = await _schedulingService.CreateScheduleAsync(
                request.UserId,
                request.ScheduleName,
                request.Frequency,
                request.ScheduledTime,
                request.DayOfWeek,
                request.DayOfMonth,
                request.TimeZoneId);

            return Created(string.Empty, new { success = true, data = schedule });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all schedules for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSchedules(Guid userId)
    {
        try
        {
            _logger.LogInformation($"Fetching schedules for user {userId}");

            var schedules = await _schedulingService.GetUserSchedulesAsync(userId);

            return Ok(new
            {
                success = true,
                data = schedules,
                count = schedules.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching schedules");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific schedule
    /// </summary>
    [HttpGet("{scheduleId}")]
    public async Task<IActionResult> GetSchedule(Guid scheduleId)
    {
        try
        {
            var schedule = await _schedulingService.GetScheduleAsync(scheduleId);

            if (schedule == null)
                return NotFound(new { success = false, message = "Schedule not found" });

            return Ok(new { success = true, data = schedule });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching schedule {scheduleId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Activates a schedule
    /// </summary>
    [HttpPost("{scheduleId}/activate")]
    public async Task<IActionResult> ActivateSchedule(Guid scheduleId)
    {
        try
        {
            _logger.LogInformation($"Activating schedule {scheduleId}");

            var schedule = await _schedulingService.ActivateScheduleAsync(scheduleId);

            return Ok(new { success = true, data = schedule });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating schedule");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivates a schedule
    /// </summary>
    [HttpPost("{scheduleId}/deactivate")]
    public async Task<IActionResult> DeactivateSchedule(Guid scheduleId)
    {
        try
        {
            _logger.LogInformation($"Deactivating schedule {scheduleId}");

            var schedule = await _schedulingService.DeactivateScheduleAsync(scheduleId);

            return Ok(new { success = true, data = schedule });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating schedule");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Schedules a video for upload
    /// </summary>
    [HttpPost("{scheduleId}/schedule-video")]
    public async Task<IActionResult> ScheduleVideoUpload(Guid scheduleId, [FromBody] ScheduleVideoRequest request)
    {
        try
        {
            _logger.LogInformation($"Scheduling video {request.VideoId} for upload");

            var scheduledUpload = await _schedulingService.ScheduleVideoUploadAsync(
                scheduleId,
                request.VideoId,
                request.ScheduledFor);

            return Created(string.Empty, new { success = true, data = scheduledUpload });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidStateException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling video");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gets pending uploads for a schedule
    /// </summary>
    [HttpGet("{scheduleId}/pending")]
    public async Task<IActionResult> GetPendingUploads(Guid scheduleId)
    {
        try
        {
            _logger.LogInformation($"Fetching pending uploads for schedule {scheduleId}");

            var uploads = await _schedulingService.GetPendingUploadsAsync(scheduleId);

            return Ok(new
            {
                success = true,
                data = uploads,
                count = uploads.Count
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending uploads");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a schedule
    /// </summary>
    [HttpDelete("{scheduleId}")]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId)
    {
        try
        {
            _logger.LogInformation($"Deleting schedule {scheduleId}");

            await _schedulingService.DeleteScheduleAsync(scheduleId);

            return Ok(new { success = true, message = "Schedule deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class CreateScheduleRequest
{
    public Guid UserId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public ScheduleFrequency Frequency { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
}

public class ScheduleVideoRequest
{
    public Guid VideoId { get; set; }
    public DateTime ScheduledFor { get; set; }
}
