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
public class ErrorHandlingMiddleware
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

        var response = new ErrorResponse();

        switch (exception)
        {
            case ArgumentNullException argNull:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ErrorResponse
                {
                    Message = "Required parameter is missing",
                    ErrorCode = "INVALID_PARAMETER",
                    Details = argNull.ParamName
                };
                break;

            case ArgumentException arg:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ErrorResponse
                {
                    Message = "Invalid argument provided",
                    ErrorCode = "INVALID_ARGUMENT",
                    Details = arg.Message
                };
                break;

            case FileNotFoundException fileNotFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ErrorResponse
                {
                    Message = "File not found",
                    ErrorCode = "FILE_NOT_FOUND",
                    Details = fileNotFound.FileName
                };
                break;

            case UnauthorizedAccessException unauthorized:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new ErrorResponse
                {
                    Message = "Unauthorized access",
                    ErrorCode = "UNAUTHORIZED",
                    Details = unauthorized.Message
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new ErrorResponse
                {
                    Message = "An unexpected error occurred",
                    ErrorCode = "INTERNAL_ERROR",
                    Details = exception.Message
                };
                break;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
