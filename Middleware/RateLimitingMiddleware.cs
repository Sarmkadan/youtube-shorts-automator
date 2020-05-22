// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Implements token bucket rate limiting per IP address
/// Prevents API abuse and ensures fair resource utilization
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitingOptions options)
    {
        _next = next;
        _options = options;
        _buckets = new ConcurrentDictionary<string, RateLimitBucket>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var bucket = _buckets.GetOrAdd(clientId,
            _ => new RateLimitBucket(_options.RequestsPerWindow, _options.WindowSizeSeconds));

        if (!bucket.AllowRequest())
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = _options.WindowSizeSeconds.ToString(CultureInfo.InvariantCulture);
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = _options.WindowSizeSeconds
            });
            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] = _options.RequestsPerWindow.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Remaining"] = bucket.RemainingRequests.ToString(CultureInfo.InvariantCulture);

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use X-Forwarded-For if behind proxy, otherwise use remote IP
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.ToString().Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public class RateLimitingOptions
{
    public int RequestsPerWindow { get; set; } = 100;
    public int WindowSizeSeconds { get; set; } = 60;
}

public class RateLimitBucket
{
    private readonly object _sync = new();
    private readonly int _capacity;
    private readonly double _windowSeconds;
    private int _tokens;
    private DateTime _lastRefill;

    public int RemainingRequests
    {
        get
        {
            lock (_sync)
            {
                return _tokens;
            }
        }
    }

    public int GetCapacity() => _capacity;

    public RateLimitBucket(int capacity)
        : this(capacity, 60)
    {
    }

    public RateLimitBucket(int capacity, int windowSeconds)
    {
        _capacity = capacity;
        _windowSeconds = windowSeconds > 0 ? windowSeconds : 60;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }

    public bool AllowRequest()
    {
        lock (_sync)
        {
            RefillTokens();

            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }

            return false;
        }
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastRefill).TotalSeconds;

        if (timePassed >= _windowSeconds)
        {
            _tokens = _capacity;
            _lastRefill = now;
        }
    }
}
