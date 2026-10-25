// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Lifecycle states of a video processing task or short.
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// The task has been created and is waiting to be picked up.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The task is currently being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Processing finished successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Processing terminated with an error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The processed video has been uploaded.
    /// </summary>
    Uploaded = 4,

    /// <summary>
    /// The task is scheduled in the processing queue and awaiting a worker.
    /// </summary>
    Queued = 5,

    /// <summary>
    /// The task was cancelled before it completed.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// The task is paused and will not be processed until it is released.
    /// </summary>
    OnHold = 7
}
