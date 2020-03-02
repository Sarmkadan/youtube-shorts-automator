// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;
using System;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Service for processing, encoding, and optimizing videos for YouTube Shorts
/// </summary>
public interface IVideoProcessingService
{
    /// <summary>
    /// Creates a new processing job for the specified video.
    /// </summary>
    /// <param name="videoId">Identifier of the video to process.</param>
    /// <param name="jobType">Type of processing job.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="videoId"/> is empty.</exception>
    Task<ProcessingJob> CreateProcessingJobAsync(Guid videoId, ProcessingJobType jobType);

    /// <summary>
    /// Executes the processing job.
    /// </summary>
    Task<ProcessingJob> ProcessVideoAsync(ProcessingJob job);

    /// <summary>
    /// Retries a failed job.
    /// </summary>
    Task<ProcessingJob> RetryJobAsync(Guid jobId);

    /// <summary>
    /// Retrieves the current status of a job.
    /// </summary>
    Task<ProcessingJob> GetJobStatusAsync(Guid jobId);
}
