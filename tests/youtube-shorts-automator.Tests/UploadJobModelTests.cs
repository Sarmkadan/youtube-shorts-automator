// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

public class UploadJobModelTests
{
    [Fact]
    public void CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue()
    {
        // Arrange
        var job = new UploadJob
        {
            Status = UploadStatus.Failed,
            AttemptCount = 1,
            MaxRetries = 3
        };

        // Act
        var result = job.CanRetry();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanRetry_WhenAttemptCountMatchesMaxRetries_ReturnsFalse()
    {
        // Arrange
        var job = new UploadJob
        {
            Status = UploadStatus.Failed,
            AttemptCount = 3,
            MaxRetries = 3
        };

        // Act
        var result = job.CanRetry();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage()
    {
        // Arrange
        var job = new UploadJob();
        const long uploadedBytes = 500_000;
        const long totalBytes = 1_000_000;

        // Act
        job.UpdateProgress(uploadedBytes, totalBytes);

        // Assert
        job.UploadProgressPercentage.Should().Be(50.0);
        job.UploadedBytes.Should().Be(uploadedBytes);
    }

    [Fact]
    public void MarkAsCompleted_AssignsVideoIdAndSetsProgressToFull()
    {
        // Arrange
        var job = new UploadJob { VideoShortId = 7 };
        const string youTubeVideoId = "dQw4w9WgXcQ";
        var before = DateTime.UtcNow;

        // Act
        job.MarkAsCompleted(youTubeVideoId);

        // Assert
        job.Status.Should().Be(UploadStatus.Completed);
        job.YouTubeVideoId.Should().Be(youTubeVideoId);
        job.UploadProgressPercentage.Should().Be(100.0);
        job.UploadedAt.Should().NotBeNull();
        job.UploadedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void UploadedBytes_WhenSet_PreservesValueForResume()
    {
        // Arrange
        var job = new UploadJob { UploadedBytes = 5242880 }; // 5MB
        var totalBytes = 10485760; // 10MB

        // Act
        job.UpdateProgress(job.UploadedBytes, totalBytes);

        // Assert
        job.UploadedBytes.Should().Be(5242880);
        job.UploadProgressPercentage.Should().Be(50.0);
    }
}
