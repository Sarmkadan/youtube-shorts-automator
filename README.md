# YouTube Shorts Automator 2 
![Build](https://github.com/sarmkadan/youtube-shorts-automator/actions/workflows/build.yml/badge.svg) 
![License](https://img.shields.io/github/license/sarmkadan/youtube-shorts-automator) 
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4) 

> **Automated YouTube Shorts upload pipeline built with .NET 10**
> FFmpeg processing • Google APIs integration • Smart scheduling • Real-time analytics dashboard 

An enterprise-grade solution for automating the entire YouTube Shorts lifecycle: from video processing and optimization to scheduled uploads and performance analytics. Built with clean architecture principles, comprehensive error handling, and production-ready implementation patterns.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [FFmpeg Setup](#ffmpeg-setup)
- [Quick Start](#quick-start)
- [Testing](#testing)
- [Configuration Guide](#configuration-guide)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [CLI Reference](#cli-reference)
- [Troubleshooting](#troubleshooting)
- [Performance](#performance)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Video Processing
- **Automated FFmpeg Transcoding**: Multi-profile encoding with customizable bitrates, resolution, and frame rates
- **Smart Video Optimization**: Auto-detects video quality and applies optimal processing parameters
- **Watermark Integration**: Overlay custom branding on processed videos with configurable positioning
- **Color Grading**: Professional color correction and enhancement during transcoding
- **Batch Processing**: Process multiple videos concurrently with configurable worker pools
- **Format Conversion**: Support for MP4, MKV, WebM, AVI with automatic codec selection

### YouTube Integration
- **OAuth 2.0 Authentication**: Secure multi-channel authentication with automatic token refresh
- **Direct Upload API**: Upload videos directly to YouTube with streaming support
- **Metadata Management**: Set titles, descriptions, tags, thumbnails, and privacy settings
- **Playlist Management**: Automatically add uploads to playlists with smart categorization
- **Advanced Permissions**: Manage channel permissions and delegated access

### Scheduling & Automation
- **Smart Scheduling**: Schedule uploads at optimal times based on channel analytics
- **Intelligent Retry Logic**: Exponential backoff with configurable retry strategies
- **Batch Operations**: Schedule bulk uploads with progress tracking
- **Job Orchestration**: Coordinate multi-step workflows with dependency management
- **Background Processing**: Continuous background services for async operations
- **Cron-based Scheduling**: Native support for cron expressions for recurring tasks

### Analytics & Monitoring
- **Real-time Metrics Dashboard**: View live performance data across all uploads
- **Engagement Tracking**: Monitor views, likes, comments, and share counts
- **Performance Analysis**: Trending video identification and performance benchmarking
- **Custom Reports**: Generate CSV/JSON reports with multiple filtering options
- **Webhook Integration**: Receive real-time notifications for upload events
- **System Health Monitoring**: CPU, memory, and processing queue metrics

### Thumbnail Generator
- **Frame Extraction**: Capture any frame from a video at a precise timestamp using FFmpeg
- **Aspect Ratio Presets**: Vertical 9:16 (720×1280, Shorts-optimised), horizontal 16:9 (1280×720), and square 1:1 (720×720)
- **Text Overlays**: Render titles and channel names via the FFmpeg `drawtext` filter with nine anchor positions, configurable font size, colour and semi-transparent background box
- **Output Formats**: JPEG (quality-tunable), PNG (lossless) and WebP
- **Batch Generation**: Extract multiple candidate frames evenly spaced across the video's duration for easy selection
- **A/B Test Integration**: Works alongside `ThumbnailAbTestService` to serve, track and evaluate competing thumbnail variants

### Title & Description Optimizer
- **Heuristic Scoring**: Data-driven `ScoreTitle` method rates titles on length (40–70 characters optimal), power-word presence, question format and numeric hooks — returns a `[0,1]` confidence score
- **Keyword Extraction**: `ExtractKeywords` strips stop-words, filters short tokens and returns the top-10 content-bearing terms from title and description
- **Strategy Suggestions**: Produces up to three ranked `OptimizationSuggestion` records per call — power-word injection, question reformatting and keyword-alignment variants
- **Hashtag Recommendations**: Appends configured trending hashtags (`#shorts`, `#fyp`, …) plus content-derived hashtags to every optimised description
- **Posting-Time Prediction**: `RecommendPostingTimesAsync` derives optimal UTC upload slots from historical engagement patterns, skipping Sundays and enforcing a configurable minimum gap between slots

## JobOrchestrationService

The `JobOrchestrationService` class orchestrates the complete video processing and upload pipeline for YouTube Shorts automation. It coordinates between video processing, upload scheduling, analytics tracking, and error recovery, providing a centralized service for managing the entire video lifecycle from validation to scheduled upload.

**Key Features:**
- Orchestrates multi-step video processing pipelines with validation and error handling
- Manages upload scheduling and YouTube API integration
- Tracks analytics synchronization and performance metrics
- Provides retry logic for failed operations
- Centralized logging and error recovery throughout the pipeline

**Public Members:**
- `ProcessFullPipelineAsync()` - Processes the complete video pipeline from validation to scheduled upload, coordinating video processing, thumbnail generation, analytics creation, and upload scheduling
- `ProcessReadyUploadsAsync()` - Processes all upload jobs that are ready to be uploaded, attempting to upload each video to YouTube
- `ProcessFailedRetriableJobsAsync()` - Processes failed upload jobs that are eligible for retry, implementing retry logic with configurable maximum attempts
- `SyncAnalyticsAsync()` - Synchronizes analytics data for all videos from YouTube, providing statistics on successful and failed sync operations

**Properties:**
- `VideoShortId` - The unique identifier of the video short being processed
- `UploadJobId` - The unique identifier of the generated upload job
- `Status` - The pipeline execution status (Success, Failed, etc.)
- `Error` - Error message if the pipeline failed; otherwise null
- `ProcessingCompleted` - Whether video processing completed successfully
- `ScheduledUploadTime` - The scheduled upload time for the video
- `CompletedAt` - The completion timestamp of the pipeline
- `Channel` - The name of the YouTube channel being synced
- `SyncedCount` - The number of videos successfully synced with analytics data
- `FailedCount` - The number of videos that failed to sync analytics data
- `Error` - Error message if the sync failed; otherwise null
- `CompletedAt` - The completion timestamp of the analytics sync

**Usage Example:**

```csharp
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

// Initialize service (typically via dependency injection)
var services = new ServiceCollection();
services.AddLogging();
var serviceProvider = services.BuildServiceProvider();
var orchestrationService = serviceProvider.GetRequiredService<JobOrchestrationService>();

// Example 1: Process complete video pipeline
var processingProfile = new ProcessingProfile 
{
    Name = "High Quality",
    Resolution = "1080p",
    Bitrate = "4000k",
    ApplyWatermark = true,
    ApplyColorGrading = true
};

var channel = new YouTubeChannel
{
    ChannelId = "UC1234567890",
    ChannelName = "My Awesome Coding Channel",
    AccessToken = "ya29.a0AfH6SMB...",
    TokenExpiresAt = DateTime.UtcNow.AddHours(1)
};

var pipelineResult = await orchestrationService.ProcessFullPipelineAsync(
    videoShortId: 42,
    processingProfile: processingProfile,
    channel: channel,
    scheduledUploadTime: DateTime.UtcNow.AddHours(2),
    cancellationToken: CancellationToken.None
);

if (pipelineResult.Status == "Success")
{
    Console.WriteLine($"✓ Pipeline completed successfully for video {pipelineResult.VideoShortId}");
    Console.WriteLine($" - Upload job ID: {pipelineResult.UploadJobId}");
    Console.WriteLine($" - Scheduled for: {pipelineResult.ScheduledUploadTime:u}");
    Console.WriteLine($" - Completed at: {pipelineResult.CompletedAt:u}");
}
else
{
    Console.WriteLine($"✗ Pipeline failed: {pipelineResult.Error}");
}

// Example 2: Process ready uploads
var uploadSuccess = await orchestrationService.ProcessReadyUploadsAsync(channel, CancellationToken.None);
Console.WriteLine($"Processed ready uploads: {(uploadSuccess ? "Success" : "Failed")}");

// Example 3: Process failed jobs eligible for retry
var retrySuccess = await orchestrationService.ProcessFailedRetriableJobsAsync(channel, CancellationToken.None);
Console.WriteLine($"Processed retriable jobs: {(retrySuccess ? "Success" : "Failed")}");

// Example 4: Sync analytics for all videos
var syncResult = await orchestrationService.SyncAnalyticsAsync(channel, CancellationToken.None);
Console.WriteLine($"Analytics sync completed: {syncResult.SyncedCount} synced, {syncResult.FailedCount} failed");
if (syncResult.Error != null)
{
    Console.WriteLine($"Error: {syncResult.Error}");
}
```

## TitleOptimizationEngine

The `TitleOptimizationEngine` class provides intelligent title and description optimization for YouTube Shorts videos. It analyzes video metadata against historical channel performance data to generate ranked optimization suggestions, extract relevant keywords, score titles for engagement potential, and recommend optimal posting times based on channel analytics.

The engine uses deterministic, data-driven scoring against historical analytics to produce optimization suggestions including power-word injection, question format conversion, and keyword alignment with top-performing content from the channel.

**Public Members:**
- `OptimizeAsync()` - Generates optimization suggestions, hashtags, and optimal posting times based on video metadata and historical performance
- `RecommendPostingTimesAsync()` - Recommends optimal UTC posting times for a channel based on historical engagement patterns
- `ScoreTitle()` - Scores a title for engagement potential (0-1 scale) based on length, power words, question format, and keyword presence
- `ExtractKeywords()` - Extracts relevant keywords from title and description by filtering stop words and short tokens




**Usage Example:**

```csharp
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;

// Initialize service (typically via dependency injection)
var titleOptimizationEngine = serviceProvider.GetRequiredService<ITitleOptimizationEngine>();

// Example 1: Optimize video title and description
var optimizationResult = await titleOptimizationEngine.OptimizeAsync(
    title: "Learn C# in 30 Days",
    description: "A comprehensive guide to mastering C# programming language fundamentals and advanced concepts",
    tags: new[] { "csharp", "programming", "tutorial", "dotnet" },
    channelId: 1
);

Console.WriteLine($"Original title: {optimizationResult.OriginalTitle}");
Console.WriteLine($"Best suggestion: {optimizationResult.Suggestions[0].SuggestedTitle}");
Console.WriteLine($"Confidence: {optimizationResult.Suggestions[0].ConfidenceScore:F2}");
Console.WriteLine($"Optimal posting hour: {optimizationResult.OptimalPostingHour}:00 UTC");
Console.WriteLine($"Recommended hashtags: {string.Join(", ", optimizationResult.RecommendedHashtags)}");

// Example 2: Score a title for engagement potential
double titleScore = titleOptimizationEngine.ScoreTitle("10 Essential Python Tips Every Developer Should Know");
Console.WriteLine($"Title score: {titleScore:F2}");

// Example 3: Extract keywords from title and description
string[] keywords = titleOptimizationEngine.ExtractKeywords(
    title: "Master JavaScript in 2024",
    description: "Learn modern JavaScript ES6+ features with practical examples and coding exercises"
);
Console.WriteLine($"Extracted keywords: {string.Join(", ", keywords)}");

// Example 4: Recommend optimal posting times for a channel
var postingTimes = await titleOptimizationEngine.RecommendPostingTimesAsync(
    channelId: 1,
    count: 5
);

Console.WriteLine("Recommended posting times:");
foreach (var time in postingTimes)
{
    Console.WriteLine($"- {time:yyyy-MM-dd HH:mm:ss} UTC");
}
```

## ThumbnailAbTestRepository

The `ThumbnailAbTestRepository` class provides data access operations for managing thumbnail variant entities used in A/B testing for YouTube Shorts thumbnails. It implements the `IRepository<ThumbnailVariant>` interface and offers specialized methods for querying thumbnail variants by video ID, retrieving active variants, finding winners, and managing the complete lifecycle of thumbnail testing entities.

The repository handles database operations for thumbnail variants including creation, retrieval, updating, and deletion, with support for tracking impression counts, click-through rates, and determining winning variants based on performance metrics.

**Public Members:**
- `GetByIdAsync()` - Retrieves a thumbnail variant by its unique identifier
- `GetAllAsync()` - Retrieves all thumbnail variants from the database
- `GetByVideoShortIdAsync()` - Retrieves all variants associated with a specific video
- `GetActiveVariantsAsync()` - Returns only the currently-active (serving) variants for a given video
- `GetWinnerAsync()` - Returns the winning variant for a video, or null if no winner has been declared yet
- `AddAsync()` - Creates a new thumbnail variant in the database
- `UpdateAsync()` - Updates an existing thumbnail variant
- `DeleteAsync()` - Removes a thumbnail variant from the database
- `ExistsAsync()` - Checks if a thumbnail variant with the given ID exists
- `CountAsync()` - Returns the total count of thumbnail variants
- `SaveChangesAsync()` - Persists changes to the database


**Usage Example:**


```csharp
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

// Initialize repository (typically via dependency injection)
var services = new ServiceCollection();
services.AddLogging();
services.AddDbContext<DatabaseContext>(options => options.UseSqlServer("YourConnectionString"));
var serviceProvider = services.BuildServiceProvider();
var repository = serviceProvider.GetRequiredService<ThumbnailAbTestRepository>();

// Example 1: Create a new thumbnail variant for A/B testing
var variantA = new ThumbnailVariant
{
    VideoShortId = 42,
    Label = "Variant A",
    ThumbnailPath = "/thumbnails/video_42_a.jpg",
    ImpressionCount = 0,
    ClickCount = 0,
    ViewRate = 0.0,
    IsActive = true,
    IsWinner = false,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await repository.AddAsync(variantA);
Console.WriteLine($"Created thumbnail variant {variantA.Id} for video {variantA.VideoShortId}");

// Example 2: Get all variants for a video
var videoVariants = await repository.GetByVideoShortIdAsync(videoShortId: 42);
Console.WriteLine($"Found {videoVariants.Count()} variants for video 42");

foreach (var variant in videoVariants)
{
    Console.WriteLine($"- Variant {variant.Id} ({variant.Label}): {variant.ThumbnailPath}");
    Console.WriteLine($"  Active: {variant.IsActive}, Winner: {variant.IsWinner}");
    Console.WriteLine($"  Impressions: {variant.ImpressionCount}, Clicks: {variant.ClickCount}");
}

// Example 3: Get active variants for serving traffic
var activeVariants = await repository.GetActiveVariantsAsync(videoShortId: 42);
Console.WriteLine($"Found {activeVariants.Count()} active variants for video 42");

// Example 4: Record impression and click events
var variantToUpdate = activeVariants.First();
variantToUpdate.ImpressionCount += 100;
variantToUpdate.ClickCount += 15;
variantToUpdate.ViewRate = variantToUpdate.ClickCount / (double)variantToUpdate.ImpressionCount * 100;

await repository.UpdateAsync(variantToUpdate);
Console.WriteLine($"Updated variant {variantToUpdate.Id}: {variantToUpdate.ImpressionCount} impressions, {variantToUpdate.ClickCount} clicks");

// Example 5: Declare a winner when sufficient data is collected
var winnerCandidate = videoVariants.OrderByDescending(v => v.ViewRate).First();
winnerCandidate.IsWinner = true;
winnerCandidate.IsActive = false; // Deactivate other variants

foreach (var variant in videoVariants.Where(v => v.Id != winnerCandidate.Id))
{
    variant.IsActive = false;
}

await repository.UpdateAsync(winnerCandidate);
await repository.UpdateRangeAsync(videoVariants.Where(v => v.Id != winnerCandidate.Id));
Console.WriteLine($"🎉 Variant {winnerCandidate.Label} declared winner with {winnerCandidate.ViewRate:F2}% view rate!");

// Example 6: Get the winning variant for a video
var winningVariant = await repository.GetWinnerAsync(videoShortId: 42);
if (winningVariant != null)
{
    Console.WriteLine($"Winning variant: {winningVariant.Label}");
    Console.WriteLine($"- Thumbnail: {winningVariant.ThumbnailPath}");
    Console.WriteLine($"- View Rate: {winningVariant.ViewRate:F2}%");
    Console.WriteLine($"- Impressions: {winningVariant.ImpressionCount:N0}");
    Console.WriteLine($"- Clicks: {winningVariant.ClickCount:N0}");
}

// Example 7: Check if variant exists and count total variants
bool variantExists = await repository.ExistsAsync(variantA.Id);
int totalVariants = await repository.CountAsync();
Console.WriteLine($"Variant {variantA.Id} exists: {variantExists}");
Console.WriteLine($"Total variants in database: {totalVariants}");

// Example 8: Delete a variant (e.g., after test conclusion)
var oldVariant = videoVariants.FirstOrDefault(v => !v.IsActive && !v.IsWinner);
if (oldVariant != null)
{
    bool deleted = await repository.DeleteAsync(oldVariant.Id);
    Console.WriteLine($"Deleted variant {oldVariant.Id}: {(deleted ? "Success" : "Failed")}");
}
```

## ThumbnailAbTestService