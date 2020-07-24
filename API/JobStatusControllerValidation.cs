// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation helpers for JobStatusController response models
// Provides validation for job status API response data integrity
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for JobStatusController response models
/// Validates job status API response data integrity and business rules
/// </summary>
public static class JobStatusControllerValidation
{
    /// <summary>
    /// Validates a JobStatusResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobStatusResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobStatusResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate JobId
        if (value.JobId == Guid.Empty)
        {
            problems.Add("JobId must be a non-empty GUID.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            problems.Add("Status must be specified.");
        }
        else
        {
            var validStatuses = new[] { "Pending", "Running", "Completed", "Failed", "Cancelled" };
            if (Array.IndexOf(validStatuses, value.Status) < 0)
            {
                problems.Add($"Status must be one of: {string.Join(", ", validStatuses)}.");
            }
        }

        // Validate Progress
        if (value.Progress < 0 || value.Progress > 100)
        {
            problems.Add("Progress must be between 0 and 100.");
        }

        // Validate CreatedAtUtc
        if (value.CreatedAtUtc == default)
        {
            problems.Add("CreatedAtUtc must be set to a non-default DateTime value.");
        }
        else if (value.CreatedAtUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("CreatedAtUtc must be in UTC format.");
        }
        else if (value.CreatedAtUtc > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("CreatedAtUtc cannot be more than 1 hour in the future.");
        }

        // Validate StartedAtUtc (if set)
        if (value.StartedAtUtc.HasValue)
        {
            var startedAt = value.StartedAtUtc.Value;
            if (startedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("StartedAtUtc must be in UTC format.");
            }
            else if (startedAt > DateTime.UtcNow.AddHours(1))
            {
                problems.Add("StartedAtUtc cannot be more than 1 hour in the future.");
            }
            else if (startedAt < value.CreatedAtUtc)
            {
                problems.Add("StartedAtUtc cannot be before CreatedAtUtc.");
            }
        }

        // Validate CompletedAtUtc (if set)
        if (value.CompletedAtUtc.HasValue)
        {
            var completedAt = value.CompletedAtUtc.Value;
            if (completedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("CompletedAtUtc must be in UTC format.");
            }
            else if (completedAt > DateTime.UtcNow.AddHours(1))
            {
                problems.Add("CompletedAtUtc cannot be more than 1 hour in the future.");
            }
            else if (value.StartedAtUtc.HasValue && completedAt < value.StartedAtUtc.Value)
            {
                problems.Add("CompletedAtUtc cannot be before StartedAtUtc.");
            }
        }

        // Validate ErrorMessage (if set)
        if (value.ErrorMessage is not null && value.ErrorMessage.Length > 1000)
        {
            problems.Add("ErrorMessage must be 1000 characters or less.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a JobResultsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobResultsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobResultsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate JobId
        if (value.JobId == Guid.Empty)
        {
            problems.Add("JobId must be a non-empty GUID.");
        }

        // Validate SuccessfulCount
        if (value.SuccessfulCount < 0)
        {
            problems.Add("SuccessfulCount cannot be negative.");
        }

        // Validate FailedCount
        if (value.FailedCount < 0)
        {
            problems.Add("FailedCount cannot be negative.");
        }

        // Validate TotalCount
        if (value.TotalCount < 0)
        {
            problems.Add("TotalCount cannot be negative.");
        }
        else if (value.TotalCount != value.SuccessfulCount + value.FailedCount)
        {
            problems.Add("TotalCount must equal SuccessfulCount + FailedCount.");
        }

        // Validate Items collection
        if (value.Items is null)
        {
            problems.Add("Items collection is null.");
        }
        else if (value.Items.Count != value.TotalCount)
        {
            problems.Add($"Items collection count ({value.Items.Count}) must match TotalCount ({value.TotalCount}).");
        }
        else
        {
            // Validate each item
            for (int i = 0; i < value.Items.Count; i++)
            {
                var item = value.Items[i];
                if (item == null)
                {
                    problems.Add($"Items[{i}] is null.");
                    continue;
                }

                // Validate Identifier
                if (string.IsNullOrWhiteSpace(item.Identifier))
                {
                    problems.Add($"Items[{i}].Identifier must be specified.");
                }

                // Validate Status
                if (string.IsNullOrWhiteSpace(item.Status))
                {
                    problems.Add($"Items[{i}].Status must be specified.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a JobListItem instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobListItem instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobListItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate JobId
        if (value.JobId == Guid.Empty)
        {
            problems.Add("JobId must be a non-empty GUID.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            problems.Add("Status must be specified.");
        }
        else
        {
            var validStatuses = new[] { "Pending", "Running", "Completed", "Failed", "Cancelled" };
            if (Array.IndexOf(validStatuses, value.Status) < 0)
            {
                problems.Add($"Status must be one of: {string.Join(", ", validStatuses)}.");
            }
        }

        // Validate Progress
        if (value.Progress < 0 || value.Progress > 100)
        {
            problems.Add("Progress must be between 0 and 100.");
        }

        // Validate CreatedAtUtc
        if (value.CreatedAtUtc == default)
        {
            problems.Add("CreatedAtUtc must be set to a non-default DateTime value.");
        }
        else if (value.CreatedAtUtc.Kind != DateTimeKind.Utc)
        {
            problems.Add("CreatedAtUtc must be in UTC format.");
        }
        else if (value.CreatedAtUtc > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("CreatedAtUtc cannot be more than 1 hour in the future.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a JobsListResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobsListResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobsListResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Jobs collection
        if (value.Jobs is null)
        {
            problems.Add("Jobs collection is null.");
        }
        else if (value.Jobs.Count == 0)
        {
            problems.Add("Jobs collection cannot be empty.");
        }
        else
        {
            // Validate each job
            for (int i = 0; i < value.Jobs.Count; i++)
            {
                var problemsForJob = Validate(value.Jobs[i]);
                if (problemsForJob.Count > 0)
                {
                    problems.AddRange(problemsForJob.Select(p => $"Jobs[{i}]: {p}"));
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a JobStatsResponse instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobStatsResponse instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobStatsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate TotalJobs
        if (value.TotalJobs < 0)
        {
            problems.Add("TotalJobs cannot be negative.");
        }

        // Validate CompletedJobs
        if (value.CompletedJobs < 0)
        {
            problems.Add("CompletedJobs cannot be negative.");
        }

        // Validate RunningJobs
        if (value.RunningJobs < 0)
        {
            problems.Add("RunningJobs cannot be negative.");
        }

        // Validate FailedJobs
        if (value.FailedJobs < 0)
        {
            problems.Add("FailedJobs cannot be negative.");
        }

        // Validate TotalJobs consistency
        if (value.TotalJobs != value.CompletedJobs + value.RunningJobs + value.FailedJobs)
        {
            problems.Add("TotalJobs must equal CompletedJobs + RunningJobs + FailedJobs.");
        }

        // Validate AverageProcessingTimeMinutes
        if (value.AverageProcessingTimeMinutes < 0)
        {
            problems.Add("AverageProcessingTimeMinutes cannot be negative.");
        }

        // Validate SuccessRate
        if (value.SuccessRate < 0 || value.SuccessRate > 100)
        {
            problems.Add("SuccessRate must be between 0 and 100.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a JobResultItem instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The JobResultItem instance to validate</param>
    /// <returns>List of human-readable validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this JobResultItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Identifier
        if (string.IsNullOrWhiteSpace(value.Identifier))
        {
            problems.Add("Identifier must be specified.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            problems.Add("Status must be specified.");
        }

        // Validate Result (if set)
        if (value.Result is not null && value.Result.Length > 2000)
        {
            problems.Add("Result must be 2000 characters or less.");
        }

        // Validate ErrorMessage (if set)
        if (value.ErrorMessage is not null && value.ErrorMessage.Length > 1000)
        {
            problems.Add("ErrorMessage must be 1000 characters or less.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified JobStatusResponse instance is valid
    /// </summary>
    /// <param name="value">The JobStatusResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobStatusResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified JobResultsResponse instance is valid
    /// </summary>
    /// <param name="value">The JobResultsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobResultsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified JobListItem instance is valid
    /// </summary>
    /// <param name="value">The JobListItem instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobListItem value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified JobsListResponse instance is valid
    /// </summary>
    /// <param name="value">The JobsListResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobsListResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified JobStatsResponse instance is valid
    /// </summary>
    /// <param name="value">The JobStatsResponse instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobStatsResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Determines whether the specified JobResultItem instance is valid
    /// </summary>
    /// <param name="value">The JobResultItem instance to check</param>
    /// <returns>True if valid; false if invalid</returns>
    public static bool IsValid(this JobResultItem value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified JobStatusResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobStatusResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobStatusResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobStatusResponse validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified JobResultsResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobResultsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobResultsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobResultsResponse validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified JobListItem instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobListItem instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobListItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobListItem validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified JobsListResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobsListResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobsListResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobsListResponse validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified JobStatsResponse instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobStatsResponse instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobStatsResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobStatsResponse validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified JobResultItem instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The JobResultItem instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages</exception>
    public static void EnsureValid(this JobResultItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"JobResultItem validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}