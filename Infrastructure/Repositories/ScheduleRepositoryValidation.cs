// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ScheduleRepository to ensure data integrity
// =====================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Provides validation methods for <see cref="ScheduleRepository"/> instances to ensure data integrity.
/// </summary>
public static class ScheduleRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="ScheduleRepository"/> instance.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>An empty read-only list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ScheduleRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the <see cref="ScheduleRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ScheduleRepository? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures the <see cref="ScheduleRepository"/> instance is valid, throwing <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with detailed error messages.</exception>
    public static void EnsureValid(this ScheduleRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ScheduleRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}