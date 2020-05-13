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

/// <summary>
/// Provides functionality for uploading videos to YouTube.
/// </summary>
public class YouTubeUploadService
{
    private readonly UploadJobRepository _uploadRepository;
    private readonly UploadHistoryRepository _historyRepository;
    private readonly ILogger<YouTubeUploadService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="YouTubeUploadService"/> class.
    /// </summary>
    /// <param name="uploadRepository">The repository for upload jobs.</param>
    /// <param name="historyRepository">The repository for upload history.</param>
    /// <param name="logger">The logger for the service.</param>
    public YouTubeUploadService(
        UploadJobRepository uploadRepository,
        UploadHistoryRepository historyRepository,
        ILogger<YouTubeUploadService> logger)
    {
        _uploadRepository  = uploadRepository  ?? throw new ArgumentNullException(nameof(uploadRepository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new upload job for a video short.
    /// </summary>
    /// <param name="videoShortId">The ID of the video short.</param>
    /// <param name="scheduledTime">The scheduled time for the upload.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The created upload job.</returns>
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

    /// <summary>
    /// Uploads a video to YouTube.
    /// </summary>
    /// <param name="uploadJob">The upload job for the video.</param>
    /// <param name="filePath">The path to the video file.</param>
    /// <param name="channel">The YouTube channel to upload to.</param>
    /// <param name="videoShort">The video short being uploaded.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The updated upload job.</returns>
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

        // Skip files that have already been uploaded successfully
        var fileName = Path.GetFileName(filePath);
        if (await _historyRepository.HasSuccessfulUploadAsync(fileName, cancellationToken))
        {
            _logger.LogInformation($"Skipping already-uploaded file: {fileName}");
            await _historyRepository.AddAsync(new UploadHistoryEntry
            {
                VideoFileName = fileName,
                UploadedAt    = DateTime.UtcNow,
                Status        = UploadHistoryStatus.Skipped
            }, cancellationToken);

            uploadJob.MarkAsFailed("Skipped: file was already uploaded successfully");
            return uploadJob;
        }

        uploadJob.MarkAsQueued();

        try
        {
            uploadJob.MarkAsUploading();
            
            var fileInfo = new FileInfo(filePath);
            long totalBytes = fileInfo.Length;
            // Resume upload from last saved position instead of starting from 0
            long uploadedBytes = uploadJob.UploadedBytes;

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
            
            uploadJob.MarkAsCompleted(videoId);

            await _historyRepository.AddAsync(new UploadHistoryEntry
            {
                VideoFileName  = fileName,
                YouTubeVideoId = videoId,
                UploadedAt     = DateTime.UtcNow,
                Status         = UploadHistoryStatus.Success
            }, cancellationToken);
            
            _logger.LogInformation($"Successfully uploaded video {uploadJob.VideoShortId} as {videoId}");
            
            return uploadJob;
        }
        catch (Exception ex)
        {
            uploadJob.IncrementAttempt();
            bool isRetryable = uploadJob.CanRetry();
            uploadJob.MarkAsFailed($"Upload failed: {ex.Message}");

            await _historyRepository.AddAsync(new UploadHistoryEntry
            {
                VideoFileName = fileName,
                UploadedAt    = DateTime.UtcNow,
                Status        = UploadHistoryStatus.Failed,
                ErrorMessage  = ex.Message
            }, cancellationToken);
            
            _logger.LogError($"Error uploading video {uploadJob.VideoShortId}: {ex.Message}");
            
            throw new UploadException($"Video upload failed: {ex.Message}", 
                uploadJob.Id, uploadJob.VideoShortId, isRetryable);
        }
    }

    /// <summary>
    /// Retries a failed upload job.
    /// </summary>
    /// <param name="uploadJob">The upload job to retry.</param>
    /// <param name="filePath">The path to the video file.</param>
    /// <param name="channel">The YouTube channel to upload to.</param>
    /// <param name="videoShort">The video short being uploaded.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>True if the retry was successful, false otherwise.</returns>
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

    /// <summary>
    /// Updates video metadata on YouTube.
    /// </summary>
    /// <param name="videoId">The ID of the video to update.</param>
    /// <param name="title">The new title for the video.</param>
    /// <param name="description">The new description for the video.</param>
    /// <param name="tags">The new tags for the video.</param>
    /// <param name="channel">The YouTube channel that owns the video.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
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

    /// <summary>
    /// Publishes a video to the YouTube channel.
    /// </summary>
    /// <param name="videoId">The ID of the video to publish.</param>
    /// <param name="channel">The YouTube channel to publish to.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>True if the publish was successful, false otherwise.</returns>
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

    /// <summary>
    /// Retrieves an upload job by ID.
    /// </summary>
    /// <param name="jobId">The ID of the upload job to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The upload job, or null if not found.</returns>
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

    /// <summary>
    /// Retrieves all jobs scheduled for upload.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A collection of upload jobs scheduled for upload.</returns>
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

    /// <summary>
    /// Generates a mock YouTube video ID.
    /// </summary>
    /// <returns>A mock YouTube video ID.</returns>
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
