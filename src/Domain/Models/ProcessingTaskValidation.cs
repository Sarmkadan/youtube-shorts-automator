using System.Globalization;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ProcessingTask"/> instances.
/// </summary>
public static class ProcessingTaskValidation
{
    /// <summary>
    /// Validates the specified processing task and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The processing task to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the processing task is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingTask value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id (must be positive)
        if (value.Id <= 0)
        {
            errors.Add("Id must be a positive integer.");
        }

        // Validate VideoShortId (must be positive)
        if (value.VideoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        // Validate TaskType (must not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.TaskType))
        {
            errors.Add("TaskType cannot be null or whitespace.");
        }
        else if (value.TaskType.Length > 50)
        {
            errors.Add("TaskType cannot exceed 50 characters.");
        }

        // Validate Status (must be a defined enum value)
        if (!Enum.IsDefined(typeof(ProcessingStatus), value.Status))
        {
            errors.Add("Status must be a valid ProcessingStatus value.");
        }

        // Validate Priority (must be between 1 and 10 inclusive)
        if (value.Priority < 1 || value.Priority > 10)
        {
            errors.Add("Priority must be between 1 and 10 inclusive.");
        }

        // Validate StartedAt (must not be default DateTime)
        if (value.StartedAt == default)
        {
            errors.Add("StartedAt cannot be the default DateTime value.");
        }

        // Validate CompletedAt (if set, must not be default DateTime)
        if (value.CompletedAt.HasValue && value.CompletedAt.Value == default)
        {
            errors.Add("CompletedAt, if set, cannot be the default DateTime value.");
        }

        // Validate ElapsedTime (if set, must be positive)
        if (value.ElapsedTime.HasValue && value.ElapsedTime.Value <= TimeSpan.Zero)
        {
            errors.Add("ElapsedTime, if set, must be a positive TimeSpan.");
        }

        // Validate ErrorMessage (if set, must not be whitespace)
        if (!string.IsNullOrWhiteSpace(value.ErrorMessage) && value.ErrorMessage.Length > 1000)
        {
            errors.Add("ErrorMessage, if set, cannot exceed 1000 characters.");
        }

        // Validate TaskLog (must not be null, individual log entries validation)
        if (value.TaskLog is null)
        {
            errors.Add("TaskLog cannot be null.");
        }
        else if (value.TaskLog.Length > 10000)
        {
            errors.Add("TaskLog cannot exceed 10000 characters.");
        }

        // Validate OutputWidth (must be positive)
        if (value.OutputWidth <= 0)
        {
            errors.Add("OutputWidth must be a positive integer.");
        }
        else if (value.OutputWidth > 8192) // 8K resolution max
        {
            errors.Add("OutputWidth cannot exceed 8192 pixels.");
        }

        // Validate OutputHeight (must be positive)
        if (value.OutputHeight <= 0)
        {
            errors.Add("OutputHeight must be a positive integer.");
        }
        else if (value.OutputHeight > 4320) // 8K resolution max
        {
            errors.Add("OutputHeight cannot exceed 4320 pixels.");
        }

        // Validate OutputBitrate (must be positive)
        if (value.OutputBitrate <= 0)
        {
            errors.Add("OutputBitrate must be a positive integer in kbps.");
        }
        else if (value.OutputBitrate > 100000) // 100 Mbps max
        {
            errors.Add("OutputBitrate cannot exceed 100000 kbps.");
        }

        // Validate OutputFormat (must not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.OutputFormat))
        {
            errors.Add("OutputFormat cannot be null or whitespace.");
        }
        else if (value.OutputFormat.Length > 20)
        {
            errors.Add("OutputFormat cannot exceed 20 characters.");
        }

        // Validate CreatedAt (must not be default DateTime)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }

        // Validate UpdatedAt (must not be default DateTime)
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be the default DateTime value.");
        }

        // Validate consistency between StartedAt, CompletedAt, and ElapsedTime
        if (value.CompletedAt.HasValue && value.StartedAt != default)
        {
            if (value.CompletedAt.Value < value.StartedAt)
            {
                errors.Add("CompletedAt cannot be earlier than StartedAt.");
            }

            if (value.ElapsedTime.HasValue)
            {
                var calculatedElapsed = value.CompletedAt.Value - value.StartedAt;
                if (Math.Abs((value.ElapsedTime.Value - calculatedElapsed).TotalSeconds) > 1)
                {
                    errors.Add("ElapsedTime does not match the difference between CompletedAt and StartedAt.");
                }
            }
        }

        // Validate that if Status is Completed, CompletedAt and ElapsedTime should be set
        if (value.Status == ProcessingStatus.Completed)
        {
            if (!value.CompletedAt.HasValue)
            {
                errors.Add("Completed tasks must have CompletedAt set.");
            }

            if (!value.ElapsedTime.HasValue)
            {
                errors.Add("Completed tasks must have ElapsedTime set.");
            }
        }

        // Validate that if Status is Failed, CompletedAt, ElapsedTime, and ErrorMessage should be set
        if (value.Status == ProcessingStatus.Failed)
        {
            if (!value.CompletedAt.HasValue)
            {
                errors.Add("Failed tasks must have CompletedAt set.");
            }

            if (!value.ElapsedTime.HasValue)
            {
                errors.Add("Failed tasks must have ElapsedTime set.");
            }

            if (string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                errors.Add("Failed tasks must have ErrorMessage set.");
            }
        }

        // Validate that if Status is Processing, StartedAt should be set
        if (value.Status == ProcessingStatus.Processing)
        {
            if (value.StartedAt == default)
            {
                errors.Add("Processing tasks must have StartedAt set.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified processing task is valid.
    /// </summary>
    /// <param name="value">The processing task to check.</param>
    /// <returns><see langword="true"/> if the processing task is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ProcessingTask value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified processing task is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The processing task to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the processing task is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this ProcessingTask value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingTask is invalid. Validation errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}