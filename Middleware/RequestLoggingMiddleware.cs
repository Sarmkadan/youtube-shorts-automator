// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Middleware for logging HTTP request/response details
/// Captures request body, response body, headers, and timing information
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Enable buffering for request body
        context.Request.EnableBuffering();

        var requestBody = await ReadStreamAsync(context.Request.Body);
        context.Request.Body.Position = 0;

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var response = await ReadStreamAsync(context.Response.Body);
            context.Response.Body.Position = 0;
            await context.Response.Body.CopyToAsync(originalBodyStream);

            // Log request/response details
            LogRequestResponse(context, requestBody, response, stopwatch.ElapsedMilliseconds);
        }
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        stream.Position = 0;
        return text;
    }

    private void LogRequestResponse(HttpContext context, string requestBody, string responseBody, long elapsedMs)
    {
        var request = context.Request;
        var response = context.Response;

        var logInfo = new
        {
            Timestamp = DateTime.UtcNow,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString,
            StatusCode = response.StatusCode,
            Duration = $"{elapsedMs}ms",
            RequestContentType = request.ContentType,
            ResponseContentType = response.ContentType,
            RequestBodyLength = requestBody.Length,
            ResponseBodyLength = responseBody.Length
        };

        if (response.StatusCode >= 500)
        {
            _logger.LogError("Error Response: {@LogInfo} RequestBody: {RequestBody} ResponseBody: {ResponseBody}",
                logInfo, TruncateBody(requestBody, 500), TruncateBody(responseBody, 1000));
        }
        else if (response.StatusCode >= 400)
        {
            _logger.LogWarning("Client Error Response: {@LogInfo}", logInfo);
        }
        else
        {
            _logger.LogInformation("Successful Response: {@LogInfo}", logInfo);
        }
    }

    private static string TruncateBody(string body, int maxLength)
    {
        if (string.IsNullOrEmpty(body)) return body;
        return body.Length > maxLength ? body[..maxLength] + "..." : body;
    }
}
