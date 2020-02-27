// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Manages video upload scheduling and execution
/// </summary>
public class SchedulingService
{
    private readonly ILogger<SchedulingService> _logger;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly YouTubeUploadService _uploadService;

    public SchedulingService(
        ILogger<SchedulingService> logger,
        IScheduleRepository scheduleRepository,
        IVideoRepository videoRepository,
        YouTubeUploadService uploadService)
    {
        _logger = logger;
        _scheduleRepository = scheduleRepository;
        _videoRepository = videoRepository;
        _uploadService = uploadService;
    }

    /// <summary>
    /// Creates a new upload schedule
    /// </summary>
    public async Task<UploadSchedule> CreateScheduleAsync(
        Guid userId,
        string scheduleName,
        ScheduleFrequency frequency,
        TimeOnly scheduledTime,
        DayOfWeek? dayOfWeek = null,
        int? dayOfMonth = null,
        string timeZoneId = "UTC")
    {
        _logger.LogInformation($"Creating upload schedule for user {userId}");

        var schedule = new UploadSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ScheduleName = scheduleName,
            Frequency = frequency,
            ScheduledTime = scheduledTime,
            DayOfWeek = dayOfWeek,
            DayOfMonth = dayOfMonth,
            TimeZoneId = timeZoneId,
            CreatedAt = DateTime.UtcNow
        };

        var (isValid, errors) = schedule.Validate();
        if (!isValid)
            throw new DomainException($"Invalid schedule: {string.Join(", ", errors)}");

        schedule.NextScheduledTime = schedule.CalculateNextScheduledTime();

        await _scheduleRepository.AddAsync(schedule);

        _logger.LogInformation($"Schedule created successfully: {schedule.Id}");
        return schedule;
    }

    /// <summary>
    /// Updates an existing schedule
    /// </summary>
    public async Task<UploadSchedule> UpdateScheduleAsync(Guid scheduleId, UploadSchedule updatedSchedule)
    {
        _logger.LogInformation($"Updating schedule {scheduleId}");

        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        var (isValid, errors) = updatedSchedule.Validate();
        if (!isValid)
            throw new DomainException($"Invalid schedule: {string.Join(", ", errors)}");

        schedule.ScheduleName = updatedSchedule.ScheduleName;
        schedule.Frequency = updatedSchedule.Frequency;
        schedule.ScheduledTime = updatedSchedule.ScheduledTime;
        schedule.DayOfWeek = updatedSchedule.DayOfWeek;
        schedule.DayOfMonth = updatedSchedule.DayOfMonth;
        schedule.TimeZoneId = updatedSchedule.TimeZoneId;

        schedule.NextScheduledTime = schedule.CalculateNextScheduledTime();

        await _scheduleRepository.UpdateAsync(schedule);

        _logger.LogInformation($"Schedule updated successfully");
        return schedule;
    }

    /// <summary>
    /// Gets all schedules for a user
    /// </summary>
    public async Task<List<UploadSchedule>> GetUserSchedulesAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving schedules for user {userId}");
        return await _scheduleRepository.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Gets a specific schedule
    /// </summary>
    public async Task<UploadSchedule?> GetScheduleAsync(Guid scheduleId)
    {
        return await _scheduleRepository.GetByIdAsync(scheduleId);
    }

    /// <summary>
    /// Activates a schedule
    /// </summary>
    public async Task<UploadSchedule> ActivateScheduleAsync(Guid scheduleId)
    {
        _logger.LogInformation($"Activating schedule {scheduleId}");

        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        schedule.Activate();
        await _scheduleRepository.UpdateAsync(schedule);

        return schedule;
    }

    /// <summary>
    /// Deactivates a schedule
    /// </summary>
    public async Task<UploadSchedule> DeactivateScheduleAsync(Guid scheduleId)
    {
        _logger.LogInformation($"Deactivating schedule {scheduleId}");

        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        schedule.Deactivate();
        await _scheduleRepository.UpdateAsync(schedule);

        return schedule;
    }

    /// <summary>
    /// Schedules a video for upload
    /// </summary>
    public async Task<ScheduledUpload> ScheduleVideoUploadAsync(Guid scheduleId, Guid videoId, DateTime scheduledFor)
    {
        _logger.LogInformation($"Scheduling video {videoId} for upload at {scheduledFor}");

        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        var video = await _videoRepository.GetByIdAsync(videoId)
            ?? throw new ResourceNotFoundException("Video not found", videoId, "Video");

        if (scheduledFor < DateTime.UtcNow)
            throw new InvalidStateException("Cannot schedule for past date", DateTime.UtcNow.ToString(), "ScheduleUpload");

        var scheduledUpload = new ScheduledUpload
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            VideoId = videoId,
            ScheduledFor = scheduledFor,
            Status = ScheduledUploadStatus.Pending
        };

        schedule.ScheduledUploads.Add(scheduledUpload);
        await _scheduleRepository.UpdateAsync(schedule);

        _logger.LogInformation($"Video scheduled for upload: {scheduledUpload.Id}");
        return scheduledUpload;
    }

    /// <summary>
    /// Executes scheduled uploads that are due
    /// </summary>
    public async Task<List<ScheduledUpload>> ExecuteDueScheduledUploadsAsync()
    {
        _logger.LogInformation("Checking for scheduled uploads due for execution");

        var dueSchedules = await _scheduleRepository.GetDueSchedulesAsync();
        var executedUploads = new List<ScheduledUpload>();

        foreach (var schedule in dueSchedules)
        {
            if (!schedule.IsActive)
                continue;

            var pendingUploads = schedule.ScheduledUploads
                .Where(su => su.Status == ScheduledUploadStatus.Pending && su.ScheduledFor <= DateTime.UtcNow)
                .ToList();

            foreach (var upload in pendingUploads)
            {
                try
                {
                    if (upload.VideoId.HasValue)
                    {
                        upload.Status = ScheduledUploadStatus.InProgress;
                        await _scheduleRepository.UpdateScheduledUploadAsync(upload);

                        // Execute the upload
                        var uploadResult = await _uploadService.UploadVideoAsync(upload.VideoId.Value, schedule.UserId);

                        upload.Status = ScheduledUploadStatus.Completed;
                        upload.ExecutedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Scheduled upload executed: {upload.Id}");
                        executedUploads.Add(upload);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to execute scheduled upload {upload.Id}");
                    upload.Status = ScheduledUploadStatus.Failed;
                    upload.ExecutionError = ex.Message;
                }

                await _scheduleRepository.UpdateScheduledUploadAsync(upload);
            }

            // Update next scheduled time
            schedule.RecordExecution();
            await _scheduleRepository.UpdateAsync(schedule);
        }

        return executedUploads;
    }

    /// <summary>
    /// Gets pending scheduled uploads for a schedule
    /// </summary>
    public async Task<List<ScheduledUpload>> GetPendingUploadsAsync(Guid scheduleId)
    {
        _logger.LogInformation($"Retrieving pending uploads for schedule {scheduleId}");

        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        return schedule.ScheduledUploads
            .Where(su => su.Status == ScheduledUploadStatus.Pending)
            .ToList();
    }

    /// <summary>
    /// Deletes a schedule
    /// </summary>
    public async Task<bool> DeleteScheduleAsync(Guid scheduleId)
    {
        _logger.LogInformation($"Deleting schedule {scheduleId}");

        await _scheduleRepository.DeleteAsync(scheduleId);

        _logger.LogInformation($"Schedule deleted successfully");
        return true;
    }
}
