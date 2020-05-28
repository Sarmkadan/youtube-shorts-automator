using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Extension methods for <see cref="ContentCalendarOptions"/> configuration validation and utility operations.
/// </summary>
public static class ContentCalendarOptionsExtensions
{
    /// <summary>
    /// Validates that optimal posting hours are within valid UTC hour range (0-23) and contain no duplicates.
    /// </summary>
    /// <param name="options">The content calendar options to validate</param>
    /// <exception cref="ArgumentException">Thrown when hours are out of range or contain duplicates</exception>
    public static void ValidateOptimalPostingHours(this ContentCalendarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var uniqueHours = new HashSet<int>();
        foreach (var hour in options.OptimalPostingHoursUtc)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentException($"Hour {hour} is outside valid UTC range (0-23)");
            if (!uniqueHours.Add(hour))
                throw new ArgumentException($"Duplicate hour {hour} in optimal posting hours");
        }
    }

    /// <summary>
    /// Checks if the provided title contains any high engagement keywords from configuration.
    /// </summary>
    /// <param name="options">The content calendar options</param>
    /// <param name="title">The title text to check</param>
    /// <returns>True if any high engagement keyword is found in the title</returns>
    public static bool ContainsHighEngagementKeyword(this ContentCalendarOptions options, string title)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(title);

        return options.HighEngagementKeywords
            .Any(keyword => title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Appends trending hashtags to the provided description, avoiding duplicates.
    /// </summary>
    /// <param name="options">The content calendar options</param>
    /// <param name="description">The base description text</param>
    /// <returns>Modified description with appended trending hashtags</returns>
    public static string AppendTrendingHashtags(this ContentCalendarOptions options, string description)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(description);

        var existingHashtags = new HashSet<string>(
            description
                .Split(' ')
                .Where(word => word.StartsWith('#'))
                .Select(word => word.ToLowerInvariant())
        );

        var newHashtags = options.TrendingHashtags
            .Where(tag => !existingHashtags.Contains(tag.ToLowerInvariant()))
            .ToList();

        if (newHashtags.Count == 0)
            return description;

        return $"{description} {string.Join(" ", newHashtags)}";
    }
}
