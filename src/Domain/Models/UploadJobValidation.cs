using System;
using System.Collections.Generic;
using System.Globalization;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

public static class UploadJobValidation
{
    /// <summary>
    /// Validates the specified <see cref="UploadJob"/> instance.
    /// </summary>
    /// <param name="value">The upload job to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadJob value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Id must be a positive integer, but was {value.Id}.");
        }

        // Validate VideoShortId
        if (value.VideoShortId <= 0)
        {
            errors.Add($"VideoShortId must be a positive integer, but was {value.VideoShortId}.");
        }

        // Validate YouTubeVideoId
        if (string.IsNullOrWhiteSpace(value.YouTubeVideoId))
        {
            errors.Add("YouTubeVideoId cannot be null or whitespace.");
        }
        else if (value.YouTubeVideoId.Length > 100)
        {
            errors.Add($"YouTubeVideoId cannot exceed 100 characters, but was {value.YouTubeVideoId.Length}.");
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(UploadStatus), value.Status))
        {
            errors.Add($"Status must be a valid UploadStatus value, but was {(int)value.Status}.");
        }

        // Validate ScheduledAt
        if (value.ScheduledAt == default)
        {
            errors.Add("ScheduledAt cannot be default(DateTime).");
        }
        else if (value.ScheduledAt > DateTime.UtcNow.AddYears(1))
        {
            errors.Add($"ScheduledAt cannot be more than 1 year in the future, but was {value.ScheduledAt:O}.");
        }

        // Validate UploadedAt
        if (value.UploadedAt.HasValue && value.UploadedAt.Value == default)
        {
            errors.Add("UploadedAt cannot be default(DateTime) when set.");
        }

        // Validate AttemptCount
        if (value.AttemptCount < 0)
        {
            errors.Add($"AttemptCount cannot be negative, but was {value.AttemptCount}.");
        }
        else if (value.AttemptCount > value.MaxRetries + 10) // Allow some buffer
        {
            errors.Add($"AttemptCount ({value.AttemptCount}) exceeds MaxRetries ({value.MaxRetries}) by too much.");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            errors.Add($"MaxRetries cannot be negative, but was {value.MaxRetries}.");
        }

        // Validate ErrorMessage
        if (value.ErrorMessage is not null && string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage cannot be empty or whitespace when set.");
        }
        else if (value.ErrorMessage?.Length > 1000)
        {
            errors.Add($"ErrorMessage cannot exceed 1000 characters, but was {value.ErrorMessage.Length}.");
        }

        // Validate UploadedBytes
        if (value.UploadedBytes < 0)
        {
            errors.Add($"UploadedBytes cannot be negative, but was {value.UploadedBytes}.");
        }

        // Validate UploadProgressPercentage
        if (value.UploadProgressPercentage < 0 || value.UploadProgressPercentage > 100)
        {
            errors.Add($"UploadProgressPercentage must be between 0 and 100, but was {value.UploadProgressPercentage.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate EstimatedTimeRemaining
        if (value.EstimatedTimeRemaining < TimeSpan.Zero)
        {
            errors.Add($"EstimatedTimeRemaining cannot be negative, but was {value.EstimatedTimeRemaining}.");
        }
        else if (value.EstimatedTimeRemaining > TimeSpan.FromDays(365))
        {
            errors.Add($"EstimatedTimeRemaining cannot exceed 1 year, but was {value.EstimatedTimeRemaining}.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be default(DateTime).");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be default(DateTime).");
        }
        else if (value.UpdatedAt < value.CreatedAt)
        {
            errors.Add("UpdatedAt cannot be earlier than CreatedAt.");
        }

        // Validate VideoShort
        if (value.VideoShort is null)
        {
            errors.Add("VideoShort cannot be null.");
        }

        // Validate CanRetry
        // CanRetry is computed, but we can validate the conditions that affect it
        if (value.Status == UploadStatus.Completed && value.CanRetry())
        {
            errors.Add("CanRetry should be false when Status is Completed.");
        }

        if (value.AttemptCount >= value.MaxRetries && value.CanRetry())
        {
            errors.Add("CanRetry should be false when AttemptCount >= MaxRetries.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UploadJob"/> instance is valid.
    /// </summary>
    /// <param name="value">The upload job to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadJob value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UploadJob"/> instance is valid.
    /// </summary>
    /// <param name="value">The upload job to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this UploadJob value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"UploadJob is invalid. Validation errors: {string.Join(", ", errors)}");
    }
}