// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for VideoShort model via VideoShortModelTests extension methods
// =====================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Tests;

public static class VideoShortModelTestsValidation
{
    /// <summary>
    /// Validates a VideoShort instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The VideoShortModelTests instance (used for extension method syntax).</param>
    /// <param name="video">The VideoShort instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> or <paramref name="video"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VideoShortModelTests _, VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var problems = new List<string>();

        // Validate Title
        if (string.IsNullOrWhiteSpace(video.Title))
        {
            problems.Add("Title is null or whitespace.");
        }
        else if (video.Title.Length > 100)
        {
            problems.Add("Title exceeds maximum length of 100 characters.");
        }

        // Validate Description
        if (video.Description.Length > 5000)
        {
            problems.Add("Description exceeds maximum length of 5000 characters.");
        }

        // Validate FilePath
        if (string.IsNullOrWhiteSpace(video.FilePath))
        {
            problems.Add("FilePath is null or whitespace.");
        }

        // Validate Duration
        if (video.Duration.TotalSeconds < 1)
        {
            problems.Add("Duration must be at least 1 second.");
        }
        else if (video.Duration.TotalSeconds > 60)
        {
            problems.Add("Duration exceeds maximum of 60 seconds.");
        }

        // Validate ProcessingProfileId
        if (video.ProcessingProfileId <= 0)
        {
            problems.Add("ProcessingProfileId must be greater than 0.");
        }

        // Validate YouTubeChannelId
        if (video.YouTubeChannelId <= 0)
        {
            problems.Add("YouTubeChannelId must be greater than 0.");
        }

        // Validate Status
        if (video.Status == ProcessingStatus.Completed && video.ProcessedAt == default)
        {
            problems.Add("ProcessedAt must be set when Status is Completed.");
        }

        // Validate ErrorMessage
        if (video.Status == ProcessingStatus.Failed && string.IsNullOrWhiteSpace(video.ErrorMessage))
        {
            problems.Add("ErrorMessage must be set when Status is Failed.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified VideoShort instance is valid.
    /// </summary>
    /// <param name="value">The VideoShortModelTests instance (used for extension method syntax).</param>
    /// <param name="video">The VideoShort instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    public static bool IsValid(this VideoShortModelTests _, VideoShort video)
    {
        return Validate(_, video).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified VideoShort instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The VideoShortModelTests instance (used for extension method syntax).</param>
    /// <param name="video">The VideoShort instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this VideoShortModelTests _, VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(video);

        var problems = Validate(_, video);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoShort validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}
