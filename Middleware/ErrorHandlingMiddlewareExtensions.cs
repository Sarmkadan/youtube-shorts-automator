// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for ErrorHandlingMiddleware to provide additional
// error handling capabilities and convenience methods
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Extension methods for ErrorHandlingMiddleware that provide additional
/// error handling capabilities and convenience methods for working with
/// error responses and middleware functionality
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Creates a standardized error response from an exception
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="exception">The exception to convert to an error response</param>
    /// <returns>A new ErrorResponse instance populated from the exception</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static ErrorResponse CreateErrorResponse(this ErrorHandlingMiddleware middleware, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new ErrorResponse
        {
            Message = GetExceptionMessage(exception),
            ErrorCode = GetExceptionErrorCode(exception),
            Details = exception.Message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a standardized error response with custom message and error code
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="message">Custom error message</param>
    /// <param name="errorCode">Custom error code</param>
    /// <param name="details">Optional additional error details</param>
    /// <returns>A new ErrorResponse instance with the provided values</returns>
    /// <exception cref="ArgumentNullException">Thrown when message or errorCode is null</exception>
    /// <exception cref="ArgumentException">Thrown when message or errorCode is empty</exception>
    public static ErrorResponse CreateErrorResponse(
        this ErrorHandlingMiddleware middleware,
        string message,
        string errorCode,
        string? details = null)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        return new ErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error response for validation failures
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="validationErrors">Collection of validation error messages</param>
    /// <param name="errorCode">Optional error code (defaults to VALIDATION_ERROR)</param>
    /// <returns>A new ErrorResponse instance with validation error details</returns>
    /// <exception cref="ArgumentNullException">Thrown when validationErrors is null</exception>
    public static ErrorResponse CreateValidationErrorResponse(
        this ErrorHandlingMiddleware middleware,
        IReadOnlyList<string> validationErrors,
        string errorCode = "VALIDATION_ERROR")
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        var errorMessages = string.Join("; ", validationErrors);
        return new ErrorResponse
        {
            Message = "Validation failed",
            ErrorCode = errorCode,
            Details = errorMessages,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error response for rate limiting scenarios
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="retryAfterSeconds">Seconds until retry is allowed</param>
    /// <param name="details">Optional additional details about rate limiting</param>
    /// <returns>A new ErrorResponse instance for rate limiting errors</returns>
    public static ErrorResponse CreateRateLimitErrorResponse(
        this ErrorHandlingMiddleware middleware,
        int retryAfterSeconds,
        string? details = null)
    {
        return new ErrorResponse
        {
            Message = $"Rate limit exceeded. Please retry after {retryAfterSeconds} seconds",
            ErrorCode = "RATE_LIMIT_EXCEEDED",
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Formats the error response as JSON string
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="response">The error response to format</param>
    /// <returns>JSON string representation of the error response</returns>
    /// <exception cref="ArgumentNullException">Thrown when response is null</exception>
    public static string FormatAsJson(this ErrorHandlingMiddleware middleware, ErrorResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return System.Text.Json.JsonSerializer.Serialize(
            response,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = false
            }
        );
    }

    /// <summary>
    /// Gets a user-friendly error message from an exception
    /// </summary>
    /// <param name="exception">The exception to get message from</param>
    /// <returns>Formatted error message</returns>
    private static string GetExceptionMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException argNull => $"Parameter '{argNull.ParamName}' cannot be null",
            ArgumentException arg => arg.Message,
            InvalidOperationException => "Operation is not valid in the current state",
            KeyNotFoundException => "The requested key was not found in the collection",
            UnauthorizedAccessException => "Access to the resource is unauthorized",
            FileNotFoundException fileNotFound => $"File not found: {fileNotFound.FileName}",
            _ => "An unexpected error occurred"
        };
    }

    /// <summary>
    /// Gets a standardized error code from an exception
    /// </summary>
    /// <param name="exception">The exception to get error code from</param>
    /// <returns>Standardized error code</returns>
    private static string GetExceptionErrorCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "INVALID_PARAMETER",
            ArgumentException => "INVALID_ARGUMENT",
            InvalidOperationException => "INVALID_OPERATION",
            KeyNotFoundException => "KEY_NOT_FOUND",
            UnauthorizedAccessException => "UNAUTHORIZED",
            FileNotFoundException => "FILE_NOT_FOUND",
            _ => "INTERNAL_ERROR"
        };
    }
}