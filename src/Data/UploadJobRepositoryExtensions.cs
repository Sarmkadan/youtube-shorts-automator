// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for UploadJobRepository providing additional convenience
// operations and batch operations
// =============================================================================

using System.Diagnostics.CodeAnalysis;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Data;

[SuppressMessage("Design", "CA1068:CancellationToken parameters should come last", Justification = "Consistent with repository interface pattern")]
public static class UploadJobRepositoryExtensions
{
    /// <summary>
    /// Gets the first upload job by VideoShortId, or null if not found
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="videoShortId">The video short identifier to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The upload job with the specified VideoShortId, or null if not found</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<UploadJob?> GetByVideoShortIdAsync(this UploadJobRepository repository, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allJobs = await repository.GetAllAsync(cancellationToken);
        return allJobs.FirstOrDefault(j => j.VideoShortId == videoShortId);
    }

    /// <summary>
    /// Gets all upload jobs with the specified status
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="status">The upload status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of upload jobs with the specified status</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<IEnumerable<UploadJob>> GetByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetByStatusAsync(status, cancellationToken);
    }

    /// <summary>
    /// Gets all upload jobs scheduled for upload that haven't started yet
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of scheduled upload jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<IEnumerable<UploadJob>> GetScheduledForUploadAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetScheduledForUploadAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all upload jobs that can be retried (failed with remaining attempts)
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of retryable failed upload jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<IEnumerable<UploadJob>> GetRetryableFailedJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.GetRetryableFailedJobsAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of upload jobs with the specified status
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="status">The upload status to count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of upload jobs with the specified status</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<int> CountByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var jobs = await repository.GetByStatusAsync(status, cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Gets the count of upload jobs scheduled for upload that haven't started yet
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of scheduled upload jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<int> CountScheduledForUploadAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var jobs = await repository.GetScheduledForUploadAsync(cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Gets the count of upload jobs that can be retried (failed with remaining attempts)
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of retryable failed upload jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<int> CountRetryableFailedJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var jobs = await repository.GetRetryableFailedJobsAsync(cancellationToken);
        return jobs.Count();
    }

    /// <summary>
    /// Batch update multiple upload jobs at once
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="jobs">Collection of upload jobs to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> or <paramref name="jobs"/> is null</exception>
    public static async Task BatchUpdateAsync(this UploadJobRepository repository, IEnumerable<UploadJob> jobs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(jobs);

        foreach (var job in jobs)
        {
            await repository.UpdateAsync(job, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Batch delete multiple upload jobs by their IDs
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="jobIds">Collection of job IDs to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of successfully deleted jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> or <paramref name="jobIds"/> is null</exception>
    public static async Task<int> BatchDeleteAsync(this UploadJobRepository repository, IEnumerable<int> jobIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(jobIds);

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
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The oldest scheduled upload job, or null if none exist</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<UploadJob?> GetOldestScheduledJobAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var scheduledJobs = await repository.GetScheduledForUploadAsync(cancellationToken);
        return scheduledJobs.OrderBy(j => j.ScheduledAt).FirstOrDefault();
    }

    /// <summary>
    /// Gets the newest upload job by CreatedAt date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newest upload job by creation date, or null if none exist</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<UploadJob?> GetNewestJobAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allJobs = await repository.GetAllAsync(cancellationToken);
        return allJobs.OrderByDescending(j => j.CreatedAt).FirstOrDefault();
    }

    /// <summary>
    /// Checks if any upload jobs exist with the specified status
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="status">The upload status to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any jobs exist with the specified status; otherwise false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<bool> AnyByStatusAsync(this UploadJobRepository repository, UploadStatus status, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var count = await repository.CountByStatusAsync(status, cancellationToken);
        return count > 0;
    }

    /// <summary>
    /// Gets all upload jobs with status Queued or Pending
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of queued and pending upload jobs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    public static async Task<IEnumerable<UploadJob>> GetQueuedOrPendingJobsAsync(this UploadJobRepository repository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var queuedJobs = await repository.GetByStatusAsync(UploadStatus.Queued, cancellationToken);
        var pendingJobs = await repository.GetByStatusAsync(UploadStatus.Pending, cancellationToken);
        return queuedJobs.Concat(pendingJobs);
    }
}