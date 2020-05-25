// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Extension methods for <see cref="VideoProcessingService"/> that provide additional video processing utilities
/// and convenience methods for common video processing scenarios.
/// </summary>
public static class VideoProcessingServiceExtensions
{
    /// <summary>
    /// Validates a video file and creates a processing task in a single operation.
    /// </summary>
    /// <param name="service">The video processing service instance</param>
    /// <param name="filePath">Path to the video file to validate and process</param>
    /// <param name="videoShort">Video short data to create processing task for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created video short if validation passes, null otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when service, filePath, or videoShort is null</exception>
    /// <exception cref="ValidationException">Thrown when videoShort data is invalid</exception>
    public static async Task<VideoShort?> ValidateAndCreateTaskAsync(this VideoProcessingService service,
        string filePath, VideoShort videoShort, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(videoShort);

        if (!await service.ValidateVideoFileAsync(filePath, cancellationToken))
        {
            return null;
        }

        return await service.CreateProcessingTaskAsync(videoShort, cancellationToken);
    }

    /// <summary>
    /// Processes a video with automatic watermark and color grading based on the video short's profile.
    /// </summary>
    /// <param name="service">The video processing service instance</param>
    /// <param name="videoShort">Video short to process</param>
    /// <param name="profile">Processing profile to use</param>
    /// <param name="watermarkPath">Path to watermark image file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing task result</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="VideoProcessingException">Thrown when processing fails</exception>
    public static async Task<ProcessingTask> ProcessWithWatermarkAndColorAsync(this VideoProcessingService service,
        VideoShort videoShort, ProcessingProfile profile, string watermarkPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(videoShort);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(watermarkPath);

        // Process video
        var processingTask = await service.ProcessVideoAsync(videoShort, profile, cancellationToken);

        // Apply watermark
        var watermarkResult = await service.ApplyWatermarkAsync(
            videoShort.FilePath,
            videoShort.FilePath, // Use original path as output for simulation
            watermarkPath,
            cancellationToken);

        if (!watermarkResult)
        {
            throw new VideoProcessingException("Watermark application failed",
                videoShort.Id, "WatermarkApplication", null);
        }

        // Apply color grading
        var colorResult = await service.ApplyColorGradingAsync(
            videoShort.FilePath,
            profile.ColorGradingProfile,
            cancellationToken);

        if (!colorResult)
        {
            throw new VideoProcessingException("Color grading failed",
                videoShort.Id, "ColorGrading", null);
        }

        return processingTask;
    }

    /// <summary>
    /// Generates multiple thumbnails at specified time offsets for a video.
    /// </summary>
    /// <param name="service">The video processing service instance</param>
    /// <param name="videoPath">Path to the video file</param>
    /// <param name="thumbnailOffsets">Collection of time offsets in seconds for thumbnail generation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Read-only collection of generated thumbnail file paths</returns>
    /// <exception cref="ArgumentNullException">Thrown when service, videoPath, or thumbnailOffsets is null</exception>
    /// <exception cref="ArgumentException">Thrown when thumbnailOffsets is empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when video file doesn't exist</exception>
    public static async Task<IReadOnlyList<string>> GenerateThumbnailsAtOffsetsAsync(
        this VideoProcessingService service,
        string videoPath,
        IEnumerable<double> thumbnailOffsets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(videoPath);
        ArgumentNullException.ThrowIfNull(thumbnailOffsets);

        var offsets = thumbnailOffsets.ToList();
        if (offsets.Count == 0)
        {
            throw new ArgumentException("At least one thumbnail offset must be specified", nameof(thumbnailOffsets));
        }

        var thumbnails = new List<string>(offsets.Count);

        foreach (var offset in offsets)
        {
            var thumbnailPath = await service.GenerateThumbnailAsync(videoPath, offset, cancellationToken);
            thumbnails.Add(thumbnailPath);
        }

        return thumbnails.AsReadOnly();
    }

    /// <summary>
    /// Validates video file, creates processing task, and generates thumbnail in a single operation.
    /// </summary>
    /// <param name="service">The video processing service instance</param>
    /// <param name="filePath">Path to the video file to validate</param>
    /// <param name="videoShort">Video short data to create processing task for</param>
    /// <param name="thumbnailOffset">Time offset in seconds for thumbnail generation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing video short, processing task, and thumbnail path if successful</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="ValidationException">Thrown when videoShort data is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when video file doesn't exist</exception>
    public static async Task<(VideoShort VideoShort, ProcessingTask ProcessingTask, string ThumbnailPath)?>
        ValidateCreateAndThumbnailAsync(this VideoProcessingService service,
        string filePath, VideoShort videoShort, double thumbnailOffset = 5.0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(videoShort);

        if (!await service.ValidateVideoFileAsync(filePath, cancellationToken))
        {
            return null;
        }

        var createdVideo = await service.CreateProcessingTaskAsync(videoShort, cancellationToken);
        var processingTask = await service.ProcessVideoAsync(videoShort, videoShort.ProcessingProfile, cancellationToken);
        var thumbnailPath = await service.GenerateThumbnailAsync(filePath, thumbnailOffset, cancellationToken);

        return (createdVideo, processingTask, thumbnailPath);
    }
}