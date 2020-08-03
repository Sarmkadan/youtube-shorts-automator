// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for the <see cref="AnalyticsReport"/> class.
/// </summary>
public static class AnalyticsServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="AnalyticsReport"/> instance.
    /// </summary>
    /// <param name="value">The analytics report instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsReport? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate PeriodStart and PeriodEnd
        if (value.PeriodStart == default)
        {
            problems.Add("PeriodStart must be set to a valid date.");
        }

        if (value.PeriodEnd == default)
        {
            problems.Add("PeriodEnd must be set to a valid date.");
        }
        else if (value.PeriodStart > value.PeriodEnd)
        {
            problems.Add("PeriodStart must be before or equal to PeriodEnd.");
        }

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

        // Validate TotalLikes
        if (value.TotalLikes < 0)
        {
            problems.Add("TotalLikes cannot be negative.");
        }

        // Validate TotalComments
        if (value.TotalComments < 0)
        {
            problems.Add("TotalComments cannot be negative.");
        }

        // Validate TotalShares
        if (value.TotalShares < 0)
        {
            problems.Add("TotalShares cannot be negative.");
        }

        // Validate AverageEngagementRate
        if (value.AverageEngagementRate < 0 || value.AverageEngagementRate > 100)
        {
            problems.Add("AverageEngagementRate must be between 0 and 100.");
        }

        // Validate TotalSubscribersGained
        if (value.TotalSubscribersGained < 0)
        {
            problems.Add("TotalSubscribersGained cannot be negative.");
        }

        // Validate GeneratedAt
        if (value.GeneratedAt == default)
        {
            problems.Add("GeneratedAt must be set to a valid date.");
        }
        else if (value.GeneratedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("GeneratedAt cannot be in the future.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalyticsReport"/> instance is valid.
    /// </summary>
    /// <param name="value">The analytics report instance to check.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this AnalyticsReport? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AnalyticsReport"/> instance is valid.
    /// </summary>
    /// <param name="value">The analytics report instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this AnalyticsReport? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AnalyticsReport validation failed:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", problems)
                }",
                nameof(value)
            );
        }
    }
}