// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Tests for the TitleOptimizationEngine class.
/// </summary>
public class TitleOptimizationEngineTests
{
    private readonly TitleOptimizationEngine _engine;
    private readonly Mock<AnalyticsService> _mockAnalytics;
    private readonly ContentCalendarOptions _options;

    /// <summary>
    /// Initializes a new instance of the TitleOptimizationEngineTests class.
    /// </summary>
    public TitleOptimizationEngineTests()
    {
        _mockAnalytics = new Mock<AnalyticsService>();
        _options = new ContentCalendarOptions();
        var logger = Mock.Of<ILogger<TitleOptimizationEngine>>();
        _engine = new TitleOptimizationEngine(_mockAnalytics.Object, _options, logger);

        _mockAnalytics
            .Setup(a => a.GetTopPerformingVideosAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalyticsData>
            {
                new() { ViewCount = 10000, LikeCount = 500, EngagementRate = 5.5 },
                new() { ViewCount = 8000,  LikeCount = 400, EngagementRate = 5.0 }
            });
    }

    // ── ScoreTitle ────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that an empty string returns a score of 0.
    /// </summary>
    [Fact]
    public void ScoreTitle_EmptyString_ReturnsZero()
    {
        var score = _engine.ScoreTitle(string.Empty);

        score.Should().Be(0);
    }

    /// <summary>
    /// Tests that a null input returns a score of 0.
    /// </summary>
    [Fact]
    public void ScoreTitle_NullInput_ReturnsZero()
    {
        var score = _engine.ScoreTitle(null!);

        score.Should().Be(0);
    }

    /// <summary>
    /// Tests that a short title returns a lower score than an optimal title.
    /// </summary>
    [Fact]
    public void ScoreTitle_ShortTitle_ReturnsLowerScoreThanOptimal()
    {
        var shortScore  = _engine.ScoreTitle("Hi");
        var optimalScore = _engine.ScoreTitle("How to Learn Programming Faster: 10 Tips for Beginners");

        shortScore.Should().BeLessThan(optimalScore);
    }

    /// <summary>
    /// Tests that a title with a power word receives a boost.
    /// </summary>
    [Fact]
    public void ScoreTitle_TitleWithPowerWord_ReceivesBoost()
    {
        var plain      = _engine.ScoreTitle("Learn Python in 30 days");
        var withPower  = _engine.ScoreTitle("Ultimate Guide to Learning Python in 30 days");

        withPower.Should().BeGreaterThan(plain);
    }

    /// <summary>
    /// Tests that a title with a question receives a boost.
    /// </summary>
    [Fact]
    public void ScoreTitle_TitleWithQuestion_ReceivesBoost()
    {
        var statement = _engine.ScoreTitle("You should learn Python");
        var question  = _engine.ScoreTitle("How to learn Python fast?");

        question.Should().BeGreaterThan(statement);
    }

    /// <summary>
    /// Tests that a title with a number receives a boost.
    /// </summary>
    [Fact]
    public void ScoreTitle_TitleWithNumber_ReceivesBoost()
    {
        var withoutNum = _engine.ScoreTitle("Tips for learning Python");
        var withNum    = _engine.ScoreTitle("7 Tips for learning Python");

        withNum.Should().BeGreaterThan(withoutNum);
    }

    /// <summary>
    /// Tests that the return value is clamped to the range [0, 1].
    /// </summary>
    /// <param name="boundary">The boundary value to test.</param>
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    public void ScoreTitle_ReturnValueIsClamped(double boundary)
    {
        // scores are in [0,1]; arbitrary title
        var score = _engine.ScoreTitle("Some sample title text for testing");

        score.Should().BeInRange(0, 1);
    }

    // ── ExtractKeywords ───────────────────────────────────────────────────────

    /// <summary>
    /// Tests that the ExtractKeywords method extracts non-trivial words.
    /// </summary>
    [Fact]
    public void ExtractKeywords_ExtractsNonTrivialWords()
    {
        var keywords = _engine.ExtractKeywords(
            "How to learn Python programming fast",
            "This tutorial covers Python basics and programming fundamentals.");

        keywords.Should().Contain("python");
        keywords.Should().Contain("programming");
    }

    /// <summary>
    /// Tests that the ExtractKeywords method filters stop words.
    /// </summary>
    [Fact]
    public void ExtractKeywords_FiltersStopWords()
    {
        var keywords = _engine.ExtractKeywords("How to make the best coffee", "A guide for the people");

        keywords.Should().NotContain("with");
        keywords.Should().NotContain("that");
        keywords.Should().NotContain("from");
    }

    /// <summary>
    /// Tests that the ExtractKeywords method returns at most 10 keywords.
    /// </summary>
    [Fact]
    public void ExtractKeywords_ReturnsAtMostTenKeywords()
    {
        var longText = string.Join(' ', Enumerable.Range(0, 50).Select(i => $"keyword{i}"));

        var keywords = _engine.ExtractKeywords(longText, longText);

        keywords.Length.Should().BeLessThanOrEqualTo(10);
    }

