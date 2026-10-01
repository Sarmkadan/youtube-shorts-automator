// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Integration;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Orchestrates video upload workflows including validation, retry logic, and status tracking
/// </summary>
public class VideoUploader
{
    private readonly ILogger<VideoUploader> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly YouTubeUploadService _youTubeUploadService;
    private readonly IWebhookPublisher _webhookPublisher;
    private readonly IConfiguration _configuration;

    private readonly int _maxRetries;

    /// <summary>
    /// Initializes a new instance of <see cref="VideoUploader"/>
    /// </summary>
    public VideoUploader(
        ILogger<VideoUploader> logger,
        IVideoRepository videoRepository,
        YouTubeUploadService youTubeUploadService,
        IWebhookPublisher webhookPublisher,
        IConfiguration configuration)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _youTubeUploadService = youTubeUploadService;
        _webhookPublisher = webhookPublisher;
        _configuration = configuration;

        _maxRetries = configuration.GetValue("Upload:MaxRetries", 3);
    }

    /// <summary>
    /// Uploads a single video with retry logic and webhook notifications
    /// </summary>
    /// <param name="videoId">The video to upload</param>
    /// <param name="userId">The owning user</param>
    /// <returns>The upload result</returns>
    public async Task<UploadResult> UploadWithRetryAsync(Guid videoId, Guid userId)
    {
        _logger.LogInformation("Starting upload with retry for video {VideoId}", videoId);

        UploadResult? result = null;
        var attempt = 0;

        while (attempt < _maxRetries)
        {
            attempt++;
            try
            {
                result = await _youTubeUploadService.UploadVideoAsync(videoId, userId);
                if (result.Status == UploadStatus.Completed)
                {
                    _logger.LogInformation("Video {VideoId} uploaded on attempt {Attempt}", videoId, attempt);
                    await NotifyUploadCompleteAsync(videoId, result);
                    return result;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Upload attempt {Attempt}/{Max} failed for video {VideoId}",
                    attempt, _maxRetries, videoId);

                if (attempt >= _maxRetries)
                {
                    _logger.LogError("All {Max} upload attempts exhausted for video {VideoId}", _maxRetries, videoId);
                    throw;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }

        return result ?? throw new DomainException("Upload produced no result after all retries");
    }

    /// <summary>
    /// Uploads a batch of videos sequentially, collecting results
    /// </summary>
    /// <param name="videoIds">Videos to upload</param>
    /// <param name="userId">The owning user</param>
    /// <returns>Dictionary mapping video ID to its upload result or error message</returns>
    public async Task<Dictionary<Guid, (UploadResult? Result, string? Error)>> UploadBatchAsync(
        IEnumerable<Guid> videoIds, Guid userId)
    {
        var results = new Dictionary<Guid, (UploadResult?, string?)>();

        foreach (var videoId in videoIds)
        {
            try
            {
                var result = await UploadWithRetryAsync(videoId, userId);
                results[videoId] = (result, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch upload failed for video {VideoId}", videoId);
                results[videoId] = (null, ex.Message);
            }
        }

        var succeeded = results.Count(r => r.Value.Item1 is not null);
        _logger.LogInformation("Batch upload complete: {Succeeded}/{Total} succeeded", succeeded, results.Count);

        return results;
    }

    /// <summary>
    /// Validates that a video is ready for upload
    /// </summary>
    /// <param name="videoId">Video to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    public async Task<List<string>> ValidateForUploadAsync(Guid videoId)
    {
        var errors = new List<string>();
        var video = await _videoRepository.GetByIdAsync(videoId);

        if (video is null)
        {
            errors.Add($"Video {videoId} not found");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(video.Title))
            errors.Add("Video title is required");

        if (video.Title.Length > ApplicationConstants.Video.MaxTitleLength)
            errors.Add($"Title exceeds {ApplicationConstants.Video.MaxTitleLength} characters");

        if (video.FileSizeBytes > ApplicationConstants.Video.MaxFileSizeBytes)
            errors.Add($"File size exceeds {ApplicationConstants.Video.MaxFileSizeBytes / (1024 * 1024 * 1024)}GB limit");

        if (!File.Exists(video.FilePath))
            errors.Add($"Video file not found at {video.FilePath}");

        if (video.Status != VideoStatus.Processed)
            errors.Add($"Video status is {video.Status}, expected Processed");

        return errors;
    }

    private async Task NotifyUploadCompleteAsync(Guid videoId, UploadResult result)
    {
        var webhookUrl = _configuration.GetValue<string>("Upload:WebhookUrl");
        if (string.IsNullOrEmpty(webhookUrl)) return;

        await _webhookPublisher.PublishEventAsync("video.uploaded", new
        {
            VideoId = videoId,
            result.YouTubeUrl,
            result.UploadedAt
        }, webhookUrl);
    }
}
