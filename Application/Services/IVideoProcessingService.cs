// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Service for processing, encoding, and optimizing videos for YouTube Shorts
/// </summary>
public interface IVideoProcessingService
{
    Task<ProcessingJob> CreateProcessingJobAsync(Guid videoId, ProcessingJobType jobType);
    Task<ProcessingJob> ProcessVideoAsync(ProcessingJob job);
    Task<ProcessingJob> RetryJobAsync(Guid jobId);
    Task<ProcessingJob> GetJobStatusAsync(Guid jobId);
}
