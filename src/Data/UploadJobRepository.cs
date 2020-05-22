// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;
using System.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Repository implementation for managing upload job entities in the database.
/// Provides CRUD operations and specialized queries for upload job management.
/// </summary>
public class UploadJobRepository : IRepository<UploadJob>
{
	private readonly DatabaseContext _context;

	/// <summary>
	/// Initializes a new instance of the <see cref="UploadJobRepository"/> class.
	/// </summary>
	/// <param name="context">The database context for data access operations.</param>
	/// <exception cref="ArgumentNullException">Thrown when the context parameter is null.</exception>
	public UploadJobRepository(DatabaseContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	/// <summary>
	/// Protected parameterless constructor to allow test mocking frameworks to
	/// generate a class proxy without requiring a real <see cref="DatabaseContext"/>.
	/// </summary>
	protected UploadJobRepository()
	{
		_context = null!;
	}

	/// <summary>
	/// Retrieves an upload job by its unique identifier.
	/// </summary>
	/// <param name="id">The unique identifier of the upload job to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>
	/// An <see cref="UploadJob"/> instance if found, otherwise null.
	/// </returns>
	public virtual async Task<UploadJob?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		// Retrieves an upload job by ID
		var query = @"
		SELECT Id, VideoShortId, YouTubeVideoId, Status, ScheduledAt, UploadedAt,
		AttemptCount, MaxRetries, ErrorMessage, UploadedBytes, UploadProgressPercentage,
		EstimatedTimeRemaining, CreatedAt, UpdatedAt
		FROM UploadJobs WHERE Id = @Id";

		var parameters = new Dictionary<string, object?> { { "@Id", id } };
		var row = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
		return row.Rows.Count > 0 ? MapToEntity(row.Rows[0]) : null;
	}

	/// <summary>
	/// Retrieves all upload jobs from the database.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A list of all upload jobs ordered by scheduled date in descending order.</returns>
	public virtual async Task<IEnumerable<UploadJob>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		// Retrieves all upload jobs
		var query = @"
		SELECT Id, VideoShortId, YouTubeVideoId, Status, ScheduledAt, UploadedAt,
		AttemptCount, MaxRetries, ErrorMessage, UploadedBytes, UploadProgressPercentage,
		EstimatedTimeRemaining, CreatedAt, UpdatedAt
		FROM UploadJobs ORDER BY ScheduledAt DESC";

		var dataTable = await _context.ExecuteDataTableAsync(query);
		return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
	}

	/// <summary>
	/// Retrieves upload jobs filtered by their status.
	/// </summary>
	/// <param name="status">The upload status to filter by.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A list of upload jobs matching the specified status, ordered by scheduled date.</returns>
	public virtual async Task<IEnumerable<UploadJob>> GetByStatusAsync(UploadStatus status, CancellationToken cancellationToken = default)
	{
		// Retrieves upload jobs by status
		var query = @"
		SELECT Id, VideoShortId, YouTubeVideoId, Status, ScheduledAt, UploadedAt,
		AttemptCount, MaxRetries, ErrorMessage, UploadedBytes, UploadProgressPercentage,
		EstimatedTimeRemaining, CreatedAt, UpdatedAt
		FROM UploadJobs WHERE Status = @Status ORDER BY ScheduledAt ASC";

		var parameters = new Dictionary<string, object?> { { "@Status", (int)status } };
		var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
		return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
	}

	/// <summary>
	/// Retrieves upload jobs that are scheduled for upload and ready to be processed.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A list of upload jobs that are queued or pending and scheduled for immediate processing.</returns>
	public virtual async Task<IEnumerable<UploadJob>> GetScheduledForUploadAsync(CancellationToken cancellationToken = default)
	{
		// Retrieves jobs scheduled for upload that haven't started yet
		var query = @"
		SELECT Id, VideoShortId, YouTubeVideoId, Status, ScheduledAt, UploadedAt,
		AttemptCount, MaxRetries, ErrorMessage, UploadedBytes, UploadProgressPercentage,
		EstimatedTimeRemaining, CreatedAt, UpdatedAt
		FROM UploadJobs
		WHERE (Status = @Queued OR Status = @Pending) AND ScheduledAt <= GETUTCDATE()
		ORDER BY ScheduledAt ASC";

		var parameters = new Dictionary<string, object?>
		{
			{ "@Queued", (int)UploadStatus.Queued },
			{ "@Pending", (int)UploadStatus.Pending }
		};

		var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
		return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
	}

	/// <summary>
	/// Retrieves failed upload jobs that are eligible for retry based on their attempt count.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>A list of failed upload jobs that haven't reached their maximum retry count.</returns>
	public virtual async Task<IEnumerable<UploadJob>> GetRetryableFailedJobsAsync(CancellationToken cancellationToken = default)
	{
		// Retrieves failed upload jobs that can be retried
		var query = @"
		SELECT Id, VideoShortId, YouTubeVideoId, Status, ScheduledAt, UploadedAt,
		AttemptCount, MaxRetries, ErrorMessage, UploadedBytes, UploadProgressPercentage,
		EstimatedTimeRemaining, CreatedAt, UpdatedAt
		FROM UploadJobs
		WHERE Status = @Failed AND AttemptCount < MaxRetries
		ORDER BY UpdatedAt ASC";

		var parameters = new Dictionary<string, object?> { { "@Failed", (int)UploadStatus.Failed } };
		var dataTable = await _context.ExecuteDataTableAsync(query, CommandType.Text, parameters);
		return dataTable.Rows.Cast<DataRow>().Select(MapToEntity).ToList();
	}

	/// <summary>
	/// Adds a new upload job to the database.
	/// </summary>
	/// <param name="entity">The upload job entity to add.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>The added upload job entity with its generated identifier.</returns>
	public virtual async Task<UploadJob> AddAsync(UploadJob entity, CancellationToken cancellationToken = default)
	{
		// Inserts a new upload job
		var query = @"
		INSERT INTO UploadJobs (VideoShortId, YouTubeVideoId, Status, ScheduledAt, AttemptCount,
		MaxRetries, UploadProgressPercentage, CreatedAt, UpdatedAt)
		VALUES (@VideoShortId, @YouTubeVideoId, @Status, @ScheduledAt, @AttemptCount,
		@MaxRetries, @UploadProgressPercentage, @CreatedAt, @UpdatedAt);
		SELECT SCOPE_IDENTITY();";

		var parameters = new Dictionary<string, object?>
		{
			{ "@VideoShortId", entity.VideoShortId },
			{ "@YouTubeVideoId", entity.YouTubeVideoId },
			{ "@Status", (int)entity.Status },
			{ "@ScheduledAt", entity.ScheduledAt },
			{ "@AttemptCount", entity.AttemptCount },
			{ "@MaxRetries", entity.MaxRetries },
			{ "@UploadProgressPercentage", entity.UploadProgressPercentage },
			{ "@CreatedAt", DateTime.UtcNow },
			{ "@UpdatedAt", DateTime.UtcNow }
		};

		var id = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
		entity.Id = id;
		return entity;
	}

	/// <summary>
	/// Updates an existing upload job in the database.
	/// </summary>
	/// <param name="entity">The upload job entity with updated values.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>The updated upload job entity.</returns>
	public virtual async Task<UploadJob> UpdateAsync(UploadJob entity, CancellationToken cancellationToken = default)
	{
		// Updates an existing upload job
		var query = @"
		UPDATE UploadJobs
		SET YouTubeVideoId = @YouTubeVideoId, Status = @Status, UploadedAt = @UploadedAt,
		AttemptCount = @AttemptCount, ErrorMessage = @ErrorMessage,
		UploadedBytes = @UploadedBytes, UploadProgressPercentage = @UploadProgressPercentage,
		EstimatedTimeRemaining = @EstimatedTimeRemaining, UpdatedAt = @UpdatedAt
		WHERE Id = @Id";

		var parameters = new Dictionary<string, object?>
		{
			{ "@Id", entity.Id },
			{ "@YouTubeVideoId", entity.YouTubeVideoId },
			{ "@Status", (int)entity.Status },
			{ "@UploadedAt", entity.UploadedAt ?? (object?)DBNull.Value },
			{ "@AttemptCount", entity.AttemptCount },
			{ "@ErrorMessage", entity.ErrorMessage ?? (object?)DBNull.Value },
			{ "@UploadedBytes", entity.UploadedBytes },
			{ "@UploadProgressPercentage", entity.UploadProgressPercentage },
			{ "@EstimatedTimeRemaining", entity.EstimatedTimeRemaining.TotalSeconds },
			{ "@UpdatedAt", DateTime.UtcNow }
		};

		await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
		return entity;
	}

	/// <summary>
	/// Deletes an upload job from the database by its identifier.
	/// </summary>
	/// <param name="id">The unique identifier of the upload job to delete.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>True if the upload job was successfully deleted, otherwise false.</returns>
	public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		// Deletes an upload job
		var query = "DELETE FROM UploadJobs WHERE Id = @Id";
		var parameters = new Dictionary<string, object?> { { "@Id", id } };
		var result = await _context.ExecuteCommandAsync(query, CommandType.Text, parameters);
		return result > 0;
	}

	/// <summary>
	/// Checks if an upload job with the specified identifier exists in the database.
	/// </summary>
	/// <param name="id">The unique identifier to check for existence.</param>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>True if the upload job exists, otherwise false.</returns>
	public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
	{
		// Checks if an upload job exists
		var query = "SELECT COUNT(1) FROM UploadJobs WHERE Id = @Id";
		var parameters = new Dictionary<string, object?> { { "@Id", id } };
		var count = await _context.ExecuteScalarAsync<int>(query, CommandType.Text, parameters);
		return count > 0;
	}

	/// <summary>
	/// Returns the total count of upload jobs in the database.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	/// <returns>The total number of upload jobs.</returns>
	public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
	{
		// Returns total count of upload jobs
		var query = "SELECT COUNT(1) FROM UploadJobs";
		return (int?)await _context.ExecuteScalarAsync<int>(query) ?? 0;
	}

	/// <summary>
	/// Placeholder method for save changes operation.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
	public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		// Placeholder for save changes
		await Task.CompletedTask;
	}

	private static UploadJob MapToEntity(DataRow row)
	{
		// Maps a DataRow to an UploadJob entity
		return new UploadJob
		{
			Id = (int)row["Id"],
			VideoShortId = (int)row["VideoShortId"],
			YouTubeVideoId = (string)row["YouTubeVideoId"],
			Status = (UploadStatus)(int)row["Status"],
			ScheduledAt = (DateTime)row["ScheduledAt"],
			UploadedAt = row["UploadedAt"] != DBNull.Value ? (DateTime?)row["UploadedAt"] : null,
			AttemptCount = (int)row["AttemptCount"],
			MaxRetries = (int)row["MaxRetries"],
			ErrorMessage = row["ErrorMessage"] != DBNull.Value ? (string)row["ErrorMessage"] : null,
			UploadedBytes = (long)row["UploadedBytes"],
			UploadProgressPercentage = (double)row["UploadProgressPercentage"],
			EstimatedTimeRemaining = TimeSpan.FromSeconds((double)row["EstimatedTimeRemaining"]),
			CreatedAt = (DateTime)row["CreatedAt"],
			UpdatedAt = (DateTime)row["UpdatedAt"]
		};
	}
}