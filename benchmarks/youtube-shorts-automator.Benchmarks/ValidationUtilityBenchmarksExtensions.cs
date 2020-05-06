// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Extension methods for <see cref="ValidationUtilityBenchmarks"/> that provide additional benchmarking scenarios
/// and utility methods for testing validation edge cases.
/// </summary>
public static class ValidationUtilityBenchmarksExtensions
{
    /// <summary>
    /// Validates a collection of email addresses and returns detailed validation results.
    /// Useful for bulk validation benchmarking.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="emails">Collection of email addresses to validate.</param>
    /// <returns>Collection of validation results with error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="emails"/> is null.</exception>
    public static IReadOnlyList<(bool IsValid, string? Error)> ValidateEmailCollection(
        this ValidationUtilityBenchmarks benchmarks,
        IEnumerable<string> emails)
    {
        ArgumentNullException.ThrowIfNull(emails);

        var results = new List<(bool IsValid, string? Error)>();
        foreach (var email in emails)
        {
            results.Add(ValidationUtility.ValidateEmail(email));
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection of URLs and returns detailed validation results.
    /// Useful for bulk validation benchmarking.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="urls">Collection of URLs to validate.</param>
    /// <returns>Collection of validation results with error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="urls"/> is null.</exception>
    public static IReadOnlyList<(bool IsValid, string? Error)> ValidateUrlCollection(
        this ValidationUtilityBenchmarks benchmarks,
        IEnumerable<string> urls)
    {
        ArgumentNullException.ThrowIfNull(urls);

        var results = new List<(bool IsValid, string? Error)>();
        foreach (var url in urls)
        {
            results.Add(ValidationUtility.ValidateUrl(url));
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection of YouTube channel IDs and returns detailed validation results.
    /// Useful for bulk validation benchmarking.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="channelIds">Collection of YouTube channel IDs to validate.</param>
    /// <returns>Collection of validation results with error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channelIds"/> is null.</exception>
    public static IReadOnlyList<(bool IsValid, string? Error)> ValidateYouTubeChannelIdCollection(
        this ValidationUtilityBenchmarks benchmarks,
        IEnumerable<string> channelIds)
    {
        ArgumentNullException.ThrowIfNull(channelIds);

        var results = new List<(bool IsValid, string? Error)>();
        foreach (var channelId in channelIds)
        {
            results.Add(ValidationUtility.ValidateYouTubeChannelId(channelId));
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection of YouTube video IDs and returns detailed validation results.
    /// Useful for bulk validation benchmarking.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="videoIds">Collection of YouTube video IDs to validate.</param>
    /// <returns>Collection of validation results with error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="videoIds"/> is null.</exception>
    public static IReadOnlyList<(bool IsValid, string? Error)> ValidateYouTubeVideoIdCollection(
        this ValidationUtilityBenchmarks benchmarks,
        IEnumerable<string> videoIds)
    {
        ArgumentNullException.ThrowIfNull(videoIds);

        var results = new List<(bool IsValid, string? Error)>();
        foreach (var videoId in videoIds)
        {
            results.Add(ValidationUtility.ValidateYouTubeVideoId(videoId));
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Validates a single email address with additional formatting options.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="email">Email address to validate.</param>
    /// <param name="allowEmpty">Whether to allow empty strings as valid.</param>
    /// <returns>Validation result with error message if invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
    public static (bool IsValid, string? Error) ValidateEmailWithOptions(
        this ValidationUtilityBenchmarks benchmarks,
        string email,
        bool allowEmpty = false)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrEmpty(email))
        {
            return allowEmpty
                ? (true, null)
                : (false, "Email cannot be null or empty");
        }

        return ValidationUtility.ValidateEmail(email);
    }

    /// <summary>
    /// Validates a single URL with additional scheme requirements.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="url">URL to validate.</param>
    /// <param name="requiredSchemes">Collection of required URI schemes (e.g., https, http).</param>
    /// <returns>Validation result with error message if invalid.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="url"/> is null.
    /// Thrown when <paramref name="requiredSchemes"/> is null.
    /// </exception>
    public static (bool IsValid, string? Error) ValidateUrlWithSchemes(
        this ValidationUtilityBenchmarks benchmarks,
        string url,
        IEnumerable<string> requiredSchemes)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(requiredSchemes);

        var validationResult = ValidationUtility.ValidateUrl(url);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return (false, "Invalid URL format");
        }

        foreach (var scheme in requiredSchemes)
        {
            if (string.Equals(uri.Scheme, scheme, StringComparison.OrdinalIgnoreCase))
            {
                return (true, null);
            }
        }

        return (false, $"URL scheme must be one of: {string.Join(", ", requiredSchemes)}");
    }

    /// <summary>
    /// Validates a YouTube channel ID with length normalization.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="channelId">YouTube channel ID to validate.</param>
    /// <param name="normalize">Whether to normalize the channel ID by removing whitespace.</param>
    /// <returns>Validation result with error message if invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channelId"/> is null.</exception>
    public static (bool IsValid, string? Error) ValidateYouTubeChannelIdNormalized(
        this ValidationUtilityBenchmarks benchmarks,
        string channelId,
        bool normalize = false)
    {
        ArgumentNullException.ThrowIfNull(channelId);

        var actualChannelId = normalize
            ? channelId.Trim()
            : channelId;

        return ValidationUtility.ValidateYouTubeChannelId(actualChannelId);
    }

    /// <summary>
    /// Validates a YouTube video ID with case normalization.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="videoId">YouTube video ID to validate.</param>
    /// <param name="normalizeCase">Whether to normalize the video ID to lowercase.</param>
    /// <returns>Validation result with error message if invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="videoId"/> is null.</exception>
    public static (bool IsValid, string? Error) ValidateYouTubeVideoIdNormalized(
        this ValidationUtilityBenchmarks benchmarks,
        string videoId,
        bool normalizeCase = false)
    {
        ArgumentNullException.ThrowIfNull(videoId);

        var actualVideoId = normalizeCase
            ? videoId.ToLowerInvariant()
            : videoId;

        return ValidationUtility.ValidateYouTubeVideoId(actualVideoId);
    }
}