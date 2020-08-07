// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ContentCalendarOptions to ensure configuration values
// are within acceptable ranges before being used by the content calendar subsystem.
// =============================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="ContentCalendarOptions"/> instances.
/// </summary>
public static class ContentCalendarOptionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ContentCalendarOptions"/> instance.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ContentCalendarOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate DefaultLookAheadDays
        if (value.DefaultLookAheadDays <= 0)
        {
            problems.Add($"DefaultLookAheadDays must be positive, but was {value.DefaultLookAheadDays}.");
        }
        else if (value.DefaultLookAheadDays > 365)
        {
            problems.Add($"DefaultLookAheadDays must not exceed 365 days, but was {value.DefaultLookAheadDays}.");
        }

        // Validate MaxTitleLength
        if (value.MaxTitleLength <= 0)
        {
            problems.Add($"MaxTitleLength must be positive, but was {value.MaxTitleLength}.");
        }
        else if (value.MaxTitleLength > 100)
        {
            problems.Add($"MaxTitleLength must not exceed 100 characters (YouTube limit), but was {value.MaxTitleLength}.");
        }

        // Validate OptimalTitleMinLength
        if (value.OptimalTitleMinLength <= 0)
        {
            problems.Add($"OptimalTitleMinLength must be positive, but was {value.OptimalTitleMinLength}.");
        }
        else if (value.OptimalTitleMinLength > value.MaxTitleLength)
        {
            problems.Add($"OptimalTitleMinLength ({value.OptimalTitleMinLength}) cannot exceed MaxTitleLength ({value.MaxTitleLength}).");
        }

        // Validate OptimalTitleMaxLength
        if (value.OptimalTitleMaxLength <= 0)
        {
            problems.Add($"OptimalTitleMaxLength must be positive, but was {value.OptimalTitleMaxLength}.");
        }
        else if (value.OptimalTitleMaxLength > value.MaxTitleLength)
        {
            problems.Add($"OptimalTitleMaxLength ({value.OptimalTitleMaxLength}) cannot exceed MaxTitleLength ({value.MaxTitleLength}).");
        }
        else if (value.OptimalTitleMinLength > value.OptimalTitleMaxLength)
        {
            problems.Add($"OptimalTitleMinLength ({value.OptimalTitleMinLength}) cannot exceed OptimalTitleMaxLength ({value.OptimalTitleMaxLength}).");
        }

        // Validate MaxDescriptionLength
        if (value.MaxDescriptionLength <= 0)
        {
            problems.Add($"MaxDescriptionLength must be positive, but was {value.MaxDescriptionLength}.");
        }
        else if (value.MaxDescriptionLength > 50000)
        {
            problems.Add($"MaxDescriptionLength must not exceed 50000 characters (YouTube limit), but was {value.MaxDescriptionLength}.");
        }

        // Validate MaxTagCount
        if (value.MaxTagCount <= 0)
        {
            problems.Add($"MaxTagCount must be positive, but was {value.MaxTagCount}.");
        }
        else if (value.MaxTagCount > 100)
        {
            problems.Add($"MaxTagCount must not exceed 100 tags, but was {value.MaxTagCount}.");
        }

        // Validate OptimizationSuggestionCount
        if (value.OptimizationSuggestionCount <= 0)
        {
            problems.Add($"OptimizationSuggestionCount must be positive, but was {value.OptimizationSuggestionCount}.");
        }
        else if (value.OptimizationSuggestionCount > 20)
        {
            problems.Add($"OptimizationSuggestionCount must not exceed 20 suggestions, but was {value.OptimizationSuggestionCount}.");
        }

        // Validate OptimalPostingHoursUtc
        if (value.OptimalPostingHoursUtc is null)
        {
            problems.Add("OptimalPostingHoursUtc must not be null.");
        }
        else if (value.OptimalPostingHoursUtc.Length == 0)
        {
            problems.Add("OptimalPostingHoursUtc must contain at least one hour.");
        }
        else
        {
            for (var i = 0; i < value.OptimalPostingHoursUtc.Length; i++)
            {
                var hour = value.OptimalPostingHoursUtc[i];
                if (hour < 0 || hour > 23)
                {
                    problems.Add($"OptimalPostingHoursUtc[{i}] must be between 0 and 23, but was {hour}.");
                }
            }

            // Check for duplicates
            var uniqueHours = new HashSet<int>(value.OptimalPostingHoursUtc);
            if (uniqueHours.Count != value.OptimalPostingHoursUtc.Length)
            {
                problems.Add("OptimalPostingHoursUtc contains duplicate hours.");
            }
        }

        // Validate KeywordWeightMultiplier
        if (value.KeywordWeightMultiplier < 0.0)
        {
            problems.Add($"KeywordWeightMultiplier must be non-negative, but was {value.KeywordWeightMultiplier.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate EngagementScoreWeight
        if (value.EngagementScoreWeight < 0.0 || value.EngagementScoreWeight > 1.0)
        {
            problems.Add($"EngagementScoreWeight must be between 0.0 and 1.0, but was {value.EngagementScoreWeight.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate MinSlotGapMinutes
        if (value.MinSlotGapMinutes <= 0)
        {
            problems.Add($"MinSlotGapMinutes must be positive, but was {value.MinSlotGapMinutes}.");
        }
        else if (value.MinSlotGapMinutes > 1440)
        {
            problems.Add($"MinSlotGapMinutes must not exceed 1440 minutes (24 hours), but was {value.MinSlotGapMinutes}.");
        }

        // Validate HighEngagementKeywords
        if (value.HighEngagementKeywords is null)
        {
            problems.Add("HighEngagementKeywords must not be null.");
        }
        else
        {
            for (var i = 0; i < value.HighEngagementKeywords.Length; i++)
            {
                var keyword = value.HighEngagementKeywords[i];
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    problems.Add($"HighEngagementKeywords[{i}] must not be null or whitespace.");
                }
                else if (keyword.Length > 100)
                {
                    problems.Add($"HighEngagementKeywords[{i}] must not exceed 100 characters, but was {keyword.Length}.");
                }
            }
        }

        // Validate TrendingHashtags
        if (value.TrendingHashtags is null)
        {
            problems.Add("TrendingHashtags must not be null.");
        }
        else
        {
            for (var i = 0; i < value.TrendingHashtags.Length; i++)
            {
                var hashtag = value.TrendingHashtags[i];
                if (string.IsNullOrWhiteSpace(hashtag))
                {
                    problems.Add($"TrendingHashtags[{i}] must not be null or whitespace.");
                }
                else if (hashtag.Length > 50)
                {
                    problems.Add($"TrendingHashtags[{i}] must not exceed 50 characters, but was {hashtag.Length}.");
                }
                else if (hashtag[0] != '#')
                {
                    problems.Add($"TrendingHashtags[{i}] must start with '#', but was '{hashtag}'.");
                }
            }
        }

        // Validate HistoricalSampleSize
        if (value.HistoricalSampleSize <= 0)
        {
            problems.Add($"HistoricalSampleSize must be positive, but was {value.HistoricalSampleSize}.");
        }
        else if (value.HistoricalSampleSize > 1000)
        {
            problems.Add($"HistoricalSampleSize must not exceed 1000 videos, but was {value.HistoricalSampleSize}.");
        }

        // Validate HighEngagementBonus
        if (value.HighEngagementBonus < 0.0 || value.HighEngagementBonus > 1.0)
        {
            problems.Add($"HighEngagementBonus must be between 0.0 and 1.0, but was {value.HighEngagementBonus.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate EngagementRateThreshold
        if (value.EngagementRateThreshold < 0.0 || value.EngagementRateThreshold > 100.0)
        {
            problems.Add($"EngagementRateThreshold must be between 0.0 and 100.0, but was {value.EngagementRateThreshold.ToString(CultureInfo.InvariantCulture)}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ContentCalendarOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ContentCalendarOptions? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ContentCalendarOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ContentCalendarOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ContentCalendarOptions is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}