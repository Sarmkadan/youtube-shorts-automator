// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Constants;

/// <summary>
/// Extension methods for <see cref="ProcessingStatus"/> describing its lifecycle and presentation.
/// </summary>
public static class ProcessingStatusExtensions
{
    /// <summary>
    /// Determines whether the status is terminal, meaning no further processing is expected
    /// (<see cref="ProcessingStatus.Completed"/>, <see cref="ProcessingStatus.Uploaded"/>,
    /// <see cref="ProcessingStatus.Failed"/>, <see cref="ProcessingStatus.Cancelled"/>).
    /// </summary>
    /// <param name="status">The processing status to check.</param>
    /// <returns><c>true</c> if the status is terminal; otherwise <c>false</c>.</returns>
    public static bool IsTerminal(this ProcessingStatus status)
    {
        return status is ProcessingStatus.Completed or ProcessingStatus.Uploaded
            or ProcessingStatus.Failed or ProcessingStatus.Cancelled;
    }

    /// <summary>
    /// Determines whether processing finished successfully
    /// (<see cref="ProcessingStatus.Completed"/> or <see cref="ProcessingStatus.Uploaded"/>).
    /// </summary>
    /// <param name="status">The processing status to check.</param>
    /// <returns><c>true</c> if the status is a successful completion; otherwise <c>false</c>.</returns>
    public static bool IsComplete(this ProcessingStatus status)
    {
        return status is ProcessingStatus.Completed or ProcessingStatus.Uploaded;
    }

    /// <summary>
    /// Determines whether processing ended in failure (<see cref="ProcessingStatus.Failed"/>).
    /// </summary>
    /// <param name="status">The processing status to check.</param>
    /// <returns><c>true</c> if the status is <see cref="ProcessingStatus.Failed"/>; otherwise <c>false</c>.</returns>
    public static bool IsFailed(this ProcessingStatus status)
    {
        return status == ProcessingStatus.Failed;
    }

    /// <summary>
    /// Determines whether an item with this status is actively being worked on or waiting to be picked up
    /// (<see cref="ProcessingStatus.Processing"/>, <see cref="ProcessingStatus.Queued"/>).
    /// </summary>
    /// <param name="status">The processing status to check.</param>
    /// <returns><c>true</c> if the item is in progress; otherwise <c>false</c>.</returns>
    public static bool IsInProgress(this ProcessingStatus status)
    {
        return status is ProcessingStatus.Processing or ProcessingStatus.Queued;
    }

    /// <summary>
    /// Returns a human-friendly label for the status suitable for display in UI or notifications.
    /// </summary>
    /// <param name="status">The processing status to describe.</param>
    /// <returns>A display label such as <c>"Completed"</c> or <c>"On Hold"</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="status"/> is not a defined value.</exception>
    public static string DisplayName(this ProcessingStatus status)
    {
        return status switch
        {
            ProcessingStatus.Pending => "Pending",
            ProcessingStatus.Processing => "Processing",
            ProcessingStatus.Completed => "Completed",
            ProcessingStatus.Failed => "Failed",
            ProcessingStatus.Uploaded => "Uploaded",
            ProcessingStatus.Queued => "Queued",
            ProcessingStatus.Cancelled => "Cancelled",
            ProcessingStatus.OnHold => "On Hold",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown processing status.")
        };
    }
}
