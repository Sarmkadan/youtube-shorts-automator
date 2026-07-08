// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using System.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>Provides data-access operations for <see cref="ThumbnailVariant"/> entities.</summary>
public class ThumbnailAbTestRepository : IRepository<ThumbnailVariant>
{
    private readonly DatabaseContext _context;

    /// <summary>Initializes a new instance of <see cref="ThumbnailAbTestRepository"/>.</summary>
    /// <param name="context">The database context.</param>
    public ThumbnailAbTestRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<ThumbnailVariant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT Id, VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                   ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt
            FROM ThumbnailVariants WHERE Id = @Id";

        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Count > 0 ? MapToEntity(table.Rows[0]) : null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ThumbnailVariant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT Id, VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                   ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt
            FROM ThumbnailVariants ORDER BY CreatedAt DESC";

        var table = await _context.ExecuteDataTableAsync(query);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <summary>Returns all variants associated with a specific video, ordered by label.</summary>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    public async Task<IEnumerable<ThumbnailVariant>> GetByVideoShortIdAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT Id, VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                   ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt
            FROM ThumbnailVariants WHERE VideoShortId = @VideoShortId ORDER BY Label";

        var parameters = new Dictionary<string, object?> { { "@VideoShortId", videoShortId } };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <summary>Returns only the currently-active (serving) variants for a given video.</summary>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    public async Task<IEnumerable<ThumbnailVariant>> GetActiveVariantsAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT Id, VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                   ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt
            FROM ThumbnailVariants WHERE VideoShortId = @VideoShortId AND IsActive = 1";

        var parameters = new Dictionary<string, object?> { { "@VideoShortId", videoShortId } };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    /// <summary>
    /// Returns the winning variant for a video, or <see langword="null"/> if no winner
    /// has been declared yet.
    /// </summary>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    public async Task<ThumbnailVariant?> GetWinnerAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        var query = @"
            SELECT Id, VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                   ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt
            FROM ThumbnailVariants WHERE VideoShortId = @VideoShortId AND IsWinner = 1";

        var parameters = new Dictionary<string, object?> { { "@VideoShortId", videoShortId } };
        var table = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return table.Rows.Count > 0 ? MapToEntity(table.Rows[0]) : null;
    }

    /// <inheritdoc/>
    public async Task<ThumbnailVariant> AddAsync(ThumbnailVariant entity, CancellationToken cancellationToken = default)
    {
        var query = @"
            INSERT INTO ThumbnailVariants
                (VideoShortId, Label, ThumbnailPath, ImpressionCount, ClickCount,
                 ViewRate, IsActive, IsWinner, CreatedAt, UpdatedAt)
            VALUES
                (@VideoShortId, @Label, @ThumbnailPath, @ImpressionCount, @ClickCount,
                 @ViewRate, @IsActive, @IsWinner, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();";

        var parameters = BuildParameters(entity);
        var id = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        entity.Id = id;
        return entity;
    }

    /// <inheritdoc/>
    public async Task<ThumbnailVariant> UpdateAsync(ThumbnailVariant entity, CancellationToken cancellationToken = default)
    {
        var query = @"
            UPDATE ThumbnailVariants
            SET Label = @Label, ThumbnailPath = @ThumbnailPath,
                ImpressionCount = @ImpressionCount, ClickCount = @ClickCount,
                ViewRate = @ViewRate, IsActive = @IsActive, IsWinner = @IsWinner,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var parameters = BuildParameters(entity);
        parameters["@Id"] = entity.Id;
        await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return entity;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "DELETE FROM ThumbnailVariants WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var result = await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return result > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(1) FROM ThumbnailVariants WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var count = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        return count > 0;
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(1) FROM ThumbnailVariants";
        return (int?)await _context.ExecuteScalarAsync<int>(query) ?? 0;
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    private static Dictionary<string, object?> BuildParameters(ThumbnailVariant entity) => new()
    {
        { "@VideoShortId", entity.VideoShortId },
        { "@Label",        entity.Label },
        { "@ThumbnailPath", entity.ThumbnailPath },
        { "@ImpressionCount", entity.ImpressionCount },
        { "@ClickCount",   entity.ClickCount },
        { "@ViewRate",     entity.ViewRate },
        { "@IsActive",     entity.IsActive },
        { "@IsWinner",     entity.IsWinner },
        { "@CreatedAt",    entity.CreatedAt },
        { "@UpdatedAt",    entity.UpdatedAt }
    };

    private static ThumbnailVariant MapToEntity(DataRow row) => new()
    {
        Id             = (int)row["Id"],
        VideoShortId   = (int)row["VideoShortId"],
        Label          = (string)row["Label"],
        ThumbnailPath  = (string)row["ThumbnailPath"],
        ImpressionCount = (long)row["ImpressionCount"],
        ClickCount     = (long)row["ClickCount"],
        ViewRate       = (double)row["ViewRate"],
        IsActive       = (bool)row["IsActive"],
        IsWinner       = (bool)row["IsWinner"],
        CreatedAt      = (DateTime)row["CreatedAt"],
        UpdatedAt      = (DateTime)row["UpdatedAt"]
    };
}
