// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// High-level schedule management: activation, pausing, due-schedule resolution, and bulk operations
/// </summary>
public class ScheduleManager
{
    private readonly ILogger<ScheduleManager> _logger;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly SchedulingService _schedulingService;
    private readonly VideoUploader _videoUploader;

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduleManager"/>
    /// </summary>
    public ScheduleManager(
        ILogger<ScheduleManager> logger,
        IScheduleRepository scheduleRepository,
        SchedulingService schedulingService,
        VideoUploader videoUploader)
    {
        _logger = logger;
        _scheduleRepository = scheduleRepository;
        _schedulingService = schedulingService;
        _videoUploader = videoUploader;
    }

    /// <summary>
    /// Finds all schedules that are due for execution and triggers their uploads
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of schedules executed</returns>
    public async Task<int> ProcessDueSchedulesAsync(CancellationToken cancellationToken = default)
    {
        var allSchedules = await _scheduleRepository.GetAllAsync();

        var dueSchedules = allSchedules.Where(s => s.IsDueForExecution()).ToList();

        _logger.LogInformation("Found {Count} due schedules", dueSchedules.Count);
        var executed = 0;

        foreach (var schedule in dueSchedules)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                await ExecuteScheduleAsync(schedule);
                executed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute schedule {ScheduleId}", schedule.Id);
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

        schedule.Activate();
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

        return new ScheduleSummary
        {
            TotalSchedules = all.Count,
            ActiveSchedules = all.Count(s => s.IsActive),
            PausedSchedules = all.Count(s => !s.IsActive),
            OverdueSchedules = all.Count(s => s.IsDueForExecution())
        };
    }

    private async Task ExecuteScheduleAsync(UploadSchedule schedule)
    {
        _logger.LogInformation("Executing schedule {ScheduleId} ({Name})", schedule.Id, schedule.ScheduleName);

        var pendingUploads = schedule.ScheduledUploads
            .Where(u => u.Status == ScheduledUploadStatus.Pending && u.VideoId.HasValue)
            .OrderBy(u => u.ScheduledFor)
            .ToList();

        if (pendingUploads.Count == 0)
        {
            _logger.LogInformation("Schedule {ScheduleId} has no pending uploads", schedule.Id);
            schedule.RecordExecution();
            await _scheduleRepository.UpdateAsync(schedule);
            return;
        }

        var nextUpload = pendingUploads[0];
        nextUpload.Status = ScheduledUploadStatus.InProgress;

        try
        {
            await _videoUploader.UploadWithRetryAsync(nextUpload.VideoId!.Value, schedule.UserId);
            nextUpload.Status = ScheduledUploadStatus.Completed;
            nextUpload.ExecutedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            nextUpload.Status = ScheduledUploadStatus.Failed;
            nextUpload.ExecutionError = ex.Message;
            throw;
        }
        finally
        {
            schedule.RecordExecution();
            await _scheduleRepository.UpdateAsync(schedule);
        }
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
