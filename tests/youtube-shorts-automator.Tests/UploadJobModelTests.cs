// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for the <see cref="UploadJob"/> model.
/// </summary>
public class UploadJobModelTests
{
    /// <summary>
    /// Verifies that <see cref="UploadJob.CanRetry"/> returns <c>true</c>
    /// when the job status is <see cref="UploadStatus.Failed"/> and the current
    /// <see cref="UploadJob.AttemptCount"/> is less than <see cref="UploadJob.MaxRetries"/>.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="UploadJob.CanRetry"/> returns <c>false</c>
    /// when the <see cref="UploadJob.AttemptCount"/> equals <see cref="UploadJob.MaxRetries"/>
    /// for a job whose status is <see cref="UploadStatus.Failed"/>.
    /// </summary>
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

    /// <summary>
    /// Ensures that <see cref="UploadJob.UpdateProgress"/> correctly calculates the
    /// <see cref="UploadJob.UploadProgressPercentage"/> when half of the total bytes
    /// have been transferred, and that the <see cref="UploadJob.UploadedBytes"/> property
    /// is set to the supplied uploaded byte count.
    /// </summary>
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

    /// <summary>
    /// Confirms that <see cref="UploadJob.MarkAsCompleted"/> sets the job status to
    /// <see cref="UploadStatus.Completed"/>, records the supplied YouTube video identifier,
    /// sets the progress percentage to 100, and populates <see cref="UploadJob.UploadedAt"/>
    /// with a timestamp that is on or after the method call.
    /// </summary>
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

    /// <summary>
    /// Validates that when <see cref="UploadJob.UploadedBytes"/> is pre‑set,
    /// calling <see cref="UploadJob.UpdateProgress"/> with the same uploaded byte count
    /// preserves the value and correctly computes the progress percentage.
    /// </summary>
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
