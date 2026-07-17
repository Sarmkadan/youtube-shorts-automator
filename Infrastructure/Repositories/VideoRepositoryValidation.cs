// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a VideoRepository instance is valid
    /// </summary>
    /// <param name="value">The VideoRepository instance to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this VideoRepository? value) => Validate(value).Count == 0;

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
    /// <exception cref="ArgumentException">Thrown if userId is Guid.Empty</exception>
    public static IReadOnlyList<string> ValidateVideoUserId(this Guid userId)
    {
        return userId == Guid.Empty
            ? ["User ID cannot be empty (Guid.Empty)"]
            : Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetByStatusAsync parameters
    /// </summary>
    /// <param name="status">The video status to filter by</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this VideoStatus status) => Array.Empty<string>();

    /// <summary>
    /// Validates GetPendingVideosAsync parameters
    /// </summary>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate() => Array.Empty<string>();

    /// <summary>
    /// Validates GetRecentVideosAsync parameters
    /// </summary>
    /// <param name="userId">The user ID to query by</param>
    /// <param name="daysBack">Number of days to look back</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if userId is Guid.Empty or daysBack is not positive</exception>
    public static IReadOnlyList<string> ValidateVideoRecentVideos(this Guid userId, int daysBack)
    {
        if (userId == Guid.Empty)
            return ["User ID cannot be empty (Guid.Empty)"];

        if (daysBack <= 0)
            return ["Days back must be greater than zero"];

        if (daysBack > 365 * 2) // 2 years max
            return ["Days back cannot exceed 2 years"];

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetByYouTubeIdAsync parameters
    /// </summary>
    /// <param name="youtubeId">The YouTube video ID to search for</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if youtubeId is null</exception>
    /// <exception cref="ArgumentException">Thrown if youtubeId is empty or contains invalid characters</exception>
    public static IReadOnlyList<string> Validate(this string youtubeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(youtubeId);

        if (youtubeId.Length < 11 || youtubeId.Length > 50)
            return ["YouTube video ID must be between 11 and 50 characters"];

        if (!youtubeId.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            return ["YouTube video ID contains invalid characters (only letters, digits, underscores, and hyphens are allowed)"];

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetPaginatedAsync parameters
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if pageNumber is less than 1 or pageSize is invalid</exception>
    public static IReadOnlyList<string> Validate(this int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            return ["Page number must be 1 or greater"];

        if (pageSize < 1)
            return ["Page size must be 1 or greater"];

        if (pageSize > 1000)
            return ["Page size cannot exceed 1000 items"];

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates GetUserVideosPaginatedAsync parameters
    /// </summary>
    /// <param name="userId">The user ID to query by</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if userId is Guid.Empty or page parameters are invalid</exception>
    public static IReadOnlyList<string> ValidateVideoUserVideosPaginated(this Guid userId, int pageNumber, int pageSize)
    {
        if (userId == Guid.Empty)
            return ["User ID cannot be empty (Guid.Empty)"];

        if (pageNumber < 1)
            return ["Page number must be 1 or greater"];

        if (pageSize < 1)
            return ["Page size must be 1 or greater"];

        if (pageSize > 1000)
            return ["Page size cannot exceed 1000 items"];

        return Array.Empty<string>();
    }
}