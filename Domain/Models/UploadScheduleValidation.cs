// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for UploadSchedule model
// =====================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public static class UploadScheduleValidation
{
    /// <summary>
    /// Validates an UploadSchedule instance and returns any validation errors
    /// </summary>
    /// <param name="value">The UploadSchedule to validate</param>
    /// <returns>List of human-readable validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this UploadSchedule? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required properties
        if (string.IsNullOrWhiteSpace(value.ScheduleName))
        {
            errors.Add("Schedule name is required");
        }
        else if (value.ScheduleName.Length > 100)
        {
            errors.Add("Schedule name must be 100 characters or less");
        }

        // Validate frequency-specific properties
        if (value.Frequency == ScheduleFrequency.Weekly && !value.DayOfWeek.HasValue)
        {
            errors.Add("Day of week is required for weekly schedules");
        }

        if (value.Frequency == ScheduleFrequency.BiWeekly && !value.DayOfWeek.HasValue)
        {
            errors.Add("Day of week is required for bi-weekly schedules");
        }

        if (value.Frequency == ScheduleFrequency.Monthly)
        {
            if (!value.DayOfMonth.HasValue)
            {
                errors.Add("Day of month is required for monthly schedules");
            }
            else if (value.DayOfMonth < 1 || value.DayOfMonth > 31)
            {
                errors.Add("Day of month must be between 1 and 31");
            }
        }

        // Validate time-related properties
        if (value.ScheduledTime == default)
        {
            errors.Add("Scheduled time is required");
        }

        if (string.IsNullOrWhiteSpace(value.TimeZoneId))
        {
            errors.Add("Time zone is required");
        }
        else
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(value.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                errors.Add("Invalid time zone ID");
            }
            catch (InvalidTimeZoneException)
            {
                errors.Add("Invalid time zone ID");
            }
        }

        // Validate calculated properties
        if (value.CreatedAt == default)
        {
            errors.Add("Created date is required");
        }

        if (value.ScheduledUploads == null)
        {
            errors.Add("Scheduled uploads collection is required");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if an UploadSchedule instance is valid
    /// </summary>
    /// <param name="value">The UploadSchedule to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this UploadSchedule? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures an UploadSchedule instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The UploadSchedule to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed error messages</exception>
    public static void EnsureValid(this UploadSchedule? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UploadSchedule is invalid. Errors: {string.Join("; ", errors)}");
        }
    }
}
