// =============================================================================
// Validation helpers for VideoProcessingService
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortAutomator.Services
{
    /// <summary>
    /// Provides validation extension methods for <see cref="VideoProcessingService"/>.
    /// </summary>
    public static class VideoProcessingServiceValidation
    {
        /// <summary>
        /// Validates the state of a <see cref="VideoProcessingService"/> instance and returns a collection
        /// of human‑readable problem descriptions.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages.
        /// The list is empty when the instance is considered valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this VideoProcessingService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // The service only holds private dependencies (repository and logger) that are
            // guaranteed to be non‑null by the constructor guard clauses. No public state
            // exists to validate, so we simply return an empty list when the instance is
            // non‑null.
            // If future public properties are added, validation logic should be extended
            // here accordingly.

            return problems;
        }

        /// <summary>
        /// Determines whether the specified <see cref="VideoProcessingService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to check.</param>
        /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this VideoProcessingService value) =>
            !value.Validate().Any();

        /// <summary>
        /// Ensures that the specified <see cref="VideoProcessingService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance contains validation problems.
        /// The exception message lists all detected issues.</exception>
        public static void EnsureValid(this VideoProcessingService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"VideoProcessingService validation failed: {string.Join("; ", problems)}",
                    nameof(value));
            }
        }
    }
}
