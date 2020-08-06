// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Tests for the AnalyticsData class.
/// </summary>
public class AnalyticsDataTests
{
    /// <summary>
    /// Verifies that the RecalculateEngagementMetrics method correctly calculates the engagement rate.
    /// </summary>
    [Fact]
    public void RecalculateEngagementMetrics_WithKnownInteractionCounts_ComputesExpectedRate()
    {
        // Arrange — 1000 views, 50 likes, 20 comments, 10 shares → (80/1000)*100 = 8 %
        var analytics = new AnalyticsData
        {
            ViewCount = 1000,
            LikeCount = 50,
            CommentCount = 20,
            ShareCount = 10
        };

        // Act
        analytics.RecalculateEngagementMetrics();

        // Assert
        analytics.EngagementRate.Should().Be(8.0);
    }

    /// <summary>
    /// Verifies that the GetPerformanceLevel method returns the correct performance level when the engagement rate is above 10.
    /// </summary>
    [Fact]
    public void GetPerformanceLevel_WhenEngagementRateIsAboveTen_ReturnsExcellent()
    {
        // Arrange
        var analytics = new AnalyticsData { EngagementRate = 15.0 };

        // Act
        var level = analytics.GetPerformanceLevel();

        // Assert
        level.Should().Be("Excellent");
    }

    /// <summary>
    /// Verifies that the UpdateRetentionData method clamps the retention percentage at 100 when it exceeds 100.
    /// </summary>
    [Fact]
    public void UpdateRetentionData_WhenRetentionExceedsHundred_ClampsAtMaximum()
    {
        // Arrange
        var analytics = new AnalyticsData();

        // Act
        analytics.UpdateRetentionData(150.0, 5000);

        // Assert
        analytics.AudienceRetentionPercentage.Should().Be(100.0);
        analytics.ImpressionCount.Should().Be(5000);
    }

    /// <summary>
    /// Verifies that the GetAllAsync method returns the expected list of video shorts when called on a mocked repository.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenCalledOnMockedRepository_ReturnsMockedVideoShorts()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<VideoShort>>();
        var expectedVideos = new List<VideoShort>
        {
            new() { Id = 1, Title = "Short #1", FilePath = "/videos/1.mp4", Duration = TimeSpan.FromSeconds(30) },
            new() { Id = 2, Title = "Short #2", FilePath = "/videos/2.mp4", Duration = TimeSpan.FromSeconds(45) }
        };
        mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVideos);

        // Act
        var result = await mockRepository.Object.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(v => v.Id == 1);
        mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
