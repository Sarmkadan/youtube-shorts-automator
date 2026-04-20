// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

public class YouTubeUploadService
{
    private readonly UploadJobRepository _uploadRepository;
    private readonly ILogger<YouTubeUploadService> _logger;

    public YouTubeUploadService(UploadJobRepository uploadRepository, ILogger<YouTubeUploadService> logger)
    {
        _uploadRepository = uploadRepository ?? throw new ArgumentNullException(nameof(uploadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UploadJob> CreateUploadJobAsync(int videoShortId, DateTime scheduledTime,
        CancellationToken cancellationToken = default)
    {
        // Creates a new upload job for a video short
        var uploadJob = new UploadJob
        {
            VideoShortId = videoShortId,
            Status = UploadStatus.Pending,
            ScheduledAt = scheduledTime,
            AttemptCount = 0,
            MaxRetries = Constants.Constants.DEFAULT_RETRY_COUNT,
            UploadProgressPercentage = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!uploadJob.IsValid())
        {
            throw new ValidationException("Invalid upload job configuration");
        }

        try
        {
            var createdJob = await _uploadRepository.AddAsync(uploadJob, cancellationToken);
            _logger.LogInformation($"Created upload job {createdJob.Id} for video {videoShortId}");
            return createdJob;
        }
        catch (Exception ex)
        {
            throw new UploadException($"Failed to create upload job: {ex.Message}", 
                0, videoShortId, false);
        }
    }

    public async Task<UploadJob> UploadVideoAsync(UploadJob uploadJob, string filePath,
        YouTubeChannel channel, VideoShort videoShort, CancellationToken cancellationToken = default)
    {
        // Uploads a video to YouTube
        if (!File.Exists(filePath))
        {
            throw new UploadException($"Video file not found: {filePath}", 
                uploadJob.Id, uploadJob.VideoShortId, false);
        }

        if (channel.IsTokenExpired())
        {
            throw new YouTubeApiException("YouTube channel token has expired", 
                channel.Id, "token_expired", 401);
        }

        uploadJob.MarkAsQueued();

        try
        {
            uploadJob.MarkAsUploading();
            
            var fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            long uploadedBytes = 0;

            // Simulate upload with progress tracking
            _logger.LogInformation($"Starting upload of video {uploadJob.VideoShortId}");
            
            while (uploadedBytes < totalBytes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    uploadJob.MarkAsFailed("Upload cancelled by user");
                    return uploadJob;
                }

                // Simulate incremental upload
                long chunkSize = Math.Min(1024 * 1024, totalBytes - uploadedBytes);
                uploadedBytes += chunkSize;
                
                uploadJob.UpdateProgress(uploadedBytes, totalBytes);
                
                // Calculate estimated time remaining
                var progressRate = uploadedBytes / (double)totalBytes;
                if (progressRate > 0)
                {
                    var elapsedSeconds = 1; // Simulate time
                    var estimatedTotalSeconds = elapsedSeconds / progressRate;
                    var remainingSeconds = estimatedTotalSeconds - elapsedSeconds;
                    uploadJob.EstimatedTimeRemaining = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
                }

                await Task.Delay(100, cancellationToken);
            }

            // Generate mock video ID (in production, this comes from YouTube API)
            var videoId = GenerateYouTubeVideoId();
            var videoUrl = $"https://youtube.com/shorts/{videoId}";
            
            uploadJob.MarkAsCompleted(videoId);
            
            _logger.LogInformation($"Successfully uploaded video {uploadJob.VideoShortId} as {videoId}");
            
            return uploadJob;
        }
        catch (Exception ex)
        {
            uploadJob.IncrementAttempt();
            bool isRetryable = uploadJob.CanRetry();
            uploadJob.MarkAsFailed($"Upload failed: {ex.Message}");
            
            _logger.LogError($"Error uploading video {uploadJob.VideoShortId}: {ex.Message}");
            
            throw new UploadException($"Video upload failed: {ex.Message}", 
                uploadJob.Id, uploadJob.VideoShortId, isRetryable);
        }
    }

    public async Task<bool> RetryFailedUploadAsync(UploadJob uploadJob, string filePath,
        YouTubeChannel channel, VideoShort videoShort, CancellationToken cancellationToken = default)
    {
        // Retries a failed upload job
        if (!uploadJob.CanRetry())
        {
            _logger.LogWarning($"Upload job {uploadJob.Id} has exceeded max retry attempts");
            return false;
        }

        try
        {
            uploadJob.Status = UploadStatus.Retrying;
            uploadJob.UpdatedAt = DateTime.UtcNow;
            
            _logger.LogInformation($"Retrying upload for job {uploadJob.Id} (attempt {uploadJob.AttemptCount + 1})");
            
            var result = await UploadVideoAsync(uploadJob, filePath, channel, videoShort, cancellationToken);
            return result.Status == UploadStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Retry failed for job {uploadJob.Id}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateVideoMetadataAsync(string videoId, string title, string description,
        string[] tags, YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        // Updates video metadata on YouTube
        try
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length > Constants.Constants.MAX_TITLE_LENGTH)
            {
                throw new ValidationException("Invalid video title");
            }

            if (description.Length > Constants.Constants.MAX_DESCRIPTION_LENGTH)
            {
                throw new ValidationException("Description exceeds maximum length");
            }

            if (tags.Length > Constants.Constants.MAX_TAGS_COUNT)
            {
                throw new ValidationException("Too many tags provided");
            }

            _logger.LogInformation($"Updating metadata for YouTube video {videoId}");
            
            // In production, this would call the YouTube API
            await Task.Delay(500, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating video metadata: {ex.Message}");
            throw new YouTubeApiException($"Failed to update metadata: {ex.Message}");
        }
    }

    public async Task<bool> PublishVideoAsync(string videoId, YouTubeChannel channel,
        CancellationToken cancellationToken = default)
    {
        // Publishes a video to the YouTube channel
        try
        {
            if (channel.IsTokenExpired())
            {
                throw new YouTubeApiException("Channel token expired", channel.Id, "token_expired", 401);
            }

            _logger.LogInformation($"Publishing video {videoId} to channel {channel.ChannelName}");
            
            // Simulate YouTube API call
            await Task.Delay(1000, cancellationToken);
            
            _logger.LogInformation($"Video {videoId} published successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing video: {ex.Message}");
            throw new YouTubeApiException($"Failed to publish video: {ex.Message}");
        }
    }

    public async Task<UploadJob?> GetUploadJobAsync(int jobId, CancellationToken cancellationToken = default)
    {
        // Retrieves an upload job by ID
        try
        {
            return await _uploadRepository.GetByIdAsync(jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving upload job {jobId}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<UploadJob>> GetScheduledJobsAsync(CancellationToken cancellationToken = default)
    {
        // Retrieves all jobs scheduled for upload
        try
        {
            return await _uploadRepository.GetScheduledForUploadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving scheduled jobs: {ex.Message}");
            throw;
        }
    }

    private string GenerateYouTubeVideoId()
    {
        // Generates a mock YouTube video ID
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
        var random = new Random();
        return new string(Enumerable.Range(0, 11)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
