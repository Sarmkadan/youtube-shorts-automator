// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for the <see cref="VideoShort"/> class validation and processing logic.
/// </summary>
public class VideoShortModelTests
{
    /// <summary>
    /// Creates a valid <see cref="VideoShort"/> instance with default test values.
    /// </summary>
    /// <returns>A configured <see cref="VideoShort"/> instance with valid properties.</returns>
    private static VideoShort BuildValidVideoShort() => new()
    {
        Title = "My YouTube Short",
        Description = "A quick test video",
        FilePath = "/tmp/videos/test.mp4",
        Duration = TimeSpan.FromSeconds(30),
        ProcessingProfileId = 1,
        YouTubeChannelId = 1
    };

    /// <summary>
    /// Verifies that a <see cref="VideoShort"/> with valid metadata returns true for <see cref="VideoShort.IsValid"/>.
    /// </summary>
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

    /// <summary>
    /// Verifies that a <see cref="VideoShort"/> with empty title returns false for <see cref="VideoShort.IsValid"/>.
    /// </summary>
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

    /// <summary>
    /// Verifies that a <see cref="VideoShort"/> with duration exceeding 60 seconds returns false for <see cref="VideoShort.IsValid"/>.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="VideoShort.MarkAsProcessed"/> sets the status to Completed, clears error message, and records the processed timestamp.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="VideoShort.MarkAsProcessed(string)"/> sets the status to Failed, preserves the error message, and clears the processed timestamp.
    /// </summary>
    /// <param name="errorMessage">The error message to pass to <see cref="VideoShort.MarkAsProcessed(string)"/>.</param>
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
