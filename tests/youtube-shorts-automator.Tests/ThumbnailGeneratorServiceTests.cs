// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailGeneratorService"/> class.
/// </summary>
public class ThumbnailGeneratorServiceTests
{
    private readonly ThumbnailGeneratorService _service;
    private readonly string _tempDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailGeneratorServiceTests"/> class, setting up the service and a temporary directory.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="ThumbnailGeneratorService"/> instance with mocked configuration and logger,
    /// and establishes a temporary directory for test file operations.
    /// </remarks>
    public ThumbnailGeneratorServiceTests()
    {
        var config = new ConfigurationBuilder().Build();
        var logger = Mock.Of<ILogger<ThumbnailGeneratorService>>();
        _service = new ThumbnailGeneratorService(config, logger);
        _tempDir = Path.Combine(Path.GetTempPath(), $"thumbtest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    // ── GetDimensions ─────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GetDimensions"/> returns 720x1280 for a vertical aspect ratio.
    /// </summary>
    [Fact]
    public void GetDimensions_Vertical_Returns720x1280()
    {
        var (w, h) = _service.GetDimensions(ThumbnailAspectRatio.Vertical);

        w.Should().Be(720);
        h.Should().Be(1280);
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GetDimensions"/> returns 1280x720 for a horizontal aspect ratio.
    /// </summary>
    [Fact]
    public void GetDimensions_Horizontal_Returns1280x720()
    {
        var (w, h) = _service.GetDimensions(ThumbnailAspectRatio.Horizontal);

        w.Should().Be(1280);
        h.Should().Be(720);
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GetDimensions"/> returns 720x720 for a square aspect ratio.
    /// </summary>
    [Fact]
    public void GetDimensions_Square_Returns720x720()
    {
        var (w, h) = _service.GetDimensions(ThumbnailAspectRatio.Square);

        w.Should().Be(720);
        h.Should().Be(720);
    }

    // ── GenerateFromVideoAsync validation ─────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> throws <see cref="ArgumentException"/> when the video path is null.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_NullVideoPath_ThrowsArgumentException()
    {
        var request = new ThumbnailGenerationRequest { OutputDirectory = _tempDir };

        Func<Task> act = () => _service.GenerateFromVideoAsync(null!, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> throws <see cref="ArgumentException"/> when the video path is empty.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_EmptyVideoPath_ThrowsArgumentException()
    {
        var request = new ThumbnailGenerationRequest { OutputDirectory = _tempDir };

        Func<Task> act = () => _service.GenerateFromVideoAsync(string.Empty, request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> throws <see cref="ArgumentNullException"/> when the request is null.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_NullRequest_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.GenerateFromVideoAsync("/some/video.mp4", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> throws <see cref="VideoProcessingException"/> when the video file does not exist.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_MissingVideoFile_ThrowsVideoProcessingException()
    {
        var request = new ThumbnailGenerationRequest { OutputDirectory = _tempDir };

        Func<Task> act = () => _service.GenerateFromVideoAsync("/nonexistent/video.mp4", request);

        await act.Should().ThrowAsync<VideoProcessingException>()
            .WithMessage("*Video file not found*");
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> throws <see cref="ValidationException"/> when the output directory is empty.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_EmptyOutputDirectory_ThrowsValidationException()
    {
        var tempVideo = Path.Combine(_tempDir, "video.mp4");
        File.WriteAllBytes(tempVideo, new byte[100]);

        var request = new ThumbnailGenerationRequest { OutputDirectory = string.Empty };

        Func<Task> act = () => _service.GenerateFromVideoAsync(tempVideo, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── GenerateWithTextOverlayAsync validation ───────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateWithTextOverlayAsync"/> throws <see cref="ArgumentException"/> when the image path is null.
    /// </summary>
    [Fact]
    public async Task GenerateWithTextOverlayAsync_NullImagePath_ThrowsArgumentException()
    {
        Func<Task> act = () => _service.GenerateWithTextOverlayAsync(null!, "text", new TextOverlayOptions());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateWithTextOverlayAsync"/> throws <see cref="ArgumentException"/> when the text is empty.
    /// </summary>
    [Fact]
    public async Task GenerateWithTextOverlayAsync_EmptyText_ThrowsArgumentException()
    {
        Func<Task> act = () => _service.GenerateWithTextOverlayAsync("/some/image.jpg", string.Empty, new TextOverlayOptions());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateWithTextOverlayAsync"/> throws <see cref="VideoProcessingException"/> when the image file does not exist.
    /// </summary>
    [Fact]
    public async Task GenerateWithTextOverlayAsync_MissingImageFile_ThrowsVideoProcessingException()
    {
        Func<Task> act = () => _service.GenerateWithTextOverlayAsync(
            "/nonexistent/image.jpg", "Test Title", new TextOverlayOptions());

        await act.Should().ThrowAsync<VideoProcessingException>()
            .WithMessage("*Image file not found*");
    }

    // ── GenerateBatchAsync validation ─────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateBatchAsync"/> throws <see cref="ArgumentOutOfRangeException"/> when the frame count is zero.
    /// </summary>
    [Fact]
    public async Task GenerateBatchAsync_ZeroFrameCount_ThrowsArgumentOutOfRangeException()
    {
        var request = new ThumbnailGenerationRequest { OutputDirectory = _tempDir };

        Func<Task> act = () => _service.GenerateBatchAsync(
            "/some/video.mp4", request, frameCount: 0, videoDuration: TimeSpan.FromSeconds(30));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Frame count must be at least 1*");
    }

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateBatchAsync"/> throws <see cref="ArgumentOutOfRangeException"/> when the video duration is negative.
    /// </summary>
    [Fact]
    public async Task GenerateBatchAsync_NegativeVideoDuration_ThrowsArgumentOutOfRangeException()
    {
        var request = new ThumbnailGenerationRequest { OutputDirectory = _tempDir };

        Func<Task> act = () => _service.GenerateBatchAsync(
            "/some/video.mp4", request, frameCount: 3, videoDuration: TimeSpan.FromSeconds(-1));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Video duration must be positive*");
    }

    // ── Default options ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGenerationRequest"/> sets correct default property values upon initialization.
    /// </summary>
    [Fact]
    public void ThumbnailGenerationRequest_DefaultValues_AreCorrect()
    {
        var request = new ThumbnailGenerationRequest();

        request.CaptureTimestamp.Should().Be(TimeSpan.FromSeconds(1));
        request.Format.Should().Be(ThumbnailOutputFormat.Jpeg);
        request.AspectRatio.Should().Be(ThumbnailAspectRatio.Vertical);
        request.Quality.Should().Be(85);
        request.OverlayText.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="TextOverlayOptions"/> sets correct default property values upon initialization.
    /// </summary>
    [Fact]
    public void TextOverlayOptions_DefaultValues_AreCorrect()
    {
        var opts = new TextOverlayOptions();

        opts.FontSize.Should().Be(48);
        opts.FontColor.Should().Be("white");
        opts.ShowBox.Should().BeTrue();
        opts.BoxColor.Should().Be("black@0.5");
        opts.Position.Should().Be(TextPosition.BottomCenter);
    }

    // ── ThumbnailGenerationResult helpers ─────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGenerationResult"/> initializes with Success set to false, OutputPath set to empty, and FileSizeBytes set to 0.
    /// </summary>
    [Fact]
    public void ThumbnailGenerationResult_SuccessFalseByDefault()
    {
        var result = new ThumbnailGenerationResult();

        result.Success.Should().BeFalse();
        result.OutputPath.Should().BeEmpty();
        result.FileSizeBytes.Should().Be(0);
    }

    // ── OutputDirectory auto-creation ─────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ThumbnailGeneratorService.GenerateFromVideoAsync"/> creates the requested output directory if it does not exist before processing.
    /// </summary>
    [Fact]
    public async Task GenerateFromVideoAsync_CreatesOutputDirectoryIfMissing()
    {
        var tempVideo = Path.Combine(_tempDir, "video.mp4");
        File.WriteAllBytes(tempVideo, new byte[100]);
        var newSubDir = Path.Combine(_tempDir, "new_subdir");

        var request = new ThumbnailGenerationRequest { OutputDirectory = newSubDir };

        // The call will fail because FFmpeg is not available in CI, but directory creation
        // happens before the FFmpeg invocation, so we catch only VideoProcessingException
        // or a failed result — either way the directory must have been created.
        try
        {
            await _service.GenerateFromVideoAsync(tempVideo, request);
        }
        catch (VideoProcessingException) { /* expected: ffmpeg missing */ }

        Directory.Exists(newSubDir).Should().BeTrue();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ThumbnailGeneratorServiceTests"/> class, deleting the temporary directory.
    /// </summary>
    ~ThumbnailGeneratorServiceTests()
    {
        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }
        catch { /* best-effort cleanup */ }
    }
}