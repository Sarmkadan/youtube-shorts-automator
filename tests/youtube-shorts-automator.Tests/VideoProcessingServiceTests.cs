// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

public class VideoProcessingServiceTests
{
    private readonly Mock<VideoShortRepository> _mockRepository;
    private readonly Mock<ILogger<VideoProcessingService>> _mockLogger;
    private readonly VideoProcessingService _service;

    public VideoProcessingServiceTests()
    {
        _mockRepository = new Mock<VideoShortRepository>();
        _mockLogger = new Mock<ILogger<VideoProcessingService>>();
        _service = new VideoProcessingService(_mockRepository.Object, _mockLogger.Object);
    }

    private VideoShort BuildValidVideoShort() => new()
    {
        Title = "Test Video",
        Description = "A test video",
        FilePath = "/tmp/test.mp4",
        Duration = TimeSpan.FromSeconds(30),
        FileSizeBytes = 10 * 1024 * 1024,
        Quality = VideoQuality.High,
        ProcessingProfileId = 1,
        YouTubeChannelId = 1
    };

    [Fact]
    public async Task ValidateVideoFileAsync_WithValidFile_ReturnsTrue()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        var filePath = Path.Combine(testDir, "test.mp4");
        var testBytes = new byte[10 * 1024 * 1024];
        File.WriteAllBytes(filePath, testBytes);

        try
        {
            var result = await _service.ValidateVideoFileAsync(filePath);

            result.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (Directory.Exists(testDir)) Directory.Delete(testDir);
        }
    }

    [Fact]
    public async Task ValidateVideoFileAsync_WithNonExistentFile_ReturnsFalse()
    {
        var result = await _service.ValidateVideoFileAsync("/nonexistent/file.mp4");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateVideoFileAsync_WithFileTooLarge_ReturnsFalse()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        var filePath = Path.Combine(testDir, "large.mp4");
        var testBytes = new byte[Constants.Constants.MAX_FILE_SIZE_BYTES + 1024];
        File.WriteAllBytes(filePath, testBytes);

        try
        {
            var result = await _service.ValidateVideoFileAsync(filePath);

            result.Should().BeFalse();
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (Directory.Exists(testDir)) Directory.Delete(testDir);
        }
    }

    [Fact]
    public async Task ValidateVideoFileAsync_WithFileTooSmall_ReturnsFalse()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        var filePath = Path.Combine(testDir, "tiny.mp4");
        var testBytes = new byte[100];
        File.WriteAllBytes(filePath, testBytes);

        try
        {
            var result = await _service.ValidateVideoFileAsync(filePath);

            result.Should().BeFalse();
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (Directory.Exists(testDir)) Directory.Delete(testDir);
        }
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_WithValidVideo_CreatesTask()
    {
        var videoShort = BuildValidVideoShort();
        var savedVideo = new VideoShort
        {
            Id = 1,
            Title = videoShort.Title,
            Status = ProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<VideoShort>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedVideo);

        var result = await _service.CreateProcessingTaskAsync(videoShort);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Status.Should().Be(ProcessingStatus.Pending);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<VideoShort>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_WithInvalidVideo_ThrowsValidationException()
    {
        var videoShort = BuildValidVideoShort();
        videoShort.Title = string.Empty;

        Func<Task> act = () => _service.CreateProcessingTaskAsync(videoShort);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_WithDurationTooLong_ThrowsValidationException()
    {
        var videoShort = BuildValidVideoShort();
        videoShort.Duration = TimeSpan.FromSeconds(61);

        Func<Task> act = () => _service.CreateProcessingTaskAsync(videoShort);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_WithRepositoryException_ThrowsVideoProcessingException()
    {
        var videoShort = BuildValidVideoShort();

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<VideoShort>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        Func<Task> act = () => _service.CreateProcessingTaskAsync(videoShort);

        await act.Should().ThrowAsync<VideoProcessingException>();
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_SetsStatusToPending()
    {
        var videoShort = BuildValidVideoShort();
        VideoShort capturedVideo = null!;

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<VideoShort>(), It.IsAny<CancellationToken>()))
            .Callback<VideoShort, CancellationToken>((v, _) => capturedVideo = v)
            .ReturnsAsync(new VideoShort { Id = 1, Status = ProcessingStatus.Pending });

        await _service.CreateProcessingTaskAsync(videoShort);

        capturedVideo.Status.Should().Be(ProcessingStatus.Pending);
    }

    [Fact]
    public async Task CreateProcessingTaskAsync_SetsCreatedAtToCurrentTime()
    {
        var videoShort = BuildValidVideoShort();
        var beforeTime = DateTime.UtcNow;
        VideoShort capturedVideo = null!;

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<VideoShort>(), It.IsAny<CancellationToken>()))
            .Callback<VideoShort, CancellationToken>((v, _) => capturedVideo = v)
            .ReturnsAsync(new VideoShort { Id = 1 });

        await _service.CreateProcessingTaskAsync(videoShort);

        var afterTime = DateTime.UtcNow;
        capturedVideo.CreatedAt.Should().BeOnOrAfter(beforeTime);
        capturedVideo.CreatedAt.Should().BeOnOrBefore(afterTime);
    }

    [Fact]
    public async Task ProcessVideoAsync_WithValidInputs_ReturnsProcessingTask()
    {
        var videoShort = BuildValidVideoShort();
        var profile = new ProcessingProfile
        {
            Id = 1,
            VideoWidth = 1080,
            VideoHeight = 1920,
            VideoBitrate = 4000,
            AudioBitrate = 128,
            FrameRate = 30,
            VideoCodec = "h264",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = true
        };

        var result = await _service.ProcessVideoAsync(videoShort, profile);

        result.Should().NotBeNull();
        result.VideoShortId.Should().Be(videoShort.Id);
        result.Status.Should().Be(ProcessingStatus.Processing);
        result.OutputWidth.Should().Be(1080);
        result.OutputHeight.Should().Be(1920);
    }

    [Fact]
    public async Task ProcessVideoAsync_WithInvalidProfile_ThrowsValidationException()
    {
        var videoShort = BuildValidVideoShort();
        var invalidProfile = new ProcessingProfile { IsActive = false };

        Func<Task> act = () => _service.ProcessVideoAsync(videoShort, invalidProfile);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ProcessVideoAsync_SetsCorrectPriority(int expectedPriority)
    {
        var videoShort = new VideoShort
        {
            Title = "Test",
            Description = "Test",
            FilePath = "/test.mp4",
            Duration = TimeSpan.FromSeconds(30),
            FileSizeBytes = 10 * 1024 * 1024,
            ProcessingProfileId = 1,
            YouTubeChannelId = 1
        };

        var profile = new ProcessingProfile
        {
            Id = 1,
            VideoWidth = 1080,
            VideoHeight = 1920,
            VideoBitrate = 4000,
            AudioBitrate = 128,
            FrameRate = 30,
            VideoCodec = "h264",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = true
        };

        var result = await _service.ProcessVideoAsync(videoShort, profile);

        result.Priority.Should().Be(5);
    }

    [Fact]
    public async Task ProcessVideoAsync_WithDifferentProfiles_AppliesCorrectSettings()
    {
        var videoShort = BuildValidVideoShort();

        var profile1 = new ProcessingProfile
        {
            Id = 1,
            VideoWidth = 720,
            VideoHeight = 1280,
            VideoBitrate = 2000,
            AudioBitrate = 96,
            FrameRate = 24,
            VideoCodec = "h264",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = true
        };

        var profile2 = new ProcessingProfile
        {
            Id = 2,
            VideoWidth = 1080,
            VideoHeight = 1920,
            VideoBitrate = 5000,
            AudioBitrate = 192,
            FrameRate = 60,
            VideoCodec = "h265",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = true
        };

        var result1 = await _service.ProcessVideoAsync(videoShort, profile1);
        var result2 = await _service.ProcessVideoAsync(videoShort, profile2);

        result1.OutputWidth.Should().Be(720);
        result2.OutputWidth.Should().Be(1080);
        result1.OutputBitrate.Should().Be(2000);
        result2.OutputBitrate.Should().Be(5000);
    }

    [Fact]
    public async Task ProcessVideoAsync_Sets_FFmpegTranscodeTaskType()
    {
        var videoShort = BuildValidVideoShort();
        var profile = new ProcessingProfile
        {
            Id = 1,
            VideoWidth = 1080,
            VideoHeight = 1920,
            VideoBitrate = 4000,
            AudioBitrate = 128,
            FrameRate = 30,
            VideoCodec = "h264",
            AudioCodec = "aac",
            Container = "mp4",
            IsActive = true
        };

        var result = await _service.ProcessVideoAsync(videoShort, profile);

        result.TaskType.Should().Be("FFmpegTranscode");
    }
}
