// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Options;
using YouTubeShortAutomator.Configuration;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for <see cref="VideoProcessorOptionsValidation"/>.
/// </summary>
public class VideoProcessorOptionsValidationTests : IDisposable
{
    private readonly VideoProcessorOptionsValidation _validator = new();
    private readonly string _tempDir;
    private readonly string _fakeFfmpeg;
    private readonly string _fakeFfprobe;

    /// <summary>
    /// Creates a temporary directory holding stand-in ffmpeg and ffprobe files, so path checks are deterministic.
    /// </summary>
    public VideoProcessorOptionsValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"vpopts_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fakeFfmpeg = CreateFile("ffmpeg");
        _fakeFfprobe = CreateFile("ffprobe");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // ── Valid configuration ───────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var result = _validator.Validate(null, ValidOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_OutputDirectoryDoesNotExistYet_Succeeds()
    {
        var options = ValidOptions();
        options.OutputDirectory = Path.Combine(_tempDir, "not", "created", "yet");

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue("a missing output directory is created on demand when its nearest ancestor is writable");
        Directory.Exists(options.OutputDirectory).Should().BeFalse("validation must not create directories");
    }

    // ── FFmpeg / FFprobe paths ────────────────────────────────────────────────

    [Fact]
    public void Validate_FfmpegPathMissingFile_FailsNamingSetting()
    {
        var result = ValidateWith(o => o.FfmpegPath = Path.Combine(_tempDir, "missing-ffmpeg"));

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFmpegPath"));
    }

    [Fact]
    public void Validate_FfmpegPathBareCommandNotOnPath_Fails()
    {
        var result = ValidateWith(o => o.FfmpegPath = "definitely-not-installed-ffmpeg-7f3a9c");

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFmpegPath"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_FfmpegPathBlank_Fails(string path)
    {
        var result = ValidateWith(o => o.FfmpegPath = path);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("Processing:FFmpegPath"));
    }

    [Fact]
    public void Validate_FfprobePathMissingFile_FailsNamingSetting()
    {
        var result = ValidateWith(o => o.FfprobePath = Path.Combine(_tempDir, "missing-ffprobe"));

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFprobePath"));
    }

    // ── Max duration (1-60 s) ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    [InlineData(int.MaxValue)]
    public void Validate_MaxDurationOutsideOneToSixty_Fails(int seconds)
    {
        var result = ValidateWith(o => o.MaxDurationSeconds = seconds);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("VideoProcessor:MaxDurationSeconds"))
            .Which.Should().Contain($"but was {seconds}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(59)]
    [InlineData(60)]
    public void Validate_MaxDurationInsideRange_Succeeds(int seconds)
    {
        var result = ValidateWith(o => o.MaxDurationSeconds = seconds);

        result.Succeeded.Should().BeTrue();
    }

    // ── Target resolution (valid 9:16) ────────────────────────────────────────

    [Theory]
    [InlineData(1080, 1920)]
    [InlineData(720, 1280)]
    [InlineData(2160, 3840)]
    public void Validate_NineBySixteenEvenResolution_Succeeds(int width, int height)
    {
        var result = ValidateWith(o =>
        {
            o.TargetWidth = width;
            o.TargetHeight = height;
        });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1920, 1080, "9:16")]
    [InlineData(1080, 1080, "9:16")]
    [InlineData(1080, 1900, "9:16")]
    [InlineData(1081, 1921, "9:16")]
    public void Validate_NotNineBySixteen_Fails(int width, int height, string expectedFragment)
    {
        var result = ValidateWith(o =>
        {
            o.TargetWidth = width;
            o.TargetHeight = height;
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("VideoProcessor:TargetWidth") && f.Contains(expectedFragment));
    }

    [Fact]
    public void Validate_OddDimensionsWithCorrectRatio_FailsBecauseYuv420pNeedsEven()
    {
        var result = ValidateWith(o =>
        {
            o.TargetWidth = 9;
            o.TargetHeight = 16;
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("must both be even"));
    }

    [Theory]
    [InlineData(0, 1920)]
    [InlineData(1080, 0)]
    [InlineData(-1080, -1920)]
    public void Validate_NonPositiveResolution_Fails(int width, int height)
    {
        var result = ValidateWith(o =>
        {
            o.TargetWidth = width;
            o.TargetHeight = height;
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("must be positive"));
    }

    [Fact]
    public void Validate_HugeRatioCheck_DoesNotOverflow()
    {
        // 1 << 30 squared overflows int; the 9:16 comparison must still report a clean failure.
        var result = ValidateWith(o =>
        {
            o.TargetWidth = 1 << 30;
            o.TargetHeight = 1 << 30;
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("9:16"));
    }

    // ── Output directory ──────────────────────────────────────────────────────

    [Fact]
    public void Validate_OutputDirectoryEmpty_Fails()
    {
        var result = ValidateWith(o => o.OutputDirectory = string.Empty);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("VideoProcessor:OutputDirectory must not be empty"));
    }

    [Fact]
    public void Validate_OutputDirectoryIsAFile_FailsAsNotWritable()
    {
        var filePath = CreateFile("not-a-directory.txt");

        var result = ValidateWith(o => o.OutputDirectory = filePath);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("VideoProcessor:OutputDirectory") && f.Contains("not writable"));
    }

    [Fact]
    public void Validate_OutputDirectoryUnderAFile_FailsAsNotWritable()
    {
        // The nearest existing ancestor is a file, so the directory could never be created.
        var result = ValidateWith(o => o.OutputDirectory = Path.Combine(_fakeFfmpeg, "child"));

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("VideoProcessor:OutputDirectory") && f.Contains("not writable"));
    }

    // ── Multiple problems ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_SeveralInvalidSettings_ReportsAllOfThemTogether()
    {
        var result = ValidateWith(o =>
        {
            o.FfmpegPath = Path.Combine(_tempDir, "missing-ffmpeg");
            o.MaxDurationSeconds = 0;
            o.TargetWidth = 1920;
            o.TargetHeight = 1080;
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(3);
        result.Failures.Should().Contain(f => f.Contains("Processing:FFmpegPath"));
        result.Failures.Should().Contain(f => f.Contains("MaxDurationSeconds"));
        result.Failures.Should().Contain(f => f.Contains("9:16"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private VideoProcessorOptions ValidOptions() => new()
    {
        FfmpegPath = _fakeFfmpeg,
        FfprobePath = _fakeFfprobe,
        OutputDirectory = Path.Combine(_tempDir, "output"),
        MaxDurationSeconds = 59,
        TargetWidth = 1080,
        TargetHeight = 1920
    };

    private ValidateOptionsResult ValidateWith(Action<VideoProcessorOptions> mutate)
    {
        var options = ValidOptions();
        mutate(options);
        return _validator.Validate(null, options);
    }

    private string CreateFile(string name)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, string.Empty);
        return path;
    }
}
