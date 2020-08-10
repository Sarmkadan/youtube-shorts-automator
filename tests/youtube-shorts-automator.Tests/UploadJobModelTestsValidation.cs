using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Tests
{
    public static class UploadJobModelTestsValidation
    {
        /// <summary>
        /// Validates the UploadJob instance for common issues.
        /// </summary>
        /// <param name="value">The UploadJob instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static IReadOnlyList<string> Validate(this global::YouTubeShortAutomator.Domain.Models.UploadJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue
            // This is a method, not a property, so no validation needed

            // Validate CanRetry_WhenAttemptCountMatchesMaxRetries_ReturnsFalse
            // This is a method, not a property, so no validation needed

            // Validate UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage
            // This is a method, not a property, so no validation needed

            // Validate MarkAsCompleted_AssignsVideoIdAndSetsProgressToFull
            // This is a method, not a property, so no validation needed

            // Validate UploadedBytes_WhenSet_PreservesValueForResume
            // This is a method, not a property, so no validation needed

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the UploadJob instance is valid.
        /// </summary>
        /// <param name="value">The UploadJob instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static bool IsValid(this global::YouTubeShortAutomator.Domain.Models.UploadJob value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the UploadJob instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The UploadJob instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if value is not valid, containing the validation errors.</exception>
        public static void EnsureValid(this global::YouTubeShortAutomator.Domain.Models.UploadJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException($"UploadJob is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}