using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="ContentCalendarRepository"/> instances.
/// </summary>
public static class ContentCalendarRepositoryValidation
{
    /// <summary>
    /// Validates a <see cref="ContentCalendarRepository"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ContentCalendarRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository methods are validated by their own contracts
        // No additional state to validate on the repository itself

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ContentCalendarRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ContentCalendarRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ContentCalendarRepository"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ContentCalendarRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ContentCalendarRepository is invalid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}