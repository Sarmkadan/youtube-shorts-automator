// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Domain.Models;

public static class ProcessingTaskExtensions
{
    /// <summary>
    /// Calculates the estimated completion percentage based on task status and elapsed time.
    /// Returns 0 for pending/queued tasks, 50 for processing tasks, and 100 for completed/failed/uploaded tasks.
    /// </summary>
    public static int GetCompletionPercentage(this ProcessingTask task)
    {
        return task.Status switch
        {
            ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Uploaded => 100,
            ProcessingStatus.Processing => 50,
            ProcessingStatus.Pending or ProcessingStatus.Queued or ProcessingStatus.OnHold => 0,
            ProcessingStatus.Cancelled => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Determines if the task is currently in a processing state (Processing, Queued, or OnHold).
    /// </summary>
    public static bool IsProcessing(this ProcessingTask task)
    {
        return task.Status is ProcessingStatus.Processing or ProcessingStatus.Queued or ProcessingStatus.OnHold;
    }

    /// <summary>
    /// Determines if the task is in a terminal state (Completed, Failed, Uploaded, or Cancelled).
    /// </summary>
    public static bool IsTerminal(this ProcessingTask task)
    {
        return task.Status is ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Uploaded or ProcessingStatus.Cancelled;
    }

    /// <summary>
    /// Gets the duration of the task in a human-readable format.
    /// Returns "Not started" for tasks without StartedAt, and formatted time for completed tasks.
    /// </summary>
    public static string GetDurationDisplay(this ProcessingTask task)
    {
        if (task.StartedAt == default)
        {
            return "Not started";
        }

        if (task.CompletedAt.HasValue && task.ElapsedTime.HasValue)
        {
            return $"{task.ElapsedTime.Value.TotalSeconds:F2}s";
        }

        if (task.Status == ProcessingStatus.Processing)
        {
            var currentDuration = DateTime.UtcNow - task.StartedAt;
            return $"{currentDuration.TotalSeconds:F2}s (in progress)";
        }

        return "N/A";
    }

    /// <summary>
    /// Updates the task priority by a relative delta.
    /// </summary>
    /// <param name="task">The processing task</param>
    /// <param name="delta">The priority change amount (positive or negative)</param>
    public static void AdjustPriority(this ProcessingTask task, int delta)
    {
        task.Priority = Math.Max(1, Math.Min(10, task.Priority + delta));
        task.UpdatedAt = DateTime.UtcNow;
    }
}