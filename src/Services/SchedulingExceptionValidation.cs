// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="SchedulingException"/> instances.
/// </summary>
public static class SchedulingExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="SchedulingException"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="SchedulingException"/> instance to validate.</param>
    /// <returns>A read-only list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SchedulingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            errors.Add("Error message must not be empty or whitespace.");
        }

        if (value.JobId.HasValue && value.JobId.Value <= 0)
        {
            errors.Add("Job ID must be a positive integer.");
        }

        if (value.ScheduledTime.HasValue && value.ScheduledTime.Value == DateTime.MinValue)
        {
            errors.Add("Scheduled time must not be the default DateTime value.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SchedulingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="SchedulingException"/> instance to validate.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SchedulingException value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SchedulingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="SchedulingException"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with a list of errors.</exception>
    public static void EnsureValid(this SchedulingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }
}
