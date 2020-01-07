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

public class TitleOptimizationEngineTests
{
    private readonly TitleOptimizationEngine _engine;
    private readonly Mock<AnalyticsService> _mockAnalytics;
    private readonly ContentCalendarOptions _options;

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

    [Fact]
    public void ScoreTitle_EmptyString_ReturnsZero()
    {
        var score = _engine.ScoreTitle(string.Empty);

        score.Should().Be(0);
    }

    [Fact]
    public void ScoreTitle_NullInput_ReturnsZero()
    {
        var score = _engine.ScoreTitle(null!);

        score.Should().Be(0);
    }

    [Fact]
    public void ScoreTitle_ShortTitle_ReturnsLowerScoreThanOptimal()
    {
        var shortScore  = _engine.ScoreTitle("Hi");
        var optimalScore = _engine.ScoreTitle("How to Learn Programming Faster: 10 Tips for Beginners");

        shortScore.Should().BeLessThan(optimalScore);
    }

    [Fact]
    public void ScoreTitle_TitleWithPowerWord_ReceivesBoost()
    {
        var plain      = _engine.ScoreTitle("Learn Python in 30 days");
        var withPower  = _engine.ScoreTitle("Ultimate Guide to Learning Python in 30 days");

        withPower.Should().BeGreaterThan(plain);
    }

    [Fact]
    public void ScoreTitle_TitleWithQuestion_ReceivesBoost()
    {
        var statement = _engine.ScoreTitle("You should learn Python");
        var question  = _engine.ScoreTitle("How to learn Python fast?");

        question.Should().BeGreaterThan(statement);
    }

    [Fact]
    public void ScoreTitle_TitleWithNumber_ReceivesBoost()
    {
        var withoutNum = _engine.ScoreTitle("Tips for learning Python");
        var withNum    = _engine.ScoreTitle("7 Tips for learning Python");

        withNum.Should().BeGreaterThan(withoutNum);
    }

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

    [Fact]
    public void ExtractKeywords_ExtractsNonTrivialWords()
    {
        var keywords = _engine.ExtractKeywords(
            "How to learn Python programming fast",
            "This tutorial covers Python basics and programming fundamentals.");

        keywords.Should().Contain("python");
        keywords.Should().Contain("programming");
    }

    [Fact]
    public void ExtractKeywords_FiltersStopWords()
    {
        var keywords = _engine.ExtractKeywords("How to make the best coffee", "A guide for the people");

        keywords.Should().NotContain("with");
        keywords.Should().NotContain("that");
        keywords.Should().NotContain("from");
    }

    [Fact]
    public void ExtractKeywords_ReturnsAtMostTenKeywords()
    {
        var longText = string.Join(' ', Enumerable.Range(0, 50).Select(i => $"keyword{i}"));

        var keywords = _engine.ExtractKeywords(longText, longText);

        keywords.Length.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void ExtractKeywords_EmptyInputs_ReturnsEmptyArray()
    {
        var keywords = _engine.ExtractKeywords(string.Empty, string.Empty);

        keywords.Should().BeEmpty();
    }

    [Fact]
    public void ExtractKeywords_ReturnsLowercaseWords()
    {
        var keywords = _engine.ExtractKeywords("Python TUTORIAL Programming GUIDE", string.Empty);

        keywords.Should().OnlyContain(k => k == k.ToLowerInvariant());
    }

    // ── OptimizeAsync ─────────────────────────────────────────────────────────

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

    [Fact]
    public async Task OptimizeAsync_BestSuggestionIsHighestConfidence()
    {
        var result = await _engine.OptimizeAsync("Build a REST API", string.Empty, [], channelId: 1);

        var best = result.BestSuggestion;
        if (best is not null && result.Suggestions.Count > 1)
            best.ConfidenceScore.Should().BeGreaterThanOrEqualTo(
                result.Suggestions.Min(s => s.ConfidenceScore));
    }

    [Fact]
    public async Task OptimizeAsync_RecommendedHashtagsIncludeShortsTag()
    {
        var result = await _engine.OptimizeAsync("Quick Python tips", string.Empty, [], channelId: 1);

        result.RecommendedHashtags.Should().Contain(h => h.Contains("shorts", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task OptimizeAsync_NullOrWhitespaceTitle_ThrowsArgumentException()
    {
        Func<Task> act = () => _engine.OptimizeAsync(string.Empty, string.Empty, [], channelId: 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task OptimizeAsync_OptimalPostingHourIsWithinValidRange()
    {
        var result = await _engine.OptimizeAsync("Some video title text here", string.Empty, [], channelId: 1);

        result.OptimalPostingHour.Should().BeInRange(0, 23);
    }

    // ── RecommendPostingTimesAsync ────────────────────────────────────────────

    [Fact]
    public async Task RecommendPostingTimesAsync_ReturnsRequestedCount()
    {
        var slots = (await _engine.RecommendPostingTimesAsync(channelId: 1, count: 3)).ToList();

        slots.Should().HaveCount(3);
    }

    [Fact]
    public async Task RecommendPostingTimesAsync_AllSlotsAreInTheFuture()
    {
        var before = DateTime.UtcNow;
        var slots  = await _engine.RecommendPostingTimesAsync(channelId: 1, count: 5);

        slots.Should().AllSatisfy(s => s.Should().BeAfter(before));
    }

    [Fact]
    public async Task RecommendPostingTimesAsync_ZeroCount_ThrowsArgumentOutOfRangeException()
    {
        Func<Task> act = () => _engine.RecommendPostingTimesAsync(channelId: 1, count: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ── TitleOptimizationResult helpers ───────────────────────────────────────

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
