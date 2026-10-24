// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for AnalyticsController response models
// Provides validation for analytics API response data integrity
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for AnalyticsController response models
/// Validates analytics response data integrity and business rules
/// </summary>
public static class AnalyticsControllerValidation
{
    /// <summary>
    /// Validates a VideoAnalyticsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The VideoAnalyticsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this VideoAnalyticsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate VideoId
        if (value.VideoId == Guid.Empty)
        {
            problems.Add("VideoId must be a non-empty GUID.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            problems.Add("Title must be specified.");
        }
        else if (value.Title.Length > 500)
        {
            problems.Add("Title must be 500 characters or less.");
        }

        // Validate ViewCount
        if (value.ViewCount < 0)
        {
            problems.Add("ViewCount cannot be negative.");
        }

        // Validate LikeCount
        if (value.LikeCount < 0)
        {
            problems.Add("LikeCount cannot be negative.");
        }

        // Validate CommentCount
        if (value.CommentCount < 0)
        {
            problems.Add("CommentCount cannot be negative.");
        }

        // Validate ShareCount
        if (value.ShareCount < 0)
        {
            problems.Add("ShareCount cannot be negative.");
        }

        // Validate WatchTimeMinutes
        if (value.WatchTimeMinutes < 0)
        {
            problems.Add("WatchTimeMinutes cannot be negative.");
        }

        // Validate EngagementRate
        if (value.EngagementRate < 0 || value.EngagementRate > 100)
        {
            problems.Add("EngagementRate must be between 0 and 100.");
        }

        // Validate AverageWatchDurationSeconds
        if (value.AverageWatchDurationSeconds < 0)
        {
            problems.Add("AverageWatchDurationSeconds cannot be negative.");
        }

        // Validate CtrPercent
        if (value.CtrPercent < 0 || value.CtrPercent > 100)
        {
            problems.Add("CtrPercent must be between 0 and 100.");
        }

        // Validate UpdatedAtUtc
        if (value.UpdatedAtUtc == default)
        {
            problems.Add("UpdatedAtUtc must be set to a non-default DateTime value.");
        }
        else if (value.UpdatedAtUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("UpdatedAtUtc must be in UTC format.");
        }
        else if (value.UpdatedAtUtc > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("UpdatedAtUtc cannot be more than 1 hour in the future.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an AnalyticsSummaryResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The AnalyticsSummaryResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsSummaryResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate TotalVideos
        if (value.TotalVideos < 0)
        {
            problems.Add("TotalVideos cannot be negative.");
        }

        // Validate TotalViews
        if (value.TotalViews < 0)
        {
            problems.Add("TotalViews cannot be negative.");
        }

        // Validate TotalEngagement
        if (value.TotalEngagement < 0)
        {
            problems.Add("TotalEngagement cannot be negative.");
        }

        // Validate AverageViewsPerVideo
        if (value.AverageViewsPerVideo < 0)
        {
            problems.Add("AverageViewsPerVideo cannot be negative.");
        }

        // Validate AverageEngagementRate
        if (value.AverageEngagementRate < 0 || value.AverageEngagementRate > 100)
        {
            problems.Add("AverageEngagementRate must be between 0 and 100.");
        }

        // Validate DateRangeStartUtc
        if (value.DateRangeStartUtc == default)
        {
            problems.Add("DateRangeStartUtc must be set to a non-default DateTime value.");
        }
        else if (value.DateRangeStartUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateRangeStartUtc must be in UTC format.");
        }

        // Validate DateRangeEndUtc
        if (value.DateRangeEndUtc == default)
        {
            problems.Add("DateRangeEndUtc must be set to a non-default DateTime value.");
        }
        else if (value.DateRangeEndUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateRangeEndUtc must be in UTC format.");
        }
        else if (value.DateRangeEndUtc > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("DateRangeEndUtc cannot be more than 1 hour in the future.");
        }

        // Validate TopPerformingVideo (if provided)
        if (value.TopPerformingVideo is not null)
        {
            var videoProblems = Validate(value.TopPerformingVideo);
            if (videoProblems.Count > 0)
            {
                problems.AddRange(videoProblems);
            }
        }

        // Validate date range consistency
        if (value.DateRangeStartUtc != default && value.DateRangeEndUtc != default && value.DateRangeStartUtc > value.DateRangeEndUtc)
        {
            problems.Add("DateRangeStartUtc must be before DateRangeEndUtc.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a VideoSummary instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The VideoSummary instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this VideoSummary value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate VideoId
        if (value.VideoId == Guid.Empty)
        {
            problems.Add("VideoId must be a non-empty GUID.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            problems.Add("Title must be specified.");
        }
        else if (value.Title.Length > 500)
        {
            problems.Add("Title must be 500 characters or less.");
        }

        // Validate ViewCount
        if (value.ViewCount < 0)
        {
            problems.Add("ViewCount cannot be negative.");
        }

        // Validate EngagementRate
        if (value.EngagementRate < 0 || value.EngagementRate > 100)
        {
            problems.Add("EngagementRate must be between 0 and 100.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an EngagementTrendResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The EngagementTrendResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this EngagementTrendResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate TrendPoints collection
        if (value.TrendPoints is null)
        {
            problems.Add("TrendPoints collection is null.");
        }
        else if (value.TrendPoints.Length == 0)
        {
            problems.Add("TrendPoints collection cannot be empty.");
        }
        else
        {
            // Validate each trend point
            for (int i = 0; i < value.TrendPoints.Length; i++)
            {
                var point = value.TrendPoints[i];
                if (point == null)
                {
                    problems.Add($"TrendPoints[{i}] is null.");
                    continue;
                }

                // Validate Date
                if (point.Date == default)
                {
                    problems.Add($"TrendPoints[{i}].Date must be set to a non-default DateTime value.");
                }
                else if (point.Date.Kind != DateTimeKind.Utc)
                {
                    problems.Add($"TrendPoints[{i}].Date must be in UTC format.");
                }
                else if (point.Date > DateTime.UtcNow.AddHours(1))
                {
                    problems.Add($"TrendPoints[{i}].Date cannot be more than 1 hour in the future.");
                }

                // Validate Views
                if (point.Views < 0)
                {
                    problems.Add($"TrendPoints[{i}].Views cannot be negative.");
                }

                // Validate Engagement
                if (point.Engagement < 0)
                {
                    problems.Add($"TrendPoints[{i}].Engagement cannot be negative.");
                }
            }
        }

        // Validate DateRangeStartUtc
        if (value.DateRangeStartUtc == default)
        {
            problems.Add("DateRangeStartUtc must be set to a non-default DateTime value.");
        }
        else if (value.DateRangeStartUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateRangeStartUtc must be in UTC format.");
        }

        // Validate DateRangeEndUtc
        if (value.DateRangeEndUtc == default)
        {
            problems.Add("DateRangeEndUtc must be set to a non-default DateTime value.");
        }
        else if (value.DateRangeEndUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateRangeEndUtc must be in UTC format.");
        }
        else if (value.DateRangeEndUtc > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("DateRangeEndUtc cannot be more than 1 hour in the future.");
        }

        // Validate date range consistency
        if (value.DateRangeStartUtc != default && value.DateRangeEndUtc != default && value.DateRangeStartUtc > value.DateRangeEndUtc)
        {
            problems.Add("DateRangeStartUtc must be before DateRangeEndUtc.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a TrendPoint instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The TrendPoint instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this TrendPoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Date
        if (value.Date == default)
        {
            problems.Add("Date must be set to a non-default DateTime value.");
        }
        else if (value.Date.Kind != DateTimeKind.Utc)
        {
            problems.Add("Date must be in UTC format.");
        }
        else if (value.Date > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("Date cannot be more than 1 hour in the future.");
        }

        // Validate Views
        if (value.Views < 0)
        {
            problems.Add("Views cannot be negative.");
        }

        // Validate Engagement
        if (value.Engagement < 0)
        {
            problems.Add("Engagement cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified VideoAnalyticsResponse instance is valid
    /// </summary>
    /// <param name="value">The VideoAnalyticsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this VideoAnalyticsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified AnalyticsSummaryResponse instance is valid
    /// </summary>
    /// <param name="value">The AnalyticsSummaryResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this AnalyticsSummaryResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified VideoSummary instance is valid
    /// </summary>
    /// <param name="value">The VideoSummary instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this VideoSummary value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified EngagementTrendResponse instance is valid
    /// </summary>
    /// <param name="value">The EngagementTrendResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this EngagementTrendResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified TrendPoint instance is valid
    /// </summary>
    /// <param name="value">The TrendPoint instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this TrendPoint value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified VideoAnalyticsResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The VideoAnalyticsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this VideoAnalyticsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoAnalyticsResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that the specified AnalyticsSummaryResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The AnalyticsSummaryResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this AnalyticsSummaryResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AnalyticsSummaryResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that the specified VideoSummary instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The VideoSummary instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this VideoSummary value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoSummary validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that the specified EngagementTrendResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The EngagementTrendResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this EngagementTrendResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EngagementTrendResponse validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures that the specified TrendPoint instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The TrendPoint instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this TrendPoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TrendPoint validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}