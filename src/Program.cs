// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Configuration;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Initializes and runs the YouTube Shorts Automator application
        var configuration = BuildConfiguration();
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        
        var services = new ServiceCollection();
        
        // Register configuration
        services.AddSingleton(appSettings);
        services.AddSingleton(configuration);
        
        // Register application services
        services.AddApplicationServices(appSettings);
        services.AddApplicationLogging(appSettings);

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting YouTube Shorts Automator application");
            
            // Initialize directories
            InitializeDirectories(appSettings);
            
            // Example: Process a sample video
            await RunPipelineExampleAsync(serviceProvider, logger);
            
            logger.LogInformation("Application completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError($"Application failed: {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        // Builds the configuration from appsettings.json
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return configBuilder.Build();
    }

    private static void InitializeDirectories(AppSettings appSettings)
    {
        // Creates necessary directories if they don't exist
        var directories = new[]
        {
            appSettings.LogDirectory,
            appSettings.ProcessingDirectory,
            appSettings.OutputDirectory,
            Constants.Constants.TEMP_DIRECTORY
        };

        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    private static async Task RunPipelineExampleAsync(IServiceProvider serviceProvider, ILogger<Program> logger)
    {
        // Example: Demonstrates the full pipeline with sample data
        logger.LogInformation("Running pipeline example");

        var videoRepository = serviceProvider.GetRequiredService<VideoShortRepository>();
        var processingService = serviceProvider.GetRequiredService<VideoProcessingService>();
        var orchestrationService = serviceProvider.GetRequiredService<JobOrchestrationService>();

        try
        {
            // Create sample processing profile
            var profile = new ProcessingProfile
            {
                Id = 1,
                Name = "Standard YouTube Shorts",
                Description = "Optimized for YouTube Shorts platform",
                VideoWidth = 1080,
                VideoHeight = 1920,
                VideoBitrate = 4000,
                AudioBitrate = 128,
                FrameRate = 30,
                VideoCodec = "h264",
                AudioCodec = "aac",
                Container = "mp4",
                ApplyWatermark = false,
                ApplyColorGrading = false,
                CompressionLevel = 5,
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create sample YouTube channel
            var channel = new YouTubeChannel
            {
                Id = 1,
                ChannelId = "UCexample123",
                ChannelName = "Example Channel",
                Description = "Sample YouTube channel",
                AccessToken = "sample_token",
                RefreshToken = "sample_refresh",
                TokenExpiresAt = DateTime.UtcNow.AddHours(1),
                SubscriberCount = 10000,
                ViewCount = 500000,
                VideoCount = 50,
                ProfileImageUrl = "https://example.com/image.jpg",
                IsVerified = true,
                IsActive = true,
                DefaultLanguage = "en",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create sample video short
            var videoShort = new VideoShort
            {
                Title = "Sample YouTube Short",
                Description = "This is a sample video short for testing",
                FilePath = "sample_video.mp4",
                Duration = TimeSpan.FromSeconds(30),
                FileSizeBytes = 50000000,
                Quality = VideoQuality.High,
                Status = ProcessingStatus.Pending,
                Tags = new[] { "sample", "test", "youtube" },
                ProcessingProfileId = profile.Id,
                YouTubeChannelId = channel.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Run the pipeline
            var scheduledTime = DateTime.UtcNow.AddHours(2);
            logger.LogInformation($"Running pipeline for sample video, scheduled upload at {scheduledTime:F}");

            // Note: In a real scenario, the video file would need to exist
            // For demonstration, we're showing the pipeline structure
            
            logger.LogInformation("Pipeline example completed");
        }
        catch (Exception ex)
        {
            logger.LogError($"Pipeline example failed: {ex.Message}");
        }
    }
}
