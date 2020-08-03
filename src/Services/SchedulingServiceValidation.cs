// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="SchedulingService"/> instances.
/// </summary>
public static class SchedulingServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="SchedulingService"/> instance.
    /// </summary>
    /// <param name="value">The scheduling service instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SchedulingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // SchedulingService itself doesn't have direct state to validate,
        // but we can validate the concept of scheduling operations
        // This provides extensibility for future state validation

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SchedulingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The scheduling service instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this SchedulingService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SchedulingService"/> instance is valid.
    /// </summary>
    /// <param name="value">The scheduling service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this SchedulingService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SchedulingService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}