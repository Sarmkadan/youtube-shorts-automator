// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Extension methods for JobStatusController providing additional functionality
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Extension methods for <see cref="JobStatusController"/> that provide additional job monitoring and utility functionality.
/// </summary>
public static class JobStatusControllerExtensions
{
    /// <summary>
    /// Checks if a job has completed (either successfully or with failure).
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>True if the job has completed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<bool> HasJobCompletedAsync(this JobStatusController controller, Guid jobId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetJobStatusAsync(jobId);
        if (result is not OkObjectResult okResult)
            return false;

        var jobStatus = okResult.Value as JobStatusResponse;
        if (jobStatus == null)
            return false;

        return jobStatus.Status is "Completed" or "Failed" or "Cancelled";
    }

    /// <summary>
    /// Gets the duration of a job in a human-readable format.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>
    /// A string representing the job duration in format "HH:mm:ss", or "N/A" if duration cannot be calculated.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<string> GetJobDurationAsync(this JobStatusController controller, Guid jobId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetJobStatusAsync(jobId);
        if (result is not OkObjectResult okResult)
            return "N/A";

        var jobStatus = okResult.Value as JobStatusResponse;
        if (jobStatus == null)
            return "N/A";

        if (jobStatus.StartedAtUtc == null || jobStatus.CompletedAtUtc == null)
            return "N/A";

        var duration = jobStatus.CompletedAtUtc.Value - jobStatus.StartedAtUtc.Value;
        return duration.ToString("hh:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the progress percentage formatted as a string with a percent sign.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>A string in format "XX%" representing the job progress.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<string> GetProgressStringAsync(this JobStatusController controller, Guid jobId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetJobStatusAsync(jobId);
        if (result is not OkObjectResult okResult)
            return "0%";

        var jobStatus = okResult.Value as JobStatusResponse;
        if (jobStatus == null)
            return "0%";

        return $"{jobStatus.Progress}%";
    }

    /// <summary>
    /// Gets a summary of job statistics including success and failure counts.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>
    /// A tuple containing (SuccessCount, FailureCount, TotalCount) or (0, 0, 0) if the job has no results.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<(int SuccessCount, int FailureCount, int TotalCount)> GetJobResultCountsAsync(
        this JobStatusController controller,
        Guid jobId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetJobResultsAsync(jobId);
        if (result is not OkObjectResult okResult)
            return (0, 0, 0);

        var jobResults = okResult.Value as JobResultsResponse;
        if (jobResults == null)
            return (0, 0, 0);

        return (jobResults.SuccessfulCount, jobResults.FailedCount, jobResults.TotalCount);
    }
}