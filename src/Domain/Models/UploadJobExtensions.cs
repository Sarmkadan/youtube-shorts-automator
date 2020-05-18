// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="UploadJob"/> class to facilitate common upload job operations.
/// </summary>
public static class UploadJobExtensions
{
    /// <summary>
    /// Determines whether the upload job is currently in a retryable state.
    /// </summary>
    /// <param name="job">The upload job to check.</param>
    /// <returns>true if the job is in a retryable state; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsRetryable(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Status == UploadStatus.Failed && job.AttemptCount < job.MaxRetries;
    }

    /// <summary>
    /// Determines whether the upload job is currently active (queued, uploading, or retrying).
    /// </summary>
    /// <param name="job">The upload job to check.</param>
    /// <returns>true if the job is active; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsActive(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Status is UploadStatus.Queued or UploadStatus.Uploading or UploadStatus.Retrying;
    }

    /// <summary>
    /// Determines whether the upload job has been completed successfully.
    /// </summary>
    /// <param name="job">The upload job to check.</param>
    /// <returns>true if the job has completed successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsCompletedSuccessfully(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Status == UploadStatus.Completed && job.UploadedAt.HasValue;
    }

    /// <summary>
    /// Determines whether the upload job has failed and cannot be retried.
    /// </summary>
    /// <param name="job">The upload job to check.</param>
    /// <returns>true if the job has failed permanently; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsFailedPermanently(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Status == UploadStatus.Failed && job.AttemptCount >= job.MaxRetries;
    }

    /// <summary>
    /// Calculates the upload speed in bytes per second.
    /// </summary>
    /// <param name="job">The upload job.</param>
    /// <returns>The upload speed in bytes per second, or 0 if the upload hasn't started or duration is unknown.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static double CalculateUploadSpeed(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (job.UploadedBytes <= 0 || job.UploadedAt == null)
        {
            return 0;
        }

        var duration = job.UploadedAt.Value - job.CreatedAt;
        return duration.TotalSeconds > 0 ? job.UploadedBytes / duration.TotalSeconds : 0;
    }

    /// <summary>
    /// Gets the estimated completion time based on current progress and upload speed.
    /// </summary>
    /// <param name="job">The upload job.</param>
    /// <returns>The estimated completion time, or null if the upload hasn't started or speed cannot be determined.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static DateTime? GetEstimatedCompletionTime(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (job.UploadProgressPercentage <= 0 || job.UploadProgressPercentage >= 100)
        {
            return null;
        }

        var speed = job.CalculateUploadSpeed();
        if (speed <= 0)
        {
            return null;
        }

        var remainingBytes = (long)((100.0 - job.UploadProgressPercentage) / 100.0 * job.UploadedBytes / Math.Max(job.UploadProgressPercentage / 100.0, 0.01));
        var remainingTime = TimeSpan.FromSeconds(remainingBytes / speed);
        return DateTime.UtcNow.Add(remainingTime);
    }

    /// <summary>
    /// Gets the retry count as a formatted string.
    /// </summary>
    /// <param name="job">The upload job.</param>
    /// <returns>A formatted string showing retry count (e.g., "Attempt 2 of 3").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static string GetRetryStatus(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return $"Attempt {job.AttemptCount} of {job.MaxRetries}";
    }

    /// <summary>
    /// Determines whether the upload job is overdue based on its scheduled time.
    /// </summary>
    /// <param name="job">The upload job to check.</param>
    /// <returns>true if the job is overdue; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsOverdue(this UploadJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.ScheduledAt < DateTime.UtcNow && !job.IsCompletedSuccessfully();
    }

    /// <summary>
    /// Gets all upload jobs that are in a specific status.
    /// </summary>
    /// <param name="jobs">The collection of upload jobs to filter.</param>
    /// <param name="status">The status to filter by.</param>
    /// <returns>An enumerable of upload jobs matching the specified status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobs"/> is null.</exception>
    public static IEnumerable<UploadJob> WhereStatus(this IEnumerable<UploadJob> jobs, UploadStatus status)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.Where(j => j.Status == status);
    }

    /// <summary>
    /// Gets all upload jobs that are in a retryable state.
    /// </summary>
    /// <param name="jobs">The collection of upload jobs to filter.</param>
    /// <returns>An enumerable of upload jobs that can be retried.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobs"/> is null.</exception>
    public static IEnumerable<UploadJob> WhereRetryable(this IEnumerable<UploadJob> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.Where(j => j.IsRetryable());
    }

    /// <summary>
    /// Gets all upload jobs that are currently active.
    /// </summary>
    /// <param name="jobs">The collection of upload jobs to filter.</param>
    /// <returns>An enumerable of upload jobs that are active.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobs"/> is null.</exception>
    public static IEnumerable<UploadJob> WhereActive(this IEnumerable<UploadJob> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.Where(j => j.IsActive());
    }

    /// <summary>
    /// Orders upload jobs by their scheduled time (earliest first).
    /// </summary>
    /// <param name="jobs">The collection of upload jobs to order.</param>
    /// <returns>An ordered enumerable of upload jobs sorted by scheduled time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobs"/> is null.</exception>
    public static IOrderedEnumerable<UploadJob> OrderByScheduledTime(this IEnumerable<UploadJob> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.OrderBy(j => j.ScheduledAt);
    }

    /// <summary>
    /// Orders upload jobs by their upload progress percentage (highest first).
    /// </summary>
    /// <param name="jobs">The collection of upload jobs to order.</param>
    /// <returns>An ordered enumerable of upload jobs sorted by upload progress.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobs"/> is null.</exception>
    public static IOrderedEnumerable<UploadJob> OrderByProgressDescending(this IEnumerable<UploadJob> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        return jobs.OrderByDescending(j => j.UploadProgressPercentage);
    }
}