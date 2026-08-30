// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using System.Diagnostics;

namespace YouTubeShortsAutomator.Integration;

/// <summary>
/// Wrapper for FFmpeg command execution
/// Handles video encoding, format conversion, and metadata extraction
/// </summary>
public interface IFFmpegWrapper
{
    Task<bool> ConvertVideoAsync(string inputPath, string outputPath, string profile, CancellationToken cancellationToken = default);
    Task<VideoMetadata?> GetVideoMetadataAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> ExtractThumbnailAsync(string videoPath, string outputPath, int secondsOffset = 1, CancellationToken cancellationToken = default);
}

public class FFmpegWrapper : IFFmpegWrapper
{
    // FrozenDictionary provides O(1) lookup with no locking and is optimised
    // for read-heavy, write-never access patterns.  OrdinalIgnoreCase means
    // profile names never need .ToLowerInvariant() before the lookup.
    private static readonly FrozenDictionary<string, string> ProfileOptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["hq"]       = "-c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k",
            ["standard"] = "-c:v libx264 -preset medium -crf 28 -c:a aac -b:a 128k",
            ["mobile"]   = "-c:v libx264 -preset fast   -crf 32 -c:a aac -b:a 96k",
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private const string DefaultProfileOptions =
        "-c:v libx264 -preset medium -crf 28 -c:a aac -b:a 128k";

    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly ILogger<FFmpegWrapper> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _timeoutSeconds;

    public FFmpegWrapper(
        ILogger<FFmpegWrapper> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _ffmpegPath = configuration.GetValue<string>("Processing:FFmpegPath") ?? "ffmpeg";
        _ffprobePath = configuration.GetValue<string>("Processing:FFprobePath") ?? "ffprobe";
        _timeoutSeconds = configuration.GetValue<int>("Processing:FFmpegTimeoutSeconds", 300);
    }

    public async Task<bool> ConvertVideoAsync(
        string inputPath,
        string outputPath,
        string profile,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputPath))
        {
            _logger.LogError("Input file not found: {InputPath}", inputPath);
            return false;
        }

        if (!OutputDirectoryExists(outputPath, "video conversion"))
            return false;

        try
        {
            var args = BuildEncodingArguments(inputPath, outputPath, profile);
            _logger.LogInformation("Starting FFmpeg conversion. Profile: {Profile}, Input: {Input}",
                profile, inputPath);

            var result = await ExecuteFFmpegAsync(_ffmpegPath, args, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting video. Profile: {Profile}", profile);
            return false;
        }
    }

    public async Task<VideoMetadata?> GetVideoMetadataAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found for metadata extraction: {FilePath}", filePath);
            return null;
        }

        try
        {
            var args = $"-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,duration " +
                       $"-of default=noprint_wrappers=1:nokey=1:nokey=1 \"{filePath}\"";

            var output = await ExecuteFFprobeAsync(_ffprobePath, args, cancellationToken);

            // Parse output and create metadata
            var metadata = new VideoMetadata
            {
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
                ExtractedAtUtc = DateTime.UtcNow
            };

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting video metadata from: {FilePath}", filePath);
            return null;
        }
    }

    public async Task<bool> ExtractThumbnailAsync(
        string videoPath,
        string outputPath,
        int secondsOffset = 1,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(videoPath))
        {
            _logger.LogError("Video file not found for thumbnail extraction: {VideoPath}", videoPath);
            return false;
        }

        if (!OutputDirectoryExists(outputPath, "thumbnail extraction"))
            return false;

        try
        {
            var args = $"-ss {secondsOffset} -i \"{videoPath}\" -vf \"scale=1280:720\" -vframes 1 \"{outputPath}\"";
            _logger.LogInformation("Extracting thumbnail from video: {VideoPath}", videoPath);

            var result = await ExecuteFFmpegAsync(_ffmpegPath, args, cancellationToken);
            return result && File.Exists(outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting thumbnail from: {VideoPath}", videoPath);
            return false;
        }
    }

    private static string BuildEncodingArguments(string inputPath, string outputPath, string profile)
    {
        var opts = ProfileOptions.GetValueOrDefault(profile, DefaultProfileOptions);
        return $"-i \"{inputPath}\" {opts} \"{outputPath}\"";
    }

    private bool OutputDirectoryExists(string outputPath, string operation)
    {
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (directory != null && Directory.Exists(directory))
                return true;

            _logger.LogError(
                "Output directory not found for {Operation}. OutputPath: {OutputPath}, Directory: {OutputDirectory}",
                operation, outputPath, directory);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            _logger.LogError(ex,
                "Invalid output path for {Operation}. OutputPath: {OutputPath}",
                operation, outputPath);
        }

        return false;
    }

    private async Task<bool> ExecuteFFmpegAsync(string executable, string arguments, CancellationToken cancellationToken = default)
    {
        // -nostdin prevents FFmpeg from blocking on stdin when a stream (e.g. audio) is absent.
        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = "-nostdin " + arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start FFmpeg process");
                return false;
            }

            // Immediately close stdin to avoid any blocking read on the FFmpeg side.
            process.StandardInput.Close();

            // Drain stdout/stderr asynchronously to prevent buffer-full deadlocks.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                _logger.LogError(
                    "FFmpeg process timed out after {Timeout}s and was terminated. Arguments: {Args}",
                    _timeoutSeconds, arguments);
                throw new TimeoutException(
                    $"FFmpeg timed out after {_timeoutSeconds} seconds. " +
                    "The input video may be missing an audio or video stream.");
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                _logger.LogInformation("FFmpeg process was cancelled and terminated");
                throw;
            }

            var (_, stderr) = await ReadProcessOutputAsync(stdoutTask, stderrTask);
            if (process.ExitCode != 0)
            {
                _logger.LogWarning("FFmpeg exited with code {ExitCode}. Stderr: {Stderr}",
                    process.ExitCode, stderr);
            }

            return process.ExitCode == 0;
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing FFmpeg");
            return false;
        }
    }

    private async Task<string> ExecuteFFprobeAsync(
        string executable,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start FFprobe process");
                return string.Empty;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                _logger.LogWarning(
                    "FFprobe process {TerminationReason} and was terminated",
                    timeoutCts.IsCancellationRequested ? "timed out" : "was cancelled");
                throw;
            }

            var (output, stderr) = await ReadProcessOutputAsync(stdoutTask, stderrTask);
            if (process.ExitCode != 0)
            {
                _logger.LogWarning("FFprobe exited with code {ExitCode}. Stderr: {Stderr}",
                    process.ExitCode, stderr);
                return string.Empty;
            }

            return output;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing FFprobe");
            return string.Empty;
        }
    }

    private static async Task<(string Stdout, string Stderr)> ReadProcessOutputAsync(
        Task<string> stdoutTask,
        Task<string> stderrTask)
    {
        await Task.WhenAll(stdoutTask, stderrTask);
        return (await stdoutTask, await stderrTask);
    }
}

public class VideoMetadata
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double DurationSeconds { get; set; }
    public double FrameRate { get; set; }
    public string Codec { get; set; } = string.Empty;
    public DateTime ExtractedAtUtc { get; set; }
}
