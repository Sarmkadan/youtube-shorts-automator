// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.BackgroundServices;
using YouTubeShortsAutomator.Caching;
using YouTubeShortsAutomator.Events;
using YouTubeShortsAutomator.Formatters;
using YouTubeShortsAutomator.Integration;

namespace YouTubeShortsAutomator.Extensions;

/// <summary>
/// Extension methods for DI container configuration
/// Centralizes service registration for middleware, services, and background jobs
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Caching services
        services.AddMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        // Formatters
        services.AddSingleton<JsonResponseFormatter>();
        services.AddSingleton<CsvExportFormatter>();

        // Integration services
        services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
        services.AddScoped<IWebhookPublisher, WebhookPublisher>();
        services.AddScoped<IFFmpegWrapper, FFmpegWrapper>();

        // Event system
        services.AddSingleton<IEventPublisher, EventPublisher>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<ProcessingBackgroundService>();
        services.AddHostedService<CleanupBackgroundService>();

        return services;
    }

    public static void UseApplicationMiddleware(this WebApplication app)
    {
        // Apply custom middleware in order
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<ApiKeyValidationMiddleware>();
    }

    public static IServiceCollection AddRateLimitingOptions(
        this IServiceCollection services,
        int requestsPerWindow = 100,
        int windowSizeSeconds = 60)
    {
        var options = new RateLimitingOptions
        {
            RequestsPerWindow = requestsPerWindow,
            WindowSizeSeconds = windowSizeSeconds
        };

        services.AddSingleton(options);
        return services;
    }
}
