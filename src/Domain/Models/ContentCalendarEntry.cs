// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Classifies the type of content being planned in the calendar to allow
/// category-specific scoring rules inside the optimisation engine.
/// </summary>
public enum ContentCategory
{
    /// <summary>Step-by-step instructional content.</summary>
    Tutorial,
    /// <summary>Entertainment and fun content.</summary>
    Entertainment,
    /// <summary>Time-sensitive news or updates.</summary>
    News,
    /// <summary>Product or brand promotional material.</summary>
    Promotional,
    /// <summary>Behind-the-scenes footage or process documentation.</summary>
    BehindTheScenes,
    /// <summary>Participation in or creation of a trending challenge.</summary>
    Challenge,
    /// <summary>Audience questions and creator answers.</summary>
    QAndA,
    /// <summary>Content capitalising on current trends.</summary>
    Trending,
    /// <summary>Educational or informational content.</summary>
    Educational,
    /// <summary>Uncategorised or miscellaneous content.</summary>
    Other
}

/// <summary>
/// Lifecycle status for a content calendar entry, progressing from draft through
/// to publication or cancellation.
/// </summary>
public enum CalendarEntryStatus
{
    /// <summary>Newly created, metadata not yet reviewed or optimised.</summary>
    Draft,
    /// <summary>Title and description have been processed by the optimisation engine.</summary>
    Optimised,
    /// <summary>Manually reviewed and approved for publication.</summary>
    Approved,
    /// <summary>Linked to a concrete <see cref="UploadJob"/> and ready to publish.</summary>
    Scheduled,
    /// <summary>Successfully published to YouTube.</summary>
    Published,
    /// <summary>Moved to long-term archive.</summary>
    Archived,
    /// <summary>Discarded before publication.</summary>
    Cancelled
}

/// <summary>
/// An immutable, ranked recommendation produced by the optimisation engine for a single
/// title and description strategy variant.
/// </summary>
/// <param name="SuggestedTitle">Recommended title text.</param>
/// <param name="SuggestedDescription">Recommended description including hashtags and keywords.</param>
/// <param name="SuggestedTags">Array of tags recommended for maximum discoverability.</param>
/// <param name="ConfidenceScore">
/// Value between 0 and 1 representing the engine's confidence in this suggestion
/// relative to the channel's historical performance patterns.
/// </param>
/// <param name="Rationale">Human-readable explanation of the optimisation strategy applied.</param>
public sealed record OptimizationSuggestion(
    string SuggestedTitle,
    string SuggestedDescription,
    string[] SuggestedTags,
    double ConfidenceScore,
    string Rationale
);

/// <summary>
/// The complete, immutable output of a single optimisation pass, containing ranked
/// suggestions and data-driven posting-time recommendations derived from historical
/// channel performance.
/// </summary>
/// <param name="OriginalTitle">The unmodified title submitted for analysis.</param>
/// <param name="OriginalDescription">The unmodified description submitted for analysis.</param>
/// <param name="Suggestions">Ranked list of title and description suggestions.</param>
/// <param name="OptimalPostingHour">
/// UTC hour (0–23) with the highest expected audience engagement based on historical data.
/// </param>
/// <param name="RecommendedHashtags">
/// Hashtags recommended for the description, combining trending tags with content keywords.
/// </param>
/// <param name="GeneratedAt">UTC timestamp when this optimisation result was produced.</param>
public sealed record TitleOptimizationResult(
    string OriginalTitle,
    string OriginalDescription,
    IReadOnlyList<OptimizationSuggestion> Suggestions,
    double OptimalPostingHour,
    IReadOnlyList<string> RecommendedHashtags,
    DateTime GeneratedAt
)
{
    /// <summary>
    /// Returns the suggestion with the highest confidence score, or <c>null</c> when the
    /// suggestions list is empty.
    /// </summary>
    public OptimizationSuggestion? BestSuggestion =>
        Suggestions.Count > 0 ? Suggestions.MaxBy(s => s.ConfidenceScore) : null;

    /// <summary>
    /// Returns <c>true</c> when at least one suggestion carries a confidence score of 0.7 or above.
    /// </summary>
    public bool HasHighConfidenceSuggestion =>
        Suggestions.Any(s => s.ConfidenceScore >= 0.7);

    /// <summary>
    /// Returns the UTC <see cref="DateTime"/> for the next occurrence of
    /// <see cref="OptimalPostingHour"/> that is strictly in the future.
    /// </summary>
    public DateTime NextOptimalSlot()
    {
        var hour = (int)OptimalPostingHour;
        var candidate = DateTime.UtcNow.Date.AddHours(hour);
        return candidate > DateTime.UtcNow ? candidate : candidate.AddDays(1);
    }
}

