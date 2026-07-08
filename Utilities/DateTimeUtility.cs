// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides datetime formatting and conversion utilities
/// Handles timezone conversions, duration calculations, and schedule parsing
/// </summary>
public static class DateTimeUtility
{
    public static string FormatUtcDateTime(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FormatIsoDateTime(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O");
    }

    public static string FormatRelativeTime(DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var timeSpan = now - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} seconds ago";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

        return $"{(int)(timeSpan.TotalDays / 30)} months ago";
    }

    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
            return $"{(int)duration.TotalSeconds}s";

        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        return $"{(int)duration.TotalHours}h {duration.Minutes}m {duration.Seconds}s";
    }

    public static DateTime ConvertToUtc(DateTime dateTime, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
        }
        catch
        {
            return dateTime.ToUniversalTime();
        }
    }

    public static DateTime ConvertFromUtc(DateTime utcDateTime, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch
        {
            return utcDateTime;
        }
    }

    public static bool IsWithinBusinessHours(DateTime dateTime, int startHour = 8, int endHour = 18)
    {
        var hour = dateTime.Hour;
        var dayOfWeek = dateTime.DayOfWeek;

        // Exclude weekends
        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            return false;

        return hour >= startHour && hour < endHour;
    }

    public static int GetDayOfYear(DateTime dateTime)
    {
        return dateTime.DayOfYear;
    }

    public static int GetWeekOfYear(DateTime dateTime)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(dateTime, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    public static DateTime GetStartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }

    public static DateTime GetEndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime GetStartOfWeek(DateTime dateTime)
    {
        var diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0)
            diff += 7;
        return dateTime.AddDays(-diff).Date;
    }

    public static DateTime GetEndOfWeek(DateTime dateTime)
    {
        return GetStartOfWeek(dateTime).AddDays(7).AddTicks(-1);
    }

    public static DateTime GetStartOfMonth(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public static DateTime GetEndOfMonth(DateTime dateTime)
    {
        var startOfMonth = GetStartOfMonth(dateTime);
        return startOfMonth.AddMonths(1).AddTicks(-1);
    }

    public static bool IsLeapYear(int year)
    {
        return DateTime.IsLeapYear(year);
    }

    public static TimeSpan CalculateDuration(DateTime start, DateTime end)
    {
        return end > start ? end - start : start - end;
    }

    public static (int Days, int Hours, int Minutes, int Seconds) FormatDurationComponents(TimeSpan duration)
    {
        var totalSeconds = (int)duration.TotalSeconds;
        var days = totalSeconds / 86400;
        var hours = (totalSeconds % 86400) / 3600;
        var minutes = (totalSeconds % 3600) / 60;
        var seconds = totalSeconds % 60;

        return (days, hours, minutes, seconds);
    }
}
