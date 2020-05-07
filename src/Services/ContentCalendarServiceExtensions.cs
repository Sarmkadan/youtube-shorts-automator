// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for <see cref="ContentCalendarService"/> to simplify common
/// content calendar operations and reduce boilerplate code in client applications.
/// </summary>
public static class ContentCalendarServiceExtensions
{
    /// <summary>
    /// Creates a new content calendar entry with the specified metadata and optional optimization.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="title">The title of the content entry.</param>
    /// <param name="channelId">The target YouTube channel identifier.</param>
    /// <param name="description">The description text.</param>
    /// <param name="category">The content category (defaults to <see cref="ContentCategory.Other"/>).</param>
    /// <param name="tags">Optional array of tags.</param>
    /// <param name="keywords">Optional seed keywords for optimization.</param>
    /// <param name="notes">Optional internal notes.</param>
    /// <param name="optimizeImmediately">Whether to immediately optimize the entry after creation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="title"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channelId"/> is not a positive integer.</exception>
    /// <returns>The created content calendar entry.</returns>
    public static async Task<ContentCalendarEntry> CreateEntryAsync(
        this ContentCalendarService service,
        string title,
        int channelId,
        string? description = null,
        ContentCategory category = ContentCategory.Other,
        string[]? tags = null,
        string[]? keywords = null,
        string? notes = null,
        bool optimizeImmediately = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(channelId, 0);

        var entry = new ContentCalendarEntry
        {
            Title = title,
            Description = description ?? string.Empty,
            YouTubeChannelId = channelId,
            Category = category,
            Tags = tags ?? Array.Empty<string>(),
            Keywords = keywords ?? Array.Empty<string>(),
            Notes = notes
        };

        var created = await service.CreateEntryAsync(entry, cancellationToken).ConfigureAwait(false);

        if (optimizeImmediately)
        {
            await service.OptimizeEntryAsync(created.Id, cancellationToken).ConfigureAwait(false);
        }

        return created;
    }

    /// <summary>
    /// Retrieves a content calendar entry by its identifier or throws if not found.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entryId"/> is not a positive integer.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the entry does not exist.</exception>
    /// <returns>The content calendar entry.</returns>
    public static async Task<ContentCalendarEntry> GetRequiredEntryAsync(
        this ContentCalendarService service,
        int entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(entryId, 0);

        var entry = await service.GetEntryAsync(entryId, cancellationToken).ConfigureAwait(false);
        return entry ?? throw new InvalidOperationException($"Content calendar entry {entryId} not found.");
    }

    /// <summary>
    /// Retrieves all content calendar entries within a specified date range.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="from">The start date (inclusive).</param>
    /// <param name="to">The end date (inclusive).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="from"/> is after <paramref name="to"/>.</exception>
    /// <returns>An enumerable of content calendar entries in the specified range.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetEntriesInRangeAsync(
        this ContentCalendarService service,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (from > to)
        {
            throw new ArgumentException("Range start cannot be after range end.", nameof(from));
        }

        return await service.GetEntriesInRangeAsync(from, to, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all upcoming content calendar entries scheduled for publication within the specified
    /// number of days.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="daysAhead">Number of days to look ahead (default: 7).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="daysAhead"/> is not a positive integer.</exception>
    /// <returns>An enumerable of upcoming content calendar entries.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetUpcomingEntriesAsync(
        this ContentCalendarService service,
        int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (daysAhead <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(daysAhead), "Days ahead must be a positive integer.");
        }

        return await service.GetUpcomingEntriesAsync(daysAhead, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Optimizes a content calendar entry and optionally applies the best suggestion automatically.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to optimize.</param>
    /// <param name="applyBestSuggestion">Whether to automatically apply the best suggestion after optimization.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entryId"/> is not a positive integer.</exception>
    /// <returns>
    /// A tuple containing the <see cref="TitleOptimizationResult"/> and, if <paramref name="applyBestSuggestion"/>
    /// is <c>true</c> and a best suggestion exists, the updated <see cref="ContentCalendarEntry"/>.
    /// </returns>
    public static async Task<(TitleOptimizationResult Result, ContentCalendarEntry? Entry)> OptimizeAndApplyAsync(
        this ContentCalendarService service,
        int entryId,
        bool applyBestSuggestion = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(entryId, 0);

        var result = await service.OptimizeEntryAsync(entryId, cancellationToken).ConfigureAwait(false);
        ContentCalendarEntry? entry = null;

        if (applyBestSuggestion && result.BestSuggestion is not null)
        {
            entry = await service.ApplyOptimizationAsync(entryId, 0, cancellationToken).ConfigureAwait(false);
        }

        return (result, entry);
    }

    /// <summary>
    /// Schedules a content calendar entry for publication at the next optimal posting time.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to schedule.</param>
    /// <param name="videoShortId">The video short identifier to link to this entry.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entryId"/> or <paramref name="videoShortId"/> is not a positive integer.</exception>
    /// <exception cref="InvalidOperationException">No optimal posting time could be determined for this entry.</exception>
    /// <returns>The scheduled content calendar entry with upload job information.</returns>
    public static async Task<ContentCalendarEntry> ScheduleAtOptimalTimeAsync(
        this ContentCalendarService service,
        int entryId,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(entryId, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(videoShortId, 0);

        var recommendedSlots = await service.GetRecommendedSlotsAsync(videoShortId, 5, cancellationToken).ConfigureAwait(false);
        var optimalSlot = recommendedSlots.FirstOrDefault();

        if (optimalSlot == default)
        {
            throw new InvalidOperationException("No optimal posting time could be determined for this entry.");
        }

        return await service.ScheduleEntryAsync(entryId, optimalSlot, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Quickly checks if an entry is ready for immediate publication without waiting for the scheduled time.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to check.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="entryId"/> is not a positive integer.</exception>
    /// <returns><c>true</c> if the entry is ready to publish immediately; otherwise <c>false</c>.</returns>
    public static async Task<bool> IsReadyToPublishAsync(
        this ContentCalendarService service,
        int entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(entryId, 0);

        var entry = await service.GetEntryAsync(entryId, cancellationToken).ConfigureAwait(false);
        return entry?.IsReadyToPublish() == true;
    }

    /// <summary>
    /// Retrieves entries that need optimization (draft entries without optimization results).
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="daysOlderThan">
    /// Optional filter: only entries older than the specified number of days.
    /// </param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="daysOlderThan"/> is negative.</exception>
    /// <returns>An enumerable of entries that need optimization.</returns>
    public static async Task<IEnumerable<ContentCalendarEntry>> GetEntriesNeedingOptimizationAsync(
        this ContentCalendarService service,
        int? daysOlderThan = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (daysOlderThan.HasValue && daysOlderThan < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(daysOlderThan), "Days older than cannot be negative.");
        }

        var cutoffDate = daysOlderThan.HasValue
            ? DateTime.UtcNow.AddDays(-daysOlderThan.Value)
            : DateTime.MinValue;

        var allEntries = await service.GetEntriesInRangeAsync(
            cutoffDate,
            DateTime.UtcNow.AddDays(1),
            cancellationToken).ConfigureAwait(false);

        return allEntries.Where(e => e.NeedsOptimization());
    }
}
