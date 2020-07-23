// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Validation helpers for VideoRepository to ensure repository operations receive valid parameters
/// </summary>
public static class VideoRepositoryValidation
{
    /// <summary>
    /// Validates a VideoRepository instance
    /// </summary>
    /// <param name="value">The VideoRepository instance to validate</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this VideoRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // VideoRepository inherits from Repository<TEntity>
        // The base Repository class requires context and logger in its constructor
        // We can't directly validate the protected fields, but we can verify the instance is not null
        // which is the primary validation concern for repository instances

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a VideoRepository instance is valid
    /// </summary>
    /// <param name="value">The VideoRepository instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this VideoRepository? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a VideoRepository instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The VideoRepository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed error messages</exception>
    public static void EnsureValid(this VideoRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"VideoRepository is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates GetByUserIdAsync parameters
    /// </summary>
    /// <param name="userId">The user ID to query by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> ValidateVideoUserId(this Guid userId)
    {
        var errors = new List<string>();

        // Guid.Empty is technically valid but may indicate uninitialized state
        // We'll treat it as a potential issue for repository operations
        if (userId == Guid.Empty)
        {
            errors.Add("User ID cannot be empty (Guid.Empty)");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetByStatusAsync parameters
    /// </summary>
    /// <param name="status">The video status to filter by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this VideoStatus status)
    {
        // All VideoStatus enum values are valid by design
        // No validation needed beyond null check (which is handled by the enum type itself)
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetPendingVideosAsync parameters
    /// </summary>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate()
    {
        // GetPendingVideosAsync has no parameters
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetRecentVideosAsync parameters
    /// </summary>
    /// <param name="userId">The user ID to query by</param>
    /// <param name="daysBack">Number of days to look back</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> ValidateVideoRecentVideos(this Guid userId, int daysBack)
    {
        var errors = new List<string>();

        if (userId == Guid.Empty)
        {
            errors.Add("User ID cannot be empty (Guid.Empty)");
        }

        if (daysBack <= 0)
        {
            errors.Add("Days back must be greater than zero");
        }
        else if (daysBack > 365 * 2) // 2 years max
        {
            errors.Add("Days back cannot exceed 2 years");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetByYouTubeIdAsync parameters
    /// </summary>
    /// <param name="youtubeId">The YouTube video ID to search for</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this string youtubeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(youtubeId);

        var errors = new List<string>();

        // YouTube video IDs are typically 11 characters (e.g., dQw4w9WgXc)
        // but can be longer in some cases
        if (youtubeId.Length < 11 || youtubeId.Length > 50)
        {
            errors.Add("YouTube video ID must be between 11 and 50 characters");
        }

        // YouTube IDs typically contain only alphanumeric characters and underscores
        if (!youtubeId.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
        {
            errors.Add("YouTube video ID contains invalid characters (only letters, digits, underscores, and hyphens are allowed)");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetPaginatedAsync parameters
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this int pageNumber, int pageSize)
    {
        var errors = new List<string>();

        if (pageNumber < 1)
        {
            errors.Add("Page number must be 1 or greater");
        }

        if (pageSize < 1)
        {
            errors.Add("Page size must be 1 or greater");
        }
        else if (pageSize > 1000)
        {
            errors.Add("Page size cannot exceed 1000 items");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates GetUserVideosPaginatedAsync parameters
    /// </summary>
    /// <param name="userId">The user ID to query by</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> ValidateVideoUserVideosPaginated(this Guid userId, int pageNumber, int pageSize)
    {
        var errors = new List<string>();

        if (userId == Guid.Empty)
        {
            errors.Add("User ID cannot be empty (Guid.Empty)");
        }

        if (pageNumber < 1)
        {
            errors.Add("Page number must be 1 or greater");
        }

        if (pageSize < 1)
        {
            errors.Add("Page size must be 1 or greater");
        }
        else if (pageSize > 1000)
        {
            errors.Add("Page size cannot exceed 1000 items");
        }

        return errors.AsReadOnly();
    }
}