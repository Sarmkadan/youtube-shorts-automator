// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

public class VideoShortModelTests
{
    private static VideoShort BuildValidVideoShort() => new()
    {
        Title = "My YouTube Short",
        Description = "A quick test video",
        FilePath = "/tmp/videos/test.mp4",
        Duration = TimeSpan.FromSeconds(30),
        ProcessingProfileId = 1,
        YouTubeChannelId = 1
    };

    [Fact]
    public void IsValid_WithValidMetadata_ReturnsTrue()
    {
        // Arrange
        var video = BuildValidVideoShort();

        // Act
        var result = video.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyTitle_ReturnsFalse()
    {
        // Arrange
        var video = BuildValidVideoShort();
        video.Title = string.Empty;

        // Act
        var result = video.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithDurationBeyond60Seconds_ReturnsFalse()
    {
        // Arrange
        var video = BuildValidVideoShort();
        video.Duration = TimeSpan.FromSeconds(61);

        // Act
        var result = video.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessed_WithoutError_SetsCompletedStatusAndTimestamp()
    {
        // Arrange
        var video = BuildValidVideoShort();
        video.Status = ProcessingStatus.Processing;
        var before = DateTime.UtcNow;

        // Act
        video.MarkAsProcessed();

        // Assert
        video.Status.Should().Be(ProcessingStatus.Completed);
        video.ErrorMessage.Should().BeNull();
        video.ProcessedAt.Should().NotBeNull();
        video.ProcessedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkAsProcessed_WithErrorMessage_SetsFailedStatusAndPreservesError()
    {
        // Arrange
        var video = BuildValidVideoShort();
        video.Status = ProcessingStatus.Processing;
        const string errorMessage = "FFmpeg process exited with code 1";

        // Act
        video.MarkAsProcessed(errorMessage);

        // Assert
        video.Status.Should().Be(ProcessingStatus.Failed);
        video.ErrorMessage.Should().Be(errorMessage);
        video.ProcessedAt.Should().BeNull();
    }
}
