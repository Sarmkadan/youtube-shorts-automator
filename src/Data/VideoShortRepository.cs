// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;
using System.Data;

namespace YouTubeShortAutomator.Data;

public class VideoShortRepository : IRepository<VideoShort>
{
    private readonly DatabaseContext _context;

    public VideoShortRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<VideoShort?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Retrieves a video short by its ID
        var query = @"
            SELECT Id, Title, Description, FilePath, ThumbnailPath, Duration, FileSizeBytes, 
                   Quality, Status, ProcessingProfileId, YouTubeChannelId, ErrorMessage, 
                   CreatedAt, UpdatedAt, ProcessedAt
            FROM VideoShorts WHERE Id = @Id";

        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var row = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);

        return row.Rows.Count > 0 ? MapToEntity(row.Rows[0]) : null;
    }

    public async Task<IEnumerable<VideoShort>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Retrieves all video shorts
        var query = @"
            SELECT Id, Title, Description, FilePath, ThumbnailPath, Duration, FileSizeBytes, 
                   Quality, Status, ProcessingProfileId, YouTubeChannelId, ErrorMessage, 
                   CreatedAt, UpdatedAt, ProcessedAt
            FROM VideoShorts ORDER BY CreatedAt DESC";

        var dataTable = await _context.ExecuteDataTableAsync(query);
        return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<VideoShort>> GetByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default)
    {
        // Retrieves video shorts by processing status
        var query = @"
            SELECT Id, Title, Description, FilePath, ThumbnailPath, Duration, FileSizeBytes, 
                   Quality, Status, ProcessingProfileId, YouTubeChannelId, ErrorMessage, 
                   CreatedAt, UpdatedAt, ProcessedAt
            FROM VideoShorts WHERE Status = @Status ORDER BY CreatedAt DESC";

        var parameters = new Dictionary<string, object?> { { "@Status", (int)status } };
        var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    public async Task<IEnumerable<VideoShort>> GetByChannelAsync(int channelId, CancellationToken cancellationToken = default)
    {
        // Retrieves all video shorts for a specific YouTube channel
        var query = @"
            SELECT Id, Title, Description, FilePath, ThumbnailPath, Duration, FileSizeBytes, 
                   Quality, Status, ProcessingProfileId, YouTubeChannelId, ErrorMessage, 
                   CreatedAt, UpdatedAt, ProcessedAt
            FROM VideoShorts WHERE YouTubeChannelId = @ChannelId ORDER BY CreatedAt DESC";

        var parameters = new Dictionary<string, object?> { { "@ChannelId", channelId } };
        var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    public async Task<VideoShort> AddAsync(VideoShort entity, CancellationToken cancellationToken = default)
    {
        // Inserts a new video short into the database
        var query = @"
            INSERT INTO VideoShorts (Title, Description, FilePath, ThumbnailPath, Duration, FileSizeBytes, 
                                    Quality, Status, ProcessingProfileId, YouTubeChannelId, CreatedAt, UpdatedAt)
            VALUES (@Title, @Description, @FilePath, @ThumbnailPath, @Duration, @FileSizeBytes, 
                    @Quality, @Status, @ProcessingProfileId, @YouTubeChannelId, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();";

        var parameters = new Dictionary<string, object?>
        {
            { "@Title", entity.Title },
            { "@Description", entity.Description },
            { "@FilePath", entity.FilePath },
            { "@ThumbnailPath", entity.ThumbnailPath ?? (object?)DBNull.Value },
            { "@Duration", entity.Duration.TotalSeconds },
            { "@FileSizeBytes", entity.FileSizeBytes },
            { "@Quality", (int)entity.Quality },
            { "@Status", (int)entity.Status },
            { "@ProcessingProfileId", entity.ProcessingProfileId },
            { "@YouTubeChannelId", entity.YouTubeChannelId },
            { "@CreatedAt", DateTime.UtcNow },
            { "@UpdatedAt", DateTime.UtcNow }
        };

        var id = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        entity.Id = id;
        return entity;
    }

    public async Task<VideoShort> UpdateAsync(VideoShort entity, CancellationToken cancellationToken = default)
    {
        // Updates an existing video short
        var query = @"
            UPDATE VideoShorts 
            SET Title = @Title, Description = @Description, FilePath = @FilePath, ThumbnailPath = @ThumbnailPath,
                Quality = @Quality, Status = @Status, ErrorMessage = @ErrorMessage, 
                ProcessedAt = @ProcessedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var parameters = new Dictionary<string, object?>
        {
            { "@Id", entity.Id },
            { "@Title", entity.Title },
            { "@Description", entity.Description },
            { "@FilePath", entity.FilePath },
            { "@ThumbnailPath", entity.ThumbnailPath ?? (object?)DBNull.Value },
            { "@Quality", (int)entity.Quality },
            { "@Status", (int)entity.Status },
            { "@ErrorMessage", entity.ErrorMessage ?? (object?)DBNull.Value },
            { "@ProcessedAt", entity.ProcessedAt ?? (object?)DBNull.Value },
            { "@UpdatedAt", DateTime.UtcNow }
        };

        await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Deletes a video short from the database
        var query = "DELETE FROM VideoShorts WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var result = await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return result > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        // Checks if a video short exists
        var query = "SELECT COUNT(1) FROM VideoShorts WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var count = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        return count > 0;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        // Returns the total count of video shorts
        var query = "SELECT COUNT(1) FROM VideoShorts";
        return (int?)await _context.ExecuteScalarAsync<int>(query) ?? 0;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder for save changes - SQL Server commits automatically
        await Task.CompletedTask;
    }

    private static VideoShort MapToEntity(DataRow row)
    {
        // Maps a DataRow to a VideoShort entity
        return new VideoShort
        {
            Id = (int)row["Id"],
            Title = (string)row["Title"],
            Description = (string)row["Description"],
            FilePath = (string)row["FilePath"],
            ThumbnailPath = row["ThumbnailPath"] != DBNull.Value ? (string)row["ThumbnailPath"] : null,
            Duration = TimeSpan.FromSeconds((double)row["Duration"]),
            FileSizeBytes = (long)row["FileSizeBytes"],
            Quality = (VideoQuality)(int)row["Quality"],
            Status = (ProcessingStatus)(int)row["Status"],
            ProcessingProfileId = (int)row["ProcessingProfileId"],
            YouTubeChannelId = (int)row["YouTubeChannelId"],
            ErrorMessage = row["ErrorMessage"] != DBNull.Value ? (string)row["ErrorMessage"] : null,
            CreatedAt = (DateTime)row["CreatedAt"],
            UpdatedAt = (DateTime)row["UpdatedAt"],
            ProcessedAt = row["ProcessedAt"] != DBNull.Value ? (DateTime?)row["ProcessedAt"] : null
        };
    }
}
