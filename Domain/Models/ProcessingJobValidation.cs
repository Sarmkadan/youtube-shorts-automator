// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="ProcessingJob"/> class
/// </summary>
public static class ProcessingJobValidation
{
    /// <summary>
    /// Validates a ProcessingJob object and returns any validation errors
    /// </summary>
    /// <param name="value">The processing job to validate</param>
    /// <returns>Read-only list of validation errors (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ProcessingJob? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
            errors.Add("Processing job ID must be a valid GUID");

        // Validate VideoId
        if (value.VideoId == Guid.Empty)
            errors.Add("Video ID must be a valid GUID");

        // Validate Video (if present)
        if (value.Video != null)
        {
            var videoValidation = value.Video.Validate();
            if (videoValidation.IsValid == false)
                errors.AddRange(videoValidation.Errors.Select(e => $"Video: {e}"));
        }
        else if (value.JobType != ProcessingJobType.MetadataExtraction)
        {
            // Video can be null only for MetadataExtraction jobs
            errors.Add("Video reference is required for this job type");
        }

        // Validate JobType
        // Enum validation handled by compiler

        // Validate Status
        // Enum validation handled by compiler

        // Validate CurrentStep
        // Enum validation handled by compiler

        // Validate CreatedAt
        if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Creation date cannot be in the future");
        else if (value.CreatedAt < DateTime.UnixEpoch)
            errors.Add("Creation date must be a valid date");

        // Validate StartedAt
        if (value.StartedAt.HasValue)
        {
            if (value.StartedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("Start date cannot be in the future");
            else if (value.StartedAt.Value < value.CreatedAt)
                errors.Add("Start date cannot occur before creation date");
        }

        // Validate CompletedAt
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("Completion date cannot be in the future");
            else if (value.StartedAt.HasValue && value.CompletedAt.Value < value.StartedAt.Value)
                errors.Add("Completion date cannot occur before start date");
        }

        // Validate OutputPath
        if (value.JobType != ProcessingJobType.MetadataExtraction && string.IsNullOrWhiteSpace(value.OutputPath))
            errors.Add("Output path is required for this job type");
        else if (!string.IsNullOrWhiteSpace(value.OutputPath) && value.OutputPath.Length > 1024)
            errors.Add("Output path cannot exceed 1024 characters");

        // Validate Steps
        if (value.Steps == null)
            errors.Add("Steps collection cannot be null");
        else if (value.Steps.Any(s => s == null))
            errors.Add("Steps collection cannot contain null entries");
        else if (value.Steps.Count > 100)
            errors.Add("Cannot have more than 100 processing steps");

        // Validate Errors
        if (value.Errors == null)
            errors.Add("Errors collection cannot be null");
        else if (value.Errors.Any(e => e == null))
            errors.Add("Errors collection cannot contain null entries");

        // Validate RetryCount
        if (value.RetryCount < 0)
            errors.Add("Retry count cannot be negative");
        else if (value.RetryCount > value.MaxRetries)
            errors.Add("Retry count cannot exceed maximum retries");

        // Validate MaxRetries
        if (value.MaxRetries < 0)
            errors.Add("Maximum retries cannot be negative");
        else if (value.MaxRetries > 10)
            errors.Add("Maximum retries cannot exceed 10");

        // Validate EstimatedDuration
        if (value.EstimatedDuration < TimeSpan.Zero)
            errors.Add("Estimated duration cannot be negative");
        else if (value.EstimatedDuration > TimeSpan.FromHours(24))
            errors.Add("Estimated duration cannot exceed 24 hours");

        // Validate ProgressPercentage
        if (value.ProgressPercentage < 0f)
            errors.Add("Progress percentage cannot be negative");
        else if (value.ProgressPercentage > 100f)
            errors.Add("Progress percentage cannot exceed 100%");

        // Validate WorkerId
        if (!string.IsNullOrWhiteSpace(value.WorkerId) && value.WorkerId.Length > 100)
            errors.Add("Worker ID cannot exceed 100 characters");

        // Validate job state consistency
        if (value.Status == ProcessingJobStatus.Completed || value.Status == ProcessingJobStatus.Failed)
        {
            if (!value.CompletedAt.HasValue)
                errors.Add("Completed/failed jobs must have a completion date");
        }

        if (value.Status == ProcessingJobStatus.Running)
        {
            if (!value.StartedAt.HasValue)
                errors.Add("Running jobs must have a start date");
        }

        if (value.Status == ProcessingJobStatus.Queued)
        {
            if (value.StartedAt.HasValue)
                errors.Add("Queued jobs cannot have a start date");
            if (value.CompletedAt.HasValue)
                errors.Add("Queued jobs cannot have a completion date");
        }

        // Validate progress consistency with status
        if (value.ProgressPercentage >= 100f && value.Status != ProcessingJobStatus.Completed)
            errors.Add("Progress at 100% requires Completed status");

        // Validate retry count with status
        if (value.Status == ProcessingJobStatus.Failed && value.RetryCount >= value.MaxRetries)
            errors.Add("Failed job has reached maximum retry attempts");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a ProcessingJob object is valid
    /// </summary>
    /// <param name="value">The processing job to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ProcessingJob? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a ProcessingJob object is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The processing job to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with error details</exception>
    public static void EnsureValid(this ProcessingJob? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Processing job validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}
