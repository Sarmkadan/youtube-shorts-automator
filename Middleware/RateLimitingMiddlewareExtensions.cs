// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Helper class for managing rate limit buckets across the application
/// </summary>
internal static class RateLimitBucketManager
{
    private static readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();
    private static readonly object _syncLock = new();

    public static RateLimitBucket GetOrCreateBucket(string clientId, int capacity)
    {
        if (_buckets.TryGetValue(clientId, out var existingBucket))
        {
            return existingBucket;
        }

        lock (_syncLock)
        {
            if (_buckets.TryGetValue(clientId, out existingBucket))
            {
                return existingBucket;
            }

            var newBucket = new RateLimitBucket(capacity);
            _buckets[clientId] = newBucket;
            return newBucket;
        }
    }
}

/// <summary>
/// Extension methods for <see cref="RateLimitingMiddleware"/> to provide additional functionality
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the HTTP pipeline with default configuration
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance</param>
    /// <returns><see cref="IApplicationBuilder"/> for chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null</exception>
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.UseMiddleware<RateLimitingMiddleware>();
    }

    /// <summary>
    /// Adds rate limiting middleware to the HTTP pipeline with custom configuration
    /// </summary>
    /// <param name="builder"><see cref="IApplicationBuilder"/> instance</param>
    /// <param name="configureOptions">Action to configure <see cref="RateLimitingOptions"/></param>
    /// <returns><see cref="IApplicationBuilder"/> for chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="configureOptions"/> is null</exception>
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        Action<RateLimitingOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        var options = new RateLimitingOptions();
        configureOptions(options);

        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }

    /// <summary>
    /// Adds rate limiting services to the dependency injection container
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> instance</param>
    /// <returns><see cref="IServiceCollection"/> for chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null</exception>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton<RateLimitingOptions>(_ => new RateLimitingOptions())
            .AddSingleton<RateLimitBucket>(provider =>
            {
                var options = provider.GetRequiredService<RateLimitingOptions>();
                return new RateLimitBucket(options.RequestsPerWindow);
            });
    }

    /// <summary>
    /// Adds rate limiting services to the dependency injection container with custom configuration
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> instance</param>
    /// <param name="configureOptions">Action to configure <see cref="RateLimitingOptions"/></param>
    /// <returns><see cref="IServiceCollection"/> for chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configureOptions"/> is null</exception>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        Action<RateLimitingOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return services.Configure(configureOptions)
            .AddSingleton<RateLimitBucket>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return new RateLimitBucket(options.RequestsPerWindow);
            });
    }

    /// <summary>
    /// Gets the current rate limit status for a specific client
    /// </summary>
    /// <param name="middleware"><see cref="RateLimitingMiddleware"/> instance</param>
    /// <param name="clientId">Client identifier (IP address or other identifier)</param>
    /// <param name="options"><see cref="RateLimitingOptions"/> containing rate limit configuration</param>
    /// <returns><see cref="ValueTuple"/> containing remaining requests and total capacity</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="options"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="clientId"/> is null or empty</exception>
    public static (int remaining, int capacity) GetClientRateLimitStatus(
        this RateLimitingMiddleware middleware,
        string clientId,
        RateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(clientId);

        var bucket = RateLimitBucketManager.GetOrCreateBucket(clientId, options.RequestsPerWindow);
        return (bucket.RemainingRequests, bucket.GetCapacity());
    }

    /// <summary>
    /// Checks if a client is currently rate limited
    /// </summary>
    /// <param name="middleware"><see cref="RateLimitingMiddleware"/> instance</param>
    /// <param name="clientId">Client identifier</param>
    /// <param name="options"><see cref="RateLimitingOptions"/> containing rate limit configuration</param>
    /// <returns>True if the client can make requests, false if rate limited</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="options"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="clientId"/> is null or empty</exception>
    public static bool IsClientRateLimited(
        this RateLimitingMiddleware middleware,
        string clientId,
        RateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(clientId);

        var bucket = RateLimitBucketManager.GetOrCreateBucket(clientId, options.RequestsPerWindow);
        return !bucket.AllowRequest();
    }
}