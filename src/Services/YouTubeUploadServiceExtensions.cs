using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for <see cref="YouTubeUploadService"/>.
/// </summary>
public static class YouTubeUploadServiceExtensions
{
    /// <summary>
    /// Retrieves an upload job by ID and throws if the job is not found.
    /// </summary>
    /// <param name="service">The YouTube upload service.</param>
    /// <param name="jobId">The ID of the upload job.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The found <see cref="UploadJob"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="UploadException">Thrown when the job is not found.</exception>
    public static async Task<UploadJob> GetUploadJobOrThrowAsync(this YouTubeUploadService service, int jobId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var job = await service.GetUploadJobAsync(jobId, cancellationToken);
        return job ?? throw new UploadException($"Upload job with ID {jobId} not found.", jobId, 0, false);
    }

    /// <summary>
    /// Attempts to publish a video, returning false instead of throwing if a <see cref="YouTubeApiException"/> occurs.
    /// </summary>
    /// <param name="service">The YouTube upload service.</param>
    /// <param name="videoId">The ID of the video to publish.</param>
    /// <param name="channel">The YouTube channel to publish to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the publish was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="channel"/> is null.</exception>
    public static async Task<bool> TryPublishVideoAsync(this YouTubeUploadService service, string videoId, YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentException.ThrowIfNullOrEmpty(videoId);

        try
        {
            return await service.PublishVideoAsync(videoId, channel, cancellationToken);
        }
        catch (YouTubeApiException)
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves all pending jobs scheduled for upload.
    /// </summary>
    /// <param name="service">The YouTube upload service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of pending upload jobs.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyList<UploadJob>> GetScheduledPendingJobsAsync(this YouTubeUploadService service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var jobs = await service.GetScheduledJobsAsync(cancellationToken);
        return jobs.Where(j => j.Status == UploadStatus.Pending).ToList().AsReadOnly();
    }
}
