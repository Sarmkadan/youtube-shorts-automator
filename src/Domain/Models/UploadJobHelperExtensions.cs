// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides side-effect-free helper methods for evaluating <see cref="UploadJob"/> instances.
/// </summary>
public static class UploadJobHelperExtensions
{
    /// <summary>
    /// Determines whether an unfinished upload job is past its scheduled time.
    /// </summary>
    /// <param name="job">The upload job to evaluate.</param>
    /// <param name="now">The date and time against which to compare the scheduled time.</param>
    /// <returns>
    /// <see langword="true"/> when the job is scheduled before <paramref name="now"/> and its
    /// status is neither <see cref="UploadStatus.Completed"/> nor <see cref="UploadStatus.Cancelled"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is <see langword="null"/>.</exception>
    public static bool IsOverdue(this UploadJob job, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(job);

        return job.ScheduledAt < now
            && job.Status is not UploadStatus.Completed and not UploadStatus.Cancelled;
    }
}
