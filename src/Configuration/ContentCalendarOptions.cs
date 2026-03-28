// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Runtime configuration for the content calendar subsystem and its embedded title
/// optimisation engine. Values can be supplied via <c>appsettings.json</c> under the
/// <c>"ContentCalendar"</c> section and are registered as a singleton through
/// <see cref="DependencyInjection.AddContentCalendar"/>.
/// </summary>
public sealed class ContentCalendarOptions
{
    /// <summary>
    /// Default number of days ahead to fetch when querying upcoming calendar entries.
    /// Defaults to <c>14</c>.
    /// </summary>
    public int DefaultLookAheadDays { get; set; } = 14;

    /// <summary>
    /// Hard character limit enforced on calendar entry titles during validation.
    /// Defaults to <c>100</c> (YouTube's maximum title length).
    /// </summary>
    public int MaxTitleLength { get; set; } = 100;

    /// <summary>
    /// Lower bound of the range considered optimal for title length scoring.
    /// Titles shorter than this receive a reduced score. Defaults to <c>40</c>.
    /// </summary>
    public int OptimalTitleMinLength { get; set; } = 40;

    /// <summary>
    /// Upper bound of the range considered optimal for title length scoring.
    /// Titles longer than this receive a reduced score. Defaults to <c>70</c>.
    /// </summary>
    public int OptimalTitleMaxLength { get; set; } = 70;

    /// <summary>
    /// Hard character limit enforced on calendar entry descriptions during validation.
    /// Defaults to <c>5000</c> (YouTube's maximum description length).
    /// </summary>
    public int MaxDescriptionLength { get; set; } = 5_000;

    /// <summary>
    /// Maximum number of tags permitted per entry. Defaults to <c>15</c>.
    /// </summary>
    public int MaxTagCount { get; set; } = 15;

    /// <summary>
    /// Number of ranked suggestions to include in each <see cref="Domain.Models.TitleOptimizationResult"/>.
    /// Defaults to <c>3</c>.
    /// </summary>
    public int OptimizationSuggestionCount { get; set; } = 3;

    /// <summary>
    /// UTC hours (0–23) considered optimal for publishing based on general audience
    /// activity patterns. The engine picks the closest match to historical data.
    /// Defaults to 08:00, 12:00, 17:00, 19:00 and 21:00.
    /// </summary>
    public int[] OptimalPostingHoursUtc { get; set; } = [8, 12, 17, 19, 21];

    /// <summary>
    /// Multiplier applied to keyword-density scores when computing the suggestion
    /// confidence value. Higher values amplify keyword signals relative to structural
    /// heuristics. Defaults to <c>1.5</c>.
    /// </summary>
    public double KeywordWeightMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Fraction (0–1) of the final confidence score attributed to historical engagement
    /// metrics. The remaining fraction is attributed to structural title signals.
    /// Defaults to <c>0.6</c>.
    /// </summary>
    public double EngagementScoreWeight { get; set; } = 0.6;

    /// <summary>
    /// Minimum gap in minutes enforced between two consecutive recommended posting slots
    /// to avoid audience fatigue. Defaults to <c>240</c> (4 hours).
    /// </summary>
    public int MinSlotGapMinutes { get; set; } = 240;

    /// <summary>
    /// When <c>true</c>, the optimisation engine is invoked automatically immediately
    /// after a new calendar entry is created. Defaults to <c>false</c>.
    /// </summary>
    public bool AutoOptimizeOnCreate { get; set; } = false;

    /// <summary>
    /// Keywords that correlate with high short-form engagement. Titles containing any
    /// of these terms receive a boosted score multiplied by <see cref="KeywordWeightMultiplier"/>.
    /// </summary>
    public string[] HighEngagementKeywords { get; set; } =
    [
        "tips", "tutorial", "how to", "best", "top", "guide",
        "easy", "quick", "step by step", "beginners", "secrets", "mistakes"
    ];

    /// <summary>
    /// Hashtags appended to every optimised description to capture algorithm-driven traffic.
    /// </summary>
    public string[] TrendingHashtags { get; set; } =
        ["#shorts", "#fyp", "#viral", "#trending"];

    /// <summary>
    /// Number of top-performing historical videos to fetch from the analytics service
    /// when building a channel's performance pattern. Defaults to <c>25</c>.
    /// </summary>
    public int HistoricalSampleSize { get; set; } = 25;

    /// <summary>
    /// Score bonus (0–1) added to the confidence of a suggestion when the average
    /// engagement rate of the channel's top performers exceeds <see cref="EngagementRateThreshold"/>.
    /// Defaults to <c>0.1</c>.
    /// </summary>
    public double HighEngagementBonus { get; set; } = 0.1;

    /// <summary>
    /// Engagement rate percentage threshold above which <see cref="HighEngagementBonus"/>
    /// is applied. Defaults to <c>5.0</c>.
    /// </summary>
    public double EngagementRateThreshold { get; set; } = 5.0;
}
