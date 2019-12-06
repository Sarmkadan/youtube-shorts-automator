// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Validates API keys from request headers
/// Supports X-API-Key header and Authorization Bearer token validation
/// </summary>
public class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyValidationMiddleware> _logger;
    private readonly string _validApiKey;
    private readonly string[] _excludedPaths;

    public ApiKeyValidationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _validApiKey = configuration.GetValue<string>("ApiSettings:ApiKey") ?? throw new InvalidOperationException("API key not configured");
        _excludedPaths = configuration.GetSection("ApiSettings:ExcludedPaths").Get<string[]>() ?? Array.Empty<string>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPathExcluded(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var apiKey = ExtractApiKey(context);

        if (string.IsNullOrEmpty(apiKey) || !IsValidApiKey(apiKey))
        {
            _logger.LogWarning("Unauthorized API request from {RemoteIp} to {Path}",
                context.Connection.RemoteIpAddress, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or missing API key",
                errorCode = "UNAUTHORIZED"
            });
            return;
        }

        await _next(context);
    }

    private string? ExtractApiKey(HttpContext context)
    {
        // Check X-API-Key header first
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return apiKeyHeader.ToString();
        }

        // Check Authorization header
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var auth = authHeader.ToString();
            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return auth.Substring("Bearer ".Length);
            }
        }

        return null;
    }

    private bool IsValidApiKey(string apiKey)
    {
        return apiKey.Equals(_validApiKey, StringComparison.Ordinal);
    }

    private bool IsPathExcluded(PathString path)
    {
        return _excludedPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }
}
