// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace YouTubeShortAutomator.IntegrationTests;

/// <summary>Stream and container facts read back from a media file with ffprobe.</summary>
public sealed record MediaInfo(int Width, int Height, double DurationSeconds, string? VideoCodec, string? AudioCodec)
{
    public double AspectRatio => (double)Width / Height;
}

/// <summary>Reads back the output of the processor so tests assert on the file itself, not on the code path.</summary>
public static class MediaProbe
{
    public static async Task<MediaInfo> ProbeAsync(string path)
    {
        var startInfo = new ProcessStartInfo("ffprobe")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add("error");
        startInfo.ArgumentList.Add("-print_format");
        startInfo.ArgumentList.Add("json");
        startInfo.ArgumentList.Add("-show_format");
        startInfo.ArgumentList.Add("-show_streams");
        startInfo.ArgumentList.Add(path);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start ffprobe.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"ffprobe could not read '{path}': {stderr}");
        }

        using var document = JsonDocument.Parse(stdout);
        var root = document.RootElement;

        string? videoCodec = null;
        string? audioCodec = null;
        int width = 0;
        int height = 0;

        foreach (var stream in root.GetProperty("streams").EnumerateArray())
        {
            var codecType = stream.GetProperty("codec_type").GetString();
            var codecName = stream.TryGetProperty("codec_name", out var codec) ? codec.GetString() : null;

            if (codecType == "video" && videoCodec is null)
            {
                videoCodec = codecName;
                width = stream.GetProperty("width").GetInt32();
                height = stream.GetProperty("height").GetInt32();
            }
            else if (codecType == "audio" && audioCodec is null)
            {
                audioCodec = codecName;
            }
        }

        var durationText = root.GetProperty("format").GetProperty("duration").GetString();
        var duration = double.Parse(durationText!, CultureInfo.InvariantCulture);

        return new MediaInfo(width, height, duration, videoCodec, audioCodec);
    }
}
