// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="VideoProcessingServiceTests"/>.
/// </summary>
public static class VideoProcessingServiceTestsExtensions
{
    /// <summary>
    /// Asserts that the task has the expected status.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="task">The task to check.</param>
    /// <param name="expectedStatus">The expected status.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="task"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the status does not match.</exception>
    public static void AssertProcessingTaskStatus(this VideoProcessingServiceTests tests, ProcessingTask task, ProcessingStatus expectedStatus)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status != expectedStatus)
        {
            throw new InvalidOperationException($"Expected status {expectedStatus}, but found {task.Status}");
        }
    }

    /// <summary>
    /// Asserts that the task has the expected type.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="task">The task to check.</param>
    /// <param name="expectedTaskType">The expected task type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="task"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expectedTaskType"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the task type does not match.</exception>
    public static void AssertProcessingTaskType(this VideoProcessingServiceTests tests, ProcessingTask task, string expectedTaskType)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrEmpty(expectedTaskType);

        if (task.TaskType != expectedTaskType)
        {
            throw new InvalidOperationException($"Expected task type {expectedTaskType}, but found {task.TaskType}");
        }
    }

    /// <summary>
    /// Asserts that the created at time is recent (within 5 seconds).
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="createdAt">The created at time.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the created at time is not recent.</exception>
    public static void AssertTaskCreatedAtIsRecent(this VideoProcessingServiceTests tests, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(tests);

        if ((DateTime.UtcNow - createdAt).TotalSeconds > 5)
        {
            throw new InvalidOperationException($"Created at time {createdAt} is not recent.");
        }
    }
}
