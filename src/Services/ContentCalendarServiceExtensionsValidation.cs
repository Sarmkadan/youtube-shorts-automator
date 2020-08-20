// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="ContentCalendarServiceExtensions"/> extension methods.
/// </summary>
public static class ContentCalendarServiceExtensionsValidation
{
    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.CreateEntryAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="title">The title of the content entry.</param>
    /// <param name="channelId">The target YouTube channel identifier.</param>
    /// <param name="description">The description text.</param>
    /// <param name="category">The content category.</param>
    /// <param name="tags">Optional array of tags.</param>
    /// <param name="keywords">Optional seed keywords for optimization.</param>
    /// <param name="notes">Optional internal notes.</param>
    /// <param name="optimizeImmediately">Whether to immediately optimize the entry after creation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateEntry(
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

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            problems.Add("Title cannot be null, empty, or whitespace.");
        }
        else if (title.Length > 100)
        {
            problems.Add("Title cannot exceed 100 characters.");
        }

        if (channelId <= 0)
        {
            problems.Add("Channel ID must be a positive integer.");
        }

        if (description?.Length > 5000)
        {
            problems.Add("Description cannot exceed 5000 characters.");
        }

        if (tags is not null)
        {
            if (tags.Length > 50)
            {
                problems.Add("Tags array cannot exceed 50 items.");
            }

            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    problems.Add("Tag cannot be null, empty, or whitespace.");
                    break;
                }

                if (tag.Length > 100)
                {
                    problems.Add("Individual tag cannot exceed 100 characters.");
                    break;
                }
            }
        }

        if (keywords is not null)
        {
            if (keywords.Length > 50)
            {
                problems.Add("Keywords array cannot exceed 50 items.");
            }

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    problems.Add("Keyword cannot be null, empty, or whitespace.");
                    break;
                }

                if (keyword.Length > 100)
                {
                    problems.Add("Individual keyword cannot exceed 100 characters.");
                    break;
                }
            }
        }

        if (notes?.Length > 2000)
        {
            problems.Add("Notes cannot exceed 2000 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.GetRequiredEntryAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetRequiredEntry(
        this ContentCalendarService service,
        int entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (entryId <= 0)
        {
            problems.Add("Entry ID must be a positive integer.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.GetEntriesInRangeAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="from">The start date (inclusive).</param>
    /// <param name="to">The end date (inclusive).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetEntriesInRange(
        this ContentCalendarService service,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (from > to)
        {
            problems.Add("Range start date cannot be after range end date.");
        }

        if (from == default)
        {
            problems.Add("Range start date cannot be the default DateTime value.");
        }

        if (to == default)
        {
            problems.Add("Range end date cannot be the default DateTime value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.GetUpcomingEntriesAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="daysAhead">Number of days to look ahead.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetUpcomingEntries(
        this ContentCalendarService service,
        int daysAhead,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (daysAhead <= 0)
        {
            problems.Add("Days ahead must be a positive integer.");
        }

        if (daysAhead > 365)
        {
            problems.Add("Days ahead cannot exceed 365 days.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.OptimizeAndApplyAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to optimize.</param>
    /// <param name="applyBestSuggestion">Whether to automatically apply the best suggestion after optimization.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateOptimizeAndApply(
        this ContentCalendarService service,
        int entryId,
        bool applyBestSuggestion = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (entryId <= 0)
        {
            problems.Add("Entry ID must be a positive integer.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.ScheduleAtOptimalTimeAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to schedule.</param>
    /// <param name="videoShortId">The video short identifier to link to this entry.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateScheduleAtOptimalTime(
        this ContentCalendarService service,
        int entryId,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (entryId <= 0)
        {
            problems.Add("Entry ID must be a positive integer.");
        }

        if (videoShortId <= 0)
        {
            problems.Add("Video short ID must be a positive integer.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.IsReadyToPublishAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="entryId">The entry identifier to check.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateIsReadyToPublish(
        this ContentCalendarService service,
        int entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (entryId <= 0)
        {
            problems.Add("Entry ID must be a positive integer.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters that would be passed to <see cref="ContentCalendarServiceExtensions.GetEntriesNeedingOptimizationAsync"/>.
    /// </summary>
    /// <param name="service">The content calendar service instance.</param>
    /// <param name="daysOlderThan">Optional filter: only entries older than the specified number of days.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetEntriesNeedingOptimization(
        this ContentCalendarService service,
        int? daysOlderThan = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (daysOlderThan.HasValue && daysOlderThan < 0)
        {
            problems.Add("Days older than cannot be negative.");
        }

        if (daysOlderThan > 3650)
        {
            problems.Add("Days older than cannot exceed 3650 days (10 years).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified content calendar service instance is valid for use.
    /// </summary>
    /// <param name="value">The content calendar service extensions instance.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ContentCalendarService value)
    {
        return value is not null;
    }

    /// <summary>
    /// Ensures that the specified content calendar service instance is valid for use.
    /// </summary>
    /// <param name="value">The content calendar service extensions instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ContentCalendarService value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}
