// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="SchedulingServiceTests"/>.
/// </summary>
public static class SchedulingServiceTestsExtensions
{
    /// <summary>
    /// Asserts that the job has the expected status.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="job">The job to check.</param>
    /// <param name="expectedStatus">The expected status.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="job"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the status does not match.</exception>
    public static void AssertUploadJobStatus(this SchedulingServiceTests tests, UploadJob job, UploadStatus expectedStatus)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(job);

        if (job.Status != expectedStatus)
        {
            throw new InvalidOperationException($"Expected status {expectedStatus}, but found {job.Status}");
        }
    }

    /// <summary>
    /// Asserts that the job scheduled at time matches the expected time.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="job">The job to check.</param>
    /// <param name="expectedScheduledAt">The expected scheduled time.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="job"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the scheduled time does not match.</exception>
    public static void AssertUploadJobScheduledAt(this SchedulingServiceTests tests, UploadJob job, DateTime expectedScheduledAt)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(job);

        if (job.ScheduledAt != expectedScheduledAt)
        {
            throw new InvalidOperationException($"Expected scheduled time {expectedScheduledAt}, but found {job.ScheduledAt}");
        }
    }

    /// <summary>
    /// Asserts that the collection of upcoming jobs contains the expected number of jobs.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="jobs">The jobs to check.</param>
    /// <param name="expectedCount">The expected number of jobs.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="jobs"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the count does not match.</exception>
    public static void AssertUpcomingJobsCount(this SchedulingServiceTests tests, IEnumerable<UploadJob> jobs, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(jobs);

        if (jobs.Count() != expectedCount)
        {
            throw new InvalidOperationException($"Expected {expectedCount} jobs, but found {jobs.Count()}");
        }
    }
}
