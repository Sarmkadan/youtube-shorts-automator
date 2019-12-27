// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

public class SchedulingService
{
    private readonly UploadJobRepository _uploadRepository;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(UploadJobRepository uploadRepository, ILogger<SchedulingService> logger)
    {
        _uploadRepository = uploadRepository ?? throw new ArgumentNullException(nameof(uploadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tolerance window for scheduling time validation. Allows scheduling for "now"
    /// without failing due to clock drift between caller and method evaluation.
    /// </summary>
    private static readonly TimeSpan SchedulingTolerance = TimeSpan.FromSeconds(5);

    public async Task<UploadJob> ScheduleUploadAsync(int videoShortId, DateTime scheduledTime,
        CancellationToken cancellationToken = default)
    {
        // Schedules a video upload for a specific time
        if (scheduledTime < DateTime.UtcNow.Subtract(SchedulingTolerance))
        {
            throw new SchedulingException(
                $"Scheduled time cannot be in the past (received {scheduledTime:O}, current UTC is {DateTime.UtcNow:O})");
        }

        try
        {
            var uploadJob = new UploadJob
            {
                VideoShortId = videoShortId,
                Status = UploadStatus.Pending,
                ScheduledAt = scheduledTime,
                AttemptCount = 0,
                MaxRetries = Constants.Constants.DEFAULT_RETRY_COUNT,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdJob = await _uploadRepository.AddAsync(uploadJob, cancellationToken);
            
            var timeUntilUpload = scheduledTime - DateTime.UtcNow;
            _logger.LogInformation($"Scheduled upload for video {videoShortId} in {timeUntilUpload.TotalHours:F2} hours");
            
            return createdJob;
        }
        catch (Exception ex)
        {
            throw new SchedulingException($"Failed to schedule upload: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<UploadJob>> GetUpcomingJobsAsync(int hoursAhead = 24,
        CancellationToken cancellationToken = default)
    {
        // Retrieves all scheduled jobs within the specified hours ahead
        try
        {
            var allJobs = await _uploadRepository.GetAllAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var cutoffTime = now.AddHours(hoursAhead);

            return allJobs
                .Where(j => j.ScheduledAt >= now && j.ScheduledAt <= cutoffTime && 
                           (j.Status == UploadStatus.Pending || j.Status == UploadStatus.Queued))
                .OrderBy(j => j.ScheduledAt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving upcoming jobs: {ex.Message}");
            throw new SchedulingException($"Failed to retrieve upcoming jobs: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<UploadJob>> GetOverdueJobsAsync(CancellationToken cancellationToken = default)
    {
        // Retrieves jobs that are overdue for upload
        try
        {
            var allJobs = await _uploadRepository.GetAllAsync(cancellationToken);
            var now = DateTime.UtcNow;

            return allJobs
                .Where(j => j.ScheduledAt <= now && j.Status == UploadStatus.Pending)
                .OrderBy(j => j.ScheduledAt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving overdue jobs: {ex.Message}");
            throw new SchedulingException($"Failed to retrieve overdue jobs: {ex.Message}", ex);
        }
    }

    public async Task<bool> RescheduleUploadAsync(int uploadJobId, DateTime newScheduledTime,
        CancellationToken cancellationToken = default)
    {
        // Reschedules an upload job to a different time
        if (newScheduledTime < DateTime.UtcNow.Subtract(SchedulingTolerance))
        {
            throw new SchedulingException(
                $"New scheduled time cannot be in the past (received {newScheduledTime:O}, current UTC is {DateTime.UtcNow:O})");
        }

        try
        {
            var job = await _uploadRepository.GetByIdAsync(uploadJobId, cancellationToken);
            if (job == null)
            {
                throw new SchedulingException($"Upload job {uploadJobId} not found");
            }

            if (job.Status == UploadStatus.Completed || job.Status == UploadStatus.Uploading)
            {
                throw new SchedulingException($"Cannot reschedule job with status {job.Status}");
            }

            job.ScheduledAt = newScheduledTime;
            job.UpdatedAt = DateTime.UtcNow;
            
            await _uploadRepository.UpdateAsync(job, cancellationToken);
            
            _logger.LogInformation($"Rescheduled upload job {uploadJobId} to {newScheduledTime}");
            return true;
        }
        catch (Exception ex)
        {
            throw new SchedulingException($"Failed to reschedule upload: {ex.Message}", ex);
        }
    }

    public async Task<bool> CancelUploadAsync(int uploadJobId, CancellationToken cancellationToken = default)
    {
        // Cancels a scheduled upload job
        try
        {
            var job = await _uploadRepository.GetByIdAsync(uploadJobId, cancellationToken);
            if (job == null)
            {
                throw new SchedulingException($"Upload job {uploadJobId} not found");
            }

            if (job.Status == UploadStatus.Uploading)
            {
                throw new SchedulingException("Cannot cancel a job that is currently uploading");
            }

            job.Status = UploadStatus.Cancelled;
            job.UpdatedAt = DateTime.UtcNow;
            
            await _uploadRepository.UpdateAsync(job, cancellationToken);
            
            _logger.LogInformation($"Cancelled upload job {uploadJobId}");
            return true;
        }
        catch (Exception ex)
        {
            throw new SchedulingException($"Failed to cancel upload: {ex.Message}", ex);
        }
    }

    public async Task<int> GetQueuedJobCountAsync(CancellationToken cancellationToken = default)
    {
        // Returns count of queued upload jobs
        try
        {
            var queuedJobs = await _uploadRepository.GetByStatusAsync(UploadStatus.Queued, cancellationToken);
            return queuedJobs.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting queued job count: {ex.Message}");
            throw;
        }
    }

    public TimeSpan CalculateOptimalUploadTime(DateTime videoCreatedAt, int estimatedProcessingMinutes)
    {
        // Calculates an optimal upload time based on creation time and processing duration
        var processingDuration = TimeSpan.FromMinutes(estimatedProcessingMinutes);
        var uploadTime = videoCreatedAt.Add(processingDuration);
        
        // Add a small buffer (5 minutes)
        uploadTime = uploadTime.AddMinutes(5);
        
        // Ensure it's not in the past
        if (uploadTime < DateTime.UtcNow)
        {
            uploadTime = DateTime.UtcNow.AddMinutes(5);
        }

        return uploadTime - DateTime.UtcNow;
    }

    public bool IsWithinOptimalUploadWindow(DateTime scheduleTime)
    {
        // Checks if the scheduled time is within optimal upload hours (avoid off-peak times)
        var hour = scheduleTime.Hour;
        // Optimal: 9 AM to 11 PM in user's timezone
        return hour >= 9 && hour <= 23;
    }
}
