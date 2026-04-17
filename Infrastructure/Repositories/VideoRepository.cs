// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Video entity
/// </summary>
public class VideoRepository : Repository<Video>, IVideoRepository
{
    public VideoRepository(ApplicationDbContext context, ILogger<VideoRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets videos by user ID
    /// </summary>
    public async Task<List<Video>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving videos for user {userId}");
        return await _dbSet
            .Where(v => v.UserId == userId)
            .Include(v => v.ProcessingJobs)
            .Include(v => v.Metrics)
            .Include(v => v.UploadResult)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets videos by status
    /// </summary>
    public async Task<List<Video>> GetByStatusAsync(VideoStatus status)
    {
        _logger.LogInformation($"Retrieving videos with status {status}");
        return await _dbSet
            .Where(v => v.Status == status)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets pending videos (not yet processed)
    /// </summary>
    public async Task<List<Video>> GetPendingVideosAsync()
    {
        _logger.LogInformation("Retrieving pending videos");
        return await _dbSet
            .Where(v => v.Status == VideoStatus.Pending)
            .OrderBy(v => v.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recent videos for a user
    /// </summary>
    public async Task<List<Video>> GetRecentVideosAsync(Guid userId, int daysBack)
    {
        _logger.LogInformation($"Retrieving videos for user {userId} from last {daysBack} days");
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        return await _dbSet
            .Where(v => v.UserId == userId && v.CreatedAt > cutoffDate)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets videos by YouTube ID
    /// </summary>
    public async Task<List<Video>> GetByYouTubeIdAsync(string youtubeId)
    {
        _logger.LogInformation($"Retrieving videos by YouTube ID {youtubeId}");
        return await _dbSet
            .Where(v => v.YouTubeVideoId == youtubeId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets paginated videos
    /// </summary>
    public async Task<(List<Video> Videos, int Total)> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation($"Retrieving paginated videos: page {pageNumber}, size {pageSize}");

        var total = await _dbSet.CountAsync();
        var videos = await _dbSet
            .OrderByDescending(v => v.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(v => v.ProcessingJobs)
            .Include(v => v.Metrics)
            .Include(v => v.UploadResult)
            .ToListAsync();

        return (videos, total);
    }

    /// <summary>
    /// Gets paginated videos for a user
    /// </summary>
    public async Task<(List<Video> Videos, int Total)> GetUserVideosPaginatedAsync(Guid userId, int pageNumber, int pageSize)
    {
        _logger.LogInformation($"Retrieving paginated videos for user {userId}: page {pageNumber}, size {pageSize}");

        var query = _dbSet.Where(v => v.UserId == userId);
        var total = await query.CountAsync();

        var videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(v => v.ProcessingJobs)
            .Include(v => v.Metrics)
            .Include(v => v.UploadResult)
            .ToListAsync();

        return (videos, total);
    }
}
