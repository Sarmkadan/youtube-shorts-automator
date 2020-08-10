// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Validation helpers for <see cref="AnalyticsServiceTests"/> instances.
/// Provides methods to validate test class instances for common issues like null values,
/// empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class AnalyticsServiceTestsValidation
{
    /// <summary>
    /// Validates the given <see cref="AnalyticsServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // AnalyticsServiceTests is a test fixture class with no stateful members to validate
        // All validation is structural (null checks on method parameters within the methods themselves)
        // This validation method exists for API consistency with other validation helpers in this project

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalyticsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this AnalyticsServiceTests? value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="AnalyticsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this AnalyticsServiceTests? value)
    {
        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"AnalyticsServiceTests instance is not valid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems),
            nameof(value));
    }
}