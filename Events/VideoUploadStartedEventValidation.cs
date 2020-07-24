// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for VideoUploadStartedEvent to ensure data integrity
// before processing or persisting events in the pipeline.
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Events;

/// <summary>
/// Provides validation helpers for <see cref="VideoUploadStartedEvent"/> instances.
/// Ensures that event data meets business rules and technical constraints
/// before processing continues.
/// </summary>
public static class VideoUploadStartedEventValidation
{
    /// <summary>
    /// Validates the specified <see cref="VideoUploadStartedEvent"/> instance.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <returns>A list of validation problems; empty if the event is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VideoUploadStartedEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate VideoId
        if (value.VideoId == Guid.Empty)
        {
            problems.Add("VideoId must not be empty (Guid.Empty).");
        }

        // Validate FileName
        if (string.IsNullOrWhiteSpace(value.FileName))
        {
            problems.Add("FileName must not be null, empty, or whitespace.");
        }
        else if (value.FileName.Length > 500)
        {
            problems.Add("FileName must not exceed 500 characters.");
        }

        // Validate FileSizeBytes
        if (value.FileSizeBytes <= 0)
        {
            problems.Add("FileSizeBytes must be a positive number greater than zero.");
        }
        else if (value.FileSizeBytes > 1073741824) // 1GB
        {
            problems.Add("FileSizeBytes must not exceed 1GB (1073741824 bytes).");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            problems.Add("Title must not be null, empty, or whitespace.");
        }
        else if (value.Title.Length > 200)
        {
            problems.Add("Title must not exceed 200 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoUploadStartedEvent"/> is valid.
    /// </summary>
    /// <param name="value">The event to check.</param>
    /// <returns>True if the event is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VideoUploadStartedEvent value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="VideoUploadStartedEvent"/> is valid.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the event is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this VideoUploadStartedEvent value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"VideoUploadStartedEvent is invalid. Problems: {string.Join("; ", problems)}",
            nameof(value)
        );
    }
}