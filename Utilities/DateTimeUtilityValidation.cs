// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// DateTimeUtility validation helpers
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for DateTimeUtility operations.
/// Validates DateTimeUtility behavior and output constraints.
/// </summary>
public static class DateTimeUtilityValidation
{
    /// <summary>
    /// Validates DateTimeUtility behavior and returns a list of human-readable problems.
    /// Checks for potential issues in DateTimeUtility methods and their outputs.
    /// </summary>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate FormatUtcDateTime
        var utcNow = DateTime.UtcNow;
        var formattedUtc = DateTimeUtility.FormatUtcDateTime(utcNow);
        if (string.IsNullOrEmpty(formattedUtc))
        {
            problems.Add("DateTimeUtility.FormatUtcDateTime should return non-empty string for valid DateTime input.");
        }
        else if (!formattedUtc.Contains("-") || !formattedUtc.Contains(":"))
        {
            problems.Add("DateTimeUtility.FormatUtcDateTime should return ISO 8601-like format with date and time separators.");
        }

        // Validate FormatIsoDateTime
        var formattedIso = DateTimeUtility.FormatIsoDateTime(utcNow);
        if (string.IsNullOrEmpty(formattedIso))
        {
            problems.Add("DateTimeUtility.FormatIsoDateTime should return non-empty string for valid DateTime input.");
        }
        else if (!formattedIso.Contains("T"))
        {
            problems.Add("DateTimeUtility.FormatIsoDateTime should return ISO 8601 format with 'T' separator.");
        }

        // Validate FormatRelativeTime
        var relativeTimePast = DateTimeUtility.FormatRelativeTime(utcNow.AddMinutes(-5));
        if (string.IsNullOrEmpty(relativeTimePast) || !relativeTimePast.Contains("minutes ago"))
        {
            problems.Add("DateTimeUtility.FormatRelativeTime should return human-readable relative time for past dates.");
        }

        var relativeTimeFuture = DateTimeUtility.FormatRelativeTime(utcNow.AddMinutes(5));
        if (string.IsNullOrEmpty(relativeTimeFuture) || !relativeTimeFuture.Contains("minutes ago"))
        {
            problems.Add("DateTimeUtility.FormatRelativeTime should return human-readable relative time for future dates.");
        }

        // Validate FormatDuration
        var duration = TimeSpan.FromMinutes(90);
        var formattedDuration = DateTimeUtility.FormatDuration(duration);
        if (string.IsNullOrEmpty(formattedDuration))
        {
            problems.Add("DateTimeUtility.FormatDuration should return non-empty string for valid TimeSpan input.");
        }
        else if (!formattedDuration.Contains("h") && !formattedDuration.Contains("m"))
        {
            problems.Add("DateTimeUtility.FormatDuration should include hours or minutes in output.");
        }

        // Validate ConvertToUtc
        var localTime = DateTime.Now;
        var convertedUtc = DateTimeUtility.ConvertToUtc(localTime, "UTC");
        if (convertedUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateTimeUtility.ConvertToUtc should return DateTime with Kind=Utc.");
        }

        // Test with invalid timezone - should gracefully handle and return UTC
        var invalidTz = DateTimeUtility.ConvertToUtc(localTime, "Invalid/Timezone");
        if (invalidTz.Kind != DateTimeKind.Utc)
        {
            problems.Add("DateTimeUtility.ConvertToUtc should return UTC time when timezone is invalid.");
        }

        // Validate ConvertFromUtc
        var utcTime = DateTime.UtcNow;
        var convertedLocal = DateTimeUtility.ConvertFromUtc(utcTime, "UTC");
        // Result should be a valid local time
        if (convertedLocal == default)
        {
            problems.Add("DateTimeUtility.ConvertFromUtc should return valid DateTime.");
        }

        // Test with invalid timezone - should gracefully handle
        var invalidTzFromUtc = DateTimeUtility.ConvertFromUtc(utcTime, "Invalid/Timezone");
        if (invalidTzFromUtc == default)
        {
            problems.Add("DateTimeUtility.ConvertFromUtc should return valid DateTime even with invalid timezone.");
        }

        // Validate IsWithinBusinessHours
        var businessHours = DateTimeUtility.IsWithinBusinessHours(new DateTime(2024, 1, 1, 10, 0, 0));
        if (!businessHours)
        {
            problems.Add("DateTimeUtility.IsWithinBusinessHours should return true for Monday-Friday 10:00.");
        }

        var weekendHours = DateTimeUtility.IsWithinBusinessHours(new DateTime(2024, 1, 6, 10, 0, 0)); // Saturday
        if (weekendHours)
        {
            problems.Add("DateTimeUtility.IsWithinBusinessHours should return false for weekend days.");
        }

