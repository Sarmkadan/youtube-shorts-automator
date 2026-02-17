// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Repositories;

/// <summary>
/// Repository for Video entity operations
/// </summary>
public interface IVideoRepository : IRepository<Video>
{
    Task<List<Video>> GetByUserIdAsync(Guid userId);
    Task<List<Video>> GetByStatusAsync(VideoStatus status);
    Task<List<Video>> GetPendingVideosAsync();
    Task<List<Video>> GetRecentVideosAsync(Guid userId, int daysBack);
    Task<List<Video>> GetByYouTubeIdAsync(string youtubeId);
    Task<(List<Video> Videos, int Total)> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<(List<Video> Videos, int Total)> GetUserVideosPaginatedAsync(Guid userId, int pageNumber, int pageSize);
}
