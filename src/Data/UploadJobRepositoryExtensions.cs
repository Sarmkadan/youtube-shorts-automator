// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for UploadJobRepository providing additional convenience
// operations and batch operations
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Data;

public static class UploadJobRepositoryExtensions
{
    /// <summary>
    /// Gets the first upload job by VideoShortId, or null if not found
    /// </summary>
    public static async Task<UploadJob?> GetByVideoShortIdAsync(this UploadJobRepository repository, int videoShortId, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var allJobs = await repository.GetAllAsync(cancellationToken);
        return allJobs.FirstOrDefault(j => j.VideoShortId == videoShortId);
    }

    /// <summary>
    /// Gets all upload jobs with the specified status
    /// </summary>
    public static async Task<IEnumerable<UploadJob>> GetByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        return await repository.GetByStatusAsync(status, cancellationToken);
    }

    /// <summary>
    /// Gets all upload jobs scheduled for upload that haven't started yet
    /// </summary>
    public static async Task<IEnumerable<UploadJob>> GetScheduledForUploadAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        return await repository.GetScheduledForUploadAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all upload jobs that can be retried (failed with remaining attempts)
    /// </summary>
    public static async Task<IEnumerable<UploadJob>> GetRetryableFailedJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        return await repository.GetRetryableFailedJobsAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of upload jobs with the specified status
    /// </summary>
    public static async Task<int> CountByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var jobs = await repository.GetByStatusAsync(status, cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Gets the count of upload jobs scheduled for upload that haven't started yet
    /// </summary>
    public static async Task<int> CountScheduledForUploadAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var jobs = await repository.GetScheduledForUploadAsync(cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Gets the count of upload jobs that can be retried (failed with remaining attempts)
    /// </summary>
    public static async Task<int> CountRetryableFailedJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var jobs = await repository.GetRetryableFailedJobsAsync(cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Batch update multiple upload jobs at once
    /// </summary>
    public static async Task BatchUpdateAsync(this UploadJobRepository repository, IEnumerable<UploadJob> jobs, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));
        if (jobs == null)
            throw new ArgumentNullException(nameof(jobs));

        foreach (var job in jobs)
        {
            await repository.UpdateAsync(job, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Batch delete multiple upload jobs by their IDs
    /// </summary>
    public static async Task<int> BatchDeleteAsync(this UploadJobRepository repository, IEnumerable<int> jobIds, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));
        if (jobIds == null)
            throw new ArgumentNullException(nameof(jobIds));

        int deletedCount = 0;
        foreach (var id in jobIds)
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
            if (deleted)
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }

    /// <summary>
    /// Gets the oldest scheduled upload job (earliest ScheduledAt date)
    /// </summary>
    public static async Task<UploadJob?> GetOldestScheduledJobAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var scheduledJobs = await repository.GetScheduledForUploadAsync(cancellationToken);
        return scheduledJobs.OrderBy(j => j.ScheduledAt).FirstOrDefault();
    }

    /// <summary>
    /// Gets the newest upload job by CreatedAt date
    /// </summary>
    public static async Task<UploadJob?> GetNewestJobAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var allJobs = await repository.GetAllAsync(cancellationToken);
        return allJobs.OrderByDescending(j => j.CreatedAt).FirstOrDefault();
    }

    /// <summary>
    /// Checks if any upload jobs exist with the specified status
    /// </summary>
    public static async Task<bool> AnyByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var count = await repository.CountByStatusAsync(status, cancellationToken);
        return count > 0;
    }

    /// <summary>
    /// Gets all upload jobs with status Queued or Pending
    /// </summary>
    public static async Task<IEnumerable<UploadJob>> GetQueuedOrPendingJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        var queuedJobs = await repository.GetByStatusAsync(UploadStatus.Queued, cancellationToken);
        var pendingJobs = await repository.GetByStatusAsync(UploadStatus.Pending, cancellationToken);
        return queuedJobs.Concat(pendingJobs);
    }
}