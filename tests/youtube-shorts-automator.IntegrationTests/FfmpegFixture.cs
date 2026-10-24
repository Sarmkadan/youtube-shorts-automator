// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace YouTubeShortAutomator.IntegrationTests;

/// <summary>
/// Generates synthetic landscape MP4 fixtures with FFmpeg once per test class.
/// When ffmpeg or ffprobe is not on PATH, <see cref="IsAvailable"/> is false and tests skip.
/// </summary>
public sealed class FfmpegFixture : IDisposable
{
    private readonly TempDirectory _directory = new();

    /// <summary>1280x720 test pattern with a sine tone, 90 seconds long. Longer than the 60s Shorts limit.</summary>
    public string LongLandscapeVideoPath { get; }

    /// <summary>1280x720 test pattern with a sine tone, 5 seconds long.</summary>
    public string ShortLandscapeVideoPath { get; }

    public bool IsAvailable { get; }

    public FfmpegFixture()
    {
        LongLandscapeVideoPath = Path.Combine(_directory.DirectoryPath, "long-landscape.mp4");
        ShortLandscapeVideoPath = Path.Combine(_directory.DirectoryPath, "short-landscape.mp4");

        IsAvailable = IsToolAvailable("ffmpeg") && IsToolAvailable("ffprobe");
        if (!IsAvailable)
            return;

        GenerateSample(LongLandscapeVideoPath, seconds: 90);
        GenerateSample(ShortLandscapeVideoPath, seconds: 5);
    }

    public void Dispose() => _directory.Dispose();

    private static bool IsToolAvailable(string tool)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(tool, "-version")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (process is null)
                return false;

            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static void GenerateSample(string outputPath, int seconds)
    {
        var arguments = new[]
        {
            "-nostdin", "-y",
            "-f", "lavfi", "-i", "testsrc=size=1280x720:rate=30",
            "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000",
            "-t", seconds.ToString(CultureInfo.InvariantCulture),
            "-c:v", "libx264", "-preset", "ultrafast", "-pix_fmt", "yuv420p",
            "-c:a", "aac", "-shortest",
            outputPath
        };

        var startInfo = new ProcessStartInfo("ffmpeg")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start ffmpeg for fixture generation.");

        process.StandardInput.Close();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        var stderr = stderrTask.GetAwaiter().GetResult();

        if (process.ExitCode != 0 || !File.Exists(outputPath))
            throw new InvalidOperationException(
                $"ffmpeg failed to generate fixture '{outputPath}' (exit {process.ExitCode}): {stderr}");
    }
}
