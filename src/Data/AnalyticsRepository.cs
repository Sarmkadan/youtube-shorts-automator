// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using System.Data;

namespace YouTubeShortAutomator.Data;

public class AnalyticsRepository : IRepository<AnalyticsData>
{
    private readonly DatabaseContext _context;

    public AnalyticsRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AnalyticsData?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Retrieves analytics by ID
        var query = @"
            SELECT Id, VideoShortId, ViewCount, LikeCount, CommentCount, ShareCount, 
                   AverageViewDuration, EngagementRate, ClickThroughRate, SubscribersGained, 
                   SubscribersLost, AudienceRetentionPercentage, TrafficSources, ImpressionCount, UpdatedAt
            FROM AnalyticsData WHERE Id = @Id";

        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var row = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return row.Rows.Count > 0 ? MapToEntity(row.Rows[0]) : null;
    }

    public async Task<IEnumerable<AnalyticsData>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Retrieves all analytics records
        var query = @"
            SELECT Id, VideoShortId, ViewCount, LikeCount, CommentCount, ShareCount, 
                   AverageViewDuration, EngagementRate, ClickThroughRate, SubscribersGained, 
                   SubscribersLost, AudienceRetentionPercentage, TrafficSources, ImpressionCount, UpdatedAt
            FROM AnalyticsData ORDER BY UpdatedAt DESC";

        var dataTable = await _context.ExecuteDataTableAsync(query);
        return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    public async Task<AnalyticsData?> GetByVideoIdAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        // Retrieves analytics for a specific video
        var query = @"
            SELECT Id, VideoShortId, ViewCount, LikeCount, CommentCount, ShareCount, 
                   AverageViewDuration, EngagementRate, ClickThroughRate, SubscribersGained, 
                   SubscribersLost, AudienceRetentionPercentage, TrafficSources, ImpressionCount, UpdatedAt
            FROM AnalyticsData WHERE VideoShortId = @VideoShortId";

        var parameters = new Dictionary<string, object?> { { "@VideoShortId", videoShortId } };
        var row = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return row.Rows.Count > 0 ? MapToEntity(row.Rows[0]) : null;
    }

    public async Task<IEnumerable<AnalyticsData>> GetTopPerformersAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        // Retrieves top performing videos by engagement rate
        var query = @"
            SELECT TOP (@Limit) Id, VideoShortId, ViewCount, LikeCount, CommentCount, ShareCount, 
                   AverageViewDuration, EngagementRate, ClickThroughRate, SubscribersGained, 
                   SubscribersLost, AudienceRetentionPercentage, TrafficSources, ImpressionCount, UpdatedAt
            FROM AnalyticsData 
            ORDER BY EngagementRate DESC, ViewCount DESC";

        var parameters = new Dictionary<string, object?> { { "@Limit", limit } };
        var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
        return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
    }

    public async Task<AnalyticsData> AddAsync(AnalyticsData entity, CancellationToken cancellationToken = default)
    {
        // Inserts new analytics record
        var query = @"
            INSERT INTO AnalyticsData (VideoShortId, ViewCount, LikeCount, CommentCount, ShareCount, 
                                      AverageViewDuration, EngagementRate, ClickThroughRate, 
                                      SubscribersGained, SubscribersLost, AudienceRetentionPercentage, 
                                      TrafficSources, ImpressionCount, UpdatedAt)
            VALUES (@VideoShortId, @ViewCount, @LikeCount, @CommentCount, @ShareCount, 
                    @AverageViewDuration, @EngagementRate, @ClickThroughRate, 
                    @SubscribersGained, @SubscribersLost, @AudienceRetentionPercentage, 
                    @TrafficSources, @ImpressionCount, @UpdatedAt);
            SELECT SCOPE_IDENTITY();";

        var parameters = new Dictionary<string, object?>
        {
            { "@VideoShortId", entity.VideoShortId },
            { "@ViewCount", entity.ViewCount },
            { "@LikeCount", entity.LikeCount },
            { "@CommentCount", entity.CommentCount },
            { "@ShareCount", entity.ShareCount },
            { "@AverageViewDuration", entity.AverageViewDuration },
            { "@EngagementRate", entity.EngagementRate },
            { "@ClickThroughRate", entity.ClickThroughRate },
            { "@SubscribersGained", entity.SubscribersGained },
            { "@SubscribersLost", entity.SubscribersLost },
            { "@AudienceRetentionPercentage", entity.AudienceRetentionPercentage },
            { "@TrafficSources", entity.TrafficSources },
            { "@ImpressionCount", entity.ImpressionCount },
            { "@UpdatedAt", DateTime.UtcNow }
        };

        var id = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        entity.Id = id;
        return entity;
    }

    public async Task<AnalyticsData> UpdateAsync(AnalyticsData entity, CancellationToken cancellationToken = default)
    {
        // Updates existing analytics record
        var query = @"
            UPDATE AnalyticsData 
            SET ViewCount = @ViewCount, LikeCount = @LikeCount, CommentCount = @CommentCount, 
                ShareCount = @ShareCount, AverageViewDuration = @AverageViewDuration, 
                EngagementRate = @EngagementRate, ClickThroughRate = @ClickThroughRate,
                SubscribersGained = @SubscribersGained, SubscribersLost = @SubscribersLost,
                AudienceRetentionPercentage = @AudienceRetentionPercentage, 
                TrafficSources = @TrafficSources, ImpressionCount = @ImpressionCount, UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var parameters = new Dictionary<string, object?>
        {
            { "@Id", entity.Id },
            { "@ViewCount", entity.ViewCount },
            { "@LikeCount", entity.LikeCount },
            { "@CommentCount", entity.CommentCount },
            { "@ShareCount", entity.ShareCount },
            { "@AverageViewDuration", entity.AverageViewDuration },
            { "@EngagementRate", entity.EngagementRate },
            { "@ClickThroughRate", entity.ClickThroughRate },
            { "@SubscribersGained", entity.SubscribersGained },
            { "@SubscribersLost", entity.SubscribersLost },
            { "@AudienceRetentionPercentage", entity.AudienceRetentionPercentage },
            { "@TrafficSources", entity.TrafficSources },
            { "@ImpressionCount", entity.ImpressionCount },
            { "@UpdatedAt", DateTime.UtcNow }
        };

        await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Deletes an analytics record
        var query = "DELETE FROM AnalyticsData WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var result = await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
        return result > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        // Checks if analytics record exists
        var query = "SELECT COUNT(1) FROM AnalyticsData WHERE Id = @Id";
        var parameters = new Dictionary<string, object?> { { "@Id", id } };
        var count = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
        return count > 0;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        // Returns total count of analytics records
        var query = "SELECT COUNT(1) FROM AnalyticsData";
        return (int?)await _context.ExecuteScalarAsync<int>(query) ?? 0;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder for save changes
        await Task.CompletedTask;
    }

    private static AnalyticsData MapToEntity(DataRow row)
    {
        // Maps a DataRow to an AnalyticsData entity
        return new AnalyticsData
        {
            Id = (int)row["Id"],
            VideoShortId = (int)row["VideoShortId"],
            ViewCount = (long)row["ViewCount"],
            LikeCount = (long)row["LikeCount"],
            CommentCount = (long)row["CommentCount"],
            ShareCount = (long)row["ShareCount"],
            AverageViewDuration = (double)row["AverageViewDuration"],
            EngagementRate = (double)row["EngagementRate"],
            ClickThroughRate = (double)row["ClickThroughRate"],
            SubscribersGained = (int)row["SubscribersGained"],
            SubscribersLost = (int)row["SubscribersLost"],
            AudienceRetentionPercentage = (double)row["AudienceRetentionPercentage"],
            TrafficSources = (int)row["TrafficSources"],
            ImpressionCount = (int)row["ImpressionCount"],
            UpdatedAt = (DateTime)row["UpdatedAt"]
        };
    }
}
