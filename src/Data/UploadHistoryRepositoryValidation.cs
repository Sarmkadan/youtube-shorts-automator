// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for UploadHistoryRepository to ensure data integrity
// before database operations.
// =============================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="UploadHistoryRepository"/> to ensure
/// <see cref="UploadHistoryEntry"/> instances are valid before database operations.
/// </summary>
public static class UploadHistoryRepositoryValidation
{
    /// <summary>
    /// Validates an <see cref="UploadHistoryEntry"/> instance and returns a list of human-readable
    /// problems. Returns an empty list if the entry is valid.
    /// </summary>
    /// <param name="value">The entry to validate.</param>
    /// <returns>List of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadHistoryEntry? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate VideoFileName
        if (string.IsNullOrWhiteSpace(value.VideoFileName))
        {
            errors.Add("VideoFileName cannot be null or whitespace.");
        }
        else if (value.VideoFileName.Length > 500)
        {
            errors.Add("VideoFileName exceeds maximum length of 500 characters.");
        }

        // Validate YouTubeVideoId if present
        if (value.YouTubeVideoId is not null && value.YouTubeVideoId.Length > 50)
        {
            errors.Add("YouTubeVideoId exceeds maximum length of 50 characters.");
        }

        // Validate UploadedAt is not default (not set)
        if (value.UploadedAt == default)
        {
            errors.Add("UploadedAt must be set to a valid DateTime.");
        }

        // Validate Status is a defined enum value
        if (!Enum.IsDefined(typeof(UploadHistoryStatus), value.Status))
        {
            errors.Add("Status must be a valid UploadHistoryStatus value.");
        }

        // Validate ErrorMessage if present
        if (value.ErrorMessage is not null && value.ErrorMessage.Length > 2000)
        {
            errors.Add("ErrorMessage exceeds maximum length of 2000 characters.");
        }

        // Validate CreatedAt is not default (not set)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="UploadHistoryEntry"/> instance is valid.
    /// </summary>
    /// <param name="value">The entry to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this UploadHistoryEntry? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an <see cref="UploadHistoryEntry"/> instance is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation problems
    /// if it is not valid.
    /// </summary>
    /// <param name="value">The entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid,
    /// containing a list of all validation errors.</exception>
    public static void EnsureValid(this UploadHistoryEntry? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"UploadHistoryEntry is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}