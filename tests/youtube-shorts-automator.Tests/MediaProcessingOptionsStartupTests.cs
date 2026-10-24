// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YouTubeShortAutomator.Configuration;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains tests that run the <c>ValidateOnStart</c> path for the media processing options through a real
/// service container, the same way the console entry point does at startup.
/// </summary>
public class MediaProcessingOptionsStartupTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _fakeFfmpeg;
    private readonly string _fakeFfprobe;

    /// <summary>
    /// Creates a temporary directory holding stand-in ffmpeg and ffprobe files.
    /// </summary>
    public MediaProcessingOptionsStartupTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"mediaopts_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fakeFfmpeg = Path.Combine(_tempDir, "ffmpeg");
        _fakeFfprobe = Path.Combine(_tempDir, "ffprobe");
        File.WriteAllText(_fakeFfmpeg, string.Empty);
        File.WriteAllText(_fakeFfprobe, string.Empty);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void StartupValidation_ValidConfiguration_DoesNotThrow()
    {
        var provider = BuildProvider(ValidSettings());

        var act = () => RunStartupValidators(provider);

        act.Should().NotThrow();
    }

    [Fact]
    public void StartupValidation_ValidateOnStartIsRegistered()
    {
        // ValidateOnStart registers one shared StartupValidator that covers every options type with the flag set.
        var provider = BuildProvider(ValidSettings());

        provider.GetServices<IStartupValidator>().Should().ContainSingle();
    }

    [Fact]
    public void StartupValidation_MaxDurationOutOfRange_FailsWithSettingName()
    {
        var settings = ValidSettings();
        settings["VideoProcessor:MaxDurationSeconds"] = "90";
        var provider = BuildProvider(settings);

        var act = () => RunStartupValidators(provider);

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*VideoProcessor:MaxDurationSeconds*");
    }

    [Fact]
    public void StartupValidation_TargetResolutionNotNineBySixteen_FailsWithSettingName()
    {
        var settings = ValidSettings();
        settings["VideoProcessor:TargetWidth"] = "1920";
        settings["VideoProcessor:TargetHeight"] = "1080";
        var provider = BuildProvider(settings);

        var act = () => RunStartupValidators(provider);

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*9:16*");
    }

    [Fact]
    public void StartupValidation_MissingFfmpeg_FailsWithSettingName()
    {
        var settings = ValidSettings();
        settings["Processing:FFmpegPath"] = Path.Combine(_tempDir, "no-such-ffmpeg");
        var provider = BuildProvider(settings);

        var act = () => RunStartupValidators(provider);

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*Processing:FFmpegPath*");
    }

    [Fact]
    public void StartupValidation_ThumbnailQualityOutOfRange_FailsWithSettingName()
    {
        var settings = ValidSettings();
        settings["ThumbnailGenerator:DefaultQuality"] = "150";
        var provider = BuildProvider(settings);

        var act = () => RunStartupValidators(provider);

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ThumbnailGenerator:DefaultQuality*");
    }

    [Fact]
    public void Binding_ProcessingSection_FeedsBothOptionTypes()
    {
        var settings = ValidSettings();
        settings["Processing:FFmpegTimeoutSeconds"] = "300";
        var provider = BuildProvider(settings);

        var thumbnail = provider.GetRequiredService<IOptions<ThumbnailGeneratorOptions>>().Value;
        var video = provider.GetRequiredService<IOptions<VideoProcessorOptions>>().Value;

        thumbnail.FfmpegTimeoutSeconds.Should().Be(300);
        thumbnail.FfmpegPath.Should().Be(_fakeFfmpeg);
        video.FfmpegPath.Should().Be(_fakeFfmpeg);
        video.FfprobePath.Should().Be(_fakeFfprobe);
    }

    private Dictionary<string, string?> ValidSettings() => new()
    {
        ["Processing:FFmpegPath"] = _fakeFfmpeg,
        ["Processing:FFprobePath"] = _fakeFfprobe,
        ["VideoProcessor:OutputDirectory"] = Path.Combine(_tempDir, "output"),
        ["VideoProcessor:MaxDurationSeconds"] = "59",
        ["VideoProcessor:TargetWidth"] = "1080",
        ["VideoProcessor:TargetHeight"] = "1920",
        ["ThumbnailGenerator:DefaultQuality"] = "85"
    };

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMediaProcessingOptions(configuration);

        return services.BuildServiceProvider();
    }

    private static void RunStartupValidators(IServiceProvider provider)
    {
        foreach (var startupValidator in provider.GetServices<IStartupValidator>())
        {
            startupValidator.Validate();
        }
    }
}
