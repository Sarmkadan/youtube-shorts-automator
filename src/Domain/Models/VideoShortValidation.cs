using System.Globalization;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="VideoShort"/> instances.
/// </summary>
public static class VideoShortValidation
{
    /// <summary>
    /// Validates the specified video short and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The video short to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the video short is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this VideoShort value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Title cannot be null or whitespace.");
        }
        else if (value.Title.Length > 100)
        {
            errors.Add("Title cannot exceed 100 characters.");
        }

        // Validate Description
        if (value.Description.Length > 5000)
        {
            errors.Add("Description cannot exceed 5000 characters.");
        }

        // Validate FilePath
        if (string.IsNullOrWhiteSpace(value.FilePath))
        {
            errors.Add("FilePath cannot be null or whitespace.");
        }

        // Validate ThumbnailPath
        if (string.IsNullOrWhiteSpace(value.ThumbnailPath))
        {
            errors.Add("ThumbnailPath cannot be null or whitespace.");
        }

        // Validate Duration (must be between 1 second and 60 seconds for YouTube Shorts)
        if (value.Duration.TotalSeconds < 1 || value.Duration.TotalSeconds > 60)
        {
            errors.Add("Duration must be between 1 and 60 seconds.");
        }

        // Validate FileSizeBytes (must be positive)
        if (value.FileSizeBytes <= 0)
        {
            errors.Add("FileSizeBytes must be a positive value.");
        }

        // Validate Quality (must be a defined enum value)
        if (!Enum.IsDefined(typeof(VideoQuality), value.Quality))
        {
            errors.Add("Quality must be a valid VideoQuality value.");
        }

        // Validate Status (must be a defined enum value)
        if (!Enum.IsDefined(typeof(ProcessingStatus), value.Status))
        {
            errors.Add("Status must be a valid ProcessingStatus value.");
        }

        // Validate Tags (must not be null, individual tags must not be null or whitespace)
        if (value.Tags is null)
        {
            errors.Add("Tags collection cannot be null.");
        }
        else
        {
            for (int i = 0; i < value.Tags.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(value.Tags[i]))
                {
                    errors.Add($"Tags[{i}] cannot be null or whitespace.");
                }
            }
        }

        // Validate ProcessingProfileId (must be positive)
        if (value.ProcessingProfileId <= 0)
        {
            errors.Add("ProcessingProfileId must be a positive integer.");
        }

        // Validate YouTubeChannelId (must be positive)
        if (value.YouTubeChannelId <= 0)
        {
            errors.Add("YouTubeChannelId must be a positive integer.");
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

        // Validate ProcessedAt (if set, must not be default DateTime)
        if (value.ProcessedAt.HasValue && value.ProcessedAt.Value == default)
        {
            errors.Add("ProcessedAt, if set, cannot be the default DateTime value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified video short is valid.
    /// </summary>
    /// <param name="value">The video short to check.</param>
    /// <returns><see langword="true"/> if the video short is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this VideoShort value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified video short is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The video short to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the video short is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this VideoShort value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"VideoShort is invalid. Validation errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}