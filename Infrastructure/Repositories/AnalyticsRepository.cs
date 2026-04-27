// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AnalyticsMetric entity
/// </summary>
public class AnalyticsRepository : Repository<AnalyticsMetric>, IAnalyticsRepository
{
    public AnalyticsRepository(ApplicationDbContext context, ILogger<AnalyticsRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets metrics for a video
    /// </summary>
    public async Task<List<AnalyticsMetric>> GetByVideoIdAsync(Guid videoId)
    {
        _logger.LogInformation($"Retrieving analytics metrics for video {videoId}");
        return await _dbSet
            .Where(m => m.VideoId == videoId)
            .Include(m => m.Demographics)
            .OrderByDescending(m => m.CollectedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets metrics by period
    /// </summary>
    public async Task<List<AnalyticsMetric>> GetByPeriodAsync(MetricsPeriod period)
    {
        _logger.LogInformation($"Retrieving analytics metrics for period {period}");
        return await _dbSet
            .Where(m => m.Period == period)
            .OrderByDescending(m => m.CollectedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recent metrics for a video
    /// </summary>
    public async Task<List<AnalyticsMetric>> GetRecentMetricsAsync(Guid videoId, int daysBack)
    {
        _logger.LogInformation($"Retrieving recent analytics for video {videoId} from last {daysBack} days");
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        return await _dbSet
            .Where(m => m.VideoId == videoId && m.CollectedAt > cutoffDate)
            .Include(m => m.Demographics)
            .OrderBy(m => m.CollectedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets top performing metrics
    /// </summary>
    public async Task<List<AnalyticsMetric>> GetTopMetricsAsync(int limit)
    {
        _logger.LogInformation($"Retrieving top {limit} performing metrics");
        return await _dbSet
            .OrderByDescending(m => m.ViewCount)
            .Take(limit)
            .Include(m => m.Video)
            .ToListAsync();
    }

    /// <summary>
    /// Gets latest metric for a video
    /// </summary>
    public async Task<AnalyticsMetric?> GetLatestMetricForVideoAsync(Guid videoId)
    {
        _logger.LogInformation($"Retrieving latest metric for video {videoId}");
        return await _dbSet
            .Where(m => m.VideoId == videoId)
            .OrderByDescending(m => m.CollectedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets paginated analytics metrics
    /// </summary>
    public async Task<(List<AnalyticsMetric> Metrics, int Total)> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation($"Retrieving paginated metrics: page {pageNumber}, size {pageSize}");

        var total = await _dbSet.CountAsync();
        var metrics = await _dbSet
            .OrderByDescending(m => m.CollectedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Video)
            .Include(m => m.Demographics)
            .ToListAsync();

        return (metrics, total);
    }

    /// <summary>
    /// Gets view counts for multiple videos
    /// </summary>
    public async Task<Dictionary<Guid, long>> GetVideoViewCountsAsync(List<Guid> videoIds)
    {
        _logger.LogInformation($"Retrieving view counts for {videoIds.Count} videos");

        var metrics = await _dbSet
            .Where(m => videoIds.Contains(m.VideoId))
            .GroupBy(m => m.VideoId)
            .Select(g => new { VideoId = g.Key, ViewCount = g.Sum(m => m.ViewCount) })
            .ToListAsync();

        return metrics.ToDictionary(m => m.VideoId, m => m.ViewCount);
    }
}
