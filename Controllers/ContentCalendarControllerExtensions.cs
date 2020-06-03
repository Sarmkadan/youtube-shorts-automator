// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for ContentCalendarController providing common operations
// and convenience methods for content calendar management.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Provides extension methods for <see cref="ContentCalendarController"/> that implement
/// common content calendar operations and convenience methods.
/// </summary>
public static class ContentCalendarControllerExtensions
{
    /// <summary>
    /// Creates a new content calendar entry with the specified title and schedules it for
    /// immediate or future publication.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="title">The title of the content entry (required).</param>
    /// <param name="channelId">The target YouTube channel identifier.</param>
    /// <param name="scheduledPublishAt">The UTC time when the content should be published.
    /// If null, defaults to current UTC time.</param>
    /// <param name="category">The content category. Defaults to <see cref="ContentCategory.Other"/>.</param>
    /// <param name="description">Optional description text.</param>
    /// <param name="tags">Optional tags for the content.</param>
    /// <param name="keywords">Optional seed keywords for optimization.</param>
    /// <returns>The created content calendar entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown if title or controller is null.</exception>
    public static async Task<IActionResult> CreateEntry(
        this ContentCalendarController controller,
        string title,
        int channelId,
        DateTime? scheduledPublishAt = null,
        ContentCategory category = ContentCategory.Other,
        string? description = null,
        string[]? tags = null,
        string[]? keywords = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(title);

        var request = new CreateCalendarEntryRequest
        {
            Title = title,
            Description = description,
            Tags = tags,
            Category = category,
            ScheduledPublishAt = scheduledPublishAt ?? DateTime.UtcNow,
            YouTubeChannelId = channelId,
            Keywords = keywords
        };

        return await controller.CreateEntry(request);
    }

    /// <summary>
    /// Retrieves a single calendar entry by its identifier and returns it as a strongly-typed
    /// <see cref="ContentCalendarEntry"/> object.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="entryId">The entry identifier.</param>
    /// <returns>The content calendar entry if found; otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<ContentCalendarEntry?> GetEntryOrDefault(
        this ContentCalendarController controller,
        int entryId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetEntry(entryId);
        return result switch
        {
            OkObjectResult okResult when okResult.Value is ContentCalendarEntry entry => entry,
            _ => null
        };
    }

    /// <summary>
    /// Retrieves upcoming calendar entries filtered by category and optionally limited to a
    /// specific channel.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="daysAhead">Number of days to look ahead. Defaults to 7.</param>
    /// <param name="category">Optional category filter. If null, returns entries from all categories.</param>
    /// <param name="channelId">Optional channel identifier filter. If null, returns entries from all channels.</param>
    /// <returns>An enumerable of content calendar entries matching the criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<IReadOnlyList<ContentCalendarEntry>> GetUpcomingEntriesByCategoryAsync(
        this ContentCalendarController controller,
        int daysAhead = 7,
        ContentCategory? category = null,
        int? channelId = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var result = await controller.GetUpcoming(daysAhead);
        if (result is not OkObjectResult okResult || okResult.Value is not { } obj)
        {
            return Array.Empty<ContentCalendarEntry>();
        }

        var entries = obj.GetType().GetProperty("data")?.GetValue(obj) as IEnumerable<ContentCalendarEntry> ?? Enumerable.Empty<ContentCalendarEntry>();
        var filtered = entries.AsEnumerable();

        if (category.HasValue)
        {
            filtered = filtered.Where(e => e.Category == category.Value);
        }

        if (channelId.HasValue)
        {
            filtered = filtered.Where(e => e.YouTubeChannelId == channelId.Value);
        }

        return filtered.ToList().AsReadOnly();
    }

    /// <summary>
    /// Schedules a content calendar entry for upload at the specified time, with optional
    /// optimization before scheduling.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="entryId">The entry identifier to schedule.</param>
    /// <param name="scheduledAt">The UTC time when the upload should be dispatched.</param>
    /// <param name="optimizeBeforeSchedule">Whether to run optimization before scheduling.
    /// Defaults to true.</param>
    /// <returns>The scheduled content calendar entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static async Task<IActionResult> ScheduleEntryWithOptimizationAsync(
        this ContentCalendarController controller,
        int entryId,
        DateTime scheduledAt,
        bool optimizeBeforeSchedule = true)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (optimizeBeforeSchedule)
        {
            await controller.OptimizeEntry(entryId);
        }

        var scheduleRequest = new ScheduleEntryRequest { ScheduledAt = scheduledAt };
        return await controller.ScheduleEntry(entryId, scheduleRequest);
    }
}