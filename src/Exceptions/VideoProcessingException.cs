// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Represents an exception that occurs during video processing.
/// </summary>
public class VideoProcessingException : Exception
{
    /// <summary>
    /// Gets the ID of the video that caused the exception.
    /// </summary>
    public int? VideoId { get; set; }

    /// <summary>
    /// Gets the ID of the processing task that caused the exception.
    /// </summary>
    public string? ProcessingTaskId { get; set; }

    /// <summary>
    /// Gets the error code that caused the exception.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public VideoProcessingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception that caused the error.</param>
    public VideoProcessingException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="videoId">The ID of the video that caused the exception.</param>
    /// <param name="errorCode">The error code that caused the exception.</param>
    public VideoProcessingException(string message, int videoId, string? errorCode = null)
        : base(message)
    {
        VideoId = videoId;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoProcessingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="videoId">The ID of the video that caused the exception.</param>
    /// <param name="taskId">The ID of the processing task that caused the exception.</param>
    /// <param name="innerException">The inner exception that caused the error.</param>
    public VideoProcessingException(string message, int videoId, string? taskId, Exception? innerException)
        : base(message, innerException)
    {
        VideoId = videoId;
        ProcessingTaskId = taskId;
    }
}
