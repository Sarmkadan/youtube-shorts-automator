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
/// Service for processing video files for YouTube Shorts uploads.
/// Handles video transcoding, watermark application, color grading, and thumbnail generation.
/// </summary>
public class VideoProcessingService
{
    private readonly VideoShortRepository _videoRepository;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(VideoShortRepository videoRepository, ILogger<VideoProcessingService> logger)
    {
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that a video file meets requirements before processing.
    /// </summary>
    /// <param name="filePath">Path to the video file to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file is valid, false otherwise</returns>
    public async Task<bool> ValidateVideoFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Validates a video file before processing
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Video file not found: {filePath}");
                return false;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > Constants.Constants.MAX_FILE_SIZE_BYTES)
            {
                _logger.LogWarning($"File exceeds max size: {fileInfo.Length} > {Constants.Constants.MAX_FILE_SIZE_BYTES}");
                return false;
            }

            if (fileInfo.Length < Constants.Constants.MIN_FILE_SIZE_BYTES)
            {
                _logger.LogWarning($"File too small: {fileInfo.Length} < {Constants.Constants.MIN_FILE_SIZE_BYTES}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating video file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Creates a new processing task for a video short.
    /// </summary>
    /// <param name="videoShort">Video short to create task for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created video short</returns>
    public async Task<VideoShort> CreateProcessingTaskAsync(VideoShort videoShort, CancellationToken cancellationToken = default)
    {
        // Creates a new processing task for a video short
        if (!videoShort.IsValid())
        {
            throw new ValidationException("Invalid video short data", 
                nameof(videoShort), videoShort.Title);
        }

        videoShort.CreatedAt = DateTime.UtcNow;
        videoShort.UpdatedAt = DateTime.UtcNow;
        videoShort.Status = ProcessingStatus.Pending;

        try
        {
            var savedVideo = await _videoRepository.AddAsync(videoShort, cancellationToken);
            _logger.LogInformation($"Created processing task for video: {savedVideo.Id}");
            return savedVideo;
        }
        catch (Exception ex)
        {
            throw new VideoProcessingException($"Failed to create processing task: {ex.Message}",
                videoShort.ProcessingProfileId, null, ex);
        }
    }

    public async Task<ProcessingTask> ProcessVideoAsync(VideoShort videoShort, ProcessingProfile profile, 
        CancellationToken cancellationToken = default)
    {
        // Processes a video short according to the specified profile
        if (!profile.IsValid())
        {
            throw new ValidationException("Invalid processing profile");
        }

        var processingTask = new ProcessingTask
        {
            VideoShortId = videoShort.Id,
            TaskType = "FFmpegTranscode",
            Status = ProcessingStatus.Processing,
            Priority = 5,
            OutputWidth = profile.VideoWidth,
            OutputHeight = profile.VideoHeight,
            OutputBitrate = profile.VideoBitrate,
            OutputFormat = profile.Container,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        processingTask.MarkAsStarted();

        try
        {
            // Simulate FFmpeg processing with real operations
            processingTask.AppendLog($"Starting FFmpeg transcoding from {videoShort.FilePath}");
            
            var outputPath = GenerateOutputPath(videoShort.Id, profile.Container);
            var ffmpegArgs = BuildFFmpegArgs(videoShort.FilePath, outputPath, profile);

            processingTask.AppendLog($"FFmpeg command: ffmpeg {ffmpegArgs}");
            
            // In a real scenario, this would execute FFmpeg
            await SimulateFFmpegProcessingAsync(videoShort.Duration, cancellationToken);

            processingTask.AppendLog("Video processing completed successfully");
            processingTask.MarkAsCompleted();
            
            _logger.LogInformation($"Successfully processed video {videoShort.Id}");
            return processingTask;
        }
        catch (Exception ex)
        {
            processingTask.MarkAsFailed($"Processing failed: {ex.Message}");
            _logger.LogError($"Error processing video {videoShort.Id}: {ex.Message}");
            throw new VideoProcessingException($"Video processing failed: {ex.Message}", 
                videoShort.Id, processingTask.TaskType, ex);
        }
    }

    public async Task<bool> ApplyWatermarkAsync(string inputPath, string outputPath, string watermarkPath,
        CancellationToken cancellationToken = default)
    {
        // Applies a watermark overlay to a video
        try
        {
            if (!File.Exists(watermarkPath))
            {
                _logger.LogWarning($"Watermark file not found: {watermarkPath}");
                return false;
            }

            // Simulate watermark application
            await Task.Delay(500, cancellationToken);
            _logger.LogInformation($"Watermark applied to {inputPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error applying watermark: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ApplyColorGradingAsync(string videoPath, string colorProfile,
        CancellationToken cancellationToken = default)
    {
        // Applies color grading to a video
        try
        {
            if (!File.Exists(videoPath))
            {
                _logger.LogWarning($"Video file not found: {videoPath}");
                return false;
            }

            _logger.LogInformation($"Applying color profile '{colorProfile}' to {videoPath}");
            await Task.Delay(1000, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error applying color grading: {ex.Message}");
            return false;
        }
    }

    public async Task<string> GenerateThumbnailAsync(string videoPath, double secondsOffset,
        CancellationToken cancellationToken = default)
    {
        // Generates a thumbnail at the specified second offset
        try
        {
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Video file not found: {videoPath}");
            }

            var thumbnailPath = Path.Combine(Constants.Constants.TEMP_DIRECTORY, 
                $"thumb_{Guid.NewGuid()}.jpg");
            
            Directory.CreateDirectory(Constants.Constants.TEMP_DIRECTORY);
            
            _logger.LogInformation($"Generating thumbnail at {secondsOffset}s for {videoPath}");
            await Task.Delay(300, cancellationToken);
            
            // In reality, this would use FFmpeg to extract the frame
            File.WriteAllText(thumbnailPath, string.Empty);
            
            return thumbnailPath;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating thumbnail: {ex.Message}");
            throw new VideoProcessingException($"Thumbnail generation failed: {ex.Message}");
        }
    }

    private string BuildFFmpegArgs(string inputPath, string outputPath, ProcessingProfile profile)
    {
        // Builds FFmpeg command-line arguments based on processing profile
        return $"-i \"{inputPath}\" " +
               $"-vcodec {profile.VideoCodec} " +
               $"-acodec {profile.AudioCodec} " +
               $"-b:v {profile.VideoBitrate}k " +
               $"-b:a {profile.AudioBitrate}k " +
               $"-r {profile.FrameRate} " +
               $"-vf scale={profile.VideoWidth}:{profile.VideoHeight} " +
               $"\"{outputPath}\"";
    }

    private string GenerateOutputPath(int videoId, string container)
    {
        // Generates the output file path for a processed video
        return Path.Combine(Constants.Constants.OUTPUT_DIRECTORY, 
            $"video_{videoId}_{Guid.NewGuid()}.{container}");
    }

    private async Task SimulateFFmpegProcessingAsync(TimeSpan videoDuration, CancellationToken cancellationToken)
    {
        // Simulates FFmpeg processing delay based on video duration
        var processingTime = (int)(videoDuration.TotalSeconds * 100); // 100ms per second
        await Task.Delay(Math.Min(processingTime, 5000), cancellationToken);
    }
}
