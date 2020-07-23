// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ScheduleRepository to ensure data integrity
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Validation helpers for ScheduleRepository ensuring data integrity
/// </summary>
public static class ScheduleRepositoryValidation
{
    /// <summary>
    /// Validates the ScheduleRepository instance
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ScheduleRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Repository-specific validations would go here if ScheduleRepository had any properties to validate
        // Since ScheduleRepository inherits from Repository<T> and only has methods,
        // there are no instance properties to validate beyond the base class

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ScheduleRepository instance is valid
    /// </summary>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ScheduleRepository? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the ScheduleRepository instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with detailed error messages</exception>
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