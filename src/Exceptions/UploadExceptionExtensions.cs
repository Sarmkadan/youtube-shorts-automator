using System;
using System.Globalization;

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="UploadException"/>.
/// </summary>
public static class UploadExceptionExtensions
{
    /// <summary>
    /// Formats the exception details into a string representation for logging or error reporting.
    /// </summary>
    /// <param name="exception">The <see cref="UploadException"/> to format.</param>
    /// <returns>A string containing the exception message and its associated context IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string FormatDetails(this UploadException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = exception.Message;
        var jobId = exception.UploadJobId?.ToString(CultureInfo.InvariantCulture) ?? "N/A";
        var videoId = exception.VideoShortId?.ToString(CultureInfo.InvariantCulture) ?? "N/A";

        return string.Format(CultureInfo.InvariantCulture, "{0} [JobId: {1}, VideoId: {2}, Retryable: {3}]", message, jobId, videoId, exception.IsRetryable);
    }

    /// <summary>
    /// Determines whether the exception has any associated context (UploadJobId or VideoShortId).
    /// </summary>
    /// <param name="exception">The <see cref="UploadException"/> to check.</param>
    /// <returns>True if either <see cref="UploadException.UploadJobId"/> or <see cref="UploadException.VideoShortId"/> is set; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool HasContext(this UploadException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.UploadJobId.HasValue || exception.VideoShortId.HasValue;
    }

    /// <summary>
    /// Gets a human-readable message indicating whether the exception is retryable.
    /// </summary>
    /// <param name="exception">The <see cref="UploadException"/> to check.</param>
    /// <returns>A message indicating the retryable status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetRetryableStatusMessage(this UploadException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.IsRetryable ? "The operation is eligible for retry." : "The operation is not eligible for retry.";
    }
}
