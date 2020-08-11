// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="Video"/> class
/// </summary>
public static class VideoValidation
{
    /// <summary>
    /// Validates a Video object and returns any validation errors
    /// </summary>
    /// <param name="value">The video to validate</param>
    /// <returns>Read-only list of validation errors (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this Video value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Title))
            errors.Add("Title is required");
        else if (value.Title.Length > 100)
            errors.Add("Title must not exceed 100 characters");

        if (value.Description.Length > 5000)
            errors.Add("Description must not exceed 5000 characters");

        if (string.IsNullOrWhiteSpace(value.FilePath))
            errors.Add("File path is required");

        if (string.IsNullOrWhiteSpace(value.ThumbnailPath))
            errors.Add("Thumbnail path is required");

        // Validate YouTubeVideoId if set
        if (value.YouTubeVideoId is not null && value.YouTubeVideoId.Length > 50)
            errors.Add("YouTube video ID must not exceed 50 characters");

        // Validate numeric ranges
        if (value.FileSizeBytes <= 0)
            errors.Add("File size must be greater than zero");
        else if (value.FileSizeBytes > 4_294_967_296) // 4GB limit
            errors.Add("File size exceeds maximum limit of 4GB");

        if (value.DurationSeconds <= 0)
            errors.Add("Duration must be greater than zero");
        else if (value.DurationSeconds > 3600) // 60 minutes max
            errors.Add("Duration must not exceed 60 minutes");

        // Validate tags
        if (value.Tags.Length > 500)
            errors.Add("Cannot have more than 500 tags");

        foreach (var tag in value.Tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
                errors.Add("Tag cannot be empty");
            else if (tag.Length > 30)
                errors.Add("Individual tags cannot exceed 30 characters");
        }

        // Validate dates
        if (value.CreatedAt == default)
            errors.Add("Created date must be set");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Created date cannot be in the future");

        if (value.ProcessedAt.HasValue && value.ProcessedAt.Value > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Processed date cannot be in the future");

        // Validate status
        if (!Enum.IsDefined(value.Status))
            errors.Add("Invalid video status");

        // Validate required IDs
        if (value.Id == Guid.Empty)
            errors.Add("Video ID must be set");

        if (value.UserId == Guid.Empty)
            errors.Add("User ID must be set");

        // Validate collections
        if (value.ProcessingJobs.Count > 100)
            errors.Add("Cannot have more than 100 processing jobs");

        if (value.Metrics.Count > 1000)
            errors.Add("Cannot have more than 1000 metrics");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the video data is valid
    /// </summary>
    /// <param name="value">The video to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this Video value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().IsValid;
    }

    /// <summary>
    /// Ensures the video data is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The video to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with error details</exception>
    public static void EnsureValid(this Video value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = value.Validate();
        if (!result.IsValid)
        {
            throw new ArgumentException(
                $"Video validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", result.Errors)}");
        }
    }
}