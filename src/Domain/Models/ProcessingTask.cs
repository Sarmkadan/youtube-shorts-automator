using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a task that processes a <see cref="VideoShort"/>.
/// </summary>
public class ProcessingTask
{
    /// <summary>
    /// Gets or sets the unique identifier of the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the <see cref="VideoShort"/> that this task processes.
    /// </summary>
    public int VideoShortId { get; set; }

    /// <summary>
    /// Gets or sets the type of the task (e.g., "Encode", "Thumbnail").
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the task.
    /// </summary>
    public ProcessingStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the priority of the task. Valid values are 1 (lowest) to 10 (highest).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the task was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the task was completed, if applicable.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time between <see cref="StartedAt"/> and <see cref="CompletedAt"/>.
    /// </summary>
    public TimeSpan? ElapsedTime { get; set; }

    /// <summary>
    /// Gets or sets the error message if the task failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a log of messages produced during task execution.
    /// </summary>
    public string TaskLog { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output width in pixels for the processed media.
    /// </summary>
    public int OutputWidth { get; set; }

    /// <summary>
    /// Gets or sets the output height in pixels for the processed media.
    /// </summary>
    public int OutputHeight { get; set; }

    /// <summary>
    /// Gets or sets the output bitrate in kbps for the processed media.
    /// </summary>
    public int OutputBitrate { get; set; }

    /// <summary>
    /// Gets or sets the output format (e.g., "mp4").
    /// </summary>
    public string OutputFormat { get; set; } = "mp4";

    /// <summary>
    /// Gets or sets the UTC timestamp when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the task was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to the <see cref="VideoShort"/> being processed.
    /// </summary>
    public VideoShort? VideoShort { get; set; }

    /// <summary>
    /// Marks the task as started, setting its status to <see cref="ProcessingStatus.Processing"/> and recording the start time.
    /// </summary>
    public void MarkAsStarted()
    {
        Status = ProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the task as completed, setting its status to <see cref="ProcessingStatus.Completed"/>, recording the completion time,
    /// calculating the elapsed time, and updating the timestamp.
    /// </summary>
    public void MarkAsCompleted()
    {
        Status = ProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ElapsedTime = CompletedAt.Value - StartedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the task as failed, setting its status to <see cref="ProcessingStatus.Failed"/>, recording the error message,
    /// completion time, elapsed time, and updating the timestamp.
    /// </summary>
    /// <param name="error">The error message describing why the task failed.</param>
    public void MarkAsFailed(string error)
    {
        Status = ProcessingStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
        ElapsedTime = CompletedAt.Value - StartedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Appends a log message to the task log, prefixed with the current UTC timestamp.
    /// </summary>
    /// <param name="logMessage">The message to append.</param>
    public void AppendLog(string logMessage)
    {
        // Appends a log message to the task log
        TaskLog += $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logMessage}";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the task's metadata to ensure it contains all required information.
    /// </summary>
    /// <returns><c>true</c> if the task is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        // Validates processing task metadata
        if (string.IsNullOrWhiteSpace(TaskType))
            return false;
        if (VideoShortId <= 0)
            return false;
        if (Priority < 1 || Priority > 10)
            return false;
        if (OutputWidth <= 0 || OutputHeight <= 0)
            return false;
        if (OutputBitrate <= 0)
            return false;
        return true;
    }

    /// <summary>
    /// Determines whether the task has completed successfully.
    /// </summary>
    /// <returns><c>true</c> if the status is <see cref="ProcessingStatus.Completed"/>; otherwise, <c>false</c>.</returns>
    public bool IsCompleted()
    {
        return Status == ProcessingStatus.Completed;
    }

    /// <summary>
    /// Determines whether the task has failed.
    /// </summary>
    /// <returns><c>true</c> if the status is <see cref="ProcessingStatus.Failed"/>; otherwise, <c>false</c>.</returns>
    public bool IsFailed()
    {
        return Status == ProcessingStatus.Failed;
    }

    /// <summary>
    /// Sets the output dimensions for the processed media and updates the timestamp.
    /// </summary>
    /// <param name="width">The desired output width in pixels.</param>
    /// <param name="height">The desired output height in pixels.</param>
    public void SetOutputDimensions(int width, int height)
    {
        OutputWidth = width;
        OutputHeight = height;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the output bitrate for the processed media and updates the timestamp.
    /// </summary>
    /// <param name="bitrate">The desired output bitrate in kbps.</param>
    public void SetOutputBitrate(int bitrate)
    {
        OutputBitrate = bitrate;
        UpdatedAt = DateTime.UtcNow;
    }
}
