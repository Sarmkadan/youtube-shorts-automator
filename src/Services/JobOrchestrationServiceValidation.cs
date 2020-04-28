// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for JobOrchestrationService to ensure data integrity
// before operations that would persist to the database or return results.
// =====================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="JobOrchestrationService"/> return types
/// (<see cref="Pipeline"/> and <see cref="SyncResult"/>) to ensure data integrity.
/// </summary>
public static class JobOrchestrationServiceValidation
{
    /// <summary>
    /// Validates a <see cref="Pipeline"/> instance and returns a list of human-readable
    /// validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The pipeline instance to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Pipeline? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate VideoShortId
        if (value.VideoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        // Validate UploadJobId (can be 0 if not yet created)
        if (value.UploadJobId < 0)
        {
            errors.Add("UploadJobId must be a non-negative integer.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status is required and cannot be empty or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Status cannot exceed 50 characters.");
        }

        // Validate Error
        if (!string.IsNullOrEmpty(value.Error) && value.Error.Length > 500)
        {
            errors.Add("Error message cannot exceed 500 characters.");
        }

        // Validate ProcessingCompleted
        // No validation needed for boolean

        // Validate ScheduledUploadTime
        if (value.ScheduledUploadTime == default)
        {
            errors.Add("ScheduledUploadTime must be a valid DateTime (cannot be default).");
        }
        else if (value.ScheduledUploadTime < DateTime.UtcNow.AddMinutes(-5))
        {
            errors.Add("ScheduledUploadTime cannot be in the past.");
        }

        // Validate CompletedAt
        if (value.CompletedAt != default && value.CompletedAt == default)
        {
            errors.Add("CompletedAt must be a valid DateTime if specified (cannot be default).");
        }
        else if (value.CompletedAt != default && value.CompletedAt < value.ScheduledUploadTime)
        {
            errors.Add("CompletedAt cannot be before ScheduledUploadTime.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="Pipeline"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this Pipeline? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="Pipeline"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed error message if it is not.
    /// </summary>
    /// <param name="value">The pipeline to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the pipeline instance is not valid.</exception>
    public static void EnsureValid(this Pipeline? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"Pipeline instance is not valid. Problems:\n{string.Join("\n", errors)}",
            nameof(value));
    }

    /// <summary>
    /// Validates a <see cref="SyncResult"/> instance and returns a list of human-readable
    /// validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The sync result instance to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SyncResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Channel
        if (string.IsNullOrWhiteSpace(value.Channel))
        {
            errors.Add("Channel is required and cannot be empty or whitespace.");
        }
        else if (value.Channel.Length > 100)
        {
            errors.Add("Channel cannot exceed 100 characters.");
        }

        // Validate SyncedCount
        if (value.SyncedCount < 0)
        {
            errors.Add("SyncedCount cannot be negative.");
        }

        // Validate FailedCount
        if (value.FailedCount < 0)
        {
            errors.Add("FailedCount cannot be negative.");
        }

        // Validate Error
        if (!string.IsNullOrEmpty(value.Error) && value.Error.Length > 500)
        {
            errors.Add("Error message cannot exceed 500 characters.");
        }

        // Validate CompletedAt
        if (value.CompletedAt == default)
        {
            errors.Add("CompletedAt must be a valid DateTime (cannot be default).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyncResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The sync result to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SyncResult? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SyncResult"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed error message if it is not.
    /// </summary>
    /// <param name="value">The sync result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the sync result instance is not valid.</exception>
    public static void EnsureValid(this SyncResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"SyncResult instance is not valid. Problems:\n{string.Join("\n", errors)}",
            nameof(value));
    }
}