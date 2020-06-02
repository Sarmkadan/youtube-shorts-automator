using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="ProcessingJob"/> class.
/// </summary>
public static class ProcessingJobExtensions
{
    /// <summary>
    /// Gets the total duration of the processing job if it has been completed or failed.
    /// </summary>
    /// <param name="job">The processing job instance.</param>
    /// <returns>The duration as a <see cref="TimeSpan"/>, or <see cref="TimeSpan.Zero"/> if the job has not finished.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static TimeSpan GetProcessingDuration(this ProcessingJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.CompletedAt.HasValue && job.StartedAt.HasValue
            ? job.CompletedAt.Value - job.StartedAt.Value
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Checks if the processing job is currently in a finished state (Completed, Failed, or Cancelled).
    /// </summary>
    /// <param name="job">The processing job instance.</param>
    /// <returns>True if the job is finished; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static bool IsFinished(this ProcessingJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Status is ProcessingJobStatus.Completed or ProcessingJobStatus.Failed or ProcessingJobStatus.Cancelled;
    }

    /// <summary>
    /// Gets the last error message recorded for the job, if any.
    /// </summary>
    /// <param name="job">The processing job instance.</param>
    /// <returns>The last error message string, or <c>null</c> if no errors exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static string? GetLastErrorMessage(this ProcessingJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.Errors.LastOrDefault()?.ErrorMessage;
    }
}
