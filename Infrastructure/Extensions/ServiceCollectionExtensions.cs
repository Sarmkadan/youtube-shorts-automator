// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// ReSharper disable UnusedParameter.Global

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing connection strings</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Connection string 'DefaultConnection' not found</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                options => options.MigrationsAssembly("YouTubeShortsAutomator")));

        return services;
    }

    /// <summary>
    /// Adds repository services to the service collection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing configuration settings</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpClient("YouTube", client =>
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddStandardResilienceHandler();

        services.AddHttpClient("GoogleOAuth", client =>
            {
                client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
            })
            .AddStandardResilienceHandler();

        return services;
    }

    /// <summary>
    /// Adds logging configuration
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddLogging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing connection strings</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> containing configuration settings</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddInfrastructure(configuration)
            .AddRepositories()
            .AddApplicationServices()
            .AddHttpClients(configuration)
            .AddCaching(configuration);

        return services;
    }
}