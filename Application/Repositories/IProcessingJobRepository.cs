// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Repositories;

/// <summary>
/// Repository for ProcessingJob entity operations
/// </summary>
public interface IProcessingJobRepository : IRepository<ProcessingJob>
{
    Task<List<ProcessingJob>> GetByVideoIdAsync(Guid videoId);
    Task<List<ProcessingJob>> GetByStatusAsync(ProcessingJobStatus status);
    Task<List<ProcessingJob>> GetPendingJobsAsync();
    Task<List<ProcessingJob>> GetFailedJobsAsync();
    Task<List<ProcessingJob>> GetJobsForRetryAsync(int maxRetries);
    Task<ProcessingJob?> GetLatestJobForVideoAsync(Guid videoId);
    Task<List<ProcessingJob>> GetJobsByTypeAsync(ProcessingJobType jobType);
    Task<(List<ProcessingJob> Jobs, int Total)> GetPaginatedAsync(int pageNumber, int pageSize);
}
