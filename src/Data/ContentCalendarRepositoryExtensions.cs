// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Extension methods for <see cref="ContentCalendarRepository"/> providing additional
/// convenience methods for common operations on content calendar entries.
/// </summary>
public static class ContentCalendarRepositoryExtensions
{
    /// <summary>
    /// Gets the first upcoming entry that matches the specified status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cutoffUtc">The upper bound of the upcoming window (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entry or null if none found.</returns>
    public static async Task<ContentCalendarEntry?> GetFirstUpcomingByStatusAsync(
        this ContentCalendarRepository repository,
        CalendarEntryStatus status,
        DateTime cutoffUtc,
        CancellationToken cancellationToken = default)
    {
        var entries = await repository.GetUpcomingAsync(cutoffUtc, cancellationToken);
        return entries.FirstOrDefault(e => e.Status == status);
    }

    /// <summary>
    /// Gets all entries for a specific YouTube channel.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="channelId">The YouTube channel ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries for the specified channel.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetByChannelIdAsync(
        this ContentCalendarRepository repository,
        int channelId,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        return allEntries.Where(e => e.YouTubeChannelId == channelId).ToList();
    }

    /// <summary>
    /// Gets all entries with a specific status.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries with the specified status.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetByStatusAsync(
        this ContentCalendarRepository repository,
        CalendarEntryStatus status,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        return allEntries.Where(e => e.Status == status).ToList();
    }

    /// <summary>
    /// Gets all entries that are scheduled for publication on a specific date.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="date">The date to filter by (time portion is ignored).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries scheduled for the specified date.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetByDateAsync(
        this ContentCalendarRepository repository,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await repository.GetByDateRangeAsync(startOfDay, endOfDay, cancellationToken);
    }

    /// <summary>
    /// Gets the next chronological entry after the specified entry ID.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="currentEntryId">The ID of the current entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next entry or null if this is the last entry.</returns>
    public static async Task<ContentCalendarEntry?> GetNextEntryAsync(
        this ContentCalendarRepository repository,
        int currentEntryId,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        var orderedEntries = allEntries.OrderBy(e => e.ScheduledPublishAt).ToList();

        var currentIndex = orderedEntries.FindIndex(e => e.Id == currentEntryId);
        return currentIndex >= 0 && currentIndex < orderedEntries.Count - 1
            ? orderedEntries[currentIndex + 1]
            : null;
    }

    /// <summary>
    /// Gets the previous chronological entry before the specified entry ID.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="currentEntryId">The ID of the current entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The previous entry or null if this is the first entry.</returns>
    public static async Task<ContentCalendarEntry?> GetPreviousEntryAsync(
        this ContentCalendarRepository repository,
        int currentEntryId,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        var orderedEntries = allEntries.OrderBy(e => e.ScheduledPublishAt).ToList();

        var currentIndex = orderedEntries.FindIndex(e => e.Id == currentEntryId);
        return currentIndex > 0
            ? orderedEntries[currentIndex - 1]
            : null;
    }

    /// <summary>
    /// Gets all entries that have optimization applied.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries with optimization applied.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetWithOptimizationAppliedAsync(
        this ContentCalendarRepository repository,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        return allEntries.Where(e => e.OptimizationApplied).ToList();
    }

    /// <summary>
    /// Gets all entries that have video short IDs assigned.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries with video short IDs.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetWithVideoShortsAsync(
        this ContentCalendarRepository repository,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        return allEntries.Where(e => e.VideoShortId.HasValue).ToList();
    }

    /// <summary>
    /// Gets all entries that have upload job IDs assigned.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of entries with upload job IDs.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetWithUploadJobsAsync(
        this ContentCalendarRepository repository,
        CancellationToken cancellationToken = default)
    {
        var allEntries = await repository.GetAllAsync(cancellationToken);
        return allEntries.Where(e => e.UploadJobId.HasValue).ToList();
    }
}