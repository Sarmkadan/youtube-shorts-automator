// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Options;
using YouTubeShortAutomator.Configuration;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for <see cref="ThumbnailGeneratorOptionsValidation"/>.
/// </summary>
public class ThumbnailGeneratorOptionsValidationTests : IDisposable
{
    private readonly ThumbnailGeneratorOptionsValidation _validator = new();
    private readonly string _tempDir;
    private readonly string _fakeFfmpeg;

    /// <summary>
    /// Creates a temporary directory holding a stand-in ffmpeg file, so the path check is deterministic.
    /// </summary>
    public ThumbnailGeneratorOptionsValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"thumbopts_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fakeFfmpeg = Path.Combine(_tempDir, "ffmpeg");
        File.WriteAllText(_fakeFfmpeg, string.Empty);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var result = _validator.Validate(null, ValidOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    public void Validate_DefaultQualityOutsideOneToHundred_Fails(int quality)
    {
        var options = ValidOptions();
        options.DefaultQuality = quality;

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("ThumbnailGenerator:DefaultQuality"))
            .Which.Should().Contain($"but was {quality}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(85)]
    [InlineData(100)]
    public void Validate_DefaultQualityInsideRange_Succeeds(int quality)
    {
        var options = ValidOptions();
        options.DefaultQuality = quality;

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_FfmpegPathMissingFile_FailsNamingSetting()
    {
        var options = ValidOptions();
        options.FfmpegPath = Path.Combine(_tempDir, "missing-ffmpeg");

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFmpegPath"));
    }

    [Fact]
    public void Validate_FfmpegPathBareCommandNotOnPath_Fails()
    {
        var options = ValidOptions();
        options.FfmpegPath = "definitely-not-installed-ffmpeg-7f3a9c";

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFmpegPath"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Validate_FfmpegTimeoutNotPositive_Fails(int seconds)
    {
        var options = ValidOptions();
        options.FfmpegTimeoutSeconds = seconds;

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle(f => f.Contains("Processing:FFmpegTimeoutSeconds"));
    }

    [Fact]
    public void Validate_SeveralInvalidSettings_ReportsAllOfThemTogether()
    {
        var options = new ThumbnailGeneratorOptions
        {
            FfmpegPath = Path.Combine(_tempDir, "missing-ffmpeg"),
            FfmpegTimeoutSeconds = 0,
            DefaultQuality = 0
        };

        var result = _validator.Validate(null, options);

        result.Failures.Should().HaveCount(3);
    }

    private ThumbnailGeneratorOptions ValidOptions() => new()
    {
        FfmpegPath = _fakeFfmpeg,
        FfmpegTimeoutSeconds = 120,
        DefaultQuality = 85
    };
}
