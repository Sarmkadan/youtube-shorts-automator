// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for <see cref="SchedulingException"/> to simplify common scheduling error handling scenarios.
/// </summary>
public static class SchedulingExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message that includes the job ID and scheduled time (if available).
    /// </summary>
    /// <param name="exception">The scheduling exception to format.</param>
    /// <returns>A formatted error message string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string FormatErrorMessage(this SchedulingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.JobId.HasValue && exception.ScheduledTime.HasValue)
        {
            return $"Job {exception.JobId.Value} scheduled at {exception.ScheduledTime.Value:yyyy-MM-dd HH:mm:ss} failed: {exception.Message}";
        }
        else if (exception.JobId.HasValue)
        {
            return $"Job {exception.JobId.Value} failed: {exception.Message}";
        }
        else if (exception.ScheduledTime.HasValue)
        {
            return $"Job scheduled at {exception.ScheduledTime.Value:yyyy-MM-dd HH:mm:ss} failed: {exception.Message}";
        }

        return exception.Message;
    }

    /// <summary>
    /// Determines whether the exception represents a job that was scheduled in the past.
    /// </summary>
    /// <param name="exception">The scheduling exception to check.</param>
    /// <returns>True if the job was scheduled in the past; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsPastDue(this SchedulingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ScheduledTime.HasValue && exception.ScheduledTime.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the exception represents a job that was scheduled in the future.
    /// </summary>
    /// <param name="exception">The scheduling exception to check.</param>
    /// <returns>True if the job was scheduled in the future; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsFutureScheduled(this SchedulingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.ScheduledTime.HasValue && exception.ScheduledTime.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a dictionary containing the exception details that can be used for logging or serialization.
    /// </summary>
    /// <param name="exception">The scheduling exception to extract details from.</param>
    /// <returns>A read-only dictionary containing the exception details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> ToLogDictionary(this SchedulingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        dict["Message"] = exception.Message;
        dict["Type"] = nameof(SchedulingException);

        if (exception.JobId.HasValue)
        {
            dict["JobId"] = exception.JobId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (exception.ScheduledTime.HasValue)
        {
            dict["ScheduledTime"] = exception.ScheduledTime.Value.ToString("o", CultureInfo.InvariantCulture);
        }

        if (exception.InnerException != null)
        {
            dict["InnerException"] = exception.InnerException.GetType().Name;
            dict["InnerMessage"] = exception.InnerException.Message;
        }

        return dict.AsReadOnly();
    }
}