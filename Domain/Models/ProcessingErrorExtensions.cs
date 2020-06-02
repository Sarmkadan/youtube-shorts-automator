using System;
using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ProcessingError"/>.
/// </summary>
public static class ProcessingErrorExtensions
{
    /// <summary>
    /// Checks if the error occurred longer ago than the specified threshold.
    /// </summary>
    /// <param name="error">The processing error.</param>
    /// <param name="threshold">The duration to check against.</param>
    /// <returns>True if the error is older than the threshold, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static bool IsStale(this ProcessingError error, TimeSpan threshold)
    {
        ArgumentNullException.ThrowIfNull(error);
        return (DateTime.UtcNow - error.OccurredAt) > threshold;
    }

    /// <summary>
    /// Returns a string representation of the error, useful for logging.
    /// </summary>
    /// <param name="error">The processing error.</param>
    /// <returns>A formatted log string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static string ToLogString(this ProcessingError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return string.Format(CultureInfo.InvariantCulture,
            "Error [ID: {0}, Code: {1}, Severity: {2}, Type: {3}, Message: {4}]",
            error.Id, error.ErrorCode ?? "N/A", error.Severity, error.ErrorType, error.ErrorMessage);
    }

    /// <summary>
    /// Determines if the error requires immediate attention (it is critical and not yet resolved).
    /// </summary>
    /// <param name="error">The processing error.</param>
    /// <returns>True if the error is critical and unresolved.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public static bool NeedsImmediateAttention(this ProcessingError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.IsCritical() && !error.IsResolved;
    }
}
