// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Global error handling middleware that catches all unhandled exceptions
/// and returns consistent error responses to clients
/// </summary>
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = MapException(exception);
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, jsonOptions);
    }

    private static (HttpStatusCode StatusCode, ErrorResponse Response) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException argNull => (HttpStatusCode.BadRequest, new ErrorResponse
            {
                Message = "Required parameter is missing",
                ErrorCode = "INVALID_PARAMETER",
                Details = argNull.ParamName
            }),
            ArgumentException arg => (HttpStatusCode.BadRequest, new ErrorResponse
            {
                Message = "Invalid argument provided",
                ErrorCode = "INVALID_ARGUMENT",
                Details = arg.Message
            }),
            FileNotFoundException fileNotFound => (HttpStatusCode.NotFound, new ErrorResponse
            {
                Message = "File not found",
                ErrorCode = "FILE_NOT_FOUND",
                Details = fileNotFound.FileName
            }),
            UnauthorizedAccessException unauthorized => (HttpStatusCode.Unauthorized, new ErrorResponse
            {
                Message = "Unauthorized access",
                ErrorCode = "UNAUTHORIZED",
                Details = unauthorized.Message
            }),
            _ => (HttpStatusCode.InternalServerError, new ErrorResponse
            {
                Message = "An unexpected error occurred",
                ErrorCode = "INTERNAL_ERROR",
                Details = exception.Message
            })
        };
    }
}

public sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
