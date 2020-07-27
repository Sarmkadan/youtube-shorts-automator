using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Exceptions
{
    /// <summary>
    /// Provides validation helpers for <see cref="UploadException"/>.
    /// </summary>
    public static class UploadExceptionValidation
    {
        /// <summary>
        /// Validates the specified <see cref="UploadException"/> instance.
        /// </summary>
        /// <param name="value">The exception to validate.</param>
        /// <returns>A read‑only list of human‑readable problem descriptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this UploadException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Message must not be null or whitespace.
            if (string.IsNullOrWhiteSpace(value.Message))
                problems.Add("Message cannot be null or empty.");

            // UploadJobId must be positive if specified.
            if (value.UploadJobId.HasValue && value.UploadJobId <= 0)
                problems.Add($"UploadJobId must be greater than zero. Actual: {value.UploadJobId}.");

            // VideoShortId must be positive if specified.
            if (value.VideoShortId.HasValue && value.VideoShortId <= 0)
                problems.Add($"VideoShortId must be greater than zero. Actual: {value.VideoShortId}.");

            return problems;
        }

        /// <summary>
        /// Determines whether the specified <see cref="UploadException"/> instance is valid.
        /// </summary>
        /// <param name="value">The exception to check.</param>
        /// <returns><c>true</c> if the exception is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this UploadException value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="UploadException"/> instance is valid.
        /// </summary>
        /// <param name="value">The exception to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the exception is invalid, containing a semicolon‑separated list of problems.
        /// </exception>
        public static void EnsureValid(this UploadException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
                throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}
