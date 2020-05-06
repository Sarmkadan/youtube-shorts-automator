// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="ProcessingTask"/> to simplify common task operations and calculations.
/// </summary>
public static class ProcessingTaskExtensions
{
	/// <summary>
	/// Calculates the estimated completion percentage based on task status.
	/// Returns 100 for terminal states (Completed, Failed, Uploaded), 50 for active processing,
	/// and 0 for pending/queued/on-hold states.
	/// </summary>
	/// <param name="task">The processing task.</param>
	/// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
	public static int GetCompletionPercentage(this ProcessingTask task)
	{
		ArgumentNullException.ThrowIfNull(task);

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
	/// Determines if the task is currently in an active processing state (Processing, Queued, or OnHold).
	/// </summary>
	/// <param name="task">The processing task.</param>
	/// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
	public static bool IsProcessing(this ProcessingTask task)
	{
		ArgumentNullException.ThrowIfNull(task);

		return task.Status is ProcessingStatus.Processing or ProcessingStatus.Queued or ProcessingStatus.OnHold;
	}

	/// <summary>
	/// Determines if the task is in a terminal state (Completed, Failed, Uploaded, or Cancelled).
	/// </summary>
	/// <param name="task">The processing task.</param>
	/// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
	public static bool IsTerminal(this ProcessingTask task)
	{
		ArgumentNullException.ThrowIfNull(task);

		return task.Status is ProcessingStatus.Completed or ProcessingStatus.Failed or ProcessingStatus.Uploaded or ProcessingStatus.Cancelled;
	}

	/// <summary>
	/// Gets the duration of the task in a human-readable format.
	/// Returns "Not started" for tasks without StartedAt, formatted duration for completed tasks,
	/// current duration with "(in progress)" suffix for active processing tasks, and "N/A" for other states.
	/// </summary>
	/// <param name="task">The processing task.</param>
	/// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
	public static string GetDurationDisplay(this ProcessingTask task)
	{
		ArgumentNullException.ThrowIfNull(task);

		if (task.StartedAt == default)
		{
			return "Not started";
		}

		if (task.CompletedAt.HasValue)
		{
			// Calculate duration from timestamps when CompletedAt is available
			var duration = task.CompletedAt.Value - task.StartedAt;
			return $"{duration.TotalSeconds:F2}s";
		}

		if (task.Status == ProcessingStatus.Processing)
		{
			// Calculate current duration for in-progress tasks
			var currentDuration = DateTime.UtcNow - task.StartedAt;
			return $"{currentDuration.TotalSeconds:F2}s (in progress)";
		}

		return "N/A";
	}

	/// <summary>
	/// Updates the task priority by a relative delta, ensuring it stays within the valid range [1, 10].
	/// Also updates the UpdatedAt timestamp to mark when the priority was last modified.
	/// </summary>
	/// <param name="task">The processing task.</param>
	/// <param name="delta">The priority change amount (positive or negative).</param>
	/// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
	public static void AdjustPriority(this ProcessingTask task, int delta)
	{
		ArgumentNullException.ThrowIfNull(task);

		task.Priority = Math.Max(1, Math.Min(10, task.Priority + delta));
		task.UpdatedAt = DateTime.UtcNow;
	}
}
