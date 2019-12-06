// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Domain.Models;

public class ProcessingTask
{
    public int Id { get; set; }
    public int VideoShortId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? ElapsedTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string TaskLog { get; set; } = string.Empty;
    public int OutputWidth { get; set; }
    public int OutputHeight { get; set; }
    public int OutputBitrate { get; set; }
    public string OutputFormat { get; set; } = "mp4";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public VideoShort? VideoShort { get; set; }

    public void MarkAsStarted()
    {
        Status = ProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = ProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ElapsedTime = CompletedAt.Value - StartedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = ProcessingStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
        ElapsedTime = CompletedAt.Value - StartedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AppendLog(string logMessage)
    {
        // Appends a log message to the task log
        TaskLog += $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {logMessage}";
        UpdatedAt = DateTime.UtcNow;
    }

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

    public bool IsCompleted()
    {
        return Status == ProcessingStatus.Completed;
    }

    public bool IsFailed()
    {
        return Status == ProcessingStatus.Failed;
    }

    public void SetOutputDimensions(int width, int height)
    {
        OutputWidth = width;
        OutputHeight = height;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOutputBitrate(int bitrate)
    {
        OutputBitrate = bitrate;
        UpdatedAt = DateTime.UtcNow;
    }
}
