// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Caching;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// API endpoints for scheduling uploads
/// Manages scheduled video uploads with timezone support and recurrence options
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly ICacheService _cacheService;

    public ScheduleController(
        ILogger<ScheduleController> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new scheduled upload
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScheduleAsync([FromBody] CreateScheduleRequest request)
    {
        try
        {
            // Validate request
            var (isValid, error) = ValidationUtility.ValidateScheduleTime(request.ScheduledUploadTimeUtc.ToString("O"));
            if (!isValid)
                return BadRequest(new { message = error });

            var scheduleId = Guid.NewGuid();

            var schedule = new ScheduledUpload
            {
                ScheduleId = scheduleId,
                VideoId = request.VideoId,
                ScheduledUploadTimeUtc = request.ScheduledUploadTimeUtc,
                Status = "Scheduled",
                CreatedAtUtc = DateTime.UtcNow,
                RecurrencePattern = request.RecurrencePattern,
                TimeZone = request.TimeZone ?? "UTC"
            };

            _cacheService.Set($"schedule:{scheduleId}", schedule, TimeSpan.FromDays(30));

            _logger.LogInformation("Upload scheduled. ScheduleId: {ScheduleId}, UploadTime: {UploadTime}",
                scheduleId, request.ScheduledUploadTimeUtc);

            return CreatedAtAction(nameof(GetScheduleAsync), new { scheduleId },
                new ScheduleResponse { ScheduleId = scheduleId, Status = "Scheduled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get details of a scheduled upload
    /// </summary>
    [HttpGet("{scheduleId}")]
    [ProducesResponseType(typeof(ScheduledUploadDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScheduleAsync(Guid scheduleId)
    {
        try
        {
            var schedule = _cacheService.Get<ScheduledUpload>($"schedule:{scheduleId}");
            if (schedule == null)
                return NotFound();

            return Ok(new ScheduledUploadDetails
            {
                ScheduleId = schedule.ScheduleId,
                VideoId = schedule.VideoId,
                ScheduledUploadTimeUtc = schedule.ScheduledUploadTimeUtc,
                Status = schedule.Status,
                RecurrencePattern = schedule.RecurrencePattern,
                TimeZone = schedule.TimeZone,
                CreatedAtUtc = schedule.CreatedAtUtc,
                LastExecutedUtc = schedule.LastExecutedUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// List all scheduled uploads
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ScheduleListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSchedulesAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters");

            // Simulate fetching from database
            var schedules = new List<ScheduleListItem>
            {
                new() { ScheduleId = Guid.NewGuid(), VideoId = Guid.NewGuid(), Status = "Scheduled", ScheduledUploadTimeUtc = DateTime.UtcNow.AddDays(1) },
                new() { ScheduleId = Guid.NewGuid(), VideoId = Guid.NewGuid(), Status = "Pending", ScheduledUploadTimeUtc = DateTime.UtcNow.AddHours(2) }
            };

            var response = new ScheduleListResponse
            {
                Schedules = schedules,
                TotalCount = schedules.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing schedules");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update a scheduled upload
    /// </summary>
    [HttpPut("{scheduleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScheduleAsync(Guid scheduleId, [FromBody] UpdateScheduleRequest request)
    {
        try
        {
            var schedule = _cacheService.Get<ScheduledUpload>($"schedule:{scheduleId}");
            if (schedule == null)
                return NotFound();

            if (request.ScheduledUploadTimeUtc.HasValue)
            {
                schedule.ScheduledUploadTimeUtc = request.ScheduledUploadTimeUtc.Value;
            }

            if (!string.IsNullOrEmpty(request.RecurrencePattern))
            {
                schedule.RecurrencePattern = request.RecurrencePattern;
            }

            _cacheService.Set($"schedule:{scheduleId}", schedule, TimeSpan.FromDays(30));

            _logger.LogInformation("Schedule updated. ScheduleId: {ScheduleId}", scheduleId);

            return Ok(new { message = "Schedule updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete a scheduled upload
    /// </summary>
    [HttpDelete("{scheduleId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduleAsync(Guid scheduleId)
    {
        try
        {
            await _cacheService.RemoveAsync($"schedule:{scheduleId}");
            _logger.LogInformation("Schedule deleted. ScheduleId: {ScheduleId}", scheduleId);
            return Ok(new { message = "Schedule deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule: {ScheduleId}", scheduleId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Import multiple scheduled uploads from a CSV file.
    /// Expected columns (header row required):
    ///   FilePath, Title, Description, Tags, ScheduledPublishTimeUtc, ThumbnailPath
    /// Tags should be pipe-separated (e.g. "tag1|tag2|tag3").
    /// Rows with validation errors are reported individually; valid rows are committed.
    /// </summary>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CsvImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportSchedulesFromCsvAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "A non-empty CSV file is required." });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only CSV files are accepted." });

        var rowErrors = new List<CsvRowError>();
        var created = new List<ScheduleResponse>();
        int lineNumber = 0;

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            string? headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
                return BadRequest(new { message = "CSV file is empty." });

            lineNumber = 1;
            var headers = ParseCsvLine(headerLine);
            var required = new[] { "FilePath", "Title", "ScheduledPublishTimeUtc" };
            var missing = required.Where(h => !headers.ContainsKey(h)).ToList();
            if (missing.Count > 0)
                return BadRequest(new { message = $"Missing required CSV columns: {string.Join(", ", missing)}" });

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvLine(line, headers.Count);
                var rowErrors2 = new List<string>();

                var row = new CsvScheduleRow
                {
                    FilePath = GetColumn(cols, headers, "FilePath"),
                    Title = GetColumn(cols, headers, "Title"),
                    Description = GetColumn(cols, headers, "Description"),
                    Tags = GetColumn(cols, headers, "Tags"),
                    ThumbnailPath = GetColumn(cols, headers, "ThumbnailPath"),
                    RawScheduledTime = GetColumn(cols, headers, "ScheduledPublishTimeUtc")
                };

                if (string.IsNullOrWhiteSpace(row.FilePath))
                    rowErrors2.Add("FilePath is required.");
                if (string.IsNullOrWhiteSpace(row.Title))
                    rowErrors2.Add("Title is required.");
                if (string.IsNullOrWhiteSpace(row.RawScheduledTime))
                    rowErrors2.Add("ScheduledPublishTimeUtc is required.");
                else if (!DateTime.TryParse(row.RawScheduledTime, null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsedTime))
                    rowErrors2.Add($"ScheduledPublishTimeUtc '{row.RawScheduledTime}' is not a valid date/time.");
                else if (parsedTime <= DateTime.UtcNow)
                    rowErrors2.Add("ScheduledPublishTimeUtc must be in the future.");
                else
                    row.ScheduledPublishTimeUtc = parsedTime;

                if (rowErrors2.Count > 0)
                {
                    rowErrors.Add(new CsvRowError { LineNumber = lineNumber, Errors = rowErrors2 });
                    continue;
                }

                var scheduleId = Guid.NewGuid();
                var schedule = new ScheduledUpload
                {
                    ScheduleId = scheduleId,
                    VideoId = Guid.Empty, // resolved later when the file is ingested
                    ScheduledUploadTimeUtc = row.ScheduledPublishTimeUtc,
                    Status = "Scheduled",
                    CreatedAtUtc = DateTime.UtcNow,
                    TimeZone = "UTC"
                };

                _cacheService.Set($"schedule:{scheduleId}", schedule, TimeSpan.FromDays(30));
                created.Add(new ScheduleResponse { ScheduleId = scheduleId, Status = "Scheduled" });

                _logger.LogInformation(
                    "CSV import: schedule created. ScheduleId: {ScheduleId}, Title: {Title}, UploadTime: {UploadTime}",
                    scheduleId, row.Title, row.ScheduledPublishTimeUtc);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV import at line {LineNumber}", lineNumber);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = $"Processing failed at line {lineNumber}: {ex.Message}" });
        }

        return Ok(new CsvImportResult
        {
            SchedulesCreated = created.Count,
            RowsWithErrors = rowErrors.Count,
            CreatedSchedules = created,
            Errors = rowErrors
        });
    }

    private static Dictionary<string, int> ParseCsvLine(string line)
    {
        var parts = SplitCsvLine(line);
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < parts.Count; i++)
            map[parts[i].Trim()] = i;
        return map;
    }

    private static List<string> ParseCsvLine(string line, int expectedCount)
        => SplitCsvLine(line);

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }

    private static string GetColumn(List<string> cols, Dictionary<string, int> headers, string name)
    {
        if (!headers.TryGetValue(name, out var idx) || idx >= cols.Count)
            return string.Empty;
        return cols[idx].Trim();
    }
}

#region Schedule Models

public class CreateScheduleRequest
{
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string? RecurrencePattern { get; set; }
    public string? TimeZone { get; set; }
}

public class UpdateScheduleRequest
{
    public DateTime? ScheduledUploadTimeUtc { get; set; }
    public string? RecurrencePattern { get; set; }
}

public class ScheduleResponse
{
    public Guid ScheduleId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScheduledUpload
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RecurrencePattern { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastExecutedUtc { get; set; }
}

public class ScheduledUploadDetails
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public DateTime ScheduledUploadTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RecurrencePattern { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastExecutedUtc { get; set; }
}

public class ScheduleListItem
{
    public Guid ScheduleId { get; set; }
    public Guid VideoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledUploadTimeUtc { get; set; }
}

public class ScheduleListResponse
{
    public List<ScheduleListItem> Schedules { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class CsvScheduleRow
{
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public string RawScheduledTime { get; set; } = string.Empty;
    public DateTime ScheduledPublishTimeUtc { get; set; }
}

public class CsvRowError
{
    public int LineNumber { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class CsvImportResult
{
    public int SchedulesCreated { get; set; }
    public int RowsWithErrors { get; set; }
    public List<ScheduleResponse> CreatedSchedules { get; set; } = new();
    public List<CsvRowError> Errors { get; set; } = new();
}

#endregion
