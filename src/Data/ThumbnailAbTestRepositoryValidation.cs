using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace YouTubeShortAutomator.Data
{
    /// <summary>
    /// Provides validation helpers for <see cref="ThumbnailAbTestRepository"/> instances.
    /// </summary>
    public static class ThumbnailAbTestRepositoryValidation
    {
        /// <summary>
        /// Validates the specified <see cref="ThumbnailAbTestRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ThumbnailAbTestRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate public members
            if (value.GetByIdAsync == null)
            {
                errors.Add("GetByIdAsync delegate cannot be null.");
            }

            if (value.GetAllAsync == null)
            {
                errors.Add("GetAllAsync delegate cannot be null.");
            }

            if (value.GetByVideoShortIdAsync == null)
            {
                errors.Add("GetByVideoShortIdAsync delegate cannot be null.");
            }

            if (value.GetActiveVariantsAsync == null)
            {
                errors.Add("GetActiveVariantsAsync delegate cannot be null.");
            }

            if (value.GetWinnerAsync == null)
            {
                errors.Add("GetWinnerAsync delegate cannot be null.");
            }

            if (value.AddAsync == null)
            {
                errors.Add("AddAsync delegate cannot be null.");
            }

            if (value.UpdateAsync == null)
            {
                errors.Add("UpdateAsync delegate cannot be null.");
            }

            if (value.DeleteAsync == null)
            {
                errors.Add("DeleteAsync delegate cannot be null.");
            }

            if (value.ExistsAsync == null)
            {
                errors.Add("ExistsAsync delegate cannot be null.");
            }

            if (value.CountAsync == null)
            {
                errors.Add("CountAsync delegate cannot be null.");
            }

            if (value.SaveChangesAsync == null)
            {
                errors.Add("SaveChangesAsync delegate cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ThumbnailAbTestRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ThumbnailAbTestRepository value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ThumbnailAbTestRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this ThumbnailAbTestRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ThumbnailAbTestRepository is not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }
    }
}