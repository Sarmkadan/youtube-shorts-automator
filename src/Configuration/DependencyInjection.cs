// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouTubeShortAutomator.Exceptions;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Extension methods that register all application-layer services, repositories,
/// and subsystem components into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, AppSettings appSettings)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

        // Adds all application services to the dependency injection container

        // Configure database context
        services.AddSingleton(_ => new DatabaseContext(appSettings.ConnectionString));

        // Register repositories
        services.AddScoped(sp => new VideoShortRepository(sp.GetRequiredService<DatabaseContext>()));
        services.AddScoped(sp => new UploadJobRepository(sp.GetRequiredService<DatabaseContext>()));
        services.AddScoped(sp => new AnalyticsRepository(sp.GetRequiredService<DatabaseContext>()));
        services.AddScoped(sp => new UploadHistoryRepository(sp.GetRequiredService<DatabaseContext>()));

        // Register services
        services.AddScoped((sp) => new VideoProcessingService(
            sp.GetRequiredService<VideoShortRepository>(),
            sp.GetRequiredService<ILogger<VideoProcessingService>>()
        ));

        services.AddScoped((sp) => new YouTubeUploadService(
            sp.GetRequiredService<UploadJobRepository>(),
            sp.GetRequiredService<UploadHistoryRepository>(),
            sp.GetRequiredService<ILogger<YouTubeUploadService>>()
        ));

        services.AddScoped((sp) => new SchedulingService(
            sp.GetRequiredService<UploadJobRepository>(),
            sp.GetRequiredService<ILogger<SchedulingService>>()
        ));

        services.AddScoped((sp) => new AnalyticsService(
            sp.GetRequiredService<AnalyticsRepository>(),
            sp.GetRequiredService<VideoShortRepository>(),
            sp.GetRequiredService<ILogger<AnalyticsService>>()
        ));

        services.AddScoped(sp => new ThumbnailAbTestRepository(sp.GetRequiredService<DatabaseContext>()));

        services.AddScoped((sp) => new ThumbnailAbTestService(
            sp.GetRequiredService<ThumbnailAbTestRepository>(),
            sp.GetRequiredService<VideoShortRepository>(),
            sp.GetRequiredService<ILogger<ThumbnailAbTestService>>()
        ));

        services.AddScoped<IThumbnailGeneratorService>(sp =>
            new ThumbnailGeneratorService(
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<ThumbnailGeneratorService>>()));

        services.AddScoped((sp) => new JobOrchestrationService(
            sp.GetRequiredService<VideoProcessingService>(),
            sp.GetRequiredService<YouTubeUploadService>(),
            sp.GetRequiredService<SchedulingService>(),
            sp.GetRequiredService<AnalyticsService>(),
            sp.GetRequiredService<VideoShortRepository>(),
            sp.GetRequiredService<UploadJobRepository>(),
            sp.GetRequiredService<ILogger<JobOrchestrationService>>()
        ));

        return services;
    }

    /// <summary>
    /// Registers all content calendar services: the repository, the title optimisation engine
    /// and the calendar orchestration service. Optionally binds configuration from
    /// <paramref name="configure"/>.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configure">Optional delegate to override <see cref="ContentCalendarOptions"/> defaults.</param>
    public static IServiceCollection AddContentCalendar(
        this IServiceCollection services,
        Action<ContentCalendarOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var options = new ContentCalendarOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddScoped(sp =>
            new ContentCalendarRepository(sp.GetRequiredService<DatabaseContext>()));

        services.AddScoped<ITitleOptimizationEngine>(sp =>
            new TitleOptimizationEngine(
                sp.GetRequiredService<AnalyticsService>(),
                sp.GetRequiredService<ContentCalendarOptions>(),
                sp.GetRequiredService<ILogger<TitleOptimizationEngine>>()));

        services.AddScoped<IContentCalendarService>(sp =>
            new ContentCalendarService(
                sp.GetRequiredService<ContentCalendarRepository>(),
                sp.GetRequiredService<SchedulingService>(),
                sp.GetRequiredService<AnalyticsService>(),
                sp.GetRequiredService<VideoShortRepository>(),
                sp.GetRequiredService<ITitleOptimizationEngine>(),
                sp.GetRequiredService<ContentCalendarOptions>(),
                sp.GetRequiredService<ILogger<ContentCalendarService>>()));

        return services;
    }

    /// <summary>Configures the application logging pipeline.</summary>
    public static IServiceCollection AddApplicationLogging(this IServiceCollection services, AppSettings appSettings)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

        // Configures logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}
