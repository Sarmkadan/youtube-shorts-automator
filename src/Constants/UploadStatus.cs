// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Lifecycle states of a video upload to YouTube.
/// </summary>
public enum UploadStatus
{
    /// <summary>
    /// The upload has been created but not yet scheduled for processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The upload is waiting in the queue for a free worker.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The video is currently being transferred to YouTube.
    /// </summary>
    Uploading = 2,

    /// <summary>
    /// The upload finished successfully.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// The upload failed and will not be retried automatically.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The upload was cancelled before it completed.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// The upload is suspended and can be resumed later.
    /// </summary>
    Paused = 6,

    /// <summary>
    /// The upload failed with a transient error and is being attempted again.
    /// </summary>
    Retrying = 7
}