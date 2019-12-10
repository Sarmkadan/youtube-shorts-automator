// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace YouTubeShortsAutomator.Integration;

/// <summary>
/// Wrapper for FFmpeg command execution
/// Handles video encoding, format conversion, and metadata extraction
/// </summary>
public interface IFFmpegWrapper
{
    Task<bool> ConvertVideoAsync(string inputPath, string outputPath, string profile);
    Task<VideoMetadata?> GetVideoMetadataAsync(string filePath);
    Task<bool> ExtractThumbnailAsync(string videoPath, string outputPath, int secondsOffset = 1);
}

public class FFmpegWrapper : IFFmpegWrapper
{
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly ILogger<FFmpegWrapper> _logger;
    private readonly IConfiguration _configuration;

    public FFmpegWrapper(
        ILogger<FFmpegWrapper> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _ffmpegPath = configuration.GetValue<string>("Processing:FFmpegPath") ?? "ffmpeg";
        _ffprobePath = configuration.GetValue<string>("Processing:FFprobePath") ?? "ffprobe";
    }

    public async Task<bool> ConvertVideoAsync(string inputPath, string outputPath, string profile)
    {
        if (!File.Exists(inputPath))
        {
            _logger.LogError("Input file not found: {InputPath}", inputPath);
            return false;
        }

        try
        {
            var args = BuildEncodingArguments(inputPath, outputPath, profile);
            _logger.LogInformation("Starting FFmpeg conversion. Profile: {Profile}, Input: {Input}",
                profile, inputPath);

            var result = await ExecuteFFmpegAsync(_ffmpegPath, args);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting video. Profile: {Profile}", profile);
            return false;
        }
    }

    public async Task<VideoMetadata?> GetVideoMetadataAsync(string filePath)
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

            var output = await ExecuteFFprobeAsync(_ffprobePath, args);

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

    public async Task<bool> ExtractThumbnailAsync(string videoPath, string outputPath, int secondsOffset = 1)
    {
        if (!File.Exists(videoPath))
        {
            _logger.LogError("Video file not found for thumbnail extraction: {VideoPath}", videoPath);
            return false;
        }

        try
        {
            var args = $"-ss {secondsOffset} -i \"{videoPath}\" -vf \"scale=1280:720\" -vframes 1 \"{outputPath}\"";
            _logger.LogInformation("Extracting thumbnail from video: {VideoPath}", videoPath);

            var result = await ExecuteFFmpegAsync(_ffmpegPath, args);
            return result && File.Exists(outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting thumbnail from: {VideoPath}", videoPath);
            return false;
        }
    }

    private string BuildEncodingArguments(string inputPath, string outputPath, string profile)
    {
        return profile.ToLowerInvariant() switch
        {
            "hq" => $"-i \"{inputPath}\" -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k \"{outputPath}\"",
            "standard" => $"-i \"{inputPath}\" -c:v libx264 -preset medium -crf 28 -c:a aac -b:a 128k \"{outputPath}\"",
            "mobile" => $"-i \"{inputPath}\" -c:v libx264 -preset fast -crf 32 -c:a aac -b:a 96k \"{outputPath}\"",
            _ => $"-i \"{inputPath}\" -c:v libx264 -preset medium -crf 28 -c:a aac -b:a 128k \"{outputPath}\""
        };
    }

    private async Task<bool> ExecuteFFmpegAsync(string executable, string arguments)
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
            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    _logger.LogError("Failed to start FFmpeg process");
                    return false;
                }

                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing FFmpeg");
            return false;
        }
    }

    private async Task<string> ExecuteFFprobeAsync(string executable, string arguments)
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

        using (var process = Process.Start(processInfo))
        {
            if (process == null)
                return string.Empty;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output;
        }
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
