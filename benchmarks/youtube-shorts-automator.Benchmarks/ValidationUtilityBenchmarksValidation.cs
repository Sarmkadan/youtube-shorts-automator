// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="ValidationUtilityBenchmarks"/> benchmark class.
/// Provides comprehensive validation for benchmark validation scenarios.
/// </summary>
public static class ValidationUtilityBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="ValidationUtilityBenchmarks"/> instance and returns any validation problems.
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <returns>List of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ValidationUtilityBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Email_Valid
        var emailValidResult = value.ValidateEmail_Valid();
        if (!emailValidResult.IsValid && emailValidResult.Error is not null)
        {
            problems.Add($"ValidateEmail_Valid validation failed: {emailValidResult.Error}");
        }

        // Validate Email_Invalid
        var emailInvalidResult = value.ValidateEmail_Invalid();
        if (emailInvalidResult.IsValid)
        {
            problems.Add("ValidateEmail_Invalid should not be valid");
        }
        else if (emailInvalidResult.Error is not null)
        {
            problems.Add($"ValidateEmail_Invalid validation failed: {emailInvalidResult.Error}");
        }

        // Validate Url_Valid
        var urlValidResult = value.ValidateUrl_Valid();
        if (!urlValidResult.IsValid && urlValidResult.Error is not null)
        {
            problems.Add($"ValidateUrl_Valid validation failed: {urlValidResult.Error}");
        }

        // Validate YouTubeChannelId_Valid
        var channelIdValidResult = value.ValidateYouTubeChannelId_Valid();
        if (!channelIdValidResult.IsValid && channelIdValidResult.Error is not null)
        {
            problems.Add($"ValidateYouTubeChannelId_Valid validation failed: {channelIdValidResult.Error}");
        }

        // Validate YouTubeVideoId_Valid
        var videoIdValidResult = value.ValidateYouTubeVideoId_Valid();
        if (!videoIdValidResult.IsValid && videoIdValidResult.Error is not null)
        {
            problems.Add($"ValidateYouTubeVideoId_Valid validation failed: {videoIdValidResult.Error}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ValidationUtilityBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check</param>
    /// <returns>True if the instance is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this ValidationUtilityBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ValidationUtilityBenchmarks"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all validation problems</exception>
    public static void EnsureValid(this ValidationUtilityBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationUtilityBenchmarks instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}
