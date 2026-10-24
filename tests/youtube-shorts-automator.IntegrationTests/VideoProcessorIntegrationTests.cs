// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using YouTubeShortAutomator.Exceptions;
using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.IntegrationTests;

/// <summary>
/// End-to-end tests for <see cref="VideoProcessor"/> that run real FFmpeg/ffprobe against synthetic MP4 files.
/// Tests skip (rather than fail) when ffmpeg or ffprobe is not installed.
/// </summary>
public class VideoProcessorIntegrationTests : IClassFixture<FfmpegFixture>, IDisposable
{
    private readonly FfmpegFixture _ffmpeg;
    private readonly TempDirectory _temp = new();
    private readonly VideoProcessor _processor = new(NullLogger<VideoProcessor>.Instance);

    public VideoProcessorIntegrationTests(FfmpegFixture ffmpeg)
    {
        _ffmpeg = ffmpeg;
    }

    [SkippableFact]
    public async Task ProcessAsync_LongLandscapeSource_ProducesTrimmedVerticalH264AacShort()
    {
        Skip.IfNot(_ffmpeg.IsAvailable, "ffmpeg and ffprobe are required on PATH");
        var output = Path.Combine(_temp.DirectoryPath, "nested", "short.mp4");

        await _processor.ProcessAsync(_ffmpeg.LongLandscapeVideoPath, output);

        File.Exists(output).Should().BeTrue();

        var info = await MediaProbe.ProbeAsync(output);
        info.Width.Should().Be(VideoProcessor.OutputWidth);
        info.Height.Should().Be(VideoProcessor.OutputHeight);
        info.AspectRatio.Should().BeApproximately(9.0 / 16.0, 0.001, "output must be 9:16 vertical");
        info.DurationSeconds.Should().BeLessThan(60, "YouTube Shorts must be under 60 seconds");
        info.DurationSeconds.Should().BeApproximately(VideoProcessor.MaxOutputSeconds, 0.5,
            "the 90s source should be trimmed to the output cap");
        info.VideoCodec.Should().Be("h264");
        info.AudioCodec.Should().Be("aac");
    }

    [SkippableFact]
    public async Task ProcessAsync_ShortSource_KeepsFullDuration()
    {
        Skip.IfNot(_ffmpeg.IsAvailable, "ffmpeg and ffprobe are required on PATH");
        var output = Path.Combine(_temp.DirectoryPath, "short.mp4");

        await _processor.ProcessAsync(_ffmpeg.ShortLandscapeVideoPath, output);

        var info = await MediaProbe.ProbeAsync(output);
        info.DurationSeconds.Should().BeApproximately(5.0, 0.5, "sources under the cap must not be trimmed");
        info.AspectRatio.Should().BeApproximately(9.0 / 16.0, 0.001);
    }

    [SkippableFact]
    public async Task ProcessAsync_CorruptInput_ThrowsVideoProcessingExceptionNamingTheFile()
    {
        Skip.IfNot(_ffmpeg.IsAvailable, "ffmpeg and ffprobe are required on PATH");
        var input = Path.Combine(_temp.DirectoryPath, "corrupt.mp4");
        var output = Path.Combine(_temp.DirectoryPath, "corrupt-out.mp4");
        await File.WriteAllBytesAsync(input, "this is not a video file"u8.ToArray());

        var act = () => _processor.ProcessAsync(input, output);

        await act.Should().ThrowAsync<VideoProcessingException>()
            .WithMessage("*not a readable video*")
            .WithMessage($"*{Path.GetFileName(input)}*");
        File.Exists(output).Should().BeFalse("no output should be left behind for a rejected input");
    }

    [Fact]
    public async Task ProcessAsync_MissingInput_ThrowsFileNotFoundException()
    {
        var input = Path.Combine(_temp.DirectoryPath, "does-not-exist.mp4");
        var output = Path.Combine(_temp.DirectoryPath, "out.mp4");

        var act = () => _processor.ProcessAsync(input, output);

        await act.Should().ThrowAsync<FileNotFoundException>();
        File.Exists(output).Should().BeFalse();
    }

    public void Dispose() => _temp.Dispose();
}
