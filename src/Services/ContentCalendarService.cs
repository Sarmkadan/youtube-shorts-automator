// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Orchestrates the content publishing calendar: manages the full entry lifecycle,
/// invokes the title optimisation engine, and integrates with <see cref="SchedulingService"/>
/// to create concrete upload jobs for approved entries.
/// </summary>
public sealed class ContentCalendarService : IContentCalendarService
{
    private readonly ContentCalendarRepository _repository;
    private readonly SchedulingService _schedulingService;
    private readonly AnalyticsService _analyticsService;
    private readonly VideoShortRepository _videoRepository;
    private readonly ITitleOptimizationEngine _optimizationEngine;
    private readonly ContentCalendarOptions _options;
    private readonly ILogger<ContentCalendarService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ContentCalendarService"/>.
    /// </summary>
    /// <param name="repository">Persistence layer for calendar entries.</param>
    /// <param name="schedulingService">Service used to create and manage upload jobs.</param>
    /// <param name="analyticsService">Analytics service providing historical performance data.</param>
    /// <param name="videoRepository">Repository for resolving linked video short records.</param>
    /// <param name="optimizationEngine">Engine that produces title and description suggestions.</param>
    /// <param name="options">Runtime configuration for the calendar subsystem.</param>
    /// <param name="logger">Logger instance.</param>
    public ContentCalendarService(
        ContentCalendarRepository repository,
        SchedulingService schedulingService,
        AnalyticsService analyticsService,
        VideoShortRepository videoRepository,
        ITitleOptimizationEngine optimizationEngine,
        ContentCalendarOptions options,
        ILogger<ContentCalendarService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _optimizationEngine = optimizationEngine ?? throw new ArgumentNullException(nameof(optimizationEngine));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ContentCalendarEntry> CreateEntryAsync(
        ContentCalendarEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ValidateEntry(entry);

        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        var created = await _repository.AddAsync(entry, cancellationToken);
        _logger.LogInformation(
            "Created calendar entry {EntryId}: \"{Title}\" scheduled for {ScheduledAt}",
            created.Id, created.Title, created.ScheduledPublishAt);

        if (_options.AutoOptimizeOnCreate)
        {
            try
            {
                await OptimizeEntryAsync(created.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Auto-optimisation failed for new entry {EntryId} — entry was still created",
                    created.Id);
            }
        }

        return created;
    }

    /// <inheritdoc />
    public Task<ContentCalendarEntry?> GetEntryAsync(int entryId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(entryId, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<ContentCalendarEntry>> GetEntriesInRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Range start cannot be after range end.", nameof(from));

        return await _repository.GetByDateRangeAsync(from, to, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ContentCalendarEntry>> GetUpcomingEntriesAsync(
        int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        if (daysAhead <= 0)
            throw new ArgumentOutOfRangeException(nameof(daysAhead), "Days ahead must be a positive integer.");

        var cutoff = DateTime.UtcNow.AddDays(daysAhead);
        return await _repository.GetUpcomingAsync(cutoff, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ContentCalendarEntry> UpdateEntryAsync(
        ContentCalendarEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!await _repository.ExistsAsync(entry.Id, cancellationToken))
            throw new ValidationException($"Calendar entry {entry.Id} not found.");

        ValidateEntry(entry);
        entry.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(entry, cancellationToken);
        _logger.LogInformation("Updated calendar entry {EntryId}", updated.Id);
        return updated;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(entryId, cancellationToken);
        if (deleted)
            _logger.LogInformation("Deleted calendar entry {EntryId}", entryId);
        return deleted;
    }

    /// <inheritdoc />
    public async Task<TitleOptimizationResult> OptimizeEntryAsync(
        int entryId,
        CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(entryId, cancellationToken);

        _logger.LogInformation("Running optimisation for calendar entry {EntryId}", entryId);

        try
        {
            var result = await _optimizationEngine.OptimizeAsync(
                entry.Title,
                entry.Description,
                entry.Tags,
                entry.YouTubeChannelId,
                cancellationToken);

            entry.LastOptimization = result;
            entry.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entry, cancellationToken);

            _logger.LogInformation(
                "Optimisation stored for entry {EntryId} — best confidence {Score:F2}",
                entryId, result.BestSuggestion?.ConfidenceScore ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimisation failed for entry {EntryId}", entryId);
            throw new InvalidOperationException($"Failed to optimise calendar entry {entryId}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ContentCalendarEntry> ApplyOptimizationAsync(
        int entryId,
        int suggestionIndex = 0,
        CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(entryId, cancellationToken);

        if (entry.LastOptimization is null)
            throw new InvalidOperationException(
                $"Entry {entryId} has no stored optimisation result. Call OptimizeEntryAsync first.");

        var suggestions = entry.LastOptimization.Suggestions;
        if (suggestionIndex < 0 || suggestionIndex >= suggestions.Count)
            throw new ArgumentOutOfRangeException(nameof(suggestionIndex),
                $"Index {suggestionIndex} is out of range for {suggestions.Count} available suggestions.");

        entry.ApplyOptimization(suggestions[suggestionIndex]);
        await _repository.UpdateAsync(entry, cancellationToken);

        _logger.LogInformation(
            "Applied suggestion {Index} to entry {EntryId} (confidence {Score:F2})",
            suggestionIndex, entryId, suggestions[suggestionIndex].ConfidenceScore);

        return entry;
    }

    /// <inheritdoc />
    public Task<IEnumerable<DateTime>> GetRecommendedSlotsAsync(
        int channelId,
        int count = 5,
        CancellationToken cancellationToken = default) =>
        _optimizationEngine.RecommendPostingTimesAsync(channelId, count, cancellationToken);

    /// <inheritdoc />
    public async Task<ContentCalendarEntry> ScheduleEntryAsync(
        int entryId,
        DateTime scheduledAt,
        CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(entryId, cancellationToken);

        if (entry.Status is CalendarEntryStatus.Published or CalendarEntryStatus.Cancelled)
            throw new InvalidOperationException(
                $"Cannot schedule an entry with status {entry.Status}.");

        if (!entry.VideoShortId.HasValue)
            throw new InvalidOperationException(
                $"Entry {entryId} must be linked to a VideoShort before scheduling. Set VideoShortId first.");

        var videoShort = await _videoRepository.GetByIdAsync(entry.VideoShortId.Value, cancellationToken)
            ?? throw new InvalidOperationException(
                $"VideoShort {entry.VideoShortId} linked to entry {entryId} was not found.");

        if (!videoShort.CanBeProcessed() && videoShort.Status != Constants.ProcessingStatus.Completed)
        {
            _logger.LogWarning(
                "Scheduling entry {EntryId} whose VideoShort {VideoShortId} has status {Status}",
                entryId, videoShort.Id, videoShort.Status);
        }

        try
        {
            var uploadJob = await _schedulingService.ScheduleUploadAsync(
                entry.VideoShortId.Value, scheduledAt, cancellationToken);

            entry.UploadJobId = uploadJob.Id;
            entry.ScheduledPublishAt = scheduledAt;
            entry.Status = CalendarEntryStatus.Scheduled;
            entry.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entry, cancellationToken);

            _logger.LogInformation(
                "Scheduled entry {EntryId} for {ScheduledAt:u} — upload job {JobId}",
                entryId, scheduledAt, uploadJob.Id);

            return entry;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to schedule calendar entry {entryId}: {ex.Message}", ex);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<ContentCalendarEntry> RequireEntryAsync(
        int entryId, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(entryId, cancellationToken)
            ?? throw new ValidationException($"Calendar entry {entryId} not found.");
    }

    private void ValidateEntry(ContentCalendarEntry entry)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(entry.Title))
            errors[nameof(entry.Title)] = "Title is required.";
        else if (entry.Title.Length > _options.MaxTitleLength)
            errors[nameof(entry.Title)] = $"Title must not exceed {_options.MaxTitleLength} characters.";

        if (entry.Description.Length > _options.MaxDescriptionLength)
            errors[nameof(entry.Description)] = $"Description must not exceed {_options.MaxDescriptionLength} characters.";

        if (entry.Tags.Length > _options.MaxTagCount)
            errors[nameof(entry.Tags)] = $"Tag count must not exceed {_options.MaxTagCount}.";

        if (entry.YouTubeChannelId <= 0)
            errors[nameof(entry.YouTubeChannelId)] = "A valid YouTube channel identifier is required.";

        if (errors.Count > 0)
            throw new ValidationException("Content calendar entry validation failed.", errors);
    }
}
