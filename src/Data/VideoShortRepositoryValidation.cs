// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="VideoShortRepository"/> instances.
/// </summary>
public static class VideoShortRepositoryValidation
{
    /// <summary>
    /// Validates the specified <see cref="VideoShortRepository"/> instance.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this VideoShortRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Note: We cannot validate the repository itself beyond null check
        // as it's just a wrapper around DatabaseContext.
        // Validation of VideoShort entities happens through the entity's own validation.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoShortRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns><c>true</c> if the repository instance is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this VideoShortRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="VideoShortRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the repository instance is not valid.</exception>
    public static void EnsureValid(this VideoShortRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoShortRepository validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="VideoShort"/> entity.
    /// </summary>
    /// <param name="entity">The video short entity to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this VideoShort entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(entity.Title))
        {
            problems.Add("Title is required and cannot be empty or whitespace.");
        }
        else if (entity.Title.Length > 100)
        {
            problems.Add("Title must be 100 characters or less.");
        }

        if (entity.Description.Length > 5000)
        {
            problems.Add("Description must be 5000 characters or less.");
        }

        if (string.IsNullOrWhiteSpace(entity.FilePath))
        {
            problems.Add("FilePath is required and cannot be empty or whitespace.");
        }

        if (entity.Duration.TotalSeconds < 1 || entity.Duration.TotalSeconds > 60)
        {
            problems.Add("Duration must be between 1 and 60 seconds.");
        }

        if (entity.FileSizeBytes <= 0)
        {
            problems.Add("FileSizeBytes must be a positive number.");
        }

        if (entity.Quality == default)
        {
            problems.Add("Quality must be a valid enum value.");
        }

        if (entity.Status == default)
        {
            problems.Add("Status must be a valid enum value.");
        }

        if (entity.ProcessingProfileId <= 0)
        {
            problems.Add("ProcessingProfileId must be a positive integer.");
        }

        if (entity.YouTubeChannelId <= 0)
        {
            problems.Add("YouTubeChannelId must be a positive integer.");
        }

        if (entity.CreatedAt == default)
        {
            problems.Add("CreatedAt must be a valid DateTime.");
        }

        if (entity.UpdatedAt == default)
        {
            problems.Add("UpdatedAt must be a valid DateTime.");
        }

        if (entity.ProcessedAt.HasValue && entity.ProcessedAt.Value == default)
        {
            problems.Add("ProcessedAt must be a valid DateTime if set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoShort"/> entity is valid.
    /// </summary>
    /// <param name="entity">The video short entity to validate.</param>
    /// <returns><c>true</c> if the entity is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this VideoShort entity)
    {
        return Validate(entity).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="VideoShort"/> entity is valid.
    /// </summary>
    /// <param name="entity">The video short entity to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the entity is not valid.</exception>
    public static void EnsureValid(this VideoShort entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var problems = Validate(entity);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoShort validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}