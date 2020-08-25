// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Unit tests for <see cref="ContentCalendarService"/> functionality.
/// Tests cover CRUD operations, validation, scheduling, and optimization features.
/// </summary>
public class ContentCalendarServiceTests
{
    private readonly Mock<ContentCalendarRepository> _mockCalendarRepo;

    /// <summary>
    /// Mock for scheduling service used to test content calendar scheduling operations.
    /// </summary>
    private readonly Mock<SchedulingService> _mockScheduling;

    /// <summary>
    /// Mock for analytics service used to test content performance tracking.
    /// </summary>
    private readonly Mock<AnalyticsService> _mockAnalytics;

    /// <summary>
    /// Mock for video repository used to test video-related operations.
    /// </summary>
    private readonly Mock<VideoShortRepository> _mockVideoRepo;

    /// <summary>
    /// Mock for title optimization engine used to test title optimization features.
    /// </summary>
    private readonly Mock<ITitleOptimizationEngine> _mockOptimizer;

    /// <summary>
    /// Content calendar configuration options for testing.
    /// </summary>
    private readonly ContentCalendarOptions _options;

    /// <summary>
    /// Instance of ContentCalendarService under test with mocked dependencies.
    /// </summary>
    private readonly ContentCalendarService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCalendarServiceTests"/> class.
    /// Sets up mock dependencies for testing ContentCalendarService functionality.
    /// </summary>
    public ContentCalendarServiceTests()
    {
        _mockCalendarRepo = new Mock<ContentCalendarRepository>();
        _mockScheduling = new Mock<SchedulingService>();
        _mockAnalytics = new Mock<AnalyticsService>();
        _mockVideoRepo = new Mock<VideoShortRepository>();
        _mockOptimizer = new Mock<ITitleOptimizationEngine>();
        _options = new ContentCalendarOptions { AutoOptimizeOnCreate = false };

        _service = new ContentCalendarService(
            _mockCalendarRepo.Object,
            _mockScheduling.Object,
            _mockAnalytics.Object,
            _mockVideoRepo.Object,
            _mockOptimizer.Object,
            _options,
            Mock.Of<ILogger<ContentCalendarService>>());
    }

    /// <summary>
    /// Builds a valid ContentCalendarEntry for testing purposes.
    /// </summary>
    /// <returns>A valid ContentCalendarEntry with sample data.</returns>
    private static ContentCalendarEntry BuildValidEntry() => new()
    {
        Title = "Tutorial: Python in 10 minutes",
        Description = "Quick intro to Python",
        Tags = ["python", "tutorial"],
        Category = ContentCategory.Tutorial,
        ScheduledPublishAt = DateTime.UtcNow.AddDays(1),
        YouTubeChannelId = 1
    };

