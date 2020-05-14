// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Represents an exception that occurs during scheduling.
/// </summary>
public class SchedulingException : Exception
{
    /// <summary>
    /// Gets the scheduled time of the job that failed.
    /// </summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>
    /// Gets the ID of the job that failed.
    /// </summary>
    public int? JobId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SchedulingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public SchedulingException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="jobId">The ID of the job that failed.</param>
    /// <param name="scheduledTime">The scheduled time of the job that failed.</param>
    public SchedulingException(string message, int jobId, DateTime scheduledTime)
        : base(message)
    {
        JobId = jobId;
        ScheduledTime = scheduledTime;
    }
}
