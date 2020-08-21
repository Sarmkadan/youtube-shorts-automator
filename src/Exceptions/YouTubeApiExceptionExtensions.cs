// =============================================================================
// Author: Automated Extension Generation
// =============================================================================

using System;
using System.Globalization;

namespace YouTubeShortAutomator.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="YouTubeApiException"/>.
    /// </summary>
    public static class YouTubeApiExceptionExtensions
    {
        /// <summary>
        /// Returns the <see cref="YouTubeApiException.ChannelId"/> value as a non‑nullable <see cref="int"/>.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>The channel identifier.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="YouTubeApiException.ChannelId"/> is <c>null</c>.</exception>
        public static int GetChannelId(this YouTubeApiException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ChannelId ?? throw new InvalidOperationException("ChannelId is null.");
        }

        /// <summary>
        /// Creates a detailed, machine‑readable error message that includes all public properties of the exception.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>A formatted error description.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static string ToDetailedMessage(this YouTubeApiException exception) =>
            $"YouTube API error (ChannelId: {exception.ChannelId?.ToString(CultureInfo.InvariantCulture) ?? "N/A"}, " +
            $"ApiErrorCode: {exception.ApiErrorCode ?? "N/A"}, " +
            $"HttpStatusCode: {exception.HttpStatusCode?.ToString(CultureInfo.InvariantCulture) ?? "N/A"}): " +
            $"{exception.Message}";

        /// <summary>
        /// Determines whether the error is likely transient (e.g., server errors or quota limits) and therefore safe to retry.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns><c>true</c> if the error is considered transient; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static bool IsTransient(this YouTubeApiException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.HttpStatusCode is >= 500 and <= 599 || exception.IsQuotaExceeded();
        }

        /// <summary>
        /// Returns the API error code, or a supplied default value when the code is <c>null</c>.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <param name="defaultValue">The value to return when <see cref="YouTubeApiException.ApiErrorCode"/> is <c>null</c>.</param>
        /// <returns>The API error code or <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="defaultValue"/> is <c>null</c> or empty.</exception>
        public static string GetErrorCodeOrDefault(this YouTubeApiException exception, string defaultValue = "unknown")
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(defaultValue);
            return exception.ApiErrorCode ?? defaultValue;
        }
    }
}
