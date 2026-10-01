// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Integration;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Generates and manages video thumbnails using FFmpeg extraction and image processing
/// </summary>
public class ThumbnailGenerator
{
    private readonly ILogger<ThumbnailGenerator> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly IFFmpegWrapper _ffmpegWrapper;
    private readonly IConfiguration _configuration;

    private readonly string _thumbnailOutputDir;
    private readonly int _defaultWidth;
    private readonly int _defaultHeight;

    /// <summary>
    /// Initializes a new instance of <see cref="ThumbnailGenerator"/>
    /// </summary>
    public ThumbnailGenerator(
        ILogger<ThumbnailGenerator> logger,
        IVideoRepository videoRepository,
        IFFmpegWrapper ffmpegWrapper,
        IConfiguration configuration)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _ffmpegWrapper = ffmpegWrapper;
        _configuration = configuration;

        _thumbnailOutputDir = configuration.GetValue<string>("Thumbnails:OutputDirectory") ?? "thumbnails";
        _defaultWidth = configuration.GetValue("Thumbnails:Width", 1280);
        _defaultHeight = configuration.GetValue("Thumbnails:Height", 720);
    }

    /// <summary>
    /// Extracts a thumbnail from a video at the specified timestamp offset
    /// </summary>
    /// <param name="videoId">The video identifier</param>
    /// <param name="secondsOffset">Seconds into the video to capture the frame</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file path of the generated thumbnail, or null on failure</returns>
    public async Task<string?> GenerateFromVideoAsync(
        Guid videoId,
        int secondsOffset = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating thumbnail for video {VideoId} at offset {Offset}s", videoId, secondsOffset);

        var video = await _videoRepository.GetByIdAsync(videoId);
        if (video is null)
        {
            _logger.LogWarning("Video {VideoId} not found", videoId);
            return null;
        }

        if (!File.Exists(video.FilePath))
        {
            _logger.LogError("Video file not found at {FilePath}", video.FilePath);
            return null;
        }

        Directory.CreateDirectory(_thumbnailOutputDir);
        var outputPath = Path.Combine(_thumbnailOutputDir, $"{videoId}.jpg");

        var success = await _ffmpegWrapper.ExtractThumbnailAsync(
            video.FilePath, outputPath, secondsOffset, cancellationToken);

        if (!success)
        {
            _logger.LogError("FFmpeg thumbnail extraction failed for video {VideoId}", videoId);
            return null;
        }

        video.ThumbnailPath = outputPath;
        await _videoRepository.UpdateAsync(video);

        _logger.LogInformation("Thumbnail saved to {Path} for video {VideoId}", outputPath, videoId);
        return outputPath;
    }

    /// <summary>
    /// Generates thumbnails for all videos that do not yet have one
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of thumbnails successfully generated</returns>
    public async Task<int> GenerateMissingThumbnailsAsync(CancellationToken cancellationToken = default)
    {
        var videos = await _videoRepository.GetAllAsync();
        var missing = videos.Where(v => string.IsNullOrEmpty(v.ThumbnailPath)).ToList();

        _logger.LogInformation("Found {Count} videos without thumbnails", missing.Count);
        var generated = 0;

        foreach (var video in missing)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var result = await GenerateFromVideoAsync(video.Id, secondsOffset: 1, cancellationToken);
            if (result is not null) generated++;
        }

        _logger.LogInformation("Generated {Generated}/{Total} missing thumbnails", generated, missing.Count);
        return generated;
    }

    /// <summary>
    /// Validates that a thumbnail file exists and meets minimum size requirements
    /// </summary>
    /// <param name="thumbnailPath">Path to the thumbnail file</param>
    /// <returns>True if the thumbnail is valid</returns>
    public bool ValidateThumbnail(string thumbnailPath)
    {
        if (!File.Exists(thumbnailPath))
        {
            _logger.LogWarning("Thumbnail file does not exist: {Path}", thumbnailPath);
            return false;
        }

        var fileInfo = new FileInfo(thumbnailPath);
        const long minSizeBytes = 1024; // 1 KB minimum

        if (fileInfo.Length < minSizeBytes)
        {
            _logger.LogWarning("Thumbnail {Path} is too small ({Size} bytes)", thumbnailPath, fileInfo.Length);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes orphaned thumbnail files that no longer have a matching video record
    /// </summary>
    /// <returns>Number of orphaned files removed</returns>
    public async Task<int> CleanupOrphanedThumbnailsAsync()
    {
        if (!Directory.Exists(_thumbnailOutputDir))
            return 0;

        var videos = await _videoRepository.GetAllAsync();
        var knownIds = new HashSet<string>(videos.Select(v => v.Id.ToString()));

        var files = Directory.GetFiles(_thumbnailOutputDir, "*.jpg");
        var removed = 0;

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!knownIds.Contains(name))
            {
                File.Delete(file);
                removed++;
                _logger.LogDebug("Removed orphaned thumbnail: {File}", file);
            }
        }

        _logger.LogInformation("Cleaned up {Count} orphaned thumbnails", removed);
        return removed;
    }
}
