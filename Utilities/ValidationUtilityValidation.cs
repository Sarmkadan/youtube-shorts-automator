// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation extension methods for <see cref="ValidationUtility"/> that return
/// comprehensive validation results as collections of problems.
/// </summary>
public static class ValidationUtilityValidation
{
    /// <summary>
    /// Validates the <see cref="ValidationUtility.ValidationUtilityInstance"/> and returns a collection
    /// of human-readable problem descriptions for any validation failures.
    /// </summary>
    /// <param name="value">The validation utility instance to validate.</param>
    /// <returns>A read-only list of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationUtility.ValidationUtilityInstance value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Email
        if (!string.IsNullOrWhiteSpace(value.Email))
        {
            var (isValid, error) = ValidationUtility.ValidateEmail(value.Email);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate Url
        if (!string.IsNullOrWhiteSpace(value.Url))
        {
            var (isValid, error) = ValidationUtility.ValidateUrl(value.Url);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate YouTubeChannelId
        if (!string.IsNullOrWhiteSpace(value.YouTubeChannelId))
        {
            var (isValid, error) = ValidationUtility.ValidateYouTubeChannelId(value.YouTubeChannelId);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate YouTubeVideoId
        if (!string.IsNullOrWhiteSpace(value.YouTubeVideoId))
        {
            var (isValid, error) = ValidationUtility.ValidateYouTubeVideoId(value.YouTubeVideoId);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate VideoTitle
        if (!string.IsNullOrWhiteSpace(value.VideoTitle))
        {
            var (isValid, error) = ValidationUtility.ValidateVideoTitle(value.VideoTitle);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate VideoDescription
        if (!string.IsNullOrWhiteSpace(value.VideoDescription))
        {
            var (isValid, error) = ValidationUtility.ValidateVideoDescription(value.VideoDescription);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate VideoTags
        if (value.VideoTags != null && value.VideoTags.Length > 0)
        {
            var (isValid, error) = ValidationUtility.ValidateVideoTags(value.VideoTags);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate FilePath
        if (!string.IsNullOrWhiteSpace(value.FilePath))
        {
            var (isValid, error) = ValidationUtility.ValidateVideoFile(value.FilePath, value.MaxFileSizeBytes);
            if (!isValid && error != null)
                problems.Add(error);
        }

        // Validate TimeSpan
        if (!string.IsNullOrWhiteSpace(value.TimeSpan) && !ValidationUtility.IsValidTimeSpan(value.TimeSpan))
        {
            problems.Add("TimeSpan must be a valid TimeSpan string");
        }

        // Validate JsonString
        if (!string.IsNullOrWhiteSpace(value.JsonString) && !ValidationUtility.IsValidJsonString(value.JsonString))
        {
            problems.Add("JsonString must be valid JSON");
        }

        // Validate ScheduledTime
        if (!string.IsNullOrWhiteSpace(value.ScheduledTime))
        {
            var (isValid, error) = ValidationUtility.ValidateScheduleTime(value.ScheduledTime);
            if (!isValid && error != null)
                problems.Add(error);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ValidationUtility.ValidationUtilityInstance"/> is valid
    /// by checking if any validation problems exist.
    /// </summary>
    /// <param name="value">The validation utility instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ValidationUtility.ValidationUtilityInstance value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="ValidationUtility.ValidationUtilityInstance"/> is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message listing all validation problems
    /// if any are found.
    /// </summary>
    /// <param name="value">The validation utility instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of all problems.</exception>
    public static void EnsureValid(this ValidationUtility.ValidationUtilityInstance value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"Validation failed with {problems.Count} problem(s):{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)
            }");
    }
}