// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, AppSettings appSettings)
    {
        // Adds all application services to the dependency injection container
        
        // Configure database context
        services.AddSingleton(_ => new DatabaseContext(appSettings.ConnectionString));

        // Register repositories
        services.AddScoped(sp => new VideoShortRepository(sp.GetRequiredService<DatabaseContext>()));
        services.AddScoped(sp => new UploadJobRepository(sp.GetRequiredService<DatabaseContext>()));
        services.AddScoped(sp => new AnalyticsRepository(sp.GetRequiredService<DatabaseContext>()));

        // Register services
        services.AddScoped((sp) => new VideoProcessingService(
            sp.GetRequiredService<VideoShortRepository>(),
            sp.GetRequiredService<ILogger<VideoProcessingService>>()
        ));

        services.AddScoped((sp) => new YouTubeUploadService(
            sp.GetRequiredService<UploadJobRepository>(),
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

    public static IServiceCollection AddApplicationLogging(this IServiceCollection services, AppSettings appSettings)
    {
        // Configures logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}