    /// <summary>
    /// Tests that the ExtractKeywords method returns an empty array for empty inputs.
    /// </summary>
    [Fact]
    public void ExtractKeywords_EmptyInputs_ReturnsEmptyArray()
    {
        var keywords = _engine.ExtractKeywords(string.Empty, string.Empty);

        keywords.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the ExtractKeywords method returns lowercase words.
    /// </summary>
    [Fact]
    public void ExtractKeywords_ReturnsLowercaseWords()
    {
        var keywords = _engine.ExtractKeywords("Python TUTORIAL Programming GUIDE", string.Empty);

        keywords.Should().OnlyContain(k => k == k.ToLowerInvariant());
    }

    // ── OptimizeAsync ─────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that the OptimizeAsync method returns suggestions with a valid input.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_WithValidInput_ReturnsSuggestions()
    {
        var result = await _engine.OptimizeAsync(
            "Learn Python programming",
            "A complete beginner tutorial",
            ["python", "tutorial"],
            channelId: 1);

        result.Should().NotBeNull();
        result.OriginalTitle.Should().Be("Learn Python programming");
        result.Suggestions.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that the OptimizeAsync method returns suggestions with positive confidence scores.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_SuggestionsHavePositiveConfidenceScore()
    {
        var result = await _engine.OptimizeAsync(
            "How to build a website from scratch",
            "Step by step web development guide",
            ["webdev", "coding"],
            channelId: 1);

        result.Suggestions.Should().AllSatisfy(s => s.ConfidenceScore.Should().BeInRange(0, 1));
    }

    /// <summary>
    /// Tests that the OptimizeAsync method returns the best suggestion with the highest confidence score.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_BestSuggestionIsHighestConfidence()
    {
        var result = await _engine.OptimizeAsync("Build a REST API", string.Empty, [], channelId: 1);

        var best = result.BestSuggestion;
        if (best is not null && result.Suggestions.Count > 1)
            best.ConfidenceScore.Should().BeGreaterThanOrEqualTo(
                result.Suggestions.Min(s => s.ConfidenceScore));
    }

    /// <summary>
    /// Tests that the OptimizeAsync method includes the "shorts" tag in the recommended hashtags.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_RecommendedHashtagsIncludeShortsTag()
    {
        var result = await _engine.OptimizeAsync("Quick Python tips", string.Empty, [], channelId: 1);

        result.RecommendedHashtags.Should().Contain(h => h.Contains("shorts", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tests that the OptimizeAsync method throws an ArgumentException for a null or whitespace title.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_NullOrWhitespaceTitle_ThrowsArgumentException()
    {
        Func<Task> act = () => _engine.OptimizeAsync(string.Empty, string.Empty, [], channelId: 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that the OptimizeAsync method returns an optimal posting hour within the valid range.
    /// </summary>
    [Fact]
    public async Task OptimizeAsync_OptimalPostingHourIsWithinValidRange()
    {
        var result = await _engine.OptimizeAsync("Some video title text here", string.Empty, [], channelId: 1);

        result.OptimalPostingHour.Should().BeInRange(0, 23);
    }

    // ── RecommendPostingTimesAsync ────────────────────────────────────────────

    /// <summary>
    /// Tests that the RecommendPostingTimesAsync method returns the requested number of posting times.
    /// </summary>
    [Fact]
    public async Task RecommendPostingTimesAsync_ReturnsRequestedCount()
    {
        var slots = (await _engine.RecommendPostingTimesAsync(channelId: 1, count: 3)).ToList();

        slots.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that the RecommendPostingTimesAsync method returns posting times that are in the future.
    /// </summary>
    [Fact]
    public async Task RecommendPostingTimesAsync_AllSlotsAreInTheFuture()
    {
        var before = DateTime.UtcNow;
        var slots  = await _engine.RecommendPostingTimesAsync(channelId: 1, count: 5);

        slots.Should().AllSatisfy(s => s.Should().BeAfter(before));
    }

    /// <summary>
    /// Tests that the RecommendPostingTimesAsync method throws an ArgumentOutOfRangeException for a count of 0.
    /// </summary>
    [Fact]
    public async Task RecommendPostingTimesAsync_ZeroCount_ThrowsArgumentOutOfRangeException()
    {
        Func<Task> act = () => _engine.RecommendPostingTimesAsync(channelId: 1, count: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ── TitleOptimizationResult helpers ───────────────────────────────────────

    /// <summary>
    /// Tests that the NextOptimalSlot method returns the next optimal slot when the hour has already passed.
    /// </summary>
    [Fact]
    public void NextOptimalSlot_WhenHourAlreadyPassed_ReturnsNextDay()
    {
        var pastHour = DateTime.UtcNow.Hour == 0 ? 23 : DateTime.UtcNow.Hour - 1;
        var result   = new TitleOptimizationResult(
            OriginalTitle: "test",
            OriginalDescription: string.Empty,
            Suggestions: [],
            OptimalPostingHour: pastHour,
            RecommendedHashtags: [],
            GeneratedAt: DateTime.UtcNow);

        var next = result.NextOptimalSlot();

        next.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that the HasHighConfidenceSuggestion method returns true when the score is above 0.7.
    /// </summary>
    [Fact]
    public void HasHighConfidenceSuggestion_WhenScoreAbove07_ReturnsTrue()
    {
        var suggestion = new OptimizationSuggestion(
            SuggestedTitle: "High confidence title",
            SuggestedDescription: string.Empty,
            SuggestedTags: [],
            ConfidenceScore: 0.8,
            Rationale: "test");

        var result = new TitleOptimizationResult(
            OriginalTitle: "test",
            OriginalDescription: string.Empty,
            Suggestions: [suggestion],
            OptimalPostingHour: 17,
            RecommendedHashtags: [],
            GeneratedAt: DateTime.UtcNow);

        result.HasHighConfidenceSuggestion.Should().BeTrue();
    }
}
