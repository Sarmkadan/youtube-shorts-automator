// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Extension methods for <see cref="UploadStatus"/> describing its lifecycle and presentation.
/// </summary>
public static class UploadStatusExtensions
{
    /// <summary>
    /// Determines whether the status is terminal, meaning no further state transitions are expected
    /// (<see cref="UploadStatus.Completed"/>, <see cref="UploadStatus.Failed"/>, <see cref="UploadStatus.Cancelled"/>).
    /// </summary>
    /// <param name="status">The upload status to check.</param>
    /// <returns><c>true</c> if the status is terminal; otherwise <c>false</c>.</returns>
    public static bool IsTerminal(this UploadStatus status)
    {
        return status is UploadStatus.Completed or UploadStatus.Failed or UploadStatus.Cancelled;
    }

    /// <summary>
    /// Determines whether the status represents a successful upload.
    /// </summary>
    /// <param name="status">The upload status to check.</param>
    /// <returns><c>true</c> if the status is <see cref="UploadStatus.Completed"/>; otherwise <c>false</c>.</returns>
    public static bool IsSuccessful(this UploadStatus status)
    {
        return status == UploadStatus.Completed;
    }

    /// <summary>
    /// Determines whether an upload with this status is actively being worked on
    /// (<see cref="UploadStatus.Queued"/>, <see cref="UploadStatus.Uploading"/>, <see cref="UploadStatus.Retrying"/>).
    /// </summary>
    /// <param name="status">The upload status to check.</param>
    /// <returns><c>true</c> if the upload is in progress; otherwise <c>false</c>.</returns>
    public static bool IsInProgress(this UploadStatus status)
    {
        return status is UploadStatus.Queued or UploadStatus.Uploading or UploadStatus.Retrying;
    }

    /// <summary>
    /// Returns a human-friendly, title-cased label for the status suitable for display in UI or notifications.
    /// </summary>
    /// <param name="status">The upload status to describe.</param>
    /// <returns>A display label such as <c>"Completed"</c> or <c>"Retrying"</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="status"/> is not a defined value.</exception>
    public static string DisplayName(this UploadStatus status)
    {
        return status switch
        {
            UploadStatus.Pending => "Pending",
            UploadStatus.Queued => "Queued",
            UploadStatus.Uploading => "Uploading",
            UploadStatus.Completed => "Completed",
            UploadStatus.Failed => "Failed",
            UploadStatus.Cancelled => "Cancelled",
            UploadStatus.Paused => "Paused",
            UploadStatus.Retrying => "Retrying",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown upload status.")
        };
    }
}
