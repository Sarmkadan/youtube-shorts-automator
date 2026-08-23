// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect

// Validation helpers for VideoUploadStartedEventHandler to ensure handler instances
// are properly initialized before use in the event processing pipeline.
// =============================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Provides validation helpers for <see cref="VideoUploadStartedEventHandler"/> instances.
/// Ensures that handler instances are properly initialized and can process events safely.
/// </summary>
public static class VideoUploadStartedEventHandlerValidation
{
    /// <summary>
    /// Validates the specified <see cref="VideoUploadStartedEventHandler"/> instance.
    /// Since the handler is a stateless service with injected dependencies, validation only checks for null.
    /// </summary>
    /// <param name="value">The handler instance to validate.</param>
    /// <returns>An empty list if the handler is valid (non-null).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VideoUploadStartedEventHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoUploadStartedEventHandler"/> is valid.
    /// </summary>
    /// <param name="value">The handler instance to check.</param>
    /// <returns>True if the handler is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VideoUploadStartedEventHandler value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="VideoUploadStartedEventHandler"/> is valid.
    /// </summary>
    /// <param name="value">The handler instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the handler is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this VideoUploadStartedEventHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoUploadStartedEventHandler is invalid. Problems: {string.Join("; ", problems)}",
                nameof(value)
            );
        }
    }
}