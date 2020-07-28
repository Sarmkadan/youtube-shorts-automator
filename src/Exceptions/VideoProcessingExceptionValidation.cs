// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Provides validation helpers for the <see cref="VideoProcessingException"/> class.
/// </summary>
public static class VideoProcessingExceptionValidation
{
    /// <summary>
    /// Validates the properties of the provided <see cref="VideoProcessingException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of human-readable validation issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VideoProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var issues = new List<string>();

        if (value.ProcessingTaskId is not null && string.IsNullOrEmpty(value.ProcessingTaskId))
        {
            issues.Add("ProcessingTaskId must not be empty when provided.");
        }

        if (value.ErrorCode is not null && string.IsNullOrEmpty(value.ErrorCode))
        {
            issues.Add("ErrorCode must not be empty when provided.");
        }

        return issues;
    }

    /// <summary>
    /// Determines whether the provided <see cref="VideoProcessingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VideoProcessingException value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures the provided <see cref="VideoProcessingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation issues are found.</exception>
    public static void EnsureValid(this VideoProcessingException value)
    {
        var issues = Validate(value);
        if (issues.Any())
        {
            throw new ArgumentException(string.Join("; ", issues));
        }
    }
}
