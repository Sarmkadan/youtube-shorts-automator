// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Integration;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// End-to-end content pipeline: ingests raw video, processes it, generates thumbnail, and uploads
/// </summary>
public class ContentPipeline
{
    private readonly ILogger<ContentPipeline> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly IFFmpegWrapper _ffmpegWrapper;
    private readonly ThumbnailGenerator _thumbnailGenerator;
    private readonly VideoUploader _videoUploader;

    /// <summary>
    /// Initializes a new instance of <see cref="ContentPipeline"/>
    /// </summary>
    public ContentPipeline(
        ILogger<ContentPipeline> logger,
        IVideoRepository videoRepository,
        IFFmpegWrapper ffmpegWrapper,
        ThumbnailGenerator thumbnailGenerator,
        VideoUploader videoUploader)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _ffmpegWrapper = ffmpegWrapper;
        _thumbnailGenerator = thumbnailGenerator;
        _videoUploader = videoUploader;
    }

    /// <summary>
    /// Runs the full pipeline for a single video: validate, process, thumbnail, upload
    /// </summary>
    /// <param name="videoId">The video to process</param>
    /// <param name="userId">The owning user</param>
    /// <param name="profile">FFmpeg encoding profile (hq, standard, mobile)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The upload result on success</returns>
    public async Task<UploadResult> RunAsync(
        Guid videoId,
        Guid userId,
        string profile = "standard",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting content pipeline for video {VideoId}", videoId);

        // Step 1: validate
        var errors = await _videoUploader.ValidateForUploadAsync(videoId);
        // Allow non-Processed status since we will process it now
        errors.RemoveAll(e => e.Contains("expected Processed"));

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Video {videoId} failed validation: {string.Join("; ", errors)}");

        var video = (await _videoRepository.GetByIdAsync(videoId))!;

        // Step 2: encode
        var processedPath = Path.Combine(
            Path.GetDirectoryName(video.FilePath) ?? ".",
            $"{Path.GetFileNameWithoutExtension(video.FilePath)}_processed.mp4");

        video.Status = VideoStatus.Processing;
        await _videoRepository.UpdateAsync(video);

        var encoded = await _ffmpegWrapper.ConvertVideoAsync(
            video.FilePath, processedPath, profile, cancellationToken);

        if (!encoded)
        {
            video.Status = VideoStatus.Error;
            await _videoRepository.UpdateAsync(video);
            throw new InvalidOperationException($"FFmpeg encoding failed for video {videoId}");
        }

        video.FilePath = processedPath;
        video.Status = VideoStatus.Processed;
        video.ProcessedAt = DateTime.UtcNow;
        await _videoRepository.UpdateAsync(video);

        // Step 3: thumbnail
        await _thumbnailGenerator.GenerateFromVideoAsync(videoId, secondsOffset: 1, cancellationToken);

        // Step 4: upload
        var result = await _videoUploader.UploadWithRetryAsync(videoId, userId);
        _logger.LogInformation("Content pipeline completed for video {VideoId}, YouTube URL: {Url}",
            videoId, result.YouTubeUrl);

        return result;
    }

    /// <summary>
    /// Runs the pipeline for multiple videos, returning per-video outcomes
    /// </summary>
    /// <param name="videoIds">Videos to process</param>
    /// <param name="userId">The owning user</param>
    /// <param name="profile">FFmpeg encoding profile</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Per-video results</returns>
    public async Task<List<PipelineResult>> RunBatchAsync(
        IEnumerable<Guid> videoIds,
        Guid userId,
        string profile = "standard",
        CancellationToken cancellationToken = default)
    {
        var results = new List<PipelineResult>();

        foreach (var videoId in videoIds)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var upload = await RunAsync(videoId, userId, profile, cancellationToken);
                results.Add(new PipelineResult(videoId, true, upload.YouTubeUrl, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline failed for video {VideoId}", videoId);
                results.Add(new PipelineResult(videoId, false, null, ex.Message));
            }
        }

        var ok = results.Count(r => r.Success);
        _logger.LogInformation("Batch pipeline: {Ok}/{Total} succeeded", ok, results.Count);
        return results;
    }

    /// <summary>
    /// Checks whether a video file can enter the pipeline (exists, correct format, within size limit)
    /// </summary>
    /// <param name="filePath">Path to the source video file</param>
    /// <returns>True if the file is eligible for processing</returns>
    public bool CanProcess(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var allowedExtensions = new HashSet<string> { ".mp4", ".mov", ".avi", ".mkv", ".webm" };

        if (!allowedExtensions.Contains(ext))
            return false;

        var size = new FileInfo(filePath).Length;
        return size is > 0 and <= 4_294_967_296; // 4GB
    }
}

/// <summary>
/// Result of a single video's pipeline execution
/// </summary>
/// <param name="VideoId">The video identifier</param>
/// <param name="Success">Whether the pipeline completed successfully</param>
/// <param name="YouTubeUrl">The resulting YouTube URL, if successful</param>
/// <param name="Error">Error message, if failed</param>
public record PipelineResult(Guid VideoId, bool Success, string? YouTubeUrl, string? Error);
