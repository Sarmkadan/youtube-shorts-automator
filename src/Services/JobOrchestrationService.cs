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
/// Orchestrates the complete video processing and upload pipeline for YouTube Shorts automation.
/// Coordinates between video processing, upload scheduling, analytics tracking, and error recovery.
/// </summary>
public class JobOrchestrationService
{
	private readonly VideoProcessingService _processingService;
	private readonly YouTubeUploadService _uploadService;
	private readonly SchedulingService _schedulingService;
	private readonly AnalyticsService _analyticsService;
	private readonly VideoShortRepository _videoRepository;
	private readonly UploadJobRepository _uploadRepository;
	private readonly ILogger<JobOrchestrationService> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobOrchestrationService"/> class.
	/// </summary>
	/// <param name="processingService">The video processing service for handling video transformations and validations.</param>
	/// <param name="uploadService">The YouTube upload service for managing video uploads to YouTube.</param>
	/// <param name="schedulingService">The scheduling service for managing upload schedules.</param>
	/// <param name="analyticsService">The analytics service for tracking video performance metrics.</param>
	/// <param name="videoRepository">The repository for accessing video short data.</param>
	/// <param name="uploadRepository">The repository for managing upload job records.</param>
	/// <param name="logger">The logger for tracking pipeline operations and errors.</param>
	/// <exception cref="ArgumentNullException">Thrown when any required service is null.</exception>
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

	/// <summary>
	/// Processes the complete video pipeline from validation to scheduled upload.
	/// Orchestrates video processing, thumbnail generation, analytics creation, and upload scheduling.
	/// </summary>
	/// <param name="videoShortId">The unique identifier of the video short to process.</param>
	/// <param name="processingProfile">The processing profile containing enhancement settings and parameters.</param>
	/// <param name="channel">The YouTube channel where the video will be uploaded.</param>
	/// <param name="scheduledUploadTime">The scheduled upload time for the video.</param>
	/// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
	/// <returns>A <see cref="Pipeline"/> object containing the pipeline execution result and status.</returns>
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

	/// <summary>
	/// Processes all upload jobs that are ready to be uploaded.
	/// Retrieves scheduled jobs from the repository and attempts to upload each video.
	/// </summary>
	/// <param name="channel">The YouTube channel where videos will be uploaded.</param>
	/// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
	/// <returns>True if at least one upload was successful; otherwise false.</returns>
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

	/// <summary>
	/// Processes failed upload jobs that are eligible for retry.
	/// Retrieves jobs that haven't exceeded maximum retry attempts and attempts to upload them again.
	/// </summary>
	/// <param name="channel">The YouTube channel where videos will be uploaded.</param>
	/// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
	/// <returns>True if retry processing completed successfully; otherwise false.</returns>
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

	/// <summary>
	/// Synchronizes analytics data for all videos from YouTube.
	/// Retrieves all videos for the channel and syncs their analytics data from YouTube.
	/// </summary>
	/// <param name="channel">The YouTube channel whose analytics will be synced.</param>
	/// <param name="cancellationToken">The cancellation token for cooperative cancellation.</param>
	/// <returns>A <see cref="SyncResult"/> object containing sync statistics and status.</returns>
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

/// <summary>
/// Represents the result of a video processing pipeline execution.
/// Contains status information, processing results, and any errors encountered.
/// </summary>
public class Pipeline
{
	/// <summary>Gets or sets the unique identifier of the video short being processed.</summary>
	public int VideoShortId { get; set; }

	/// <summary>Gets or sets the unique identifier of the generated upload job.</summary>
	public int UploadJobId { get; set; }

	/// <summary>Gets or sets the pipeline execution status (Success, Failed, etc.).</summary>
	public string Status { get; set; } = string.Empty;

	/// <summary>Gets or sets the error message if the pipeline failed; otherwise null.</summary>
	public string? Error { get; set; }

	/// <summary>Gets or sets a value indicating whether video processing completed successfully.</summary>
	public bool ProcessingCompleted { get; set; }

	/// <summary>Gets or sets the scheduled upload time for the video.</summary>
	public DateTime ScheduledUploadTime { get; set; }

	/// <summary>Gets or sets the completion timestamp of the pipeline.</summary>
	public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Represents the result of an analytics synchronization operation.
/// Contains statistics about synced and failed videos during the sync process.
/// </summary>
public class SyncResult
{
	/// <summary>Gets or sets the name of the YouTube channel being synced.</summary>
	public string Channel { get; set; } = string.Empty;

	/// <summary>Gets or sets the number of videos successfully synced with analytics data.</summary>
	public int SyncedCount { get; set; }

	/// <summary>Gets or sets the number of videos that failed to sync analytics data.</summary>
	public int FailedCount { get; set; }

	/// <summary>Gets or sets the error message if the sync failed; otherwise null.</summary>
	public string? Error { get; set; }

	/// <summary>Gets or sets the completion timestamp of the analytics sync.</summary>
	public DateTime CompletedAt { get; set; }
}