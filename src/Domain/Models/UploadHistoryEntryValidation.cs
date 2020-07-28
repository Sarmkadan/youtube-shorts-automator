using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="UploadHistoryEntry"/> instances.
/// </summary>
public static class UploadHistoryEntryValidation
{
    /// <summary>
    /// Validates the specified <see cref="UploadHistoryEntry"/> value.
    /// </summary>
    /// <param name="value">The upload history entry to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadHistoryEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Id must be a positive integer, but was {value.Id}.");
        }

        // Validate VideoFileName
        if (string.IsNullOrWhiteSpace(value.VideoFileName))
        {
            errors.Add("VideoFileName cannot be null or whitespace.");
        }
        else if (value.VideoFileName.Length > 500)
        {
            errors.Add("VideoFileName cannot exceed 500 characters.");
        }

        // Validate YouTubeVideoId (optional but must be valid if provided)
        if (!string.IsNullOrEmpty(value.YouTubeVideoId))
        {
            if (value.YouTubeVideoId.Length != 11)
            {
                errors.Add("YouTubeVideoId must be exactly 11 characters long if provided.");
            }
            else if (!IsValidYouTubeVideoId(value.YouTubeVideoId))
            {
                errors.Add("YouTubeVideoId contains invalid characters.");
            }
        }

        // Validate UploadedAt
        if (value.UploadedAt == default)
        {
            errors.Add("UploadedAt cannot be the default DateTime value.");
        }
        else if (value.UploadedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("UploadedAt cannot be in the future.");
        }

        // Validate Status
        if (!Enum.IsDefined(typeof(UploadHistoryStatus), value.Status))
        {
            errors.Add($"Status must be a valid UploadHistoryStatus value, but was {(int)value.Status}.");
        }

        // Validate ErrorMessage (optional but must be valid if provided)
        if (!string.IsNullOrEmpty(value.ErrorMessage))
        {
            if (value.ErrorMessage.Length > 1000)
            {
                errors.Add("ErrorMessage cannot exceed 1000 characters.");
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }
        else if (value.CreatedAt > value.UploadedAt)
        {
            errors.Add("CreatedAt cannot be after UploadedAt.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UploadHistoryEntry"/> value is valid.
    /// </summary>
    /// <param name="value">The upload history entry to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadHistoryEntry value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UploadHistoryEntry"/> value is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The upload history entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this UploadHistoryEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UploadHistoryEntry is invalid. Problems: {string.Join(" ", errors)}");
        }
    }

    private static bool IsValidYouTubeVideoId(string videoId)
    {
        if (videoId.Length != 11)
        {
            return false;
        }

        foreach (var c in videoId)
        {
            if (!IsValidYouTubeVideoIdChar(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidYouTubeVideoIdChar(char c)
    {
        return (c >= 'A' && c <= 'Z') ||
               (c >= 'a' && c <= 'z') ||
               (c >= '0' && c <= '9') ||
               c == '_' ||
               c == '-';
    }
}