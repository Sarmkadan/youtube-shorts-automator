// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Infrastructure.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Extensions;

/// <summary>
/// Service collection extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds database context and repositories to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly("YouTubeShortsAutomator")));

        return services;
    }

    /// <summary>
    /// Adds repository services to the service collection
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IProcessingJobRepository, ProcessingJobRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IApiCredentialRepository, ApiCredentialRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();

        return services;
    }

    /// <summary>
    /// Adds application services to the service collection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<VideoProcessingService>();
        services.AddScoped<YouTubeUploadService>();
        services.AddScoped<AnalyticsService>();
        services.AddScoped<SchedulingService>();
        services.AddScoped<ConfigurationService>();
        services.AddScoped<IApiCredentialService, ApiCredentialService>();

        return services;
    }

    /// <summary>
    /// Adds HTTP clients for external API integrations
    /// </summary>
    public static IServiceCollection AddHttpClients(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("YouTube", client =>
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigureHttpClientDefaults(builder =>
        {
            builder.AddStandardResilienceHandler();
        });

        services.AddHttpClient("GoogleOAuth", client =>
        {
            client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
        })
        .ConfigureHttpClientDefaults(builder =>
        {
            builder.AddStandardResilienceHandler();
        });

        return services;
    }

    /// <summary>
    /// Adds logging configuration
    /// </summary>
    public static IServiceCollection AddLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    /// <summary>
    /// Adds caching services
    /// </summary>
    public static IServiceCollection AddCaching(this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }
        else
        {
            services.AddMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Configures all infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddInfrastructure(configuration)
            .AddRepositories()
            .AddApplicationServices()
            .AddHttpClients(configuration)
            .AddCaching(configuration);

        return services;
    }
}
