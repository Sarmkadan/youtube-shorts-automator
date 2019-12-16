// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Repositories;

/// <summary>
/// Repository for AnalyticsMetric entity operations
/// </summary>
public interface IAnalyticsRepository : IRepository<AnalyticsMetric>
{
    Task<List<AnalyticsMetric>> GetByVideoIdAsync(Guid videoId);
    Task<List<AnalyticsMetric>> GetByPeriodAsync(MetricsPeriod period);
    Task<List<AnalyticsMetric>> GetRecentMetricsAsync(Guid videoId, int daysBack);
    Task<List<AnalyticsMetric>> GetTopMetricsAsync(int limit);
    Task<AnalyticsMetric?> GetLatestMetricForVideoAsync(Guid videoId);
    Task<(List<AnalyticsMetric> Metrics, int Total)> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<Dictionary<Guid, long>> GetVideoViewCountsAsync(List<Guid> videoIds);
}
