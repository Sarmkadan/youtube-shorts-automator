using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Repositories
{
    /// <summary>
    /// Validation helpers for ProcessingJobRepository to ensure repository operations receive valid parameters
    /// </summary>
    public static class ProcessingJobRepositoryValidation
    {
        /// <summary>
        /// Validates a ProcessingJobRepository instance
        /// </summary>
        /// <param name="value">The ProcessingJobRepository instance to validate</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this ProcessingJobRepository? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // Repository-level validations would go here if there were any
            // Currently the repository itself has no state to validate beyond being non-null

            return Array.Empty<string>();
        }

        /// <summary>
        /// Checks if a ProcessingJobRepository instance is valid
        /// </summary>
        /// <param name="value">The ProcessingJobRepository instance to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(this ProcessingJobRepository? value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures a ProcessingJobRepository instance is valid, throwing ArgumentException if not
        /// </summary>
        /// <param name="value">The ProcessingJobRepository instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if value is invalid with detailed error messages</exception>
        public static void EnsureValid(this ProcessingJobRepository? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ProcessingJobRepository is invalid:{Environment.NewLine}- {
                        string.Join($"{Environment.NewLine}- ", problems)
                    }");
            }
        }

        /// <summary>
        /// Validates GetByVideoIdAsync parameters
        /// </summary>
        /// <param name="videoId">The video ID to query by</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <exception cref="ArgumentException">Thrown if videoId is empty</exception>
        public static IReadOnlyList<string> ValidateProcessingJobVideoId(this Guid videoId)
        {
            var errors = new List<string>();

            if (videoId == Guid.Empty)
            {
                errors.Add("Video ID cannot be empty (Guid.Empty)");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates GetByStatusAsync parameters
        /// </summary>
        /// <param name="status">The processing job status to filter by</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <remarks>All ProcessingJobStatus enum values are valid by design</remarks>
        public static IReadOnlyList<string> Validate(this ProcessingJobStatus status) => Array.Empty<string>();

        /// <summary>
        /// Validates GetPendingJobsAsync parameters
        /// </summary>
        /// <returns>List of validation error messages, empty if valid</returns>
        public static IReadOnlyList<string> Validate() => Array.Empty<string>();

        /// <summary>
        /// Validates GetFailedJobsAsync parameters
        /// </summary>
        /// <returns>List of validation error messages, empty if valid</returns>
        public static IReadOnlyList<string> ValidateFailedJobs() => Array.Empty<string>();

        /// <summary>
        /// Validates GetJobsForRetryAsync parameters
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <exception cref="ArgumentException">Thrown if maxRetries is not positive or exceeds reasonable limit</exception>
        public static IReadOnlyList<string> Validate(this int maxRetries)
        {
            var errors = new List<string>();

            if (maxRetries <= 0)
            {
                errors.Add("Max retries must be greater than zero");
            }
            else if (maxRetries > 100)
            {
                errors.Add("Max retries cannot exceed 100");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates GetJobsByTypeAsync parameters
        /// </summary>
        /// <param name="jobType">The processing job type to filter by</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <remarks>All ProcessingJobType enum values are valid by design</remarks>
        public static IReadOnlyList<string> Validate(this ProcessingJobType jobType) => Array.Empty<string>();

        /// <summary>
        /// Validates GetPaginatedAsync parameters
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        /// <exception cref="ArgumentException">Thrown if pageNumber or pageSize are invalid</exception>
        public static IReadOnlyList<string> Validate(this int pageNumber, int pageSize)
        {
            var errors = new List<string>();

            if (pageNumber < 1)
            {
                errors.Add("Page number must be 1 or greater");
            }

            if (pageSize < 1)
            {
                errors.Add("Page size must be 1 or greater");
            }
            else if (pageSize > 1000)
            {
                errors.Add("Page size cannot exceed 1000 items");
            }

            return errors.AsReadOnly();
        }
    }
}
