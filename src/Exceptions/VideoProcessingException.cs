// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

public class VideoProcessingException : Exception
{
    public int? VideoId { get; set; }
    public string? ProcessingTaskId { get; set; }
    public string? ErrorCode { get; set; }

    public VideoProcessingException(string message) : base(message) { }

    public VideoProcessingException(string message, Exception innerException) 
        : base(message, innerException) { }

    public VideoProcessingException(string message, int videoId, string? errorCode = null)
        : base(message)
    {
        VideoId = videoId;
        ErrorCode = errorCode;
    }

    public VideoProcessingException(string message, int videoId, string? taskId, Exception? innerException)
        : base(message, innerException)
    {
        VideoId = videoId;
        ProcessingTaskId = taskId;
    }
}
