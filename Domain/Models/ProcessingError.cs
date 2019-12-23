// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class ProcessingError
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public ProcessingJob? Job { get; set; }
    public ProcessingErrorType ErrorType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime OccurredAt { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Medium;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public int RetryAttemptCount { get; set; }

    /// <summary>
    /// Marks the error as resolved
    /// </summary>
    public void MarkAsResolved(string resolutionNotes)
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = resolutionNotes;
    }

    /// <summary>
    /// Records a retry attempt
    /// </summary>
    public void RecordRetryAttempt()
    {
        RetryAttemptCount++;
    }

    /// <summary>
    /// Determines if the error is critical
    /// </summary>
    public bool IsCritical()
    {
        return Severity == ErrorSeverity.Critical ||
               ErrorType == ProcessingErrorType.ValidationFailure ||
               ErrorType == ProcessingErrorType.CorruptedFile;
    }

    /// <summary>
    /// Determines if the error is retryable
    /// </summary>
    public bool IsRetryable()
    {
        return ErrorType switch
        {
            ProcessingErrorType.TemporaryFailure => true,
            ProcessingErrorType.NetworkError => true,
            ProcessingErrorType.TimeoutError => true,
            ProcessingErrorType.ResourceUnavailable => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets human-readable error message
    /// </summary>
    public string GetUserMessage()
    {
        return ErrorType switch
        {
            ProcessingErrorType.ValidationFailure => "Video failed validation. Please check the file format and metadata.",
            ProcessingErrorType.CorruptedFile => "The video file appears to be corrupted. Please try again with a different file.",
            ProcessingErrorType.TimeoutError => "Processing took too long. Please retry or use a smaller file.",
            ProcessingErrorType.NetworkError => "Network error occurred. Your upload will retry automatically.",
            ProcessingErrorType.UploadFailure => "Upload to YouTube failed. Please check your credentials and network connection.",
            ProcessingErrorType.ProcessingFailure => "Video processing failed. Please try again.",
            ProcessingErrorType.ResourceUnavailable => "Service is temporarily unavailable. Please try again later.",
            _ => "An unexpected error occurred. Please try again."
        };
    }
}

public enum ProcessingErrorType
{
    ValidationFailure = 0,
    CorruptedFile = 1,
    EncodingError = 2,
    ProcessingFailure = 3,
    TimeoutError = 4,
    NetworkError = 5,
    UploadFailure = 6,
    AuthenticationError = 7,
    TemporaryFailure = 8,
    ResourceUnavailable = 9,
    QuotaExceeded = 10
}

public enum ErrorSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
