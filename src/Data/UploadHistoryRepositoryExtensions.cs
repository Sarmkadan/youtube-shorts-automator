// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Extension methods for <see cref="UploadHistoryRepository"/> that provide additional
/// query capabilities for upload history management.
/// </summary>
public static class UploadHistoryRepositoryExtensions
{
    /// <summary>
    /// Returns all failed upload history entries, optionally filtered by a time window.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="sinceUtc">Optional minimum upload timestamp (inclusive).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>An enumerable of failed history entries.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<IReadOnlyList<UploadHistoryEntry>> GetFailedUploadsAsync(
        this UploadHistoryRepository repository,
        DateTime? sinceUtc = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var allEntries = await repository.GetRecentAsync(int.MaxValue, cancellationToken);
        var failedEntries = allEntries.Where(e => e.Status == UploadHistoryStatus.Failed).ToList();

        if (sinceUtc.HasValue)
        {
            failedEntries = failedEntries.Where(e => e.UploadedAt >= sinceUtc.Value).ToList();
        }

        return failedEntries.AsReadOnly();
    }

    /// <summary>
    /// Returns all successful upload history entries for the specified file name.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="videoFileName">The video file name to filter by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>An enumerable of successful history entries for the file.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="videoFileName"/> is null or empty.</exception>
    public static async Task<IReadOnlyList<UploadHistoryEntry>> GetSuccessfulUploadsByFileNameAsync(
        this UploadHistoryRepository repository,
        string videoFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(videoFileName);

        var allEntries = await repository.GetByFileNameAsync(videoFileName, cancellationToken);
        return allEntries.Where(e => e.Status == UploadHistoryStatus.Success).ToList().AsReadOnly();
    }

    /// <summary>
    /// Returns the most recent successful upload entry, or null if none exists.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>The most recent successful upload entry, or null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<UploadHistoryEntry?> GetMostRecentSuccessfulUploadAsync(
        this UploadHistoryRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var entries = await repository.GetRecentAsync(1, cancellationToken);
        return entries.FirstOrDefault(e => e.Status == UploadHistoryStatus.Success);
    }

    /// <summary>
    /// Returns the most recent upload entry for the specified file name, or null if none exists.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="videoFileName">The video file name to look up.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>The most recent upload entry for the file, or null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="videoFileName"/> is null or empty.</exception>
    public static async Task<UploadHistoryEntry?> GetMostRecentUploadByFileNameAsync(
        this UploadHistoryRepository repository,
        string videoFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentException.ThrowIfNullOrEmpty(videoFileName);

        var entries = await repository.GetByFileNameAsync(videoFileName, cancellationToken);
        return entries.FirstOrDefault();
    }
}