// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Globalization;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Converts a source video into a YouTube Shorts-ready MP4 using FFmpeg:
/// trims it to fewer than 60 seconds, center-crops it to 9:16, scales it to 1080x1920
/// and re-encodes it to H.264/AAC.
/// </summary>
public class VideoProcessor
{
    /// <summary>Output duration cap. Shorts must be under 60s, so the trim stays one second short of the limit.</summary>
    public const int MaxOutputSeconds = Constants.Constants.MAX_DURATION_SECONDS - 1;

    /// <summary>Output frame size for 9:16 vertical video.</summary>
    public const int OutputWidth = 1080;
    public const int OutputHeight = 1920;

    private const int ProcessTimeoutSeconds = 600;

    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly ILogger<VideoProcessor> _logger;

    public VideoProcessor(ILogger<VideoProcessor> logger, string ffmpegPath = "ffmpeg", string ffprobePath = "ffprobe")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ffmpegPath = ffmpegPath;
        _ffprobePath = ffprobePath;
    }

    /// <summary>
    /// Processes <paramref name="inputPath"/> and writes the Shorts-ready video to <paramref name="outputPath"/>.
    /// Missing parent directories of the output are created. A partially written output is deleted on failure.
    /// </summary>
    /// <exception cref="FileNotFoundException">The input file does not exist.</exception>
    /// <exception cref="VideoProcessingException">The input is not a readable video, or FFmpeg failed.</exception>
    public async Task ProcessAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input video not found", inputPath);

        var sourceSeconds = await ProbeDurationSecondsAsync(inputPath, cancellationToken);
        var outputSeconds = Math.Min(sourceSeconds, MaxOutputSeconds);

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        _logger.LogInformation("Processing {Input} ({Source:F1}s) -> {Output} ({Output:F1}s)",
            inputPath, sourceSeconds, outputPath, outputSeconds);

        var arguments = BuildFFmpegArguments(inputPath, outputPath, outputSeconds);
        var (exitCode, stderr) = await RunProcessAsync(_ffmpegPath, arguments, cancellationToken);

        if (exitCode != 0)
        {
            TryDelete(outputPath);
            _logger.LogError("FFmpeg failed with exit code {ExitCode} for {Input}", exitCode, inputPath);
            throw new VideoProcessingException(
                $"FFmpeg failed (exit code {exitCode}) while processing {inputPath}: {LastLines(stderr)}");
        }

        _logger.LogInformation("Finished processing {Input}", inputPath);
    }

    private static IReadOnlyList<string> BuildFFmpegArguments(string inputPath, string outputPath, double outputSeconds)
    {
        // Crop the widest 9:16 window from the centre of the frame, then scale to the Shorts resolution.
        var filter = $"crop='min(iw,ih*9/16)':ih,scale={OutputWidth}:{OutputHeight}:flags=lanczos,setsar=1";

        return
        [
            "-nostdin", "-y",
            "-i", inputPath,
            "-t", outputSeconds.ToString("F3", CultureInfo.InvariantCulture),
            "-vf", filter,
            "-c:v", "libx264", "-preset", "medium", "-crf", "23", "-pix_fmt", "yuv420p",
            "-c:a", "aac", "-b:a", "128k",
            "-movflags", "+faststart",
            outputPath
        ];
    }

    private async Task<double> ProbeDurationSecondsAsync(string inputPath, CancellationToken cancellationToken)
    {
        var (exitCode, stdout, stderr) = await RunProcessWithOutputAsync(_ffprobePath,
            ["-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", inputPath],
            cancellationToken);

        if (exitCode != 0 || !double.TryParse(stdout.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            || seconds <= 0)
        {
            var reason = string.IsNullOrWhiteSpace(stderr) ? $"no duration reported (output: '{stdout.Trim()}')" : LastLines(stderr);
            throw new VideoProcessingException($"Input is not a readable video: {inputPath} ({reason})");
        }

        return seconds;
    }

    private async Task<(int ExitCode, string Stderr)> RunProcessAsync(
        string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var (exitCode, _, stderr) = await RunProcessWithOutputAsync(fileName, arguments, cancellationToken);
        return (exitCode, stderr);
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunProcessWithOutputAsync(
        string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new VideoProcessingException($"Failed to start '{fileName}'. Is it installed and on PATH?");

        process.StandardInput.Close();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ProcessTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { /* already exited */ }
            throw new VideoProcessingException(timeoutCts.IsCancellationRequested
                ? $"'{fileName}' timed out after {ProcessTimeoutSeconds}s"
                : $"'{fileName}' was cancelled");
        }

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static string LastLines(string text, int count = 3)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.Length == 0 ? "no error output" : string.Join(" | ", lines.TakeLast(count));
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // Best effort: the original failure is more useful to the caller than a cleanup error.
        }
    }
}
