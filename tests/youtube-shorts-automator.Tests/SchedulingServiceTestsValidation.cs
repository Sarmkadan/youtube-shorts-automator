// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SchedulingServiceTests"/> instances.
/// Provides methods to validate test class instances for common issues like null values,
/// empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class SchedulingServiceTestsValidation
{
    /// <summary>
    /// Validates the given <see cref="SchedulingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SchedulingServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // SchedulingServiceTests is a test fixture class with no stateful members to validate
        // All validation is structural (null checks on method parameters within the methods themselves)
        // This validation method exists for API consistency with other validation helpers in this project

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SchedulingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SchedulingServiceTests? value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SchedulingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this SchedulingServiceTests? value)
    {
        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SchedulingServiceTests instance is not valid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems),
            nameof(value));
    }
}