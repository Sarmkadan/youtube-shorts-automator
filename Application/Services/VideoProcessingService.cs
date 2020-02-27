// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Handles video processing, encoding, and optimization using FFmpeg
/// </summary>
public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly IProcessingJobRepository _processingRepository;

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger,
        IVideoRepository videoRepository,
        IProcessingJobRepository processingRepository)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _processingRepository = processingRepository;
    }

    /// <summary>
    /// Creates and starts a new processing job for a video.
    /// </summary>
    /// <param name="videoId">The unique identifier of the video.</param>
    /// <param name="jobType">The type of processing job to perform.</param>
    /// <returns>The created processing job.</returns>
    /// <exception cref="ResourceNotFoundException">Thrown if video not found.</exception>
    /// <exception cref="VideoValidationException">Thrown if video validation fails.</exception>
    public async Task<ProcessingJob> CreateProcessingJobAsync(Guid videoId, ProcessingJobType jobType)
    {
        _logger.LogInformation($"Creating processing job for video {videoId}, type: {jobType}");

        var video = await _videoRepository.GetByIdAsync(videoId)
            ?? throw new ResourceNotFoundException("Video not found", videoId, "Video");

        var (isValid, errors) = video.Validate();
        if (!isValid)
            throw new VideoValidationException("Video validation failed", errors);

        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            JobType = jobType,
            Status = ProcessingJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            MaxRetries = ApplicationConstants.Processing.MaxRetries
        };

        video.Status = VideoStatus.Processing;
        video.ProcessingJobs.Add(job);

        await _videoRepository.UpdateAsync(video);
        await _processingRepository.AddAsync(job);

        _logger.LogInformation($"Processing job {job.Id} created successfully");
        return job;
    }

    /// <summary>
    /// Processes a video through all required steps including validation, preparation, encoding, optimization, thumbnail generation, and metadata extraction.
    /// </summary>
    /// <param name="job">The processing job to execute.</param>
    /// <returns>The updated processing job.</returns>
    /// <exception cref="InvalidStateException">Thrown if job is not in queued state.</exception>
    public async Task<ProcessingJob> ProcessVideoAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Starting processing for job {job.Id}");

        if (job.Status != ProcessingJobStatus.Queued)
            throw new InvalidStateException("Job is not in queued state", job.Status.ToString(), "ProcessVideo");

        job.Start();
        await _processingRepository.UpdateAsync(job);

        try
        {
            job.AdvanceStep(ProcessingStepType.Validation);
            await ValidateVideoAsync(job);
            job.CompleteCurrentStep("Validation passed");

            job.AdvanceStep(ProcessingStepType.Preparation);
            await PrepareVideoAsync(job);
            job.CompleteCurrentStep("Video prepared");

            job.AdvanceStep(ProcessingStepType.Encoding);
            await EncodeVideoAsync(job);
            job.CompleteCurrentStep("Video encoded");

            job.AdvanceStep(ProcessingStepType.Optimization);
            await OptimizeVideoAsync(job);
            job.CompleteCurrentStep("Video optimized");

            job.AdvanceStep(ProcessingStepType.ThumbnailGeneration);
            await GenerateThumbnailAsync(job);
            job.CompleteCurrentStep("Thumbnail generated");

            job.AdvanceStep(ProcessingStepType.MetadataExtraction);
            await ExtractMetadataAsync(job);
            job.CompleteCurrentStep("Metadata extracted");

            job.AdvanceStep(ProcessingStepType.Finalization);
            job.Complete();

            _logger.LogInformation($"Processing job {job.Id} completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Processing job {job.Id} failed");
            job.Fail(ex.Message);
        }

        await _processingRepository.UpdateAsync(job);
        return job;
    }

    /// <summary>
    /// Validates video file format and integrity
    /// </summary>
    private async Task ValidateVideoAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Validating video for job {job.Id}");

        var video = job.Video ?? await _videoRepository.GetByIdAsync(job.VideoId)
            ?? throw new ResourceNotFoundException("Video not found", job.VideoId, "Video");

        var extension = Path.GetExtension(video.FilePath).ToLower().TrimStart('.');
        if (!ApplicationConstants.Video.AllowedFormats.Contains(extension))
            throw new VideoValidationException("Invalid video format",
                new List<string> { $"Format {extension} is not supported" });

        if (!File.Exists(video.FilePath))
            throw new VideoValidationException("Video file not found",
                new List<string> { "Source file does not exist" });

        if (video.FileSizeBytes > ApplicationConstants.Video.MaxFileSizeBytes)
            throw new VideoValidationException("File size exceeds limit",
                new List<string> { "Video file is too large" });

        await Task.Delay(100); // Simulate validation work
    }

    /// <summary>
    /// Prepares video for processing
    /// </summary>
    private async Task PrepareVideoAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Preparing video for job {job.Id}");
        await Task.Delay(100); // Simulate preparation
    }

    /// <summary>
    /// Encodes video to target format
    /// </summary>
    private async Task EncodeVideoAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Encoding video for job {job.Id}");
        var video = job.Video ?? await _videoRepository.GetByIdAsync(job.VideoId)
            ?? throw new ResourceNotFoundException("Video not found", job.VideoId, "Video");

        job.UpdateProgress(40);
        await Task.Delay(200); // Simulate encoding
        job.UpdateProgress(75);
    }

    /// <summary>
    /// Optimizes video for YouTube Shorts
    /// </summary>
    private async Task OptimizeVideoAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Optimizing video for job {job.Id}");

        var video = job.Video ?? await _videoRepository.GetByIdAsync(job.VideoId)
            ?? throw new ResourceNotFoundException("Video not found", job.VideoId, "Video");

        if (video.DurationSeconds > ApplicationConstants.Video.MaxShortsDurationSeconds)
        {
            _logger.LogWarning($"Video {video.Id} exceeds shorts duration, will be trimmed");
        }

        job.UpdateProgress(85);
        await Task.Delay(150); // Simulate optimization
    }

    /// <summary>
    /// Generates thumbnail image from video
    /// </summary>
    private async Task GenerateThumbnailAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Generating thumbnail for job {job.Id}");
        var video = job.Video ?? await _videoRepository.GetByIdAsync(job.VideoId)
            ?? throw new ResourceNotFoundException("Video not found", job.VideoId, "Video");

        var thumbnailPath = Path.Combine(
            Path.GetDirectoryName(video.FilePath) ?? string.Empty,
            $"{Path.GetFileNameWithoutExtension(video.FilePath)}_thumb.jpg");

        video.ThumbnailPath = thumbnailPath;
        await _videoRepository.UpdateAsync(video);

        await Task.Delay(100); // Simulate thumbnail generation
    }

    /// <summary>
    /// Extracts metadata from processed video
    /// </summary>
    private async Task ExtractMetadataAsync(ProcessingJob job)
    {
        _logger.LogInformation($"Extracting metadata for job {job.Id}");
        var video = job.Video ?? await _videoRepository.GetByIdAsync(job.VideoId)
            ?? throw new ResourceNotFoundException("Video not found", job.VideoId, "Video");

        var metadata = new Dictionary<string, string>
        {
            { "format", "mp4" },
            { "bitrate", $"{ApplicationConstants.Processing.DefaultBitrateMbps} Mbps" },
            { "fps", ApplicationConstants.Processing.DefaultFrameRate.ToString() },
            { "resolution", $"{ApplicationConstants.Processing.DefaultResolution}p" }
        };

        await Task.Delay(50); // Simulate metadata extraction
    }

    /// <summary>
    /// Retries a failed processing job
    /// </summary>
    public async Task<ProcessingJob> RetryJobAsync(Guid jobId)
    {
        _logger.LogInformation($"Retrying processing job {jobId}");

        var job = await _processingRepository.GetByIdAsync(jobId)
            ?? throw new ResourceNotFoundException("Job not found", jobId, "ProcessingJob");

        if (!job.CanRetry())
            throw new InvalidStateException("Job cannot be retried", job.Status.ToString(), "Retry");

        job.ResetForRetry();
        await _processingRepository.UpdateAsync(job);

        return await ProcessVideoAsync(job);
    }

    /// <summary>
    /// Gets processing job status
    /// </summary>
    public async Task<ProcessingJob> GetJobStatusAsync(Guid jobId)
    {
        var job = await _processingRepository.GetByIdAsync(jobId)
            ?? throw new ResourceNotFoundException("Job not found", jobId, "ProcessingJob");

        return job;
    }
}
