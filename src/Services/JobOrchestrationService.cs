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

public class JobOrchestrationService
{
    private readonly VideoProcessingService _processingService;
    private readonly YouTubeUploadService _uploadService;
    private readonly SchedulingService _schedulingService;
    private readonly AnalyticsService _analyticsService;
    private readonly VideoShortRepository _videoRepository;
    private readonly UploadJobRepository _uploadRepository;
    private readonly ILogger<JobOrchestrationService> _logger;

    public JobOrchestrationService(
        VideoProcessingService processingService,
        YouTubeUploadService uploadService,
        SchedulingService schedulingService,
        AnalyticsService analyticsService,
        VideoShortRepository videoRepository,
        UploadJobRepository uploadRepository,
        ILogger<JobOrchestrationService> logger)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _uploadRepository = uploadRepository ?? throw new ArgumentNullException(nameof(uploadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Pipeline> ProcessFullPipelineAsync(int videoShortId,
        ProcessingProfile processingProfile, YouTubeChannel channel,
        DateTime scheduledUploadTime, CancellationToken cancellationToken = default)
    {
        // Orchestrates the complete pipeline from processing to scheduling
        var result = new Pipeline { VideoShortId = videoShortId };

        try
        {
            _logger.LogInformation($"Starting full pipeline for video {videoShortId}");

            // Step 1: Retrieve video short
            var videoShort = await _videoRepository.GetByIdAsync(videoShortId, cancellationToken);
            if (videoShort == null)
            {
                result.Status = "Failed";
                result.Error = "Video not found";
                return result;
            }

            // Step 2: Validate video file
            if (!await _processingService.ValidateVideoFileAsync(videoShort.FilePath, cancellationToken))
            {
                result.Status = "Failed";
                result.Error = "Video file validation failed";
                return result;
            }

            // Step 3: Process video
            _logger.LogInformation("Processing video with FFmpeg...");
            videoShort.MarkAsProcessing();
            await _videoRepository.UpdateAsync(videoShort, cancellationToken);

            var processingTask = await _processingService.ProcessVideoAsync(
                videoShort, processingProfile, cancellationToken);
            
            if (!processingTask.IsCompleted())
            {
                result.Status = "Failed";
                result.Error = "Video processing failed";
                return result;
            }

            // Step 4: Generate thumbnail
            var thumbnailPath = await _processingService.GenerateThumbnailAsync(
                videoShort.FilePath, 2.0, cancellationToken);
            videoShort.ThumbnailPath = thumbnailPath;

            // Step 5: Apply enhancements if configured
            if (processingProfile.ApplyWatermark && processingProfile.WatermarkPath != null)
            {
                await _processingService.ApplyWatermarkAsync(
                    videoShort.FilePath, videoShort.FilePath, processingProfile.WatermarkPath, cancellationToken);
            }

            if (processingProfile.ApplyColorGrading && processingProfile.ColorGradingProfile != null)
            {
                await _processingService.ApplyColorGradingAsync(
                    videoShort.FilePath, processingProfile.ColorGradingProfile, cancellationToken);
            }

            videoShort.MarkAsProcessed();
            await _videoRepository.UpdateAsync(videoShort, cancellationToken);
            result.ProcessingCompleted = true;

            // Step 6: Create analytics record
            _logger.LogInformation("Creating analytics record...");
            await _analyticsService.CreateAnalyticsRecordAsync(videoShortId, cancellationToken);

            // Step 7: Schedule upload
            _logger.LogInformation($"Scheduling upload for {scheduledUploadTime:F}...");
            var uploadJob = await _uploadService.CreateUploadJobAsync(videoShortId, scheduledUploadTime, cancellationToken);
            result.UploadJobId = uploadJob.Id;
            result.ScheduledUploadTime = scheduledUploadTime;

            result.Status = "Success";
            result.CompletedAt = DateTime.UtcNow;
            
            _logger.LogInformation($"Pipeline completed successfully for video {videoShortId}");
            return result;
        }
        catch (Exception ex)
        {
            result.Status = "Failed";
            result.Error = ex.Message;
            _logger.LogError($"Pipeline failed for video {videoShortId}: {ex.Message}");
            return result;
        }
    }

    public async Task<bool> ProcessReadyUploadsAsync(YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        // Processes all uploads that are ready to be uploaded
        try
        {
            var readyJobs = await _uploadRepository.GetScheduledForUploadAsync(cancellationToken);
            _logger.LogInformation($"Found {readyJobs.Count()} ready for upload");

            var successCount = 0;
            foreach (var job in readyJobs.Take(Constants.Constants.MAX_CONCURRENT_UPLOADS))
            {
                try
                {
                    var video = await _videoRepository.GetByIdAsync(job.VideoShortId, cancellationToken);
                    if (video == null) continue;

                    var uploadResult = await _uploadService.UploadVideoAsync(job, video.FilePath, channel, video, cancellationToken);
                    if (uploadResult.Status == UploadStatus.Completed)
                    {
                        successCount++;
                    }

                    await _uploadRepository.UpdateAsync(job, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error uploading job {job.Id}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Successfully uploaded {successCount} videos");
            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing uploads: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ProcessFailedRetriableJobsAsync(YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        // Retries failed upload jobs that haven't exceeded max retries
        try
        {
            var retriableJobs = await _uploadRepository.GetRetryableFailedJobsAsync(cancellationToken);
            _logger.LogInformation($"Found {retriableJobs.Count()} retriable failed jobs");

            foreach (var job in retriableJobs.Take(Constants.Constants.MAX_CONCURRENT_UPLOADS))
            {
                try
                {
                    var video = await _videoRepository.GetByIdAsync(job.VideoShortId, cancellationToken);
                    if (video == null) continue;

                    var success = await _uploadService.RetryFailedUploadAsync(job, video.FilePath, channel, video, cancellationToken);
                    if (success)
                    {
                        await _uploadRepository.UpdateAsync(job, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error retrying job {job.Id}: {ex.Message}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing retriable jobs: {ex.Message}");
            return false;
        }
    }

    public async Task<SyncResult> SyncAnalyticsAsync(YouTubeChannel channel, CancellationToken cancellationToken = default)
    {
        // Syncs analytics data for all videos from YouTube
        var result = new SyncResult { Channel = channel.ChannelName };

        try
        {
            var allVideos = await _videoRepository.GetByChannelAsync(channel.Id, cancellationToken);
            _logger.LogInformation($"Syncing analytics for {allVideos.Count()} videos");

            foreach (var video in allVideos)
            {
                try
                {
                    await _analyticsService.SyncAnalyticsFromYouTubeAsync(video.Id, "", channel, cancellationToken);
                    result.SyncedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to sync video {video.Id}: {ex.Message}");
                    result.FailedCount++;
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation($"Analytics sync completed: {result.SyncedCount} synced, {result.FailedCount} failed");
            return result;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            _logger.LogError($"Analytics sync failed: {ex.Message}");
            return result;
        }
    }
}

public class Pipeline
{
    public int VideoShortId { get; set; }
    public int UploadJobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public bool ProcessingCompleted { get; set; }
    public DateTime ScheduledUploadTime { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class SyncResult
{
    public string Channel { get; set; } = string.Empty;
    public int SyncedCount { get; set; }
    public int FailedCount { get; set; }
    public string? Error { get; set; }
    public DateTime CompletedAt { get; set; }
}
