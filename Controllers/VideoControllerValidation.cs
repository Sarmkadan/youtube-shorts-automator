// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for VideoController
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Provides validation helpers for VideoController
/// </summary>
public static class VideoControllerValidation
{
    /// <summary>
    /// Validates a VideoController instance
    /// </summary>
    /// <param name="value">The VideoController instance to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this VideoController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // VideoController has no public properties to validate
        // All validation is done on method parameters

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a VideoController instance is valid
    /// </summary>
    /// <param name="value">The VideoController instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this VideoController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return true; // VideoController has no public properties to validate
    }

    /// <summary>
    /// Ensures a VideoController instance is valid, throwing if not
    /// </summary>
    /// <param name="value">The VideoController instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this VideoController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // VideoController has no public properties to validate
        // All validation is done on method parameters at runtime
    }
}