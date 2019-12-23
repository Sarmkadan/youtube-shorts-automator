// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public class DomainException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? Context { get; set; }

    public DomainException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    public DomainException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    public void AddContext(string key, object value)
    {
        Context?.Add(key, value);
    }
}

/// <summary>
/// Thrown when video validation fails
/// </summary>
public class VideoValidationException : DomainException
{
    public List<string> ValidationErrors { get; set; } = new();

    public VideoValidationException(string message, List<string> errors)
        : base(message, "VIDEO_VALIDATION_FAILED")
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Thrown when processing job fails
/// </summary>
public class ProcessingJobException : DomainException
{
    public Guid JobId { get; set; }
    public ProcessingJobException(string message, Guid jobId)
        : base(message, "PROCESSING_JOB_FAILED")
    {
        JobId = jobId;
    }
}

/// <summary>
/// Thrown when upload fails
/// </summary>
public class UploadException : DomainException
{
    public string? YouTubeErrorCode { get; set; }

    public UploadException(string message, string? youtubeErrorCode = null)
        : base(message, "UPLOAD_FAILED")
    {
        YouTubeErrorCode = youtubeErrorCode;
    }
}

/// <summary>
/// Thrown when credentials are invalid
/// </summary>
public class CredentialException : DomainException
{
    public CredentialException(string message) : base(message, "CREDENTIAL_INVALID") { }
}

/// <summary>
/// Thrown when API call fails
/// </summary>
public class ApiException : DomainException
{
    public int? HttpStatusCode { get; set; }
    public string? ApiResponse { get; set; }

    public ApiException(string message, int? statusCode = null)
        : base(message, "API_ERROR")
    {
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when user quota is exceeded
/// </summary>
public class QuotaExceededException : DomainException
{
    public long CurrentUsageBytes { get; set; }
    public long QuotaBytes { get; set; }

    public QuotaExceededException(string message, long currentUsage, long quota)
        : base(message, "QUOTA_EXCEEDED")
    {
        CurrentUsageBytes = currentUsage;
        QuotaBytes = quota;
    }
}

/// <summary>
/// Thrown when resource is not found
/// </summary>
public class ResourceNotFoundException : DomainException
{
    public Guid ResourceId { get; set; }
    public string ResourceType { get; set; } = string.Empty;

    public ResourceNotFoundException(string message, Guid resourceId, string resourceType)
        : base(message, "RESOURCE_NOT_FOUND")
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
    }
}

/// <summary>
/// Thrown when operation is invalid for current state
/// </summary>
public class InvalidStateException : DomainException
{
    public string CurrentState { get; set; } = string.Empty;
    public string RequestedOperation { get; set; } = string.Empty;

    public InvalidStateException(string message, string currentState, string operation)
        : base(message, "INVALID_STATE")
    {
        CurrentState = currentState;
        RequestedOperation = operation;
    }
}
