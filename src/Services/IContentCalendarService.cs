// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Defines the contract for managing the content publishing calendar, covering full
/// entry lifecycle (create → optimise → approve → schedule → publish) together with
/// integration into the scheduling and analytics subsystems.
/// </summary>
public interface IContentCalendarService
{
    /// <summary>
    /// Persists a new calendar entry and, when auto-optimisation is configured, immediately
    /// invokes the optimisation engine.
    /// </summary>
    /// <param name="entry">Populated entry to create. <see cref="ContentCalendarEntry.Title"/> is required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted entry with its assigned <see cref="ContentCalendarEntry.Id"/>.</returns>
    Task<ContentCalendarEntry> CreateEntryAsync(ContentCalendarEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single calendar entry by its identifier, or <c>null</c> when not found.</summary>
    /// <param name="entryId">The entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ContentCalendarEntry?> GetEntryAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entries whose <see cref="ContentCalendarEntry.ScheduledPublishAt"/> falls
    /// within the inclusive date range <paramref name="from"/> to <paramref name="to"/>,
    /// ordered by scheduled time ascending.
    /// </summary>
    /// <param name="from">Range start (inclusive, UTC).</param>
    /// <param name="to">Range end (inclusive, UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<ContentCalendarEntry>> GetEntriesInRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns non-cancelled, non-archived entries scheduled within the next
    /// <paramref name="daysAhead"/> days, ordered by scheduled time ascending.
    /// </summary>
    /// <param name="daysAhead">Number of days ahead to look. Must be positive. Defaults to 7.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<ContentCalendarEntry>> GetUpcomingEntriesAsync(int daysAhead = 7, CancellationToken cancellationToken = default);

    /// <summary>Validates and persists changes to an existing calendar entry.</summary>
    /// <param name="entry">Modified entry. The <see cref="ContentCalendarEntry.Id"/> must already exist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ContentCalendarEntry> UpdateEntryAsync(ContentCalendarEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes the specified calendar entry.</summary>
    /// <param name="entryId">Identifier of the entry to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when deleted successfully; <c>false</c> when the entry was not found.</returns>
    Task<bool> DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the optimisation engine against the entry's current title, description and tags,
    /// stores the result on the entry, and returns the full <see cref="TitleOptimizationResult"/>
    /// without modifying the entry's published metadata.
    /// </summary>
    /// <param name="entryId">Identifier of the entry to optimise.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TitleOptimizationResult> OptimizeEntryAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies the suggestion at position <paramref name="suggestionIndex"/> from the entry's
    /// last stored <see cref="ContentCalendarEntry.LastOptimization"/> and returns the updated entry.
    /// Call <see cref="OptimizeEntryAsync"/> first if <c>LastOptimization</c> is <c>null</c>.
    /// </summary>
    /// <param name="entryId">Identifier of the entry to update.</param>
    /// <param name="suggestionIndex">Zero-based index into <see cref="TitleOptimizationResult.Suggestions"/>. Defaults to 0 (highest confidence).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ContentCalendarEntry> ApplyOptimizationAsync(int entryId, int suggestionIndex = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <paramref name="count"/> recommended UTC posting timestamps for the given channel,
    /// ranked by projected audience engagement based on historical analytics.
    /// </summary>
    /// <param name="channelId">Target YouTube channel identifier.</param>
    /// <param name="count">Number of slot recommendations to return. Defaults to 5.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<DateTime>> GetRecommendedSlotsAsync(int channelId, int count = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links the entry to a new <see cref="UploadJob"/> via the scheduling service,
    /// sets <see cref="ContentCalendarEntry.ScheduledPublishAt"/> to <paramref name="scheduledAt"/>,
    /// and transitions the entry status to <see cref="CalendarEntryStatus.Scheduled"/>.
    /// The entry must already have a linked <see cref="ContentCalendarEntry.VideoShortId"/>.
    /// </summary>
    /// <param name="entryId">Identifier of the entry to schedule.</param>
    /// <param name="scheduledAt">UTC time at which the upload should be dispatched.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ContentCalendarEntry> ScheduleEntryAsync(int entryId, DateTime scheduledAt, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for the engine that analyses video metadata against historical
/// performance data and produces ranked title and description optimisation suggestions
/// using deterministic, data-driven heuristics.
/// </summary>
public interface ITitleOptimizationEngine
{
    /// <summary>
    /// Analyses the provided metadata against historical performance patterns and returns
    /// a <see cref="TitleOptimizationResult"/> containing ranked suggestions and posting-time
    /// recommendations.
    /// </summary>
    /// <param name="title">Current title to analyse and improve.</param>
    /// <param name="description">Current description to analyse and improve.</param>
    /// <param name="tags">Existing tag array.</param>
    /// <param name="channelId">Channel identifier used to fetch channel-specific analytics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TitleOptimizationResult> OptimizeAsync(
        string title,
        string description,
        string[] tags,
        int channelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of recommended UTC posting timestamps for the given channel,
    /// ordered by projected engagement potential.
    /// </summary>
    /// <param name="channelId">Target YouTube channel identifier.</param>
    /// <param name="count">Number of slot recommendations to return. Defaults to 5.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IEnumerable<DateTime>> RecommendPostingTimesAsync(
        int channelId,
        int count = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a heuristic quality score between 0 (very poor) and 1 (excellent) for the
    /// given title, based on length, structural patterns, power-word presence and configured
    /// high-engagement keyword matches.
    /// </summary>
    /// <param name="title">Title text to score.</param>
    double ScoreTitle(string title);

    /// <summary>
    /// Extracts the most significant, non-trivial content keywords from the combined
    /// title and description text, suitable for use as optimisation seed terms.
    /// </summary>
    /// <param name="title">Title text to analyse.</param>
    /// <param name="description">Description text to analyse.</param>
    string[] ExtractKeywords(string title, string description);
}
