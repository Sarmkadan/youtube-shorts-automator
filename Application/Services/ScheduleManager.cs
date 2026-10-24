// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// High-level schedule management: queueing, activation, pausing, due-upload dispatch, and bulk operations
/// </summary>
public class ScheduleManager
{
    private readonly ILogger<ScheduleManager> _logger;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IVideoUploader _videoUploader;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduleManager"/>
    /// </summary>
    /// <param name="logger">Logger for schedule activity</param>
    /// <param name="scheduleRepository">Repository that stores schedules</param>
    /// <param name="videoUploader">Uploader that publishes due videos</param>
    /// <param name="timeProvider">Clock used for all due-time decisions; defaults to the system clock</param>
    public ScheduleManager(
        ILogger<ScheduleManager> logger,
        IScheduleRepository scheduleRepository,
        IVideoUploader videoUploader,
        TimeProvider? timeProvider = null)
    {
        _logger = logger;
        _scheduleRepository = scheduleRepository;
        _videoUploader = videoUploader;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Queues a video for upload at its publish time. Rejects a video that is already queued or published
    /// in the same schedule, and a publish time that another queued upload already occupies.
    /// </summary>
    /// <param name="scheduleId">Schedule to add the upload to</param>
    /// <param name="videoId">Video to upload</param>
    /// <param name="scheduledFor">Publish time in UTC</param>
    /// <returns>The persisted queue entry</returns>
    /// <exception cref="ResourceNotFoundException">Schedule does not exist</exception>
    /// <exception cref="DomainException">Duplicate video (<c>DUPLICATE_SCHEDULE_ENTRY</c>) or overlapping slot (<c>SCHEDULE_OVERLAP</c>)</exception>
    public async Task<ScheduledUpload> EnqueueUploadAsync(Guid scheduleId, Guid videoId, DateTime scheduledFor)
    {
        var schedule = await _scheduleRepository.GetByIdWithUploadsAsync(scheduleId)
            ?? throw new ResourceNotFoundException("Schedule not found", scheduleId, "UploadSchedule");

        var duplicate = schedule.ScheduledUploads
            .FirstOrDefault(u => u.VideoId == videoId && u.Status is not (ScheduledUploadStatus.Failed or ScheduledUploadStatus.Skipped));
        if (duplicate is not null)
        {
            throw new DomainException(
                $"Video {videoId} is already scheduled in schedule {scheduleId}", "DUPLICATE_SCHEDULE_ENTRY");
        }

        var overlap = schedule.ScheduledUploads
            .FirstOrDefault(u => u.ScheduledFor == scheduledFor &&
                                 u.Status is ScheduledUploadStatus.Pending or ScheduledUploadStatus.InProgress);
        if (overlap is not null)
        {
            throw new DomainException(
                $"Publish slot {scheduledFor:O} is already taken in schedule {scheduleId}", "SCHEDULE_OVERLAP");
        }

        var upload = new ScheduledUpload
        {
            Id = Guid.NewGuid(),
            ScheduleId = scheduleId,
            VideoId = videoId,
            ScheduledFor = scheduledFor,
            Status = ScheduledUploadStatus.Pending
        };

        await _scheduleRepository.AddScheduledUploadAsync(upload);
        _logger.LogInformation("Queued video {VideoId} for schedule {ScheduleId} at {ScheduledFor}",
            videoId, scheduleId, scheduledFor);

        return upload;
    }

    /// <summary>
    /// Uploads every queued video whose publish time has arrived, earliest first within each schedule
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of uploads that completed successfully</returns>
    public async Task<int> ProcessDueSchedulesAsync(CancellationToken cancellationToken = default)
    {
        var now = UtcNow();
        var activeSchedules = await _scheduleRepository.GetActiveSchedulesAsync();

        var dueSchedules = activeSchedules.Where(s => HasDispatchableUploads(s, now)).ToList();

        _logger.LogInformation("Found {Count} schedules with due uploads", dueSchedules.Count);
        var executed = 0;

        foreach (var schedule in dueSchedules)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                executed += await ExecuteDueUploadsAsync(schedule, now, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process schedule {ScheduleId}", schedule.Id);
            }
        }

        return executed;
    }

