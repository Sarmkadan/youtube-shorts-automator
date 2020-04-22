# RateLimitingMiddleware

The `RateLimitingMiddleware` is a component designed to regulate the volume of incoming traffic within the `youtube-shorts-automator` application. By enforcing constraints on request frequency based on configurable time windows, this middleware safeguards against resource exhaustion and ensures compliance with upstream API rate limiting policies.

## API

### RateLimitingMiddleware
Initializes a new instance of the `RateLimitingMiddleware` class.

### InvokeAsync
Processes an incoming `HttpContext` to determine if the request should proceed or be throttled.
- **Parameters:** `HttpContext context`, `RequestDelegate next`
- **Returns:** `Task` representing the asynchronous operation.
- **Behavior:** Evaluates the request against current rate limits. If the request is permitted, it delegates to the `next` component. If throttled, it terminates the request pipeline and returns a 429 Too Many Requests response.

### RequestsPerWindow
The maximum number of requests permitted within the defined `WindowSizeSeconds`.

### WindowSizeSeconds
The duration of the time window, in seconds, for which the `RequestsPerWindow` constraint applies.

### RateLimitBucket
The internal mechanism representing the current state of rate limit consumption for the defined window.

### AllowRequest
Evaluates whether a new request is permitted based on the current state of the `RateLimitBucket`.
- **Returns:** `bool` indicating whether the request is allowed.

## Usage

### Configuring Middleware
```csharp
public void Configure(IApplicationBuilder app)
{
    // Register the middleware within the ASP.NET Core pipeline
    app.UseMiddleware<RateLimitingMiddleware>();
    app.UseRouting();
    // ...
}
```

### Checking Rate Limits Manually
```csharp
// Example of checking allowance programmatically
if (!rateLimitingMiddleware.AllowRequest())
{
    _logger.LogWarning("Request rejected: Rate limit exceeded.");
    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    return;
}
```

## Notes

*   **Thread Safety:** The `RateLimitingMiddleware` and its associated `RateLimitBucket` must implement thread-safe operations to correctly track request counts across concurrent HTTP requests. Implementations should utilize atomic operations or appropriate synchronization primitives to ensure accurate state management.
*   **Configurability:** Constraints defined by `RequestsPerWindow` and `WindowSizeSeconds` should be tuned according to the requirements of the downstream services being accessed by the `youtube-shorts-automator` to prevent service-side banning.
*   **Edge Cases:** If `WindowSizeSeconds` is configured as zero, the middleware behavior is undefined; it is recommended to ensure a positive integer value for all time-based constraints to prevent division-by-zero or logic errors.
