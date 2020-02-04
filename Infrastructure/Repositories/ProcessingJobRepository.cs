// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProcessingJob entity
/// </summary>
public class ProcessingJobRepository : Repository<ProcessingJob>, IProcessingJobRepository
{
    public ProcessingJobRepository(ApplicationDbContext context, ILogger<ProcessingJobRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets processing jobs for a video
    /// </summary>
    public async Task<List<ProcessingJob>> GetByVideoIdAsync(Guid videoId)
    {
        _logger.LogInformation($"Retrieving processing jobs for video {videoId}");
        return await _dbSet
            .Where(j => j.VideoId == videoId)
            .Include(j => j.Steps)
            .Include(j => j.Errors)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets processing jobs by status
    /// </summary>
    public async Task<List<ProcessingJob>> GetByStatusAsync(ProcessingJobStatus status)
    {
        _logger.LogInformation($"Retrieving processing jobs with status {status}");
        return await _dbSet
            .Where(j => j.Status == status)
            .Include(j => j.Steps)
            .Include(j => j.Errors)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets pending (queued) jobs
    /// </summary>
    public async Task<List<ProcessingJob>> GetPendingJobsAsync()
    {
        _logger.LogInformation("Retrieving pending processing jobs");
        return await _dbSet
            .Where(j => j.Status == ProcessingJobStatus.Queued)
            .Include(j => j.Video)
            .OrderBy(j => j.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Gets failed jobs
    /// </summary>
    public async Task<List<ProcessingJob>> GetFailedJobsAsync()
    {
        _logger.LogInformation("Retrieving failed processing jobs");
        return await _dbSet
            .Where(j => j.Status == ProcessingJobStatus.Failed)
            .Include(j => j.Errors)
            .OrderByDescending(j => j.CompletedAt)
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Gets failed jobs eligible for retry
    /// </summary>
    public async Task<List<ProcessingJob>> GetJobsForRetryAsync(int maxRetries)
    {
        _logger.LogInformation($"Retrieving jobs eligible for retry (max retries: {maxRetries})");
        return await _dbSet
            .Where(j => j.Status == ProcessingJobStatus.Failed &&
                       j.RetryCount < maxRetries)
            .OrderBy(j => j.CompletedAt)
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the latest job for a video
    /// </summary>
    public async Task<ProcessingJob?> GetLatestJobForVideoAsync(Guid videoId)
    {
        _logger.LogInformation($"Retrieving latest processing job for video {videoId}");
        return await _dbSet
            .Where(j => j.VideoId == videoId)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets jobs by type
    /// </summary>
    public async Task<List<ProcessingJob>> GetJobsByTypeAsync(ProcessingJobType jobType)
    {
        _logger.LogInformation($"Retrieving processing jobs of type {jobType}");
        return await _dbSet
            .Where(j => j.JobType == jobType)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets paginated processing jobs
    /// </summary>
    public async Task<(List<ProcessingJob> Jobs, int Total)> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation($"Retrieving paginated jobs: page {pageNumber}, size {pageSize}");

        var total = await _dbSet.CountAsync();
        var jobs = await _dbSet
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(j => j.Video)
            .Include(j => j.Steps)
            .Include(j => j.Errors)
            .ToListAsync();

        return (jobs, total);
    }
}
