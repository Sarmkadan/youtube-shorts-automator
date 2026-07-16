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

## ContentCalendarRepository

The `ContentCalendarRepository` class provides data access operations for managing content calendar entries that define the scheduling and planning of YouTube Shorts content. It offers comprehensive CRUD operations for content calendar entries with specialized methods for querying by date ranges, retrieving upcoming content, and checking content existence.

The repository handles database operations for content calendar entries including creation, retrieval, updating, and deletion, with support for managing content schedules, publication dates, and associated metadata.

**Public Members:**
- `GetByIdAsync()` - Retrieves a content calendar entry by its unique identifier
- `GetAllAsync()` - Retrieves all content calendar entries from the database
- `GetByDateRangeAsync()` - Retrieves content calendar entries within a specific date range
- `GetUpcomingAsync()` - Retrieves content calendar entries that are scheduled for future publication dates
- `AddAsync()` - Creates a new content calendar entry in the database
- `UpdateAsync()` - Updates an existing content calendar entry
- `DeleteAsync()` - Removes a content calendar entry from the database
- `ExistsAsync()` - Checks if a content calendar entry with the given ID exists
- `CountAsync()` - Returns the total count of content calendar entries
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
var repository = serviceProvider.GetRequiredService<ContentCalendarRepository>();

// Example 1: Create a new content calendar entry
var entry1 = new ContentCalendarEntry
{
    VideoShortId = 1,
    ScheduledDate = DateTime.UtcNow.Date.AddDays(7),
    Status = ContentStatus.Scheduled,
    Title = "Advanced C# Features Tutorial",
    Description = "Deep dive into C# 10 features with practical examples",
    Tags = new[] { "csharp", "dotnet", "tutorial", "advanced" },
    Priority = ContentPriority.High,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await repository.AddAsync(entry1);
Console.WriteLine($"Created content calendar entry {entry1.Id} for video {entry1.VideoShortId}");

// Example 2: Get a content calendar entry by ID
var existingEntry = await repository.GetByIdAsync(entry1.Id);
if (existingEntry != null)
{
    Console.WriteLine($"Found entry: {existingEntry.Title}");
    Console.WriteLine($"Scheduled for: {existingEntry.ScheduledDate:yyyy-MM-dd}");
    Console.WriteLine($"Status: {existingEntry.Status}");
}

// Example 3: Get all content calendar entries
var allEntries = await repository.GetAllAsync();
Console.WriteLine($"Total content calendar entries: {allEntries.Count()}");

// Example 4: Get entries within a specific date range
var dateRangeEntries = await repository.GetByDateRangeAsync(
    startDate: DateTime.UtcNow.Date,
    endDate: DateTime.UtcNow.Date.AddDays(30)
);
Console.WriteLine($"Entries in next 30 days: {dateRangeEntries.Count()}");

// Example 5: Get upcoming content (future scheduled entries)
var upcomingEntries = await repository.GetUpcomingAsync();
Console.WriteLine($"Upcoming content entries: {upcomingEntries.Count()}");

// Example 6: Update an existing entry
if (existingEntry != null)
{
    existingEntry.Status = ContentStatus.InProgress;
    existingEntry.UpdatedAt = DateTime.UtcNow;
    await repository.UpdateAsync(existingEntry);
    Console.WriteLine("Updated entry status to InProgress");
}

// Example 7: Check if entry exists and get total count
bool entryExists = await repository.ExistsAsync(entry1.Id);
int totalEntries = await repository.CountAsync();
Console.WriteLine($"Entry {entry1.Id} exists: {entryExists}");
Console.WriteLine($"Total entries in database: {totalEntries}");

// Example 8: Delete a content calendar entry
bool deleted = await repository.DeleteAsync(entry1.Id);
Console.WriteLine($"Deleted entry {entry1.Id}: {(deleted ? "Success" : "Failed")}");

// Don't forget to save changes after batch operations
await repository.SaveChangesAsync();
```

## UploadJobRepository

The `UploadJobRepository` class provides data access operations for managing upload job entities in the YouTube Shorts automation system. It implements the `IRepository<UploadJob>` interface and offers specialized methods for querying upload jobs by status, retrieving jobs scheduled for upload, and finding retryable failed jobs. The repository handles all database operations for upload jobs including creation, retrieval, updating, and deletion, with support for tracking upload progress, retry attempts, and error messages.

**Public Members:**
- `GetByIdAsync()` - Retrieves an upload job by its unique identifier
- `GetAllAsync()` - Retrieves all upload jobs from the database
- `GetByStatusAsync()` - Retrieves upload jobs filtered by their status
- `GetScheduledForUploadAsync()` - Retrieves upload jobs that are scheduled for upload and ready to be processed
- `GetRetryableFailedJobsAsync()` - Retrieves failed upload jobs that are eligible for retry based on their attempt count
- `AddAsync()` - Adds a new upload job to the database
- `UpdateAsync()` - Updates an existing upload job in the database
- `DeleteAsync()` - Deletes an upload job from the database by its identifier
- `ExistsAsync()` - Checks if an upload job with the specified identifier exists in the database
- `CountAsync()` - Returns the total count of upload jobs in the database
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
var repository = serviceProvider.GetRequiredService<UploadJobRepository>();

// Example 1: Create a new upload job for a video
var uploadJob = new UploadJob
{
    VideoShortId = 42,
    Status = UploadStatus.Queued,
    ScheduledAt = DateTime.UtcNow.AddHours(2),
    MaxRetries = 3,
    AttemptCount = 0,
    UploadProgressPercentage = 0.0,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await repository.AddAsync(uploadJob);
Console.WriteLine($"Created upload job {uploadJob.Id} for video {uploadJob.VideoShortId}");

// Example 2: Get an upload job by ID
var existingJob = await repository.GetByIdAsync(uploadJob.Id);
if (existingJob != null)
{
    Console.WriteLine($"Found upload job: Status={existingJob.Status}, Scheduled={existingJob.ScheduledAt:u}");
}

// Example 3: Get all upload jobs
var allJobs = await repository.GetAllAsync();
Console.WriteLine($"Total upload jobs: {allJobs.Count()}");

// Example 4: Get upload jobs by status
var queuedJobs = await repository.GetByStatusAsync(UploadStatus.Queued);
Console.WriteLine($"Queued jobs: {queuedJobs.Count()}");

var failedJobs = await repository.GetByStatusAsync(UploadStatus.Failed);
Console.WriteLine($"Failed jobs: {failedJobs.Count()}");

// Example 5: Get jobs scheduled for upload (ready to process)
var scheduledJobs = await repository.GetScheduledForUploadAsync();
Console.WriteLine($"Jobs ready for upload: {scheduledJobs.Count()}");

foreach (var job in scheduledJobs)
{
    Console.WriteLine($"- Job {job.Id}: Video {job.VideoShortId}, Scheduled: {job.ScheduledAt:u}");
}

// Example 6: Get retryable failed jobs (failed but not exceeded max retries)
var retryableJobs = await repository.GetRetryableFailedJobsAsync();
Console.WriteLine($"Retryable failed jobs: {retryableJobs.Count()}");

foreach (var job in retryableJobs)
{
    Console.WriteLine($"- Job {job.Id}: Attempt {job.AttemptCount}/{job.MaxRetries}, Error: {job.ErrorMessage}");
}

// Example 7: Update an upload job (e.g., after upload completes)
if (existingJob != null)
{
    existingJob.Status = UploadStatus.Completed;
    existingJob.UploadedAt = DateTime.UtcNow;
    existingJob.YouTubeVideoId = "dQw4w9WgXcQ"; // YouTube video ID
    existingJob.UploadProgressPercentage = 100.0;
    existingJob.UpdatedAt = DateTime.UtcNow;
    
    await repository.UpdateAsync(existingJob);
    Console.WriteLine("Updated upload job status to Completed");
}

// Example 8: Check if job exists and get total count
bool jobExists = await repository.ExistsAsync(uploadJob.Id);
int totalJobs = await repository.CountAsync();
Console.WriteLine($"Job {uploadJob.Id} exists: {jobExists}");
Console.WriteLine($"Total jobs in database: {totalJobs}");

// Example 9: Delete a completed upload job
bool deleted = await repository.DeleteAsync(uploadJob.Id);
Console.WriteLine($"Deleted job {uploadJob.Id}: {(deleted ? "Success" : "Failed")}");
```

## DatabaseContext

The `DatabaseContext` class provides a lightweight database access layer for executing SQL commands against a Microsoft SQL Server database. It manages database connections, executes parameterized SQL commands, and returns results in various formats including scalar values, DataTables, and row counts. The context is designed for direct SQL execution scenarios where Entity Framework's DbContext is not required, providing lower-level control over database operations.



**Public Members:**
- `GetConnectionAsync()` - Opens and returns a SQL Server connection
- `ExecuteCommandAsync()` - Executes a parameterized SQL command and returns the number of affected rows
- `ExecuteScalarAsync<T>()` - Executes a scalar SQL query and returns a single value of type T
- `ExecuteDataTableAsync()` - Executes a SQL query and returns results as a DataTable
- `DisposeAsync()` - Properly closes and disposes the database connection


**Usage Example:**

```csharp
using YouTubeShortAutomator.Data;
using System.Data;

// Initialize database context with connection string
var connectionString = "Server=localhost;Database=YouTubeShortsAutomator;User Id=sa;Password=YourPassword;";
var databaseContext = new DatabaseContext(connectionString);

// Example 1: Execute a simple SQL command
int rowsAffected = await databaseContext.ExecuteCommandAsync(
    "UPDATE Videos SET Status = 'Processed' WHERE VideoShortId = @id",
    parameters: new Dictionary<string, object?> { { "@id", 42 } 
);
Console.WriteLine($"Updated {rowsAffected} rows");

// Example 2: Execute a scalar query to get a count
int videoCount = await databaseContext.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM Videos WHERE Status = 'Queued'"
);
Console.WriteLine($"Queued videos: {videoCount}");

// Example 3: Execute a parameterized query with multiple parameters
var parameters = new Dictionary<string, object?> 
{
    { "@title", "Learn C# in 30 Days" },
    { "@description", "A comprehensive guide to mastering C#" },
    { "@channelId", 1 }
};

int newVideoId = await databaseContext.ExecuteScalarAsync<int>(
    @"INSERT INTO Videos (Title, Description, ChannelId, CreatedAt)
      VALUES (@title, @description, @channelId, GETUTCDATE());
      SELECT SCOPE_IDENTITY()",
    parameters: parameters
);
Console.WriteLine($"Created new video with ID: {newVideoId}");

// Example 4: Execute a query and return results as DataTable
var dataTable = await databaseContext.ExecuteDataTableAsync(
    "SELECT VideoShortId, Title, Status, CreatedAt FROM Videos WHERE Status = 'Queued'"
);

foreach (DataRow row in dataTable.Rows)
{
    Console.WriteLine($"Video {row["VideoShortId"]}: {row["Title"]} - {row["Status"]}");
}

// Example 5: Execute a command with CommandType.StoredProcedure
int result = await databaseContext.ExecuteCommandAsync(
    "sp_UpdateVideoStatus",
    commandType: CommandType.StoredProcedure,
    parameters: new Dictionary<string, object?> { { "@videoId", 42 }, { "@newStatus", "Processed" } }
);
Console.WriteLine($"Stored procedure executed, result: {result}");

// Example 6: Get a database connection for direct ADO.NET usage
using var connection = await databaseContext.GetConnectionAsync();
Console.WriteLine($"Connection opened: {connection.State}");

// The context should be disposed when done (typically via using statement or async disposal)
await databaseContext.DisposeAsync();
```

## ThumbnailAbTestService