// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// REST endpoints for managing the content publishing calendar, including full entry
/// lifecycle (create → optimise → approve → schedule), posting-slot recommendations
/// and batch retrieval.
/// </summary>
[ApiController]
[Route("api/content-calendar")]
public class ContentCalendarController : ControllerBase
{
    private readonly IContentCalendarService _calendarService;
    private readonly ILogger<ContentCalendarController> _logger;

    /// <summary>Initialises a new instance of <see cref="ContentCalendarController"/>.</summary>
    public ContentCalendarController(
        IContentCalendarService calendarService,
        ILogger<ContentCalendarController> logger)
    {
        _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Creates a new content calendar entry.</summary>
    /// <param name="request">Entry creation payload.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ContentCalendarEntry), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEntry([FromBody] CreateCalendarEntryRequest request)
    {
        try
        {
            var entry = new ContentCalendarEntry
            {
                Title              = request.Title,
                Description        = request.Description ?? string.Empty,
                Tags               = request.Tags ?? [],
                Category           = request.Category,
                ScheduledPublishAt = request.ScheduledPublishAt,
                YouTubeChannelId   = request.YouTubeChannelId,
                Notes              = request.Notes,
                Keywords           = request.Keywords ?? []
            };

            var created = await _calendarService.CreateEntryAsync(entry);
            _logger.LogInformation("Created calendar entry {EntryId}", created.Id);
            return CreatedAtAction(nameof(GetEntry), new { entryId = created.Id }, created);
        }
        catch (Exception ex) when (IsClientError(ex))
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create calendar entry");
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    /// <summary>Retrieves a single calendar entry by its identifier.</summary>
    /// <param name="entryId">Entry identifier.</param>
    [HttpGet("{entryId:int}")]
    [ProducesResponseType(typeof(ContentCalendarEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntry(int entryId)
    {
        var entry = await _calendarService.GetEntryAsync(entryId);
        return entry is null
            ? NotFound(new { success = false, message = $"Calendar entry {entryId} not found." })
            : Ok(new { success = true, data = entry });
    }

    /// <summary>Returns upcoming calendar entries within the next <paramref name="daysAhead"/> days.</summary>
    /// <param name="daysAhead">Lookahead window in days (1–90). Defaults to 7.</param>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IEnumerable<ContentCalendarEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int daysAhead = 7)
    {
        try
        {
            var entries = (await _calendarService.GetUpcomingEntriesAsync(daysAhead)).ToList();
            return Ok(new { success = true, data = entries, count = entries.Count });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>Returns entries whose scheduled time falls within the given UTC date range.</summary>
    /// <param name="from">Range start (UTC).</param>
    /// <param name="to">Range end (UTC).</param>
    [HttpGet("range")]
    [ProducesResponseType(typeof(IEnumerable<ContentCalendarEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInRange([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var entries = (await _calendarService.GetEntriesInRangeAsync(from, to)).ToList();
            return Ok(new { success = true, data = entries, count = entries.Count });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>Updates an existing calendar entry.</summary>
    /// <param name="entryId">Entry identifier.</param>
    /// <param name="request">Updated entry payload.</param>
    [HttpPut("{entryId:int}")]
    [ProducesResponseType(typeof(ContentCalendarEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEntry(int entryId, [FromBody] UpdateCalendarEntryRequest request)
    {
        try
        {
            var entry = await _calendarService.GetEntryAsync(entryId);
            if (entry is null)
                return NotFound(new { success = false, message = $"Calendar entry {entryId} not found." });

            entry.Title              = request.Title;
            entry.Description        = request.Description ?? entry.Description;
            entry.Tags               = request.Tags        ?? entry.Tags;
            entry.Category           = request.Category    ?? entry.Category;
            entry.ScheduledPublishAt = request.ScheduledPublishAt ?? entry.ScheduledPublishAt;
            entry.Notes              = request.Notes        ?? entry.Notes;
            entry.Keywords           = request.Keywords     ?? entry.Keywords;

            var updated = await _calendarService.UpdateEntryAsync(entry);
            return Ok(new { success = true, data = updated });
        }
        catch (Exception ex) when (IsClientError(ex))
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update calendar entry {EntryId}", entryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    /// <summary>Permanently deletes a calendar entry.</summary>
    /// <param name="entryId">Entry identifier.</param>
    [HttpDelete("{entryId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEntry(int entryId)
    {
        var deleted = await _calendarService.DeleteEntryAsync(entryId);
        return deleted
            ? Ok(new { success = true, message = "Entry deleted." })
            : NotFound(new { success = false, message = $"Calendar entry {entryId} not found." });
    }

    /// <summary>
    /// Runs the title and description optimisation engine against the entry's current metadata
    /// and stores the result on the entry without modifying the published fields.
    /// </summary>
    /// <param name="entryId">Entry identifier.</param>
    [HttpPost("{entryId:int}/optimize")]
    [ProducesResponseType(typeof(TitleOptimizationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OptimizeEntry(int entryId)
    {
        try
        {
            var result = await _calendarService.OptimizeEntryAsync(entryId);
            return Ok(new { success = true, data = result });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimisation failed for entry {EntryId}", entryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Applies one of the stored optimisation suggestions to the entry's title, description
    /// and tags, transitioning it to <c>Optimised</c> status.
    /// </summary>
    /// <param name="entryId">Entry identifier.</param>
    /// <param name="suggestionIndex">Zero-based index into the suggestions list. Defaults to 0 (best).</param>
    [HttpPost("{entryId:int}/apply-optimization")]
    [ProducesResponseType(typeof(ContentCalendarEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyOptimization(int entryId, [FromQuery] int suggestionIndex = 0)
    {
        try
        {
            var updated = await _calendarService.ApplyOptimizationAsync(entryId, suggestionIndex);
            return Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex) when (IsClientError(ex))
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Apply optimization failed for entry {EntryId}", entryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Schedules the entry for upload by linking it to a new upload job and setting its
    /// scheduled publish time.
    /// </summary>
    /// <param name="entryId">Entry identifier.</param>
    /// <param name="request">Scheduling parameters.</param>
    [HttpPost("{entryId:int}/schedule")]
    [ProducesResponseType(typeof(ContentCalendarEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScheduleEntry(int entryId, [FromBody] ScheduleEntryRequest request)
    {
        try
        {
            var updated = await _calendarService.ScheduleEntryAsync(entryId, request.ScheduledAt);
            return Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex) when (IsClientError(ex))
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule failed for entry {EntryId}", entryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Returns recommended UTC posting timestamps for the given channel, ordered by
    /// projected audience engagement.
    /// </summary>
    /// <param name="channelId">Target YouTube channel identifier.</param>
    /// <param name="count">Number of slots to return (1–20). Defaults to 5.</param>
    [HttpGet("recommended-slots")]
    [ProducesResponseType(typeof(IEnumerable<DateTime>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecommendedSlots([FromQuery] int channelId, [FromQuery] int count = 5)
    {
        try
        {
            var slots = (await _calendarService.GetRecommendedSlotsAsync(channelId, count)).ToList();
            return Ok(new { success = true, data = slots, count = slots.Count });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recommended slots for channel {ChannelId}", channelId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
        }
    }

    private static bool IsClientError(Exception ex) =>
        ex is ArgumentException or InvalidOperationException or ArgumentNullException;
}

// ── Request / Response models ─────────────────────────────────────────────────

/// <summary>Payload for creating a new calendar entry.</summary>
public sealed class CreateCalendarEntryRequest
{
    /// <summary>Video title (required, max 100 characters).</summary>
    public required string Title { get; set; }

    /// <summary>Optional description text.</summary>
    public string? Description { get; set; }

    /// <summary>Tags to associate with the video.</summary>
    public string[]? Tags { get; set; }

    /// <summary>Content category for optimisation scoring.</summary>
    public ContentCategory Category { get; set; } = ContentCategory.Other;

    /// <summary>Planned UTC publish time.</summary>
    public DateTime ScheduledPublishAt { get; set; }

    /// <summary>Target YouTube channel identifier.</summary>
    public int YouTubeChannelId { get; set; }

    /// <summary>Internal notes visible only to the content team.</summary>
    public string? Notes { get; set; }

    /// <summary>Seed keywords used by the optimisation engine.</summary>
    public string[]? Keywords { get; set; }
}

/// <summary>Payload for updating an existing calendar entry (all fields optional).</summary>
public sealed class UpdateCalendarEntryRequest
{
    /// <summary>Replacement title (required).</summary>
    public required string Title { get; set; }

    /// <summary>Replacement description, or <c>null</c> to keep the existing value.</summary>
    public string? Description { get; set; }

    /// <summary>Replacement tags, or <c>null</c> to keep existing tags.</summary>
    public string[]? Tags { get; set; }

    /// <summary>Replacement category, or <c>null</c> to keep the existing value.</summary>
    public ContentCategory? Category { get; set; }

    /// <summary>New scheduled publish time, or <c>null</c> to keep the existing value.</summary>
    public DateTime? ScheduledPublishAt { get; set; }

    /// <summary>Replacement notes, or <c>null</c> to keep existing notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Replacement keywords, or <c>null</c> to keep existing keywords.</summary>
    public string[]? Keywords { get; set; }
}

/// <summary>Payload for scheduling a calendar entry for upload.</summary>
public sealed class ScheduleEntryRequest
{
    /// <summary>UTC time at which the upload should be dispatched.</summary>
    public DateTime ScheduledAt { get; set; }
}