    /// <summary>
    /// Pauses a schedule, preventing further automatic executions
    /// </summary>
    /// <param name="scheduleId">Schedule to pause</param>
    /// <returns>True if the schedule was paused</returns>
    public async Task<bool> PauseScheduleAsync(Guid scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule is null)
        {
            _logger.LogWarning("Schedule {ScheduleId} not found", scheduleId);
            return false;
        }

        if (!schedule.IsActive)
        {
            _logger.LogInformation("Schedule {ScheduleId} is already paused", scheduleId);
            return true;
        }

        schedule.IsActive = false;
        await _scheduleRepository.UpdateAsync(schedule);
        _logger.LogInformation("Schedule {ScheduleId} paused", scheduleId);
        return true;
    }

    /// <summary>
    /// Resumes a paused schedule and recalculates the next run time
    /// </summary>
    /// <param name="scheduleId">Schedule to resume</param>
    /// <returns>True if the schedule was resumed</returns>
    public async Task<bool> ResumeScheduleAsync(Guid scheduleId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule is null)
        {
            _logger.LogWarning("Schedule {ScheduleId} not found", scheduleId);
            return false;
        }

        schedule.Activate(UtcNow());
        await _scheduleRepository.UpdateAsync(schedule);
        _logger.LogInformation("Schedule {ScheduleId} resumed, next run at {NextRun}",
            scheduleId, schedule.NextScheduledTime);
        return true;
    }

    /// <summary>
    /// Returns summary statistics about active, paused, and overdue schedules
    /// </summary>
    public async Task<ScheduleSummary> GetSummaryAsync()
    {
        var all = await _scheduleRepository.GetAllAsync();
        var now = UtcNow();
        var active = await _scheduleRepository.GetActiveSchedulesAsync();

        return new ScheduleSummary
        {
            TotalSchedules = all.Count,
            ActiveSchedules = all.Count(s => s.IsActive),
            PausedSchedules = all.Count(s => !s.IsActive),
            OverdueSchedules = active.Count(s => HasDispatchableUploads(s, now))
        };
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    private static bool IsDispatchable(ScheduledUpload upload, DateTime now) =>
        upload.Status == ScheduledUploadStatus.Pending && upload.VideoId.HasValue && upload.ScheduledFor <= now;

    private static bool HasDispatchableUploads(UploadSchedule schedule, DateTime now) =>
        schedule.ScheduledUploads.Any(u => IsDispatchable(u, now));

    private async Task<int> ExecuteDueUploadsAsync(UploadSchedule schedule, DateTime now, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing due uploads for schedule {ScheduleId} ({Name})",
            schedule.Id, schedule.ScheduleName);

        var dueUploads = schedule.ScheduledUploads
            .Where(u => IsDispatchable(u, now))
            .OrderBy(u => u.ScheduledFor)
            .ToList();

        var completed = 0;

        foreach (var upload in dueUploads)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Persist InProgress before uploading so a restart mid-upload cannot re-trigger the same entry
            upload.Status = ScheduledUploadStatus.InProgress;
            await _scheduleRepository.UpdateAsync(schedule);

            try
            {
                await _videoUploader.UploadWithRetryAsync(upload.VideoId!.Value, schedule.UserId);
                upload.Status = ScheduledUploadStatus.Completed;
                upload.ExecutedAt = UtcNow();
                completed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload of video {VideoId} from schedule {ScheduleId} failed",
                    upload.VideoId, schedule.Id);
                upload.Status = ScheduledUploadStatus.Failed;
                upload.ExecutionError = ex.Message;
            }

            schedule.RecordExecution(now);
            await _scheduleRepository.UpdateAsync(schedule);
        }

        return completed;
    }
}

/// <summary>
/// Summary statistics for schedules
/// </summary>
public class ScheduleSummary
{
    /// <summary>Total number of schedules</summary>
    public int TotalSchedules { get; set; }

    /// <summary>Number of active schedules</summary>
    public int ActiveSchedules { get; set; }

    /// <summary>Number of paused schedules</summary>
    public int PausedSchedules { get; set; }

    /// <summary>Number of overdue schedules</summary>
    public int OverdueSchedules { get; set; }
}