        var outsideHours = DateTimeUtility.IsWithinBusinessHours(new DateTime(2024, 1, 1, 19, 0, 0));
        if (outsideHours)
        {
            problems.Add("DateTimeUtility.IsWithinBusinessHours should return false for hours outside business range (8-18).");
        }

        // Validate GetDayOfYear
        var dayOfYear = DateTimeUtility.GetDayOfYear(new DateTime(2024, 6, 15));
        if (dayOfYear < 1 || dayOfYear > 366)
        {
            problems.Add("DateTimeUtility.GetDayOfYear should return value between 1 and 366.");
        }

        // Validate GetWeekOfYear
        var weekOfYear = DateTimeUtility.GetWeekOfYear(new DateTime(2024, 1, 1));
        if (weekOfYear < 1 || weekOfYear > 53)
        {
            problems.Add("DateTimeUtility.GetWeekOfYear should return value between 1 and 53.");
        }

        // Validate GetStartOfDay / GetEndOfDay
        var testDate = new DateTime(2024, 6, 15, 14, 30, 45);
        var startOfDay = DateTimeUtility.GetStartOfDay(testDate);
        if (startOfDay.Hour != 0 || startOfDay.Minute != 0 || startOfDay.Second != 0)
        {
            problems.Add("DateTimeUtility.GetStartOfDay should return DateTime with time set to 00:00:00.");
        }

        var endOfDay = DateTimeUtility.GetEndOfDay(testDate);
        if (endOfDay.Hour != 23 || endOfDay.Minute != 59 || endOfDay.Second != 59)
        {
            problems.Add("DateTimeUtility.GetEndOfDay should return DateTime with time set to 23:59:59.");
        }

        // Validate GetStartOfWeek / GetEndOfWeek
        var startOfWeek = DateTimeUtility.GetStartOfWeek(testDate);
        if (startOfWeek.DayOfWeek != DayOfWeek.Monday)
        {
            problems.Add("DateTimeUtility.GetStartOfWeek should return DateTime for Monday of the week.");
        }

        var endOfWeek = DateTimeUtility.GetEndOfWeek(testDate);
        if (endOfWeek.DayOfWeek != DayOfWeek.Sunday)
        {
            problems.Add("DateTimeUtility.GetEndOfWeek should return DateTime for Sunday of the week.");
        }

        // Validate GetStartOfMonth / GetEndOfMonth
        var startOfMonth = DateTimeUtility.GetStartOfMonth(testDate);
        if (startOfMonth.Day != 1)
        {
            problems.Add("DateTimeUtility.GetStartOfMonth should return DateTime with day set to 1.");
        }

        var endOfMonth = DateTimeUtility.GetEndOfMonth(testDate);
        var expectedEndDay = DateTime.DaysInMonth(testDate.Year, testDate.Month);
        if (endOfMonth.Day != expectedEndDay)
        {
            problems.Add("DateTimeUtility.GetEndOfMonth should return DateTime with day set to last day of month.");
        }

        // Validate IsLeapYear
        var isLeapYear2024 = DateTimeUtility.IsLeapYear(2024);
        if (!isLeapYear2024)
        {
            problems.Add("DateTimeUtility.IsLeapYear(2024) should return true.");
        }

        var isLeapYear2023 = DateTimeUtility.IsLeapYear(2023);
        if (isLeapYear2023)
        {
            problems.Add("DateTimeUtility.IsLeapYear(2023) should return false.");
        }

        // Validate CalculateDuration
        var start = new DateTime(2024, 1, 1, 10, 0, 0);
        var end = new DateTime(2024, 1, 1, 12, 30, 15);
        var durationResult = DateTimeUtility.CalculateDuration(start, end);
        if (durationResult.TotalHours != 2.5)
        {
            problems.Add("DateTimeUtility.CalculateDuration should return correct duration between two dates.");
        }

        // Validate reverse order duration
        var reverseDuration = DateTimeUtility.CalculateDuration(end, start);
        if (reverseDuration.TotalHours != 2.5)
        {
            problems.Add("DateTimeUtility.CalculateDuration should return absolute duration regardless of order.");
        }

        // Validate FormatDurationComponents
        var durationComponents = DateTimeUtility.FormatDurationComponents(new TimeSpan(2, 5, 30, 0));
        if (durationComponents.Days != 2 || durationComponents.Hours != 5 || durationComponents.Minutes != 30 || durationComponents.Seconds != 0)
        {
            problems.Add("DateTimeUtility.FormatDurationComponents should return correct (Days, Hours, Minutes, Seconds) tuple.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if DateTimeUtility behavior is valid.
    /// </summary>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid()
    {
        try
        {
            return Validate().Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures DateTimeUtility behavior is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails with list of problems.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "DateTimeUtility validation failed:" + Environment.NewLine + "- " +
                string.Join(Environment.NewLine + "- ", problems));
        }
    }
}