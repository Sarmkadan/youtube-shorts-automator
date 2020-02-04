// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Services;

public class SchedulingException : Exception
{
    public DateTime? ScheduledTime { get; set; }
    public int? JobId { get; set; }

    public SchedulingException(string message) : base(message) { }

    public SchedulingException(string message, Exception innerException)
        : base(message, innerException) { }

    public SchedulingException(string message, int jobId, DateTime scheduledTime)
        : base(message)
    {
        JobId = jobId;
        ScheduledTime = scheduledTime;
    }
}
