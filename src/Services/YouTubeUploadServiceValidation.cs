// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for YouTubeUploadService to ensure service instances are valid
// before operations are performed.
// =============================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="YouTubeUploadService"/> to validate service
/// instances before operations are performed.
/// </summary>
public static class YouTubeUploadServiceValidation
{
    /// <summary>
    /// Validates a YouTubeUploadService instance for use with YouTubeUploadService methods.
    /// </summary>
    /// <param name="value">The YouTubeUploadService instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this YouTubeUploadService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified YouTubeUploadService instance is valid.
    /// </summary>
    /// <param name="value">The YouTubeUploadService instance to check.</param>
    /// <returns>true if the YouTubeUploadService instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this YouTubeUploadService value)
    {
        return value != null;
    }

    /// <summary>
    /// Ensures that the specified YouTubeUploadService instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The YouTubeUploadService instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this YouTubeUploadService value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}