using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="UploadResult"/> instances.
/// </summary>
public static class UploadResultValidation
{
    /// <summary>
    /// Validates an <see cref="UploadResult"/> instance.
    /// </summary>
    /// <param name="value">The upload result to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Id must be a positive integer, but was {value.Id}.");
        }

        // Validate UploadJobId
        if (value.UploadJobId <= 0)
        {
            errors.Add($"UploadJobId must be a positive integer, but was {value.UploadJobId}.");
        }

        // Validate VideoId
        if (string.IsNullOrWhiteSpace(value.VideoId))
        {
            errors.Add("VideoId cannot be null or whitespace.");
        }
        else if (value.VideoId.Length > 100)
        {
            errors.Add($"VideoId exceeds maximum length of 100 characters, but was {value.VideoId.Length}.");
        }

        // Validate VideoUrl
        if (string.IsNullOrWhiteSpace(value.VideoUrl))
        {
            errors.Add("VideoUrl cannot be null or whitespace.");
        }
        else if (!Uri.IsWellFormedUriString(value.VideoUrl, UriKind.Absolute))
        {
            errors.Add($"VideoUrl must be a well-formed absolute URI, but was '{value.VideoUrl}'.");
        }

        // Validate IsSuccessful
        // No validation needed - boolean can always be valid

        // Validate ErrorDetails (only if IsSuccessful is false)
        if (!value.IsSuccessful && string.IsNullOrWhiteSpace(value.ErrorDetails))
        {
            errors.Add("ErrorDetails must be provided when IsSuccessful is false.");
        }

        // Validate UploadedBytes
        if (value.UploadedBytes < 0)
        {
            errors.Add($"UploadedBytes cannot be negative, but was {value.UploadedBytes}.");
        }

        if (value.TotalBytes > 0 && value.UploadedBytes > value.TotalBytes)
        {
            errors.Add($"UploadedBytes ({value.UploadedBytes}) cannot exceed TotalBytes ({value.TotalBytes}).");
        }

        // Validate TotalBytes
        if (value.TotalBytes < 0)
        {
            errors.Add($"TotalBytes cannot be negative, but was {value.TotalBytes}.");
        }

        // Validate UploadDuration
        if (value.UploadDuration < TimeSpan.Zero)
        {
            errors.Add($"UploadDuration cannot be negative, but was {value.UploadDuration}.");
        }

        // Validate AverageUploadSpeed
        if (double.IsNaN(value.AverageUploadSpeed) || double.IsInfinity(value.AverageUploadSpeed))
        {
            errors.Add($"AverageUploadSpeed must be a valid number, but was {value.AverageUploadSpeed}.");
        }
        else if (value.AverageUploadSpeed < 0)
        {
            errors.Add($"AverageUploadSpeed cannot be negative, but was {value.AverageUploadSpeed}.");
        }

        // Validate CompletedAt
        if (value.CompletedAt == default)
        {
            errors.Add("CompletedAt cannot be the default DateTime value.");
        }
        else if (value.CompletedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add($"CompletedAt cannot be in the future, but was {value.CompletedAt:O}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="UploadResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The upload result to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadResult value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="UploadResult"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The upload result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this UploadResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UploadResult is not valid. Problems: {string.Join(" ", errors)}");
        }
    }
}