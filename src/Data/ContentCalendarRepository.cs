// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using System.Data;
using System.Text.Json;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// ADO.NET-backed persistence layer for <see cref="ContentCalendarEntry"/> records,
/// following the raw-SQL repository pattern used throughout the data access layer.
/// The <c>TitleOptimizationResult</c> object graph is serialised to JSON and stored
/// in the <c>OptimizationJson</c> column.
/// </summary>
public class ContentCalendarRepository : IRepository<ContentCalendarEntry>
{
    private readonly DatabaseContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initialises a new instance of <see cref="ContentCalendarRepository"/>.
    /// </summary>
    /// <param name="context">Shared database context.</param>
    public ContentCalendarRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Protected parameterless constructor to allow test mocking frameworks to
    /// generate a class proxy without requiring a real <see cref="DatabaseContext"/>.
    /// </summary>
    protected ContentCalendarRepository()
    {
        _context = null!;
    }

    /// <inheritdoc />
    public virtual async Task<ContentCalendarEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT Id, Title, Description, Tags, Category, Status, ScheduledPublishAt,
                   VideoShortId, UploadJobId, YouTubeChannelId, OptimizationJson,
                   OptimizationApplied, Notes, Keywords, CreatedAt, UpdatedAt
            FROM ContentCalendarEntries
            WHERE Id = @Id";

        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Count > 0 ? MapToEntity(table.Rows[0]) : null;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<ContentCalendarEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT Id, Title, Description, Tags, Category, Status, ScheduledPublishAt,
                   VideoShortId, UploadJobId, YouTubeChannelId, OptimizationJson,
                   OptimizationApplied, Notes, Keywords, CreatedAt, UpdatedAt
            FROM ContentCalendarEntries
            ORDER BY ScheduledPublishAt ASC";

        var table = await _context.ExecuteDataTableAsync(query);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <summary>
    /// Returns all entries whose <c>ScheduledPublishAt</c> falls within the inclusive
    /// range [<paramref name="from"/>, <paramref name="to"/>], ordered ascending.
    /// </summary>
    /// <param name="from">Range start (inclusive, UTC).</param>
    /// <param name="to">Range end (inclusive, UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual async Task<IEnumerable<ContentCalendarEntry>> GetByDateRangeAsync(
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT Id, Title, Description, Tags, Category, Status, ScheduledPublishAt,
                   VideoShortId, UploadJobId, YouTubeChannelId, OptimizationJson,
                   OptimizationApplied, Notes, Keywords, CreatedAt, UpdatedAt
            FROM ContentCalendarEntries
            WHERE ScheduledPublishAt >= @From AND ScheduledPublishAt <= @To
            ORDER BY ScheduledPublishAt ASC";

        var parameters = new Dictionary<string, object?>
        {
            { "@From", from },
            { "@To", to }
        };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <summary>
    /// Returns entries with <c>ScheduledPublishAt</c> between now and
    /// <paramref name="cutoffUtc"/>, excluding cancelled and archived records.
    /// </summary>
    /// <param name="cutoffUtc">Upper bound of the upcoming window (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual async Task<IEnumerable<ContentCalendarEntry>> GetUpcomingAsync(
        DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT Id, Title, Description, Tags, Category, Status, ScheduledPublishAt,
                   VideoShortId, UploadJobId, YouTubeChannelId, OptimizationJson,
                   OptimizationApplied, Notes, Keywords, CreatedAt, UpdatedAt
            FROM ContentCalendarEntries
            WHERE ScheduledPublishAt >= @Now
              AND ScheduledPublishAt <= @Cutoff
              AND Status NOT IN (@Cancelled, @Archived)
            ORDER BY ScheduledPublishAt ASC";

        var parameters = new Dictionary<string, object?>
        {
            { "@Now", DateTime.UtcNow },
            { "@Cutoff", cutoffUtc },
            { "@Cancelled", (int)CalendarEntryStatus.Cancelled },
            { "@Archived", (int)CalendarEntryStatus.Archived }
        };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<ContentCalendarEntry> AddAsync(ContentCalendarEntry entity, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO ContentCalendarEntries
                (Title, Description, Tags, Category, Status, ScheduledPublishAt,
                 VideoShortId, UploadJobId, YouTubeChannelId, OptimizationJson,
                 OptimizationApplied, Notes, Keywords, CreatedAt, UpdatedAt)
            VALUES
                (@Title, @Description, @Tags, @Category, @Status, @ScheduledPublishAt,
                 @VideoShortId, @UploadJobId, @YouTubeChannelId, @OptimizationJson,
                 @OptimizationApplied, @Notes, @Keywords, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();";

        var parameters = BuildParameters(entity);
        parameters["@CreatedAt"] = DateTime.UtcNow;
        parameters["@UpdatedAt"] = DateTime.UtcNow;

        var id = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        entity.Id = id;
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<ContentCalendarEntry> UpdateAsync(ContentCalendarEntry entity, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE ContentCalendarEntries
            SET Title              = @Title,
                Description        = @Description,
                Tags               = @Tags,
                Category           = @Category,
                Status             = @Status,
                ScheduledPublishAt = @ScheduledPublishAt,
                VideoShortId       = @VideoShortId,
                UploadJobId        = @UploadJobId,
                YouTubeChannelId   = @YouTubeChannelId,
                OptimizationJson   = @OptimizationJson,
                OptimizationApplied= @OptimizationApplied,
                Notes              = @Notes,
                Keywords           = @Keywords,
                UpdatedAt          = @UpdatedAt
            WHERE Id = @Id";

        var parameters = BuildParameters(entity);
        parameters["@Id"] = entity.Id;
        parameters["@UpdatedAt"] = DateTime.UtcNow;

        await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string query = "DELETE FROM ContentCalendarEntries WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var affected = await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return affected > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT COUNT(1) FROM ContentCalendarEntries WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var count = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        return count > 0;
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        const string query = "SELECT COUNT(1) FROM ContentCalendarEntries";
        return (int?)await _context.ExecuteScalarAsync<int>(query) ?? 0;
    }

    /// <inheritdoc />
    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    // ── Private helpers ───────────────────────────────────────────────────────

    private static ContentCalendarEntry MapToEntity(DataRow row)
    {
        TitleOptimizationResult? lastOptimization = null;
        if (row["OptimizationJson"] != DBNull.Value)
        {
            var json = (string)row["OptimizationJson"];
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    lastOptimization = JsonSerializer.Deserialize<TitleOptimizationResult>(json, JsonOptions);
                }
                catch (JsonException)
                {
                    // Corrupt or schema-mismatched JSON — treat as no optimisation data.
                }
            }
        }

        var tagsRaw = row["Tags"] != DBNull.Value ? (string)row["Tags"] : string.Empty;
        var keywordsRaw = row["Keywords"] != DBNull.Value ? (string)row["Keywords"] : string.Empty;

        return new ContentCalendarEntry
        {
            Id = (int)row["Id"],
            Title = (string)row["Title"],
            Description = row["Description"] != DBNull.Value ? (string)row["Description"] : string.Empty,
            Tags = SplitDelimited(tagsRaw),
            Category = (ContentCategory)(int)row["Category"],
            Status = (CalendarEntryStatus)(int)row["Status"],
            ScheduledPublishAt = (DateTime)row["ScheduledPublishAt"],
            VideoShortId = row["VideoShortId"] != DBNull.Value ? (int?)row["VideoShortId"] : null,
            UploadJobId = row["UploadJobId"] != DBNull.Value ? (int?)row["UploadJobId"] : null,
            YouTubeChannelId = (int)row["YouTubeChannelId"],
            LastOptimization = lastOptimization,
            OptimizationApplied = (bool)row["OptimizationApplied"],
            Notes = row["Notes"] != DBNull.Value ? (string)row["Notes"] : null,
            Keywords = SplitDelimited(keywordsRaw),
            CreatedAt = (DateTime)row["CreatedAt"],
            UpdatedAt = (DateTime)row["UpdatedAt"]
        };
    }

    private static Dictionary<string, object?> BuildParameters(ContentCalendarEntry entity)
    {
        string? optimizationJson = entity.LastOptimization is not null
            ? JsonSerializer.Serialize(entity.LastOptimization, JsonOptions)
            : null;

        return new Dictionary<string, object?>
        {
            { "@Title", entity.Title },
            { "@Description", entity.Description },
            { "@Tags", JoinDelimited(entity.Tags) },
            { "@Category", (int)entity.Category },
            { "@Status", (int)entity.Status },
            { "@ScheduledPublishAt", entity.ScheduledPublishAt },
            { "@VideoShortId", entity.VideoShortId ?? (object?)DBNull.Value },
            { "@UploadJobId", entity.UploadJobId ?? (object?)DBNull.Value },
            { "@YouTubeChannelId", entity.YouTubeChannelId },
            { "@OptimizationJson", optimizationJson ?? (object?)DBNull.Value },
            { "@OptimizationApplied", entity.OptimizationApplied },
            { "@Notes", entity.Notes ?? (object?)DBNull.Value },
            { "@Keywords", JoinDelimited(entity.Keywords) },
            { "@UpdatedAt", entity.UpdatedAt }
        };
    }

    private static string[] SplitDelimited(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string JoinDelimited(string[] values) =>
        values.Length == 0 ? string.Empty : string.Join('|', values);
}
