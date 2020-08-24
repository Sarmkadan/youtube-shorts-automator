// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="VideoProcessingException"/>.
/// </summary>
public static class VideoProcessingExceptionExtensions
{
    /// <summary>
    /// Determines whether the <see cref="VideoProcessingException"/> contains information about the video ID.
    /// </summary>
    /// <param name="exception">The <see cref="VideoProcessingException"/> to check.</param>
    /// <returns><c>true</c> if the exception contains a video ID; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool HasVideoId(this VideoProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.VideoId.HasValue;
    }

    /// <summary>
    /// Determines whether the <see cref="VideoProcessingException"/> contains information about the processing task ID.
    /// </summary>
    /// <param name="exception">The <see cref="VideoProcessingException"/> to check.</param>
    /// <returns><c>true</c> if the exception contains a processing task ID; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool HasProcessingTaskId(this VideoProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return !string.IsNullOrEmpty(exception.ProcessingTaskId);
    }

    /// <summary>
    /// Gets a string that represents the error details of the <see cref="VideoProcessingException"/>.
    /// </summary>
    /// <param name="exception">The <see cref="VideoProcessingException"/> to get the error details from.</param>
    /// <returns>A string that represents the error details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static string GetErrorDetails(this VideoProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var details = new System.Text.StringBuilder(exception.Message);

        if (exception.HasVideoId())
        {
            details.Append($" Video ID: {exception.VideoId}");
        }

        if (exception.HasProcessingTaskId())
        {
            details.Append($" Processing Task ID: {exception.ProcessingTaskId}");
        }

        if (!string.IsNullOrEmpty(exception.ErrorCode))
        {
            details.Append($" Error Code: {exception.ErrorCode}");
        }

        return details.ToString();
    }
}
