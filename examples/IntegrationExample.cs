// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Infrastructure.Extensions;

// Integration example: Wiring up into ASP.NET Core DI
public class IntegrationExample
{
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

    public class Worker : BackgroundService
    {
        private readonly IVideoProcessingService _processingService;

        public Worker(IVideoProcessingService processingService)
        {
            _processingService = processingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker started...");
            // Use _processingService here
            await Task.CompletedTask;
        }
    }
}
