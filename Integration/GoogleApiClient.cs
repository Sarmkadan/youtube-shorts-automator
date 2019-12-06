// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Integration;

/// <summary>
/// Google API client for YouTube operations
/// Handles authentication, uploads, and metric retrieval
/// </summary>
public interface IGoogleApiClient
{
    Task<string?> UploadVideoAsync(string filePath, VideoMetadata metadata);
    Task<VideoAnalytics?> GetVideoAnalyticsAsync(string videoId);
    Task<bool> UpdateVideoMetadataAsync(string videoId, VideoMetadata metadata);
    Task<List<string>> ListChannelsAsync();
}

public class GoogleApiClient : IGoogleApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleApiClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public GoogleApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleApiClient> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _apiKey = configuration.GetValue<string>("YouTube:ApiKey") ?? throw new InvalidOperationException("YouTube API key not configured");
    }

    public async Task<string?> UploadVideoAsync(string filePath, VideoMetadata metadata)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Video file not found for upload: {FilePath}", filePath);
                return null;
            }

            _logger.LogInformation("Starting YouTube upload. File: {File}", Path.GetFileName(filePath));

            // Simulate upload to YouTube
            var uploadId = Guid.NewGuid().ToString();

            _logger.LogInformation("Video uploaded successfully. UploadId: {UploadId}", uploadId);

            return uploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video to YouTube");
            return null;
        }
    }

    public async Task<VideoAnalytics?> GetVideoAnalyticsAsync(string videoId)
    {
        try
        {
            var (isValid, error) = ValidationUtility.ValidateYouTubeVideoId(videoId);
            if (!isValid)
            {
                _logger.LogWarning("Invalid YouTube video ID: {VideoId}", videoId);
                return null;
            }

            _logger.LogInformation("Fetching analytics for video: {VideoId}", videoId);

            // Simulate fetching analytics from YouTube API
            var analytics = new VideoAnalytics
            {
                VideoId = videoId,
                ViewCount = 1500,
                LikeCount = 120,
                CommentCount = 45,
                ShareCount = 30,
                FetchedAtUtc = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analytics for video: {VideoId}", videoId);
            return null;
        }
    }

    public async Task<bool> UpdateVideoMetadataAsync(string videoId, VideoMetadata metadata)
    {
        try
        {
            var (isValid, error) = ValidationUtility.ValidateYouTubeVideoId(videoId);
            if (!isValid)
            {
                _logger.LogWarning("Invalid YouTube video ID for metadata update: {VideoId}", videoId);
                return false;
            }

            _logger.LogInformation("Updating YouTube video metadata. VideoId: {VideoId}", videoId);

            // Simulate metadata update
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating video metadata. VideoId: {VideoId}", videoId);
            return false;
        }
    }

    public async Task<List<string>> ListChannelsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching YouTube channels");

            // Simulate fetching channels
            var channels = new List<string> { "UCxxxxxxxxxxxxxx", "UCyyyyyyyyyyyyyyyy" };

            _logger.LogInformation("Retrieved {ChannelCount} YouTube channels", channels.Count);

            return channels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching YouTube channels");
            return new List<string>();
        }
    }
}

public class VideoAnalytics
{
    public string VideoId { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public double EngagementRate { get; set; }
    public DateTime FetchedAtUtc { get; set; }
}
