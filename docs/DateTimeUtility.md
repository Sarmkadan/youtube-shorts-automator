# DateTimeUtility

`DateTimeUtility` provides a centralized set of static methods for common date and time manipulation, formatting, and calculation tasks within the `youtube-shorts-automator` application. It simplifies operations involving UTC conversions, range calculations, and duration processing to ensure consistency and readability across the project.

## API

*   **`FormatUtcDateTime(DateTime dateTime)`**: Formats a `DateTime` object as a standard UTC string.
*   **`FormatIsoDateTime(DateTime dateTime)`**: Formats a `DateTime` object into an ISO 8601 compliant string.
*   **`FormatRelativeTime(DateTime dateTime)`**: Returns a human-readable string representing the time elapsed since or remaining until the provided `DateTime` (e.g., "2 hours ago").
*   **`FormatDuration(TimeSpan duration)`**: Returns a string representation of the `TimeSpan` in a standard format (e.g., "HH:mm:ss").
*   **`ConvertToUtc(DateTime dateTime)`**: Converts a given `DateTime` to its UTC equivalent.
*   **`ConvertFromUtc(DateTime dateTime, TimeZoneInfo destinationTimeZone)`**: Converts a UTC `DateTime` to the specified local `TimeZoneInfo`.
*   **`IsWithinBusinessHours(DateTime dateTime)`**: Evaluates whether the provided `DateTime` falls within standard business hours.
*   **`GetDayOfYear(DateTime dateTime)`**: Returns the day of the year for the specified `DateTime`.
*   **`GetWeekOfYear(DateTime dateTime)`**: Returns the week of the year for the specified `DateTime` based on current culture settings.
*   **`GetStartOfDay(DateTime dateTime)`**: Returns a `DateTime` representing 00:00:00 of the specified day.
*   **`GetEndOfDay(DateTime dateTime)`**: Returns a `DateTime` representing 23:59:59.999 of the specified day.
*   **`GetStartOfWeek(DateTime dateTime)`**: Returns a `DateTime` representing the start of the week for the given date.
*   **`GetEndOfWeek(DateTime dateTime)`**: Returns a `DateTime` representing the end of the week for the given date.
*   **`GetStartOfMonth(DateTime dateTime)`**: Returns a `DateTime` representing the first day of the month for the given date.
*   **`GetEndOfMonth(DateTime dateTime)`**: Returns a `DateTime` representing the last day of the month for the given date.
*   **`IsLeapYear(int year)`**: Determines if the specified year is a leap year.
*   **`CalculateDuration(DateTime start, DateTime end)`**: Computes the `TimeSpan` between two `DateTime` values.
*   **`FormatDurationComponents(TimeSpan duration)`**: Decomposes a `TimeSpan` into a tuple of `(int Days, int Hours, int Minutes, int Seconds)`.

## Usage

```csharp
// Example 1: Formatting and business hour validation
DateTime scheduledTime = DateTime.UtcNow.AddDays(1);
string isoString = DateTimeUtility.FormatIsoDateTime(scheduledTime);

if (DateTimeUtility.IsWithinBusinessHours(scheduledTime))
{
    Console.WriteLine($"Scheduled time {isoString} is within business hours.");
}
```

```csharp
// Example 2: Calculating duration within a month
DateTime start = DateTimeUtility.GetStartOfMonth(DateTime.Now);
DateTime end = DateTimeUtility.GetEndOfMonth(DateTime.Now);
TimeSpan duration = DateTimeUtility.CalculateDuration(start, end);

var components = DateTimeUtility.FormatDurationComponents(duration);
Console.WriteLine($"Days in month: {components.Days}");
```

## Notes

*   **Thread-Safety**: All methods within `DateTimeUtility` are static and stateless. They are thread-safe and can be accessed concurrently from multiple threads without locking.
*   **Time Zones**: Methods performing conversions rely on the underlying .NET `TimeZoneInfo` and `DateTime.Kind` properties. Ensure that `DateTime` inputs have the correct `Kind` (Local, Utc, or Unspecified) to avoid unexpected results.
*   **Leap Year/Month Boundaries**: Methods like `GetEndOfMonth` and `IsLeapYear` correctly handle leap years and variable month lengths according to the Gregorian calendar.
*   **Exceptions**: Methods may throw standard .NET exceptions (e.g., `ArgumentOutOfRangeException`) if input `DateTime` values are outside the supported range of the `DateTime` structure.
