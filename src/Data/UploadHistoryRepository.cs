// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using System.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Persists and queries upload history entries so the application can avoid
/// duplicate uploads and expose a history view via the CLI.
/// </summary>
public class UploadHistoryRepository
{
    private readonly DatabaseContext _context;

    public UploadHistoryRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>Creates the UploadHistory table if it does not already exist.</summary>
    public async Task EnsureTableExistsAsync(CancellationToken cancellationToken = default)
    {
        const string ddl = @"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = 'UploadHistory'
            )
            BEGIN
                CREATE TABLE UploadHistory (
                    Id            INT IDENTITY(1,1) PRIMARY KEY,
                    VideoFileName NVARCHAR(500)  NOT NULL,
                    YouTubeVideoId NVARCHAR(50)  NULL,
                    UploadedAt    DATETIME2      NOT NULL,
                    Status        INT            NOT NULL,
                    ErrorMessage  NVARCHAR(2000) NULL,
                    CreatedAt     DATETIME2      NOT NULL DEFAULT GETUTCDATE()
                );
                CREATE INDEX IX_UploadHistory_VideoFileName
                    ON UploadHistory (VideoFileName);
            END";

        await _context.ExecuteCommandAsync(ddl);
    }

    /// <summary>Adds a new entry to the upload history log.</summary>
    public async Task<UploadHistoryEntry> AddAsync(UploadHistoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO UploadHistory (VideoFileName, YouTubeVideoId, UploadedAt, Status, ErrorMessage, CreatedAt)
            VALUES (@VideoFileName, @YouTubeVideoId, @UploadedAt, @Status, @ErrorMessage, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        var parameters = new Dictionary<string, object?>
        {
            { "@VideoFileName",  entry.VideoFileName },
            { "@YouTubeVideoId", entry.YouTubeVideoId ?? (object?)DBNull.Value },
            { "@UploadedAt",     entry.UploadedAt },
            { "@Status",         (int)entry.Status },
            { "@ErrorMessage",   entry.ErrorMessage ?? (object?)DBNull.Value },
            { "@CreatedAt",      DateTime.UtcNow }
        };

        entry.Id = (int?)await _context.ExecuteScalarAsync<int>(sql, CommandType.Text, parameters) ?? 0;
        return entry;
    }

    /// <summary>
    /// Returns true if a successful upload already exists for the given file name.
    /// Used to skip duplicate uploads.
    /// </summary>
    public async Task<bool> HasSuccessfulUploadAsync(string videoFileName,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1) FROM UploadHistory
            WHERE VideoFileName = @VideoFileName AND Status = @Status";

        var parameters = new Dictionary<string, object?>
        {
            { "@VideoFileName", videoFileName },
            { "@Status",        (int)UploadHistoryStatus.Success }
        };

        var count = await _context.ExecuteScalarAsync<int>(sql, CommandType.Text, parameters);
        return count > 0;
    }

    /// <summary>Returns the most recent <paramref name="limit"/> history entries.</summary>
    public async Task<IEnumerable<UploadHistoryEntry>> GetRecentAsync(int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var sql = $@"
            SELECT TOP (@Limit) Id, VideoFileName, YouTubeVideoId, UploadedAt, Status, ErrorMessage, CreatedAt
            FROM UploadHistory
            ORDER BY UploadedAt DESC";

        var parameters = new Dictionary<string, object?> { { "@Limit", limit } };
        var table = await _context.ExecuteDataTableAsync(sql, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntry).ToList();
    }

    /// <summary>Returns all history entries for the specified file name.</summary>
    public async Task<IEnumerable<UploadHistoryEntry>> GetByFileNameAsync(string videoFileName,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, VideoFileName, YouTubeVideoId, UploadedAt, Status, ErrorMessage, CreatedAt
            FROM UploadHistory
            WHERE VideoFileName = @VideoFileName
            ORDER BY UploadedAt DESC";

        var parameters = new Dictionary<string, object?> { { "@VideoFileName", videoFileName } };
        var table = await _context.ExecuteDataTableAsync(sql, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntry).ToList();
    }

    private static UploadHistoryEntry MapToEntry(DataRow row) => new()
    {
        Id            = (int)row["Id"],
        VideoFileName = (string)row["VideoFileName"],
        YouTubeVideoId = row["YouTubeVideoId"] != DBNull.Value ? (string)row["YouTubeVideoId"] : null,
        UploadedAt    = (DateTime)row["UploadedAt"],
        Status        = (UploadHistoryStatus)(int)row["Status"],
        ErrorMessage  = row["ErrorMessage"] != DBNull.Value ? (string)row["ErrorMessage"] : null,
        CreatedAt     = (DateTime)row["CreatedAt"]
    };
}