/// <summary>
/// Represents a planned piece of short-form content in the publishing calendar,
/// encapsulating draft metadata, optimisation history and scheduling linkage.
/// </summary>
public class ContentCalendarEntry
{
    /// <summary>Auto-generated primary key.</summary>
    public int Id { get; set; }

    /// <summary>Working or final title for the video short (max 100 characters).</summary>
    public required string Title { get; set; }

    /// <summary>Full description text including hashtags.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Tags to be applied to the video on upload.</summary>
    public string[] Tags { get; set; } = [];

    /// <summary>Content classification used to select category-specific optimisation heuristics.</summary>
    public ContentCategory Category { get; set; } = ContentCategory.Other;

    /// <summary>Current lifecycle status of this entry.</summary>
    public CalendarEntryStatus Status { get; set; } = CalendarEntryStatus.Draft;

    /// <summary>UTC timestamp at which this content is planned for publication.</summary>
    public DateTime ScheduledPublishAt { get; set; }

    /// <summary>Optional foreign key to the processed <see cref="VideoShort"/> for this entry.</summary>
    public int? VideoShortId { get; set; }

    /// <summary>Optional foreign key to the <see cref="UploadJob"/> created during scheduling.</summary>
    public int? UploadJobId { get; set; }

    /// <summary>Identifier of the target YouTube channel.</summary>
    public int YouTubeChannelId { get; set; }

    /// <summary>The most recent optimisation result, or <c>null</c> when not yet optimised.</summary>
    public TitleOptimizationResult? LastOptimization { get; set; }

    /// <summary>Indicates whether an optimisation suggestion has been applied to the current metadata.</summary>
    public bool OptimizationApplied { get; set; }

    /// <summary>Free-form internal notes for content planning purposes.</summary>
    public string? Notes { get; set; }

    /// <summary>Seed keywords associated with this entry used by the optimisation engine.</summary>
    public string[] Keywords { get; set; } = [];

    /// <summary>UTC timestamp when this entry was first created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the most recent modification.</summary>
    public DateTime UpdatedAt { get; set; }

    // ── Navigation properties ─────────────────────────────────────────────────

    /// <summary>The processed video short linked to this calendar entry.</summary>
    public VideoShort? VideoShort { get; set; }

    /// <summary>The upload job created when this entry was scheduled.</summary>
    public UploadJob? UploadJob { get; set; }

    /// <summary>The YouTube channel this entry targets.</summary>
    public YouTubeChannel? Channel { get; set; }

    // ── Behavioural methods ───────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the entry meets all prerequisites for immediate publication:
    /// status is <see cref="CalendarEntryStatus.Approved"/>, a video short is linked,
    /// and the scheduled time falls within the next five minutes.
    /// </summary>
    public bool IsReadyToPublish() =>
        Status == CalendarEntryStatus.Approved &&
        VideoShortId.HasValue &&
        ScheduledPublishAt <= DateTime.UtcNow.AddMinutes(5);

    /// <summary>
    /// Returns <c>true</c> when the entry is in <see cref="CalendarEntryStatus.Draft"/>
    /// state and has not yet been through an optimisation pass.
    /// </summary>
    public bool NeedsOptimization() =>
        Status == CalendarEntryStatus.Draft && LastOptimization is null;

    /// <summary>
    /// Applies the selected <paramref name="suggestion"/> to the entry's title, description
    /// and tags, and transitions the status to <see cref="CalendarEntryStatus.Optimised"/>.
    /// </summary>
    /// <param name="suggestion">The optimisation suggestion to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is <c>null</c>.</exception>
    public void ApplyOptimization(OptimizationSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        Title = suggestion.SuggestedTitle;
        Description = suggestion.SuggestedDescription;
        Tags = suggestion.SuggestedTags;
        OptimizationApplied = true;
        Status = CalendarEntryStatus.Optimised;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Transitions the entry to <see cref="CalendarEntryStatus.Approved"/>.</summary>
    public void Approve()
    {
        Status = CalendarEntryStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Moves the entry to <see cref="CalendarEntryStatus.Archived"/>.</summary>
    public void Archive()
    {
        Status = CalendarEntryStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Cancels the entry, preventing further scheduling or optimisation.</summary>
    public void Cancel()
    {
        Status = CalendarEntryStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
