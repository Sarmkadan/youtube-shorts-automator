// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ContentCalendarServiceTests"/> instances.
/// Provides methods to validate test class instances for common issues like null values,
/// empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class ContentCalendarServiceTestsValidation
{
    /// <summary>
    /// Validates the given <see cref="ContentCalendarServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ContentCalendarServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ContentCalendarServiceTests is a test fixture class with no stateful members to validate
        // All validation is structural (null checks on method parameters within the methods themselves)
        // This validation method exists for API consistency with other validation helpers in this project

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ContentCalendarServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ContentCalendarServiceTests? value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ContentCalendarServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ContentCalendarServiceTests? value)
    {
        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ContentCalendarServiceTests instance is not valid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems),
            nameof(value));
    }
}