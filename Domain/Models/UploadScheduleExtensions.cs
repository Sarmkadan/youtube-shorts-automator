using System;
using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="UploadSchedule"/>.
/// </summary>
public static class UploadScheduleExtensions
{
    /// <summary>
    /// Returns a formatted summary string of the schedule frequency, time, and timezone.
    /// </summary>
    /// <param name="schedule">The upload schedule.</param>
    /// <returns>A string representing the schedule.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schedule"/> is null.</exception>
    public static string GetScheduleSummary(this UploadSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return string.Format(CultureInfo.InvariantCulture, "{0} at {1} in {2}", schedule.Frequency, schedule.ScheduledTime.ToString("HH:mm", CultureInfo.InvariantCulture), schedule.TimeZoneId);
    }

    /// <summary>
    /// Determines if the schedule is currently active and has a future scheduled time.
    /// </summary>
    /// <param name="schedule">The upload schedule.</param>
    /// <returns>True if the schedule is active and upcoming, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schedule"/> is null.</exception>
    public static bool IsUpcoming(this UploadSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return schedule.IsActive && schedule.NextScheduledTime.HasValue && schedule.NextScheduledTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the count of associated scheduled uploads.
    /// </summary>
    /// <param name="schedule">The upload schedule.</param>
    /// <returns>The number of scheduled uploads.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schedule"/> is null.</exception>
    public static int GetTotalUploadsCount(this UploadSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        return schedule.ScheduledUploads?.Count ?? 0;
    }
}