    // ── CreateEntryAsync ──────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync successfully creates and persists a valid content calendar entry.
    /// </summary>
    public async Task CreateEntryAsync_WithValidEntry_ReturnsPersistedEntry()
    {
        var entry = BuildValidEntry();
        var saved = new ContentCalendarEntry { Id = 42, Title = entry.Title, YouTubeChannelId = 1 };

        _mockCalendarRepo
            .Setup(r => r.AddAsync(It.IsAny<ContentCalendarEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);

        var result = await _service.CreateEntryAsync(entry);

        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        _mockCalendarRepo.Verify(r => r.AddAsync(It.IsAny<ContentCalendarEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync throws ArgumentNullException when null entry is provided.
    /// </summary>
    public async Task CreateEntryAsync_NullEntry_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.CreateEntryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync throws ValidationException when entry has empty title.
    /// </summary>
    public async Task CreateEntryAsync_EmptyTitle_ThrowsValidationException()
    {
        var entry = BuildValidEntry();
        entry.Title = string.Empty;

        Func<Task> act = () => _service.CreateEntryAsync(entry);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync throws ValidationException when title exceeds maximum length.
    /// </summary>
    public async Task CreateEntryAsync_TitleTooLong_ThrowsValidationException()
    {
        var entry = BuildValidEntry();
        entry.Title = new string('x', _options.MaxTitleLength + 1);

        Func<Task> act = () => _service.CreateEntryAsync(entry);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync throws ValidationException when YouTubeChannelId is invalid.
    /// </summary>
    public async Task CreateEntryAsync_InvalidChannelId_ThrowsValidationException()
    {
        var entry = BuildValidEntry();
        entry.YouTubeChannelId = 0;

        Func<Task> act = () => _service.CreateEntryAsync(entry);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync throws ValidationException when entry has too many tags.
    /// </summary>
    public async Task CreateEntryAsync_TooManyTags_ThrowsValidationException()
    {
        var entry = BuildValidEntry();
        entry.Tags = Enumerable.Range(0, _options.MaxTagCount + 1).Select(i => $"tag{i}").ToArray();

        Func<Task> act = () => _service.CreateEntryAsync(entry);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    /// <summary>
    /// Tests that CreateEntryAsync sets CreatedAt and UpdatedAt timestamps on the entry.
    /// </summary>
    public async Task CreateEntryAsync_SetsCreatedAtAndUpdatedAt()
    {
        var entry = BuildValidEntry();
        ContentCalendarEntry? captured = null;
        var before = DateTime.UtcNow;

        _mockCalendarRepo
            .Setup(r => r.AddAsync(It.IsAny<ContentCalendarEntry>(), It.IsAny<CancellationToken>()))
            .Callback<ContentCalendarEntry, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync(new ContentCalendarEntry { Id = 1, Title = "T", YouTubeChannelId = 1 });

        await _service.CreateEntryAsync(entry);

        captured!.CreatedAt.Should().BeOnOrAfter(before);
        captured.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // ── GetEntryAsync ─────────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that GetEntryAsync returns an existing entry by ID.
    /// </summary>
    public async Task GetEntryAsync_ExistingEntry_ReturnsIt()
    {
        var stored = new ContentCalendarEntry { Id = 5, Title = "My Video", YouTubeChannelId = 1 };
        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var result = await _service.GetEntryAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    /// <summary>
    /// Tests that GetEntryAsync returns null when entry does not exist.
    /// </summary>
    public async Task GetEntryAsync_NonExistingEntry_ReturnsNull()
    {
        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentCalendarEntry?)null);

        var result = await _service.GetEntryAsync(999);

        result.Should().BeNull();
    }

    // ── GetEntriesInRangeAsync ────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that GetEntriesInRangeAsync throws ArgumentException when start date is after end date.
    /// </summary>
    public async Task GetEntriesInRangeAsync_StartAfterEnd_ThrowsArgumentException()
    {
        var from = DateTime.UtcNow.AddDays(2);
        var to = DateTime.UtcNow;

        Func<Task> act = () => _service.GetEntriesInRangeAsync(from, to);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    /// <summary>
    /// Tests that GetEntriesInRangeAsync delegates to repository with valid date range.
    /// </summary>
    public async Task GetEntriesInRangeAsync_ValidRange_DelegatesToRepository()
    {
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(7);
        _mockCalendarRepo
            .Setup(r => r.GetByDateRangeAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentCalendarEntry>());

        var result = await _service.GetEntriesInRangeAsync(from, to);

        result.Should().NotBeNull();
        _mockCalendarRepo.Verify(r => r.GetByDateRangeAsync(from, to, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetUpcomingEntriesAsync ───────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that GetUpcomingEntriesAsync throws ArgumentOutOfRangeException when days parameter is zero.
    /// </summary>
    public async Task GetUpcomingEntriesAsync_ZeroDays_ThrowsArgumentOutOfRangeException()
    {
        Func<Task> act = () => _service.GetUpcomingEntriesAsync(0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    /// <summary>
    /// Tests that GetUpcomingEntriesAsync returns upcoming entries for the specified number of days.
    /// </summary>
    public async Task GetUpcomingEntriesAsync_PositiveDays_ReturnsEntries()
    {
        var entries = new List<ContentCalendarEntry>
        {
            new() { Id = 1, Title = "Video A", YouTubeChannelId = 1 }
        };

        _mockCalendarRepo
            .Setup(r => r.GetUpcomingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = (await _service.GetUpcomingEntriesAsync(7)).ToList();

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Video A");
    }

    // ── DeleteEntryAsync ──────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that DeleteEntryAsync returns true when entry exists and is successfully deleted.
    /// </summary>
    public async Task DeleteEntryAsync_ExistingEntry_ReturnsTrue()
    {
        _mockCalendarRepo
            .Setup(r => r.DeleteAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.DeleteEntryAsync(10);

        result.Should().BeTrue();
    }

    [Fact]
    /// <summary>
    /// Tests that DeleteEntryAsync returns false when entry does not exist.
    /// </summary>
    public async Task DeleteEntryAsync_NonExistingEntry_ReturnsFalse()
    {
        _mockCalendarRepo
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.DeleteEntryAsync(999);

        result.Should().BeFalse();
    }

    // ── GetRecommendedSlotsAsync ──────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that GetRecommendedSlotsAsync delegates to the title optimization engine.
    /// </summary>
    public async Task GetRecommendedSlotsAsync_DelegatesToOptimizationEngine()
    {
        var expected = new[] { DateTime.UtcNow.AddDays(1) };

        _mockOptimizer
            .Setup(o => o.RecommendPostingTimesAsync(1, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var slots = (await _service.GetRecommendedSlotsAsync(channelId: 1, count: 3)).ToList();

        slots.Should().HaveCount(1);
        _mockOptimizer.Verify(o => o.RecommendPostingTimesAsync(1, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ApplyOptimizationAsync ────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that ApplyOptimizationAsync throws InvalidOperationException when no optimization is stored.
    /// </summary>
    public async Task ApplyOptimizationAsync_WhenNoOptimizationStored_ThrowsInvalidOperationException()
    {
        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentCalendarEntry
            {
                Id = 1,
                Title = "Draft",
                YouTubeChannelId = 1,
                LastOptimization = null
            });

        Func<Task> act = () => _service.ApplyOptimizationAsync(1, 0);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no stored optimisation*");
    }

    [Fact]
    /// <summary>
    /// Tests that ApplyOptimizationAsync throws ArgumentOutOfRangeException when suggestion index is out of range.
    /// </summary>
    public async Task ApplyOptimizationAsync_OutOfRangeIndex_ThrowsArgumentOutOfRangeException()
    {
        var optimization = new TitleOptimizationResult(
            OriginalTitle: "Title",
            OriginalDescription: string.Empty,
            Suggestions: [
                new OptimizationSuggestion("Better title", string.Empty, [], 0.9, "test")
            ],
            OptimalPostingHour: 17,
            RecommendedHashtags: [],
            GeneratedAt: DateTime.UtcNow);

        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentCalendarEntry
            {
                Id = 1,
                Title = "Draft",
                YouTubeChannelId = 1,
                LastOptimization = optimization
            });

        Func<Task> act = () => _service.ApplyOptimizationAsync(1, suggestionIndex: 5);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ── UpdateEntryAsync ──────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that UpdateEntryAsync throws ArgumentNullException when null entry is provided.
    /// </summary>
    public async Task UpdateEntryAsync_NullEntry_ThrowsArgumentNullException()
    {
        Func<Task> act = () => _service.UpdateEntryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    /// <summary>
    /// Tests that UpdateEntryAsync throws ValidationException when entry does not exist.
    /// </summary>
    public async Task UpdateEntryAsync_NonExistingEntry_ThrowsValidationException()
    {
        _mockCalendarRepo
            .Setup(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var entry = BuildValidEntry();
        entry.Id = 999;

        Func<Task> act = () => _service.UpdateEntryAsync(entry);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── ScheduleEntryAsync ────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that ScheduleEntryAsync throws InvalidOperationException when entry is already published.
    /// </summary>
    public async Task ScheduleEntryAsync_EntryAlreadyPublished_ThrowsInvalidOperationException()
    {
        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentCalendarEntry
            {
                Id = 1,
                Title = "Published",
                Status = CalendarEntryStatus.Published,
                YouTubeChannelId = 1
            });

        Func<Task> act = () => _service.ScheduleEntryAsync(1, DateTime.UtcNow.AddDays(1));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    /// <summary>
    /// Tests that ScheduleEntryAsync throws InvalidOperationException when entry has no video linked.
    /// </summary>
    public async Task ScheduleEntryAsync_NoVideoLinked_ThrowsInvalidOperationException()
    {
        _mockCalendarRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContentCalendarEntry
            {
                Id = 1,
                Title = "Draft",
                Status = CalendarEntryStatus.Approved,
                VideoShortId = null,
                YouTubeChannelId = 1
            });

        Func<Task> act = () => _service.ScheduleEntryAsync(1, DateTime.UtcNow.AddDays(1));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*VideoShort*");
    }
}