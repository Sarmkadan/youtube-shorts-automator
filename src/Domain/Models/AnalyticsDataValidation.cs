using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsData"/> instances.
/// </summary>
public static class AnalyticsDataValidation
{
    /// <summary>
    /// Validates the specified <see cref="AnalyticsData"/> instance.
    /// </summary>
    /// <param name="value">The analytics data to validate.</param>
    /// <returns>A list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Id must be a positive integer, but was {value.Id}.");
        }

        // Validate VideoShortId
        if (value.VideoShortId <= 0)
        {
            errors.Add($"VideoShortId must be a positive integer, but was {value.VideoShortId}.");
        }

        // Validate ViewCount (non-negative)
        if (value.ViewCount < 0)
        {
            errors.Add($"ViewCount cannot be negative, but was {value.ViewCount}.");
        }

        // Validate LikeCount (non-negative)
        if (value.LikeCount < 0)
        {
            errors.Add($"LikeCount cannot be negative, but was {value.LikeCount}.");
        }

        // Validate CommentCount (non-negative)
        if (value.CommentCount < 0)
        {
            errors.Add($"CommentCount cannot be negative, but was {value.CommentCount}.");
        }

        // Validate ShareCount (non-negative)
        if (value.ShareCount < 0)
        {
            errors.Add($"ShareCount cannot be negative, but was {value.ShareCount}.");
        }

        // Validate AverageViewDuration (0-1 range for percentage, but stored as double)
        if (value.AverageViewDuration < 0 || value.AverageViewDuration > 1)
        {
            errors.Add($"AverageViewDuration must be between 0 and 1, but was {value.AverageViewDuration.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate EngagementRate (0-1 range for percentage)
        if (value.EngagementRate < 0 || value.EngagementRate > 1)
        {
            errors.Add($"EngagementRate must be between 0 and 1, but was {value.EngagementRate.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate ClickThroughRate (0-1 range for percentage)
        if (value.ClickThroughRate < 0 || value.ClickThroughRate > 1)
        {
            errors.Add($"ClickThroughRate must be between 0 and 1, but was {value.ClickThroughRate.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate SubscribersGained (can be negative if lost more than gained)
        // No specific validation needed beyond being an integer

        // Validate SubscribersLost (can be negative if gained more than lost)
        // No specific validation needed beyond being an integer

        // Validate AudienceRetentionPercentage (0-100 range)
        if (value.AudienceRetentionPercentage < 0 || value.AudienceRetentionPercentage > 100)
        {
            errors.Add($"AudienceRetentionPercentage must be between 0 and 100, but was {value.AudienceRetentionPercentage}.");
        }

        // Validate TrafficSources (non-negative)
        if (value.TrafficSources < 0)
        {
            errors.Add($"TrafficSources cannot be negative, but was {value.TrafficSources}.");
        }

        // Validate ImpressionCount (non-negative)
        if (value.ImpressionCount < 0)
        {
            errors.Add($"ImpressionCount cannot be negative, but was {value.ImpressionCount}.");
        }

        // Validate UpdatedAt (not default DateTime)
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be the default DateTime value.");
        }

        // Validate VideoShort reference
        if (value.VideoShort is null)
        {
            errors.Add("VideoShort cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalyticsData"/> instance is valid.
    /// </summary>
    /// <param name="value">The analytics data to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalyticsData value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AnalyticsData"/> instance is valid.
    /// </summary>
    /// <param name="value">The analytics data to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this AnalyticsData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AnalyticsData is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}