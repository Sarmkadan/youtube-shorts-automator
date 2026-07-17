// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for AnalyticsRepository to ensure repository operations receive valid parameters
// ===========================================================================

using System.Globalization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Validation helpers for AnalyticsRepository to ensure repository operations receive valid parameters
/// </summary>
public static class AnalyticsRepositoryValidation
{
    /// <summary>
    /// Validates an AnalyticsRepository instance
    /// </summary>
    /// <param name="value">The AnalyticsRepository instance to validate</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if an AnalyticsRepository instance is valid
    /// </summary>
    /// <param name="value">The AnalyticsRepository instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this AnalyticsRepository? value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures an AnalyticsRepository instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The AnalyticsRepository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed error messages</exception>
    public static void EnsureValid(this AnalyticsRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AnalyticsRepository is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates GetByVideoIdAsync parameters
    /// </summary>
    /// <param name="videoId">The video ID to query by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if videoId is empty</exception>
    public static IReadOnlyList<string> ValidateAnalyticsVideoId(this Guid videoId)
    {
        var errors = new List<string>();

        if (videoId == Guid.Empty)
        {
            errors.Add("Video ID cannot be empty (Guid.Empty)");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetByPeriodAsync parameters
    /// </summary>
    /// <param name="period">The metrics period to filter by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if period is not a defined enum value</exception>
    public static IReadOnlyList<string> ValidateAnalyticsMetricsPeriod(this MetricsPeriod period)
    {
        // All MetricsPeriod enum values are valid by design
        // No validation needed beyond null check (which is handled by the enum type itself)
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetRecentMetricsAsync parameters
    /// </summary>
    /// <param name="videoId">The video ID to query by</param>
    /// <param name="daysBack">Number of days to look back</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if videoId is empty or daysBack is invalid</exception>
    public static IReadOnlyList<string> ValidateAnalyticsRecentMetrics(this Guid videoId, int daysBack)
    {
        var errors = new List<string>();

        if (videoId == Guid.Empty)
        {
            errors.Add("Video ID cannot be empty (Guid.Empty)");
        }

        if (daysBack <= 0)
        {
            errors.Add("Days back must be greater than zero");
        }
        else if (daysBack > 365 * 2) // 2 years max
        {
            errors.Add("Days back cannot exceed 2 years");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetTopMetricsAsync parameters
    /// </summary>
    /// <param name="limit">Maximum number of metrics to return</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if limit is less than or equal to zero or exceeds maximum</exception>
    public static IReadOnlyList<string> ValidateAnalyticsMetricsLimit(this int limit)
    {
        var errors = new List<string>();

        if (limit <= 0)
        {
            errors.Add("Limit must be greater than zero");
        }
        else if (limit > 1000)
        {
            errors.Add("Limit cannot exceed 1000 items");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetPaginatedAsync parameters
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if pageNumber is less than 1 or pageSize is invalid</exception>
    public static IReadOnlyList<string> ValidateAnalyticsPaginated(this int pageNumber, int pageSize)
    {
        var errors = new List<string>();

        if (pageNumber < 1)
        {
            errors.Add("Page number must be 1 or greater");
        }

        if (pageSize < 1)
        {
            errors.Add("Page size must be 1 or greater");
        }
        else if (pageSize > 1000)
        {
            errors.Add("Page size cannot exceed 1000 items");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetVideoViewCountsAsync parameters
    /// </summary>
    /// <param name="videoIds">List of video IDs to query by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if videoIds is null</exception>
    public static IReadOnlyList<string> ValidateAnalyticsVideoViewCounts(this List<Guid> videoIds)
    {
        ArgumentNullException.ThrowIfNull(videoIds);

        var errors = new List<string>();

        if (videoIds.Count == 0)
        {
            errors.Add("Video IDs list cannot be empty");
        }

        if (videoIds.Count > 1000)
        {
            errors.Add("Video IDs list cannot exceed 1000 items");
        }

        if (videoIds.Any(id => id == Guid.Empty))
        {
            errors.Add("Video IDs list cannot contain empty GUIDs (Guid.Empty)");
        }

        return errors.AsReadOnly();
    }
}