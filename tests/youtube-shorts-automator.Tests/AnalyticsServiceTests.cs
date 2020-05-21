// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Unit tests for the <see cref="AnalyticsService"/> class.
/// Tests various scenarios including analytics record creation, synchronization from YouTube,
/// retrieval of analytics data, performance analysis, and channel growth calculations.
/// </summary>
public class AnalyticsServiceTests
{
    private readonly Mock<AnalyticsRepository> _mockAnalyticsRepository;
    private readonly Mock<VideoShortRepository> _mockVideoRepository;
    private readonly Mock<ILogger<AnalyticsService>> _mockLogger;
    private readonly AnalyticsService _service;

    /// <summary>
    /// Initializes mock dependencies and creates an instance of <see cref="AnalyticsService"/> for testing.
    /// </summary>
    public AnalyticsServiceTests()
    {
        _mockAnalyticsRepository = new Mock<AnalyticsRepository>();
        _mockVideoRepository = new Mock<VideoShortRepository>();
        _mockLogger = new Mock<ILogger<AnalyticsService>>();
        _service = new AnalyticsService(_mockAnalyticsRepository.Object, _mockVideoRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.CreateAnalyticsRecordAsync"/> successfully creates an analytics record
    /// when provided with a valid video ID.
    /// </summary>
    [Fact]
    public async Task CreateAnalyticsRecordAsync_WithValidVideoId_CreatesRecord()
    {
        var videoShortId = 1;
        var createdAnalytics = new AnalyticsData
        {
            Id = 1,
            VideoShortId = videoShortId,
            ViewCount = 0,
            LikeCount = 0,
            UpdatedAt = DateTime.UtcNow
        };

        _mockAnalyticsRepository
            .Setup(r => r.AddAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAnalytics);

        var result = await _service.CreateAnalyticsRecordAsync(videoShortId);

        result.Should().NotBeNull();
        result.VideoShortId.Should().Be(videoShortId);
        result.ViewCount.Should().Be(0);
        result.EngagementRate.Should().Be(0);
        _mockAnalyticsRepository.Verify(r => r.AddAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.CreateAnalyticsRecordAsync"/> throws an <see cref="InvalidOperationException"/>
    /// when the repository encounters an error during analytics record creation.
    /// </summary>
    [Fact]
    public async Task CreateAnalyticsRecordAsync_WithRepositoryException_ThrowsInvalidOperationException()
    {
        _mockAnalyticsRepository
            .Setup(r => r.AddAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        Func<Task> act = () => _service.CreateAnalyticsRecordAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to create analytics record*");
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.SyncAnalyticsFromYouTubeAsync"/> successfully updates existing analytics
    /// when provided with valid video ID, YouTube video ID, and channel information.
    /// </summary>
    [Fact]
    public async Task SyncAnalyticsFromYouTubeAsync_WithValidInputs_UpdatesAnalytics()
    {
        var videoId = 1;
        var youtubeVideoId = "dQw4w9WgXcQ";
        var channel = new YouTubeChannel { Id = 1, ChannelId = "UCexample" };

        var existingAnalytics = new AnalyticsData { Id = 1, VideoShortId = videoId };
        var updatedAnalytics = new AnalyticsData
        {
            Id = 1,
            VideoShortId = videoId,
            ViewCount = 5000,
            LikeCount = 250,
            UpdatedAt = DateTime.UtcNow
        };

        _mockAnalyticsRepository
            .Setup(r => r.GetByVideoIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnalytics);

        _mockAnalyticsRepository
            .Setup(r => r.UpdateAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedAnalytics);

        var result = await _service.SyncAnalyticsFromYouTubeAsync(videoId, youtubeVideoId, channel);

        result.Should().NotBeNull();
        result.ViewCount.Should().BeGreaterThan(0);
        _mockAnalyticsRepository.Verify(r => r.UpdateAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.SyncAnalyticsFromYouTubeAsync"/> throws an <see cref="ArgumentNullException"/>
    /// when the YouTube video ID parameter is null.
    /// </summary>
    [Fact]
    public async Task SyncAnalyticsFromYouTubeAsync_WithNullYoutubeId_ThrowsArgumentNullException()
    {
        var channel = new YouTubeChannel();

        Func<Task> act = () => _service.SyncAnalyticsFromYouTubeAsync(1, null!, channel);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.SyncAnalyticsFromYouTubeAsync"/> throws an <see cref="ArgumentNullException"/>
    /// when the channel parameter is null.
    /// </summary>
    [Fact]
    public async Task SyncAnalyticsFromYouTubeAsync_WithNullChannel_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.SyncAnalyticsFromYouTubeAsync(1, "videoId", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.SyncAnalyticsFromYouTubeAsync"/> creates a new analytics record
    /// when no existing record is found for the specified video ID.
    /// </summary>
    [Fact]
    public async Task SyncAnalyticsFromYouTubeAsync_WhenNoExistingRecord_CreatesNew()
    {
        var videoId = 1;
        var youtubeVideoId = "video123";
        var channel = new YouTubeChannel();

        _mockAnalyticsRepository
            .Setup(r => r.GetByVideoIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnalyticsData?)null);

        _mockAnalyticsRepository
            .Setup(r => r.AddAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsData { VideoShortId = videoId });

        _mockAnalyticsRepository
            .Setup(r => r.UpdateAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnalyticsData { VideoShortId = videoId, ViewCount = 1000 });

        var result = await _service.SyncAnalyticsFromYouTubeAsync(videoId, youtubeVideoId, channel);

        result.Should().NotBeNull();
        _mockAnalyticsRepository.Verify(r => r.AddAsync(It.IsAny<AnalyticsData>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GetVideoAnalyticsAsync"/> successfully retrieves analytics data
    /// when provided with a valid video ID that exists in the repository.
    /// </summary>
    [Fact]
    public async Task GetVideoAnalyticsAsync_WithValidVideoId_ReturnsAnalytics()
    {
        var videoId = 1;
        var analytics = new AnalyticsData { Id = 1, VideoShortId = videoId, ViewCount = 1000 };

        _mockAnalyticsRepository
            .Setup(r => r.GetByVideoIdAsync(videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        var result = await _service.GetVideoAnalyticsAsync(videoId);

        result.Should().NotBeNull();
        result.ViewCount.Should().Be(1000);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GetVideoAnalyticsAsync"/> returns null
    /// when no analytics record exists for the specified video ID.
    /// </summary>
    [Fact]
    public async Task GetVideoAnalyticsAsync_WhenNotFound_ReturnsNull()
    {
        _mockAnalyticsRepository
            .Setup(r => r.GetByVideoIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnalyticsData?)null);

        var result = await _service.GetVideoAnalyticsAsync(999);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GetTopPerformingVideosAsync"/> successfully retrieves top performing videos
    /// when provided with a valid limit value.
    /// </summary>
    [Fact]
    public async Task GetTopPerformingVideosAsync_WithValidLimit_ReturnsTopVideos()
    {
        var topVideos = new List<AnalyticsData>
        {
            new() { ViewCount = 10000, EngagementRate = 12.5 },
            new() { ViewCount = 8000, EngagementRate = 10.2 }
        };

        _mockAnalyticsRepository
            .Setup(r => r.GetTopPerformersAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topVideos);

        var result = await _service.GetTopPerformingVideosAsync(10);

        result.Should().HaveCount(2);
        result.First().ViewCount.Should().Be(10000);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GetTopPerformingVideosAsync"/> throws an <see cref="ArgumentOutOfRangeException"/>
    /// when the limit parameter is zero.
    /// </summary>
    [Fact]
    public async Task GetTopPerformingVideosAsync_WithInvalidLimit_ThrowsArgumentOutOfRangeException()
    {
        Func<Task> act = () => _service.GetTopPerformingVideosAsync(0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GetTopPerformingVideosAsync"/> throws an <see cref="ArgumentOutOfRangeException"/>
    /// when the limit parameter is negative.
    /// </summary>
    [Fact]
    public async Task GetTopPerformingVideosAsync_WithNegativeLimit_ThrowsArgumentOutOfRangeException()
    {
        Func<Task> act = () => _service.GetTopPerformingVideosAsync(-5);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GeneratePeriodReportAsync"/> successfully generates a period report
    /// when provided with valid date range parameters.
    /// </summary>
    [Fact]
    public async Task GeneratePeriodReportAsync_WithValidDateRange_ReturnsReport()
    {
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;
        var analytics = new List<AnalyticsData>
        {
            new() { ViewCount = 5000, LikeCount = 250, CommentCount = 50, ShareCount = 25, EngagementRate = 6.5 },
            new() { ViewCount = 3000, LikeCount = 150, CommentCount = 30, ShareCount = 15, EngagementRate = 6.5 }
        };

        _mockAnalyticsRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        var report = await _service.GeneratePeriodReportAsync(startDate, endDate);

        report.Should().NotBeNull();
        report.TotalVideos.Should().Be(2);
        report.TotalViews.Should().Be(8000);
        report.TotalLikes.Should().Be(400);
        report.PeriodStart.Should().Be(startDate);
        report.PeriodEnd.Should().Be(endDate);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GeneratePeriodReportAsync"/> throws an <see cref="ArgumentException"/>
    /// when the start date is after the end date (inverted dates).
    /// </summary>
    [Fact]
    public async Task GeneratePeriodReportAsync_WithInvertedDates_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(-1);

        Func<Task> act = () => _service.GeneratePeriodReportAsync(startDate, endDate);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.GeneratePeriodReportAsync"/> returns an empty report
    /// when no analytics data exists within the specified date range.
    /// </summary>
    [Fact]
    public async Task GeneratePeriodReportAsync_WithEmptyAnalytics_ReturnsEmptyReport()
    {
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        _mockAnalyticsRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalyticsData>());

        var report = await _service.GeneratePeriodReportAsync(startDate, endDate);

        report.Should().NotBeNull();
        report.TotalVideos.Should().Be(0);
        report.TotalViews.Should().Be(0);
        report.AverageEngagementRate.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.AnalyzePerformanceMetrics"/> returns performance insights
    /// when provided with valid analytics data containing engagement metrics.
    /// </summary>
    [Fact]
    public void AnalyzePerformanceMetrics_WithValidData_ReturnsInsights()
    {
        var analytics = new AnalyticsData
        {
            ViewCount = 5000,
            EngagementRate = 12.5,
            AverageViewDuration = 25.5,
            AudienceRetentionPercentage = 75.0
        };
        analytics.RecalculateEngagementMetrics();

        var insights = _service.AnalyzePerformanceMetrics(analytics);

        insights.Should().NotBeNullOrEmpty();
        insights.Should().Contain("5000");
        insights.Should().Contain("Excellent engagement");
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.AnalyzePerformanceMetrics"/> returns a warning message
    /// when analytics data indicates low engagement metrics.
    /// </summary>
    [Fact]
    public void AnalyzePerformanceMetrics_WithLowEngagement_ReturnsWarning()
    {
        var analytics = new AnalyticsData
        {
            ViewCount = 1000,
            LikeCount = 5,
            CommentCount = 2,
            ShareCount = 1,
            EngagementRate = 0.8,
            AverageViewDuration = 5.0,
            AudienceRetentionPercentage = 25.0
        };

        var insights = _service.AnalyzePerformanceMetrics(analytics);

        insights.Should().Contain("Low engagement");
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.AnalyzePerformanceMetrics"/> returns a default message
    /// when provided with empty or invalid analytics data.
    /// </summary>
    [Fact]
    public void AnalyzePerformanceMetrics_WithoutValidData_ReturnsDefaultMessage()
    {
        var analytics = new AnalyticsData();

        var insights = _service.AnalyzePerformanceMetrics(analytics);

        insights.Should().Be("No performance data available yet.");
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.CalculateChannelGrowthAsync"/> returns the total channel growth
    /// by summing subscribers gained and subtracting subscribers lost across all analytics records.
    /// </summary>
    [Fact]
    public async Task CalculateChannelGrowthAsync_WithMultipleAnalytics_ReturnsTotalGrowth()
    {
        var allAnalytics = new List<AnalyticsData>
        {
            new() { SubscribersGained = 100, SubscribersLost = 10 },
            new() { SubscribersGained = 150, SubscribersLost = 20 }
        };

        _mockAnalyticsRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allAnalytics);

        var growth = await _service.CalculateChannelGrowthAsync(1);

        growth.Should().Be(220); // (100 + 150) - (10 + 20)
    }

    /// <summary>
    /// Tests that <see cref="AnalyticsService.CalculateChannelGrowthAsync"/> returns a negative value
    /// when subscribers lost exceeds subscribers gained in the analytics records.
    /// </summary>
    [Fact]
    public async Task CalculateChannelGrowthAsync_WithNegativeGrowth_ReturnsNegativeValue()
    {
        var allAnalytics = new List<AnalyticsData>
        {
            new() { SubscribersGained = 50, SubscribersLost = 100 }
        };

        _mockAnalyticsRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allAnalytics);

        var growth = await _service.CalculateChannelGrowthAsync(1);

        growth.Should().Be(-50);
    }
}
