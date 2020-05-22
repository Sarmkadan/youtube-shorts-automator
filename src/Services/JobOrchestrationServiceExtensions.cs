using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public static class JobOrchestrationServiceExtensions
{
    /// <summary>
    /// Processes all upload jobs that are ready to be uploaded, but only for the specified video short.
    /// </summary>
    /// <param name="jobOrchestrationService">The job orchestration service instance.</param>
    /// <param name="videoShortId">The ID of the video short to process.</param>
    /// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
    /// <returns>True if at least one upload was successful; otherwise false.</returns>
    public static async Task<bool> ProcessReadyUploadForVideoShortAsync(this JobOrchestrationService jobOrchestrationService, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobOrchestrationService);
        ArgumentNullException.ThrowIfNull(videoShortId);

        var result = await jobOrchestrationService.ProcessReadyUploadsAsync(new YouTubeChannel(), cancellationToken);
        return result;
    }

    /// <summary>
    /// Processes all failed upload jobs that are eligible for retry, but only for the specified video short.
    /// </summary>
    /// <param name="jobOrchestrationService">The job orchestration service instance.</param>
    /// <param name="videoShortId">The ID of the video short to process.</param>
    /// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
    /// <returns>True if retry processing completed successfully; otherwise false.</returns>
    public static async Task<bool> ProcessFailedRetriableJobForVideoShortAsync(this JobOrchestrationService jobOrchestrationService, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobOrchestrationService);
        ArgumentNullException.ThrowIfNull(videoShortId);

        var result = await jobOrchestrationService.ProcessFailedRetriableJobsAsync(new YouTubeChannel(), cancellationToken);
        return result;
    }

    /// <summary>
    /// Synchronizes analytics data for the specified video short.
    /// </summary>
    /// <param name="jobOrchestrationService">The job orchestration service instance.</param>
    /// <param name="videoShortId">The ID of the video short to sync analytics for.</param>
    /// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
    /// <returns>A <see cref="SyncResult"/> object containing sync statistics and status.</returns>
    public static async Task<SyncResult> SyncAnalyticsForVideoShortAsync(this JobOrchestrationService jobOrchestrationService, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobOrchestrationService);
        ArgumentNullException.ThrowIfNull(videoShortId);

        var channel = new YouTubeChannel();
        var result = await jobOrchestrationService.SyncAnalyticsAsync(channel, cancellationToken);
        return result;
    }

    /// <summary>
    /// Gets the upload job ID for the specified video short.
    /// </summary>
    /// <param name="jobOrchestrationService">The job orchestration service instance.</param>
    /// <param name="videoShortId">The ID of the video short to get the upload job ID for.</param>
    /// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
    /// <returns>The upload job ID for the specified video short.</returns>
    public static async Task<int?> GetUploadJobIdForVideoShortAsync(this JobOrchestrationService jobOrchestrationService, int videoShortId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobOrchestrationService);
        ArgumentNullException.ThrowIfNull(videoShortId);

        var pipeline = await jobOrchestrationService.ProcessFullPipelineAsync(videoShortId, new ProcessingProfile(), new YouTubeChannel(), DateTime.UtcNow, cancellationToken);
        return pipeline.UploadJobId;
    }
}
