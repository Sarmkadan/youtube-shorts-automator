using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsMetric"/> instances
/// </summary>
public static class AnalyticsMetricValidation
{
    /// <summary>
    /// Validates an <see cref="AnalyticsMetric"/> instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The metric to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsMetric value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ViewCount
        if (value.ViewCount < 0)
        {
            problems.Add($"ViewCount must be non-negative, but was {value.ViewCount}");
        }

        // Validate LikeCount
        if (value.LikeCount < 0)
        {
            problems.Add($"LikeCount must be non-negative, but was {value.LikeCount}");
        }

        // Validate CommentCount
        if (value.CommentCount < 0)
        {
            problems.Add($"CommentCount must be non-negative, but was {value.CommentCount}");
        }

        // Validate ShareCount
        if (value.ShareCount < 0)
        {
            problems.Add($"ShareCount must be non-negative, but was {value.ShareCount}");
        }

        // Validate SubscriberGainedCount
        if (value.SubscriberGainedCount < 0)
        {
            problems.Add($"SubscriberGainedCount must be non-negative, but was {value.SubscriberGainedCount}");
        }

        // Validate SubscriberLostCount
        if (value.SubscriberLostCount < 0)
        {
            problems.Add($"SubscriberLostCount must be non-negative, but was {value.SubscriberLostCount}");
        }

        // Validate AverageViewDurationSeconds
        if (value.AverageViewDurationSeconds < 0)
        {
            problems.Add($"AverageViewDurationSeconds must be non-negative, but was {value.AverageViewDurationSeconds.ToString(CultureInfo.InvariantCulture)}");
        }
        else if (value.AverageViewDurationSeconds > 3600)
        {
            problems.Add($"AverageViewDurationSeconds must be reasonable (≤ 3600s), but was {value.AverageViewDurationSeconds.ToString(CultureInfo.InvariantCulture)}");
        }

        // Validate EngagementRatePercent
        if (value.EngagementRatePercent < 0 || value.EngagementRatePercent > 100)
        {
            problems.Add($"EngagementRatePercent must be between 0 and 100, but was {value.EngagementRatePercent.ToString(CultureInfo.InvariantCulture)}");
        }

        // Validate ClickThroughRatePercent
        if (value.ClickThroughRatePercent < 0 || value.ClickThroughRatePercent > 100)
        {
            problems.Add($"ClickThroughRatePercent must be between 0 and 100, but was {value.ClickThroughRatePercent.ToString(CultureInfo.InvariantCulture)}");
        }

        // Validate Period
        if (!Enum.IsDefined(typeof(MetricsPeriod), value.Period))
        {
            problems.Add($"Period must be a valid MetricsPeriod value, but was {value.Period}");
        }

        // Validate CollectedAt
        if (value.CollectedAt == default)
        {
            problems.Add("CollectedAt must be set to a valid DateTime");
        }
        else if (value.CollectedAt > DateTime.UtcNow.AddHours(1))
        {
            problems.Add($"CollectedAt cannot be in the future, but was {value.CollectedAt:O}");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                problems.Add("UpdatedAt must be a valid DateTime if set");
            }
            else if (value.UpdatedAt.Value > DateTime.UtcNow.AddHours(1))
            {
                problems.Add($"UpdatedAt cannot be in the future, but was {value.UpdatedAt.Value:O}");
            }
            else if (value.UpdatedAt.Value < value.CollectedAt)
            {
                problems.Add($"UpdatedAt ({value.UpdatedAt.Value:O}) cannot be before CollectedAt ({value.CollectedAt:O})");
            }
        }

        // Validate TrafficSource
        if (value.TrafficSource is { Length: 0 })
        {
            problems.Add("TrafficSource must not be an empty string");
        }

        // Validate DeviceType
        if (value.DeviceType is { Length: 0 })
        {
            problems.Add("DeviceType must not be an empty string");
        }

        // Validate Demographics
        if (value.Demographics == null)
        {
            problems.Add("Demographics collection must not be null");
        }
        else
        {
            for (var i = 0; i < value.Demographics.Count; i++)
            {
                var demographic = value.Demographics[i];
                if (demographic == null)
                {
                    problems.Add($"Demographics[{i}] must not be null");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(demographic.AgeGroup))
                    {
                        problems.Add($"Demographics[{i}].AgeGroup must not be null or whitespace");
                    }

                    if (string.IsNullOrWhiteSpace(demographic.Gender))
                    {
                        problems.Add($"Demographics[{i}].Gender must not be null or whitespace");
                    }

                    if (demographic.ViewCount < 0)
                    {
                        problems.Add($"Demographics[{i}].ViewCount must be non-negative, but was {demographic.ViewCount}");
                    }

                    if (demographic.RecordedAt == default)
                    {
                        problems.Add($"Demographics[{i}].RecordedAt must be set to a valid DateTime");
                    }
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="AnalyticsMetric"/> instance is valid
    /// </summary>
    /// <param name="value">The metric to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this AnalyticsMetric value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="AnalyticsMetric"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The metric to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, containing the list of problems</exception>
    public static void EnsureValid(this AnalyticsMetric value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"AnalyticsMetric is invalid:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)
            }");
    }
}