// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="TitleOptimizationEngine"/> instances.
/// </summary>
public static class TitleOptimizationEngineValidation
{
    /// <summary>
    /// Validates the specified <see cref="TitleOptimizationEngine"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TitleOptimizationEngine? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Constructor parameters validation would be validated at construction time
        // No additional validation needed for the engine itself beyond null check

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TitleOptimizationEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this TitleOptimizationEngine? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="TitleOptimizationEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this TitleOptimizationEngine? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"TitleOptimizationEngine is invalid. Validation errors: {string.Join(", ", errors)}");
        }
    }
}