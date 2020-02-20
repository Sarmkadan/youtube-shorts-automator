// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Defines the contract for programmatic thumbnail creation from video sources,
/// including frame extraction, text overlay and batch generation.
/// </summary>
public interface IThumbnailGeneratorService
{
    /// <summary>
    /// Extracts a single frame from <paramref name="videoPath"/> at the timestamp defined in
    /// <paramref name="request"/>, optionally composites a text overlay, and writes the result
    /// to <see cref="ThumbnailGenerationRequest.OutputDirectory"/>.
    /// </summary>
    /// <param name="videoPath">Absolute path to the source video file.</param>
    /// <param name="request">Generation parameters including timestamp, format and overlay settings.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A <see cref="ThumbnailGenerationResult"/> describing the outcome.</returns>
    Task<ThumbnailGenerationResult> GenerateFromVideoAsync(
        string videoPath,
        ThumbnailGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a text overlay to an existing image file and writes the result to a new file
    /// alongside the source. The original image is not modified.
    /// </summary>
    /// <param name="imagePath">Absolute path to the source image.</param>
    /// <param name="text">Text string to render on the image.</param>
    /// <param name="options">Appearance settings for the overlay.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A <see cref="ThumbnailGenerationResult"/> describing the outcome.</returns>
    Task<ThumbnailGenerationResult> GenerateWithTextOverlayAsync(
        string imagePath,
        string text,
        TextOverlayOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple thumbnails from evenly spaced timestamps across the video's
    /// duration. Useful for selecting the best candidate frame before publishing.
    /// </summary>
    /// <param name="videoPath">Absolute path to the source video file.</param>
    /// <param name="request">Base generation parameters applied to every frame.</param>
    /// <param name="frameCount">Number of evenly spaced frames to extract. Must be ≥ 1.</param>
    /// <param name="videoDuration">Total duration of the video, used to space capture timestamps.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>Collection of results ordered by capture timestamp.</returns>
    Task<IEnumerable<ThumbnailGenerationResult>> GenerateBatchAsync(
        string videoPath,
        ThumbnailGenerationRequest request,
        int frameCount,
        TimeSpan videoDuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the pixel dimensions (width, height) for the given aspect ratio target.
    /// </summary>
    (int Width, int Height) GetDimensions(ThumbnailAspectRatio aspectRatio);
}

/// <summary>
/// Implements thumbnail generation by invoking FFmpeg for frame extraction and compositing.
/// Text overlays are rendered via the FFmpeg <c>drawtext</c> filter, which requires a
/// TrueType font accessible on the host system.
/// </summary>
public sealed class ThumbnailGeneratorService : IThumbnailGeneratorService
{
    private readonly string _ffmpegPath;
    private readonly int _timeoutSeconds;
    private readonly ILogger<ThumbnailGeneratorService> _logger;

    private static readonly IReadOnlyDictionary<ThumbnailAspectRatio, (int W, int H)> Dimensions =
        new Dictionary<ThumbnailAspectRatio, (int, int)>
        {
            [ThumbnailAspectRatio.Horizontal] = (1280, 720),
            [ThumbnailAspectRatio.Vertical]   = (720, 1280),
            [ThumbnailAspectRatio.Square]      = (720, 720),
        };

    private static readonly IReadOnlyDictionary<ThumbnailOutputFormat, string> FormatExtensions =
        new Dictionary<ThumbnailOutputFormat, string>
        {
            [ThumbnailOutputFormat.Jpeg] = ".jpg",
            [ThumbnailOutputFormat.Png]  = ".png",
            [ThumbnailOutputFormat.WebP] = ".webp",
        };

    /// <summary>
    /// Initialises a new instance of <see cref="ThumbnailGeneratorService"/>.
    /// </summary>
    /// <param name="configuration">Application configuration; reads <c>Processing:FFmpegPath</c>
    /// and <c>Processing:FFmpegTimeoutSeconds</c>.</param>
    /// <param name="logger">Logger instance.</param>
    public ThumbnailGeneratorService(
        IConfiguration configuration,
        ILogger<ThumbnailGeneratorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ffmpegPath = configuration?.GetValue<string>("Processing:FFmpegPath") ?? "ffmpeg";
        _timeoutSeconds = configuration?.GetValue<int>("Processing:FFmpegTimeoutSeconds", 120) ?? 120;
    }

    /// <inheritdoc />
    public async Task<ThumbnailGenerationResult> GenerateFromVideoAsync(
        string videoPath,
        ThumbnailGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoPath);
        ArgumentNullException.ThrowIfNull(request);

        if (!File.Exists(videoPath))
            throw new VideoProcessingException($"Video file not found: {videoPath}");

        if (string.IsNullOrWhiteSpace(request.OutputDirectory))
            throw new ValidationException("OutputDirectory is required.", nameof(request.OutputDirectory), string.Empty);

        Directory.CreateDirectory(request.OutputDirectory);

        var (width, height) = GetDimensions(request.AspectRatio);
        var ext = FormatExtensions[request.Format];
        var outputFileName = $"thumb_{Guid.NewGuid():N}{ext}";
        var outputPath = Path.Combine(request.OutputDirectory, outputFileName);

        var args = BuildExtractionArguments(videoPath, outputPath, request, width, height);

        _logger.LogInformation(
            "Generating thumbnail from {VideoPath} at {Timestamp} → {OutputPath}",
            videoPath, request.CaptureTimestamp, outputPath);

        var (success, errorOutput) = await ExecuteFFmpegAsync(args, cancellationToken);

        if (!success || !File.Exists(outputPath))
        {
            _logger.LogWarning("Thumbnail generation failed for {VideoPath}: {Error}", videoPath, errorOutput);
            return new ThumbnailGenerationResult
            {
                Success = false,
                ErrorMessage = string.IsNullOrWhiteSpace(errorOutput) ? "FFmpeg process failed." : errorOutput,
                CaptureTimestamp = request.CaptureTimestamp,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var info = new FileInfo(outputPath);
        _logger.LogInformation(
            "Thumbnail generated: {OutputPath} ({Bytes} bytes, {W}×{H})",
            outputPath, info.Length, width, height);

        return new ThumbnailGenerationResult
        {
            Success = true,
            OutputPath = outputPath,
            FileSizeBytes = info.Length,
            Width = width,
            Height = height,
            CaptureTimestamp = request.CaptureTimestamp,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<ThumbnailGenerationResult> GenerateWithTextOverlayAsync(
        string imagePath,
        string text,
        TextOverlayOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(options);

        if (!File.Exists(imagePath))
            throw new VideoProcessingException($"Image file not found: {imagePath}");

        var outputPath = Path.Combine(
            Path.GetDirectoryName(imagePath)!,
            $"{Path.GetFileNameWithoutExtension(imagePath)}_text{Path.GetExtension(imagePath)}");

        var safeText = text.Replace("'", "\\'").Replace(":", "\\:");
        var drawTextFilter = BuildDrawTextFilter(safeText, options);
        var args = $"-i \"{imagePath}\" -vf \"{drawTextFilter}\" -y \"{outputPath}\"";

        _logger.LogInformation("Applying text overlay to {ImagePath}", imagePath);
        var (success, errorOutput) = await ExecuteFFmpegAsync(args, cancellationToken);

        if (!success || !File.Exists(outputPath))
        {
            return new ThumbnailGenerationResult
            {
                Success = false,
                ErrorMessage = string.IsNullOrWhiteSpace(errorOutput) ? "FFmpeg drawtext failed." : errorOutput,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var info = new FileInfo(outputPath);
        return new ThumbnailGenerationResult
        {
            Success = true,
            OutputPath = outputPath,
            FileSizeBytes = info.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ThumbnailGenerationResult>> GenerateBatchAsync(
        string videoPath,
        ThumbnailGenerationRequest request,
        int frameCount,
        TimeSpan videoDuration,
        CancellationToken cancellationToken = default)
    {
        if (frameCount < 1)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Frame count must be at least 1.");

        if (videoDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(videoDuration), "Video duration must be positive.");

        var step = videoDuration.TotalSeconds / (frameCount + 1);
        var results = new List<ThumbnailGenerationResult>(frameCount);

        for (var i = 1; i <= frameCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captureAt = TimeSpan.FromSeconds(step * i);
            var frameRequest = new ThumbnailGenerationRequest
            {
                CaptureTimestamp = captureAt,
                OutputDirectory  = request.OutputDirectory,
                OverlayText      = request.OverlayText,
                TextOverlay      = request.TextOverlay,
                Format           = request.Format,
                AspectRatio      = request.AspectRatio,
                Quality          = request.Quality
            };

            var result = await GenerateFromVideoAsync(videoPath, frameRequest, cancellationToken);
            results.Add(result);
        }

        _logger.LogInformation(
            "Batch generation complete: {Success}/{Total} thumbnails succeeded.",
            results.Count(r => r.Success), results.Count);

        return results;
    }

    /// <inheritdoc />
    public (int Width, int Height) GetDimensions(ThumbnailAspectRatio aspectRatio)
    {
        var dim = Dimensions[aspectRatio];
        return (dim.W, dim.H);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string BuildExtractionArguments(
        string videoPath,
        string outputPath,
        ThumbnailGenerationRequest request,
        int width,
        int height)
    {
        var ts = request.CaptureTimestamp.TotalSeconds;
        var scale = $"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:(ow-iw)/2:(oh-ih)/2";

        var filters = new List<string> { scale };

        if (!string.IsNullOrWhiteSpace(request.OverlayText))
        {
            var safeText = request.OverlayText.Replace("'", "\\'").Replace(":", "\\:");
            filters.Add(BuildDrawTextFilter(safeText, request.TextOverlay));
        }

        var vf = string.Join(",", filters);
        var qualityArg = request.Format switch
        {
            ThumbnailOutputFormat.Jpeg => $"-q:v {Math.Clamp(32 - (int)(request.Quality / 3.5), 1, 31)}",
            ThumbnailOutputFormat.WebP => $"-quality {request.Quality}",
            _ => string.Empty
        };

        return $"-ss {ts:F3} -i \"{videoPath}\" -vf \"{vf}\" -vframes 1 {qualityArg} -y \"{outputPath}\"";
    }

    private static string BuildDrawTextFilter(string text, TextOverlayOptions opts)
    {
        var (x, y) = GetTextCoordinates(opts.Position, opts.BoxPadding);
        var boxArg = opts.ShowBox
            ? $":box=1:boxcolor={opts.BoxColor}:boxborderw={opts.BoxPadding}"
            : string.Empty;

        return $"drawtext=text='{text}'" +
               $":fontcolor={opts.FontColor}:fontsize={opts.FontSize}" +
               $":x={x}:y={y}{boxArg}";
    }

    private static (string X, string Y) GetTextCoordinates(TextPosition position, int padding) =>
        position switch
        {
            TextPosition.TopLeft      => ($"{padding}", $"{padding}"),
            TextPosition.TopCenter    => ("(w-text_w)/2", $"{padding}"),
            TextPosition.TopRight     => ($"w-text_w-{padding}", $"{padding}"),
            TextPosition.MiddleLeft   => ($"{padding}", "(h-text_h)/2"),
            TextPosition.Center       => ("(w-text_w)/2", "(h-text_h)/2"),
            TextPosition.MiddleRight  => ($"w-text_w-{padding}", "(h-text_h)/2"),
            TextPosition.BottomLeft   => ($"{padding}", $"h-text_h-{padding}"),
            TextPosition.BottomCenter => ("(w-text_w)/2", $"h-text_h-{padding * 2}"),
            TextPosition.BottomRight  => ($"w-text_w-{padding}", $"h-text_h-{padding}"),
            _ => ("(w-text_w)/2", $"h-text_h-{padding * 2}")
        };

    private async Task<(bool Success, string StdErr)> ExecuteFFmpegAsync(
        string arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = "-nostdin " + arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                _logger.LogError("Failed to start FFmpeg process.");
                return (false, "Process could not be started.");
            }

            process.StandardInput.Close();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var linked  = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

            try
            {
                await process.WaitForExitAsync(linked.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                return (false, $"FFmpeg timed out after {_timeoutSeconds}s.");
            }

            var stderr = await stderrTask;
            return (process.ExitCode == 0, stderr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing FFmpeg.");
            return (false, ex.Message);
        }
    }
}
