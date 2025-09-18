// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

public class UploadException : Exception
{
    public int? UploadJobId { get; set; }
    public int? VideoShortId { get; set; }
    public bool IsRetryable { get; set; }

    public UploadException(string message) : base(message) { }

    public UploadException(string message, Exception innerException)
        : base(message, innerException) { }

    public UploadException(string message, int uploadJobId, int videoShortId, bool isRetryable = true)
        : base(message)
    {
        UploadJobId = uploadJobId;
        VideoShortId = videoShortId;
        IsRetryable = isRetryable;
    }
}
