// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>Status values for an upload history entry.</summary>
public enum UploadHistoryStatus
{
    Success = 0,
    Failed = 1,
    Skipped = 2
}

/// <summary>
/// Immutable record of a single upload attempt, persisted to the UploadHistory table.
/// </summary>
public class UploadHistoryEntry
{
    public int Id { get; set; }

    /// <summary>Original file name of the video that was uploaded (or attempted).</summary>
    public string VideoFileName { get; set; } = string.Empty;

    /// <summary>YouTube video ID assigned after a successful upload; null otherwise.</summary>
    public string? YouTubeVideoId { get; set; }

    /// <summary>UTC timestamp of the upload attempt.</summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>Outcome of the upload attempt.</summary>
    public UploadHistoryStatus Status { get; set; }

    /// <summary>Error detail when Status is Failed; null otherwise.</summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
