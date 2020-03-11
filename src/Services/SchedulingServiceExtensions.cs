// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

public static class SchedulingServiceExtensions
{
    /// <summary>
    /// Extension method to schedule multiple uploads at once with validation
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="jobs">Collection of upload job specifications</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of created upload jobs</returns>
    public static async Task<IEnumerable<UploadJob>> ScheduleUploadsAsync(
        this SchedulingService service,
        IEnumerable<(int VideoShortId, DateTime ScheduledTime)> jobs,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (jobs == null)
        {
            throw new ArgumentNullException(nameof(jobs));
        }

        var createdJobs = new List<UploadJob>();
        var logger = service.GetServiceLogger();

        foreach (var (videoShortId, scheduledTime) in jobs)
        {
            try
            {
                var job = await service.ScheduleUploadAsync(videoShortId, scheduledTime, cancellationToken);
                createdJobs.Add(job);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to schedule upload for video {VideoShortId}", videoShortId);
                throw;
            }
        }

        return createdJobs;
    }

    /// <summary>
    /// Extension method to get upcoming jobs filtered by video short IDs
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="hoursAhead">Hours ahead to look</param>
    /// <param name="videoShortIds">Filter by specific video short IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered collection of upcoming upload jobs</returns>
    public static async Task<IEnumerable<UploadJob>> GetUpcomingJobsAsync(
        this SchedulingService service,
        int hoursAhead,
        IEnumerable<int> videoShortIds,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (videoShortIds == null)
        {
            throw new ArgumentNullException(nameof(videoShortIds));
        }

        var allJobs = await service.GetUpcomingJobsAsync(hoursAhead, cancellationToken);
        var idSet = new HashSet<int>(videoShortIds);

        return allJobs.Where(job => idSet.Contains(job.VideoShortId));
    }

    /// <summary>
    /// Extension method to get overdue jobs filtered by video short IDs
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="videoShortIds">Filter by specific video short IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered collection of overdue upload jobs</returns>
    public static async Task<IEnumerable<UploadJob>> GetOverdueJobsAsync(
        this SchedulingService service,
        IEnumerable<int> videoShortIds,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (videoShortIds == null)
        {
            throw new ArgumentNullException(nameof(videoShortIds));
        }

        var allJobs = await service.GetOverdueJobsAsync(cancellationToken);
        var idSet = new HashSet<int>(videoShortIds);

        return allJobs.Where(job => idSet.Contains(job.VideoShortId));
    }

    /// <summary>
    /// Extension method to reschedule multiple uploads at once
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="rescheduleJobs">Collection of reschedule operations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of reschedule results</returns>
    public static async Task<IEnumerable<bool>> RescheduleUploadsAsync(
        this SchedulingService service,
        IEnumerable<(int UploadJobId, DateTime NewScheduledTime)> rescheduleJobs,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (rescheduleJobs == null)
        {
            throw new ArgumentNullException(nameof(rescheduleJobs));
        }

        var results = new List<bool>();
        var logger = service.GetServiceLogger();

        foreach (var (uploadJobId, newScheduledTime) in rescheduleJobs)
        {
            try
            {
                var result = await service.RescheduleUploadAsync(uploadJobId, newScheduledTime, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to reschedule upload job {UploadJobId}", uploadJobId);
                results.Add(false);
            }
        }

        return results;
    }

    /// <summary>
    /// Extension method to cancel multiple uploads at once
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="uploadJobIds">Collection of upload job IDs to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of cancellation results</returns>
    public static async Task<IEnumerable<bool>> CancelUploadsAsync(
        this SchedulingService service,
        IEnumerable<int> uploadJobIds,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (uploadJobIds == null)
        {
            throw new ArgumentNullException(nameof(uploadJobIds));
        }

        var results = new List<bool>();
        var logger = service.GetServiceLogger();

        foreach (var uploadJobId in uploadJobIds)
        {
            try
            {
                var result = await service.CancelUploadAsync(uploadJobId, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to cancel upload job {UploadJobId}", uploadJobId);
                results.Add(false);
            }
        }

        return results;
    }

    /// <summary>
    /// Extension method to check if any jobs are overdue
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any overdue jobs exist, false otherwise</returns>
    public static async Task<bool> HasOverdueJobsAsync(
        this SchedulingService service,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var overdueJobs = await service.GetOverdueJobsAsync(cancellationToken);
        return overdueJobs.Any();
    }

    /// <summary>
    /// Extension method to get upcoming jobs count
    /// </summary>
    /// <param name="service">The scheduling service instance</param>
    /// <param name="hoursAhead">Hours ahead to count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of upcoming jobs</returns>
    public static async Task<int> GetUpcomingJobsCountAsync(
        this SchedulingService service,
        int hoursAhead = 24,
        CancellationToken cancellationToken = default)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var jobs = await service.GetUpcomingJobsAsync(hoursAhead, cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Helper method to get logger from service (using reflection to access private field)
    /// </summary>
    private static ILogger? GetServiceLogger(this SchedulingService service)
    {
        if (service == null)
        {
            return null;
        }

        var loggerField = typeof(SchedulingService).GetField(
            "_logger",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return loggerField?.GetValue(service) as ILogger;
    }
}