// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Handles YouTube API interactions and video uploads
/// </summary>
public class YouTubeUploadService
{
    private readonly ILogger<YouTubeUploadService> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly IApiCredentialService _credentialService;

    public YouTubeUploadService(
        ILogger<YouTubeUploadService> logger,
        IVideoRepository videoRepository,
        IApiCredentialService credentialService)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _credentialService = credentialService;
    }

    /// <summary>
    /// Uploads a processed video to YouTube
    /// </summary>
    public async Task<UploadResult> UploadVideoAsync(Guid videoId, Guid userId)
    {
        _logger.LogInformation($"Starting YouTube upload for video {videoId}");

        var video = await _videoRepository.GetByIdAsync(videoId)
            ?? throw new ResourceNotFoundException("Video not found", videoId, "Video");

        if (video.Status != VideoStatus.Processed && video.Status != VideoStatus.Processing)
            throw new InvalidStateException(
                "Video must be processed before upload",
                video.Status.ToString(),
                "Upload");

        var credential = await _credentialService.GetActiveCredentialAsync(userId)
            ?? throw new CredentialException("No valid YouTube credentials found");

        if (!credential.IsValid)
            throw new CredentialException("Credentials are invalid or expired");

        var uploadResult = new UploadResult
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            Status = UploadStatus.InProgress,
            UploadedAt = DateTime.UtcNow
        };

        try
        {
            var youtubeVideoId = await UploadToYouTubeAsync(video, credential);
            var youtubeUrl = $"https://www.youtube.com/watch?v={youtubeVideoId}";

            uploadResult.MarkAsCompleted(youtubeUrl);
            uploadResult.YouTubeVideoId = youtubeVideoId;

            video.MarkAsUploaded(youtubeVideoId);
            video.UploadResult = uploadResult;

            await _videoRepository.UpdateAsync(video);

            _logger.LogInformation($"Video {videoId} uploaded successfully. YouTube ID: {youtubeVideoId}");

            return uploadResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"YouTube upload failed for video {videoId}");
            uploadResult.RecordFailure(ex.Message);
            throw new UploadException($"Upload failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes an uploaded video
    /// </summary>
    public async Task<bool> PublishVideoAsync(Guid uploadResultId)
    {
        _logger.LogInformation($"Publishing video with upload result {uploadResultId}");

        try
        {
            // Simulate YouTube API call to publish
            await Task.Delay(100);

            _logger.LogInformation($"Video published successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish video");
            throw new UploadException($"Publishing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates video metadata on YouTube
    /// </summary>
    public async Task<bool> UpdateMetadataAsync(string youtubeVideoId, string title, string description, string[] tags)
    {
        _logger.LogInformation($"Updating metadata for YouTube video {youtubeVideoId}");

        if (title.Length > ApplicationConstants.Video.MaxTitleLength)
            throw new VideoValidationException("Title too long",
                new List<string> { $"Title must not exceed {ApplicationConstants.Video.MaxTitleLength} characters" });

        if (description.Length > ApplicationConstants.Video.MaxDescriptionLength)
            throw new VideoValidationException("Description too long",
                new List<string> { $"Description must not exceed {ApplicationConstants.Video.MaxDescriptionLength} characters" });

        try
        {
            // Simulate YouTube API metadata update
            await Task.Delay(50);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update metadata");
            throw new ApiException("Metadata update failed", 500);
        }
    }

    /// <summary>
    /// Adds tags to a YouTube video
    /// </summary>
    public async Task<bool> AddTagsAsync(string youtubeVideoId, string[] tags)
    {
        _logger.LogInformation($"Adding {tags.Length} tags to YouTube video {youtubeVideoId}");

        if (tags.Length > ApplicationConstants.Video.MaxTagsCount)
            throw new VideoValidationException("Too many tags",
                new List<string> { $"Cannot have more than {ApplicationConstants.Video.MaxTagsCount} tags" });

        if (tags.Any(t => t.Length > ApplicationConstants.Video.MaxTagLength))
            throw new VideoValidationException("Tag too long",
                new List<string> { $"Tags cannot exceed {ApplicationConstants.Video.MaxTagLength} characters" });

        try
        {
            await Task.Delay(50);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add tags");
            throw new ApiException("Tag addition failed", 500);
        }
    }

    /// <summary>
    /// Sets video thumbnail on YouTube
    /// </summary>
    public async Task<bool> SetThumbnailAsync(string youtubeVideoId, string thumbnailPath)
    {
        _logger.LogInformation($"Setting thumbnail for YouTube video {youtubeVideoId}");

        if (!File.Exists(thumbnailPath))
            throw new DomainException("Thumbnail file not found");

        try
        {
            // Simulate YouTube API thumbnail upload
            await Task.Delay(75);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set thumbnail");
            throw new ApiException("Thumbnail upload failed", 500);
        }
    }

    /// <summary>
    /// Gets upload status from YouTube
    /// </summary>
    public async Task<UploadResult> GetUploadStatusAsync(string youtubeVideoId)
    {
        _logger.LogInformation($"Checking upload status for YouTube video {youtubeVideoId}");

        try
        {
            // Simulate YouTube API status check
            var status = new UploadResult
            {
                YouTubeVideoId = youtubeVideoId,
                Status = UploadStatus.Published,
                ProcessingStatus = "Done"
            };

            await Task.Delay(50);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get upload status");
            throw new ApiException("Status check failed", 500);
        }
    }

    /// <summary>
    /// Cancels an in-progress upload
    /// </summary>
    public async Task<bool> CancelUploadAsync(string youtubeVideoId)
    {
        _logger.LogInformation($"Cancelling upload for YouTube video {youtubeVideoId}");

        try
        {
            // Simulate upload cancellation
            await Task.Delay(50);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel upload");
            throw new UploadException("Cancellation failed");
        }
    }

    /// <summary>
    /// Performs the actual YouTube upload
    /// </summary>
    private async Task<string> UploadToYouTubeAsync(Video video, ApiCredential credential)
    {
        _logger.LogInformation($"Executing YouTube upload for video {video.Id}");

        if (!File.Exists(video.FilePath))
            throw new DomainException("Video file not found");

        var fileInfo = new FileInfo(video.FilePath);
        var uploadStartTime = DateTime.UtcNow;

        try
        {
            var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / ApplicationConstants.Upload.ChunkSizeBytes);
            _logger.LogInformation($"Uploading {totalChunks} chunks for video {video.Id}");

            // Simulate chunked upload
            for (int i = 0; i < totalChunks; i++)
            {
                var progress = ((i + 1) / (double)totalChunks) * 100;
                _logger.LogDebug($"Upload progress: {progress:F1}%");
                await Task.Delay(100); // Simulate chunk upload
            }

            var uploadDuration = DateTime.UtcNow - uploadStartTime;
            _logger.LogInformation($"Upload completed in {uploadDuration.TotalSeconds:F1} seconds");

            // Generate mock YouTube ID
            var youtubeVideoId = $"yt_{Guid.NewGuid().ToString().Substring(0, 8)}";
            return youtubeVideoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Upload failed for video {video.Id}");
            throw;
        }
    }
}
