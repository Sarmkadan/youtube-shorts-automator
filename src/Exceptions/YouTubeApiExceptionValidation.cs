// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Represents validation helpers for YouTubeApiException.
/// </summary>
public static class YouTubeApiExceptionValidation
{
    /// <summary>
    /// Validates the YouTubeApiException instance.
    /// </summary>
    /// <param name="value">The YouTubeApiException instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this YouTubeApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.ChannelId == null)
        {
            problems.Add("ChannelId is null.");
        }
        else if (value.ChannelId < 0)
        {
            problems.Add("ChannelId is out of range.");
        }

        if (string.IsNullOrEmpty(value.ApiErrorCode))
        {
            problems.Add("ApiErrorCode is null or empty.");
        }

        if (value.HttpStatusCode == null)
        {
            problems.Add("HttpStatusCode is null.");
        }
        else if (value.HttpStatusCode < 0)
        {
            problems.Add("HttpStatusCode is out of range.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the YouTubeApiException instance is valid.
    /// </summary>
    /// <param name="value">The YouTubeApiException instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this YouTubeApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures the YouTubeApiException instance is valid.
    /// </summary>
    /// <param name="value">The YouTubeApiException instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this YouTubeApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Any())
        {
            throw new ArgumentException($"YouTubeApiException is invalid: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
