// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

public enum UploadStatus
{
    Pending = 0,
    Queued = 1,
    Uploading = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Paused = 6,
    Retrying = 7
}
