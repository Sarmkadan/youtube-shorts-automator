// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Extensions;
using YouTubeShortsAutomator.Infrastructure.Extensions;

/// <summary>
/// Provides an example of how to integrate the YouTube Shorts Automator into an ASP.NET Core application.
/// </summary>
public class IntegrationExample
{
    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    public static void ConfigureServices(IServiceCollection services)
    {
        Console.WriteLine("=== Integration: Registering Services ===");

        // Register core automator services
        services.AddYouTubeShortsAutomator(options =>
        {
            options.ApiKey = "your-api-key";
            options.ConnectionString = "Server=localhost;Database=YouTubeShortsAutomator;Integrated Security=true;";
        });

        // Register custom worker
        services.AddHostedService<Worker>();
    }

    /// <summary>
    /// A sample worker that demonstrates how to use the <see cref="IVideoProcessingService"/> instance.
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly IVideoProcessingService _processingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="processingService">The <see cref="IVideoProcessingService"/> instance.</param>
        public Worker(IVideoProcessingService processingService)
        {
            _processingService = processingService;
        }

        /// <summary>
        /// Executes the worker's background task.
        /// </summary>
        /// <param name="stoppingToken">The <see cref="CancellationToken"/> instance.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker started...");
            // Use _processingService here
            await Task.CompletedTask;
        }
    }
}
