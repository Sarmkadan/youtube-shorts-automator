// =============================================================================
// Validation helpers for ContentCalendarService
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation extension methods for <see cref="ContentCalendarService"/>.
/// </summary>
public static class ContentCalendarServiceValidation
{
    /// <summary>
    /// Validates the <see cref="ContentCalendarService"/> instance and returns a collection of
    /// human‑readable problems. Because the service does not expose any public state,
    /// the only validation performed is a null‑check.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages,
    /// or an empty list if the instance is considered valid.</returns>
    public static IReadOnlyList<string> Validate(this ContentCalendarService? value)
    {
        var errors = new List<string>();

        if (value is null)
        {
            errors.Add("ContentCalendarService instance is null.");
            return errors;
        }

        // No public properties/state to validate at the moment.
        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="ContentCalendarService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ContentCalendarService? value) =>
        !value.Validate().Any();

    /// <summary>
    /// Ensures that the <see cref="ContentCalendarService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this ContentCalendarService? value)
    {
        var errors = value.Validate();
        if (errors.Any())
        {
            throw new ArgumentException(
                $"ContentCalendarService validation failed: {string.Join("; ", errors)}",
                nameof(value));
        }
    }
}
