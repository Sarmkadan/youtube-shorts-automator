// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Integration;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Tracks and aggregates video performance analytics from YouTube API data
/// </summary>
public class AnalyticsTracker
{
    private readonly ILogger<AnalyticsTracker> _logger;
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IGoogleApiClient _googleApiClient;

    /// <summary>
    /// Initializes a new instance of <see cref="AnalyticsTracker"/>
    /// </summary>
    public AnalyticsTracker(
        ILogger<AnalyticsTracker> logger,
        IAnalyticsRepository analyticsRepository,
        IVideoRepository videoRepository,
        IGoogleApiClient googleApiClient)
    {
        _logger = logger;
        _analyticsRepository = analyticsRepository;
        _videoRepository = videoRepository;
        _googleApiClient = googleApiClient;
    }

    /// <summary>
    /// Fetches latest analytics from YouTube for a specific video and persists the snapshot
    /// </summary>
    /// <param name="videoId">Internal video identifier</param>
    /// <returns>The recorded metric, or null if the video has no YouTube ID</returns>
    public async Task<AnalyticsMetric?> SyncVideoAnalyticsAsync(Guid videoId)
    {
        var video = await _videoRepository.GetByIdAsync(videoId);
        if (video is null || string.IsNullOrEmpty(video.YouTubeVideoId))
        {
            _logger.LogWarning("Cannot sync analytics: video {VideoId} not found or not uploaded", videoId);
            return null;
        }

        var analytics = await _googleApiClient.GetVideoAnalyticsAsync(video.YouTubeVideoId);
        if (analytics is null)
        {
            _logger.LogWarning("YouTube returned no analytics for {YouTubeId}", video.YouTubeVideoId);
            return null;
        }

        var metric = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            ViewCount = analytics.ViewCount,
            LikeCount = analytics.LikeCount,
            CommentCount = analytics.CommentCount,
            ShareCount = analytics.ShareCount,
            CollectedAt = DateTime.UtcNow
        };

        await _analyticsRepository.AddAsync(metric);
        _logger.LogInformation("Recorded analytics snapshot for video {VideoId}: {Views} views", videoId, metric.ViewCount);

        return metric;
    }

    /// <summary>
    /// Syncs analytics for all uploaded videos that have a YouTube ID
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of videos successfully synced</returns>
    public async Task<int> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var videos = await _videoRepository.GetAllAsync();
        var uploaded = videos.Where(v => !string.IsNullOrEmpty(v.YouTubeVideoId)).ToList();

        _logger.LogInformation("Syncing analytics for {Count} uploaded videos", uploaded.Count);
        var synced = 0;

        foreach (var video in uploaded)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var result = await SyncVideoAnalyticsAsync(video.Id);
                if (result is not null) synced++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync analytics for video {VideoId}", video.Id);
            }
        }

        _logger.LogInformation("Analytics sync complete: {Synced}/{Total}", synced, uploaded.Count);
        return synced;
    }

    /// <summary>
    /// Calculates the total view count across all tracked videos
    /// </summary>
    /// <returns>Aggregate view count</returns>
    public async Task<long> GetTotalViewsAsync()
    {
        var metrics = await _analyticsRepository.GetAllAsync();

        // Take the latest metric per video to avoid double-counting
        var latestPerVideo = metrics
            .GroupBy(m => m.VideoId)
            .Select(g => g.OrderByDescending(m => m.CollectedAt).First())
            .ToList();

        var total = latestPerVideo.Sum(m => m.ViewCount);
        _logger.LogDebug("Total views across {Count} videos: {Views}", latestPerVideo.Count, total);
        return total;
    }

    /// <summary>
    /// Returns the top performing videos by view count
    /// </summary>
    /// <param name="count">Number of top videos to return</param>
    /// <returns>Ordered list of video IDs and their latest view counts</returns>
    public async Task<List<(Guid VideoId, long Views)>> GetTopVideosAsync(int count = 10)
    {
        var metrics = await _analyticsRepository.GetAllAsync();

        return metrics
            .GroupBy(m => m.VideoId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(m => m.CollectedAt).First();
                return (g.Key, latest.ViewCount);
            })
            .OrderByDescending(x => x.ViewCount)
            .Take(count)
            .ToList();
    }
}
