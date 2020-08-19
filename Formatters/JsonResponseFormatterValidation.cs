// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Provides validation helpers for <see cref="JsonResponseFormatter"/>.
/// </summary>
public static class JsonResponseFormatterValidation
{
    /// <summary>
    /// Validates a <see cref="JsonResponseFormatter"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation errors, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this JsonResponseFormatter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // JsonResponseFormatter is a stateless service class for formatting.
        // There are no public properties to validate.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a <see cref="JsonResponseFormatter"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this JsonResponseFormatter value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Ensures a <see cref="JsonResponseFormatter"/> instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValid(this JsonResponseFormatter value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}", nameof(value));
        }
    }
}
