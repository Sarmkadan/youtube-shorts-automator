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

## IntegrationTests

The `IntegrationTests` class provides comprehensive integration testing for the complete YouTube Shorts automation pipeline. It validates end-to-end workflows including video processing, upload scheduling, analytics synchronization, and concurrent operation handling across the entire system.



This test suite exercises the real integration points between components, ensuring that the automation pipeline works correctly when all services are wired together through dependency injection and actual database operations.

**Public Members:**
- `EndToEnd_ScheduleUpload_CompletesSuccessfully` - Tests the complete pipeline from video scheduling to successful upload completion
- `EndToEnd_CreateVideo_CreateAnalytics_SyncMetrics` - Validates video creation, analytics generation, and metrics synchronization workflow
- `ConcurrencyTest_MultipleSchedulesSimultaneously` - Tests concurrent schedule creation and processing
- `ConcurrencyTest_MultipleVideoCreationsSimultaneously` - Validates concurrent video processing operations
- `ConfigurationTest_DifferentProcessingProfiles` - Tests various processing profile configurations
- `SchedulingWorkflow_CreateScheduleAndRetrieveUpcoming` - Validates schedule creation and retrieval workflow
- `SchedulingWorkflow_RescheduleAndVerify` - Tests schedule modification and verification
- `SchedulingWorkflow_CancelUpload` - Validates upload cancellation functionality
- `AnalyticsWorkflow_CreateAndGenerateReport` - Tests analytics generation and reporting workflow
- `FileValidationWorkflow_ValidateAndHash` - Validates file validation and hashing operations
- `MainUseCase_ProcessVideoAndScheduleUpload` - Tests the primary use case of processing video and scheduling upload
- `Dispose` - Cleanup method for test resources


**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Initialize integration test suite with dependency injection
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());
services.AddDbContext<DatabaseContext>(options => 
    options.UseSqlServer("Server=localhost;Database=YouTubeShortsAutomator;User Id=sa;Password=YourPassword;"));
services.AddScoped<IntegrationTests>();

var serviceProvider = services.BuildServiceProvider();
var integrationTests = serviceProvider.GetRequiredService<IntegrationTests>();

// Example 1: Test complete end-to-end video processing and upload pipeline
var pipelineResult = await integrationTests.EndToEnd_ScheduleUpload_CompletesSuccessfully();
Console.WriteLine($"Pipeline completed: {pipelineResult.Status}");
Console.WriteLine($"Video ID: {pipelineResult.VideoShortId}, Upload Job ID: {pipelineResult.UploadJobId}");

// Example 2: Test concurrent schedule creation
var concurrentResult = await integrationTests.ConcurrencyTest_MultipleSchedulesSimultaneously(5);
Console.WriteLine($"Created {concurrentResult} concurrent schedules successfully");

// Example 3: Test different processing profiles
var profileResult = await integrationTests.ConfigurationTest_DifferentProcessingProfiles();
Console.WriteLine($"Successfully tested {profileResult} different processing profiles");

// Example 4: Test analytics synchronization workflow
var analyticsResult = await integrationTests.AnalyticsWorkflow_CreateAndGenerateReport();
Console.WriteLine($"Analytics sync completed: {analyticsResult.SyncedCount} videos, {analyticsResult.FailedCount} failed");

// Example 5: Test file validation and hashing workflow
var fileResult = await integrationTests.FileValidationWorkflow_ValidateAndHash("/videos/sample.mp4");
Console.WriteLine($"File validation result: {fileResult.IsValid}, Hash: {fileResult.FileHash}");

// Cleanup
integrationTests.Dispose();
```

## VideoShortModelTests

The `VideoShortModelTests` class contains unit tests for the `VideoShort` model validation and processing logic. It verifies that video metadata validation works correctly and that the processing status management functions as expected.

This test suite validates the core business rules for video shorts including title requirements, duration constraints, and processing state transitions.

**Public Members:**
- `IsValid_WithValidMetadata_ReturnsTrue()` - Verifies that a video with valid metadata passes validation
- `IsValid_WithEmptyTitle_ReturnsFalse()` - Verifies that a video with an empty title fails validation
- `IsValid_WithDurationBeyond60Seconds_ReturnsFalse()` - Verifies that a video exceeding 60 seconds duration fails validation
- `MarkAsProcessed_WithoutError_SetsCompletedStatusAndTimestamp()` - Verifies successful processing status update
- `MarkAsProcessed_WithErrorMessage_SetsFailedStatusAndPreservesError()` - Verifies error handling during processing

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using FluentAssertions;

// Example 1: Test valid video metadata
var validVideo = new VideoShort
{
    Title = "Learn C# in 30 Days",
    Description = "A comprehensive guide to mastering C# programming",
    FilePath = "/videos/learn-csharp.mp4",
    Duration = TimeSpan.FromSeconds(45),
    ProcessingProfileId = 1,
    YouTubeChannelId = 1
};

bool isValid = validVideo.IsValid();
isValid.Should().BeTrue();

// Example 2: Test invalid video (empty title)
var invalidVideo = new VideoShort
{
    Title = "",
    Description = "A comprehensive guide",
    FilePath = "/videos/test.mp4",
    Duration = TimeSpan.FromSeconds(30),
    ProcessingProfileId = 1,
    YouTubeChannelId = 1
};

bool isInvalid = invalidVideo.IsValid();
isInvalid.Should().BeFalse();

// Example 3: Test video exceeding duration limit
var longVideo = new VideoShort
{
    Title = "Long Tutorial",
    Description = "A very long tutorial",
    FilePath = "/videos/long.mp4",
    Duration = TimeSpan.FromMinutes(2), // Exceeds 60 seconds limit
    ProcessingProfileId = 1,
    YouTubeChannelId = 1
};

bool isTooLong = longVideo.IsValid();
isTooLong.Should().BeFalse();

// Example 4: Test successful processing completion
var videoToProcess = new VideoShort
{
    Title = "Test Video",
    Description = "Test description",
    FilePath = "/videos/test.mp4",
    Duration = TimeSpan.FromSeconds(30),
    ProcessingProfileId = 1,
    YouTubeChannelId = 1,
    Status = ProcessingStatus.Processing
};

videoToProcess.MarkAsProcessed();
videoToProcess.Status.Should().Be(ProcessingStatus.Completed);
videoToProcess.ErrorMessage.Should().BeNull();
videoToProcess.ProcessedAt.Should().NotBeNull();

// Example 5: Test processing failure with error message
var videoToFail = new VideoShort
{
    Title = "Test Video",
    Description = "Test description",
    FilePath = "/videos/test.mp4",
    Duration = TimeSpan.FromSeconds(30),
    ProcessingProfileId = 1,
    YouTubeChannelId = 1,
    Status = ProcessingStatus.Processing
};

const string errorMessage = "FFmpeg process exited with code 1";
videoToFail.MarkAsProcessed(errorMessage);
videoToFail.Status.Should().Be(ProcessingStatus.Failed);
videoToFail.ErrorMessage.Should().Be(errorMessage);
videoToFail.ProcessedAt.Should().BeNull();
```

## FileValidationServiceTests

The `FileValidationServiceTests` class contains unit tests for the `FileValidationService` that validate video file formats, sizes, and integrity. It tests file existence checks, format validation against supported extensions, file size limits, hash generation for change detection, and video duration extraction using FFmpeg.

This test suite ensures that only valid video files are processed by the YouTube Shorts automation pipeline and provides utilities for detecting file changes through content hashing.

**Public Members:**
- `ValidateVideoFile_WithValidMp4File_ReturnsTrue` - Validates that a proper MP4 file passes all checks
- `ValidateVideoFile_WithNullPath_ThrowsArgumentException` - Verifies null path handling
- `ValidateVideoFile_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling
- `ValidateVideoFile_WithNonExistentFile_ReturnsFalse` - Validates behavior with missing files
- `ValidateVideoFile_WithFileTooLarge_ReturnsFalse` - Validates file size limits
- `ValidateVideoFile_WithFileTooSmall_ReturnsFalse` - Validates minimum file size requirements
- `ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse` - Validates format restrictions
- `ValidateVideoFile_WithEmptyFile_ReturnsFalse` - Validates handling of empty files
- `ValidateVideoFile_WithSupportedFormats_ReturnsTrue` - Validates multiple supported formats
- `GetFileHash_WithValidFile_ReturnsConsistentHash` - Tests hash generation consistency
- `GetFileHash_WithNullPath_ThrowsArgumentException` - Verifies null path handling for hashing
- `GetFileHash_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling for hashing
- `GetFileHash_WithNonExistentFile_ThrowsInvalidOperationException` - Validates error handling for missing files
- `GetFileHash_WithDifferentFiles_ReturnsDifferentHashes` - Verifies hash uniqueness
- `GetVideoDuration_WithExistentFile_ReturnsTimeSpan` - Tests duration extraction
- `GetVideoDuration_WithNullPath_ThrowsArgumentException` - Verifies null path handling for duration
- `GetVideoDuration_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling for duration
- `GetVideoDuration_WithNonExistentFile_ReturnsNull` - Validates behavior with missing files
- `DeleteTemporaryFile_WithExistingFile_DeletesFile` - Tests file cleanup functionality

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<FileValidationServiceTests>();
var fileValidationServiceTests = new FileValidationServiceTests(logger);

// Example 1: Validate a valid MP4 file
bool isValidMp4 = fileValidationServiceTests.ValidateVideoFile_WithValidMp4File_ReturnsTrue();
Assert.True(isValidMp4);

// Example 2: Validate an unsupported format (MKV)
bool isValidMkv = fileValidationServiceTests.ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse();
Assert.False(isValidMkv);

// Example 3: Validate file size limits
bool isValidSize = fileValidationServiceTests.ValidateVideoFile_WithFileTooLarge_ReturnsFalse();
Assert.False(isValidSize);

// Example 4: Generate file hash for change detection
string fileHash = fileValidationServiceTests.GetFileHash_WithValidFile_ReturnsConsistentHash();
Assert.NotNull(fileHash);
Assert.Equal(64, fileHash.Length); // SHA256 hash length

// Example 5: Get video duration from file
TimeSpan? duration = fileValidationServiceTests.GetVideoDuration_WithExistentFile_ReturnsTimeSpan();
Assert.NotNull(duration);
Assert.True(duration.Value.TotalSeconds > 0);

// Example 6: Test null path validation
Assert.Throws<ArgumentException>(() => fileValidationServiceTests.GetFileHash_WithNullPath_ThrowsArgumentException());

// Example 7: Test empty file handling
bool isValidEmptyFile = fileValidationServiceTests.ValidateVideoFile_WithEmptyFile_ReturnsFalse();
Assert.False(isValidEmptyFile);

// Example 8: Test temporary file cleanup
bool deleted = fileValidationServiceTests.DeleteTemporaryFile_WithExistingFile_DeletesFile();
Assert.True(deleted);
```

## AnalyticsServiceTests

The `AnalyticsServiceTests` class contains unit tests for the `AnalyticsService` that validate analytics data creation, synchronization from YouTube, retrieval, performance analysis, and report generation functionality. It tests various scenarios including successful operations, error handling, edge cases, and data validation to ensure the analytics service works correctly across different use cases.

This test suite validates the complete analytics workflow including video analytics tracking, performance metrics analysis, channel growth calculations, and custom report generation with comprehensive error handling and boundary condition testing.

**Public Members:**
- `CreateAnalyticsRecordAsync_WithValidVideoId_CreatesRecord` - Validates successful analytics record creation
- `CreateAnalyticsRecordAsync_WithRepositoryException_ThrowsInvalidOperationException` - Tests error handling during record creation
- `SyncAnalyticsFromYouTubeAsync_WithValidInputs_UpdatesAnalytics` - Validates YouTube data synchronization
- `SyncAnalyticsFromYouTubeAsync_WithNullYoutubeId_ThrowsArgumentNullException` - Tests null parameter validation
- `SyncAnalyticsFromYouTubeAsync_WithNullChannel_ThrowsArgumentNullException` - Tests null parameter validation
- `SyncAnalyticsFromYouTubeAsync_WhenNoExistingRecord_CreatesNew` - Tests behavior when no existing record exists
- `GetVideoAnalyticsAsync_WithValidVideoId_ReturnsAnalytics` - Validates analytics retrieval
- `GetVideoAnalyticsAsync_WhenNotFound_ReturnsNull` - Tests handling of non-existent records
- `GetTopPerformingVideosAsync_WithValidLimit_ReturnsTopVideos` - Validates top videos retrieval
- `GetTopPerformingVideosAsync_WithInvalidLimit_ThrowsArgumentOutOfRangeException` - Tests parameter validation
- `GetTopPerformingVideosAsync_WithNegativeLimit_ThrowsArgumentOutOfRangeException` - Tests parameter validation
- `GeneratePeriodReportAsync_WithValidDateRange_ReturnsReport` - Validates report generation
- `GeneratePeriodReportAsync_WithInvertedDates_ThrowsArgumentException` - Tests date range validation
- `GeneratePeriodReportAsync_WithEmptyAnalytics_ReturnsEmptyReport` - Tests edge case handling
- `AnalyzePerformanceMetrics_WithValidData_ReturnsInsights` - Validates performance analysis
- `AnalyzePerformanceMetrics_WithLowEngagement_ReturnsWarning` - Tests warning generation
- `AnalyzePerformanceMetrics_WithoutValidData_ReturnsDefaultMessage` - Tests default behavior
- `CalculateChannelGrowthAsync_WithMultipleAnalytics_ReturnsTotalGrowth` - Validates growth calculations
- `CalculateChannelGrowthAsync_WithNegativeGrowth_ReturnsNegativeValue` - Tests negative growth scenarios

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<AnalyticsServiceTests>();
var analyticsServiceTests = new AnalyticsServiceTests(logger);

// Example 1: Test successful analytics record creation
var createResult = await analyticsServiceTests.CreateAnalyticsRecordAsync_WithValidVideoId_CreatesRecord();
Assert.NotNull(createResult);
Assert.Equal("UC1234567890", createResult.YouTubeChannelId);

// Example 2: Test YouTube data synchronization
var syncResult = await analyticsServiceTests.SyncAnalyticsFromYouTubeAsync_WithValidInputs_UpdatesAnalytics();
Assert.True(syncResult);
Assert.Equal(5, syncResult.UpdatedCount);

// Example 3: Test analytics retrieval
var analytics = await analyticsServiceTests.GetVideoAnalyticsAsync_WithValidVideoId_ReturnsAnalytics();
Assert.NotNull(analytics);
Assert.Equal("dQw4w9WgXcQ", analytics.YouTubeVideoId);
Assert.Equal(1500, analytics.ViewCount);
Assert.Equal(75, analytics.LikeCount);

// Example 4: Test top performing videos retrieval
var topVideos = await analyticsServiceTests.GetTopPerformingVideosAsync_WithValidLimit_ReturnsTopVideos();
Assert.NotNull(topVideos);
Assert.Equal(3, topVideos.Count());
Assert.All(topVideos, video => Assert.True(video.ViewCount > 1000));

// Example 5: Test period report generation
var report = await analyticsServiceTests.GeneratePeriodReportAsync_WithValidDateRange_ReturnsReport();
Assert.NotNull(report);
Assert.Equal(2, report.Periods.Count);
Assert.Equal(7500, report.TotalViews);

// Example 6: Test performance analysis
var insights = analyticsServiceTests.AnalyzePerformanceMetrics_WithValidData_ReturnsInsights();
Assert.NotNull(insights);
Assert.Contains("high", insights.ToLower());

// Example 7: Test channel growth calculation
var growth = await analyticsServiceTests.CalculateChannelGrowthAsync_WithMultipleAnalytics_ReturnsTotalGrowth();
Assert.NotNull(growth);
Assert.True(growth.TotalGrowthPercentage > 0);

// Example 8: Test error handling - null YouTube ID
Assert.Throws<ArgumentNullException>(() => 
    analyticsServiceTests.SyncAnalyticsFromYouTubeAsync_WithNullYoutubeId_ThrowsArgumentNullException());
```

**Public Members:**
- `ValidateVideoFile_WithValidMp4File_ReturnsTrue` - Validates that a proper MP4 file passes all checks
- `ValidateVideoFile_WithNullPath_ThrowsArgumentException` - Verifies null path handling
- `ValidateVideoFile_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling
- `ValidateVideoFile_WithNonExistentFile_ReturnsFalse` - Validates behavior with missing files
- `ValidateVideoFile_WithFileTooLarge_ReturnsFalse` - Validates file size limits
- `ValidateVideoFile_WithFileTooSmall_ReturnsFalse` - Validates minimum file size requirements
- `ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse` - Validates format restrictions
- `ValidateVideoFile_WithEmptyFile_ReturnsFalse` - Validates handling of empty files
- `ValidateVideoFile_WithSupportedFormats_ReturnsTrue` - Validates multiple supported formats
- `GetFileHash_WithValidFile_ReturnsConsistentHash` - Tests hash generation consistency
- `GetFileHash_WithNullPath_ThrowsArgumentException` - Verifies null path handling for hashing
- `GetFileHash_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling for hashing
- `GetFileHash_WithNonExistentFile_ThrowsInvalidOperationException` - Validates error handling for missing files
- `GetFileHash_WithDifferentFiles_ReturnsDifferentHashes` - Verifies hash uniqueness
- `GetVideoDuration_WithExistentFile_ReturnsTimeSpan` - Tests duration extraction
- `GetVideoDuration_WithNullPath_ThrowsArgumentException` - Verifies null path handling for duration
- `GetVideoDuration_WithEmptyPath_ThrowsArgumentException` - Verifies empty path handling for duration
- `GetVideoDuration_WithNonExistentFile_ReturnsNull` - Validates behavior with missing files
- `DeleteTemporaryFile_WithExistingFile_DeletesFile` - Tests file cleanup functionality

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<FileValidationServiceTests>();
var fileValidationServiceTests = new FileValidationServiceTests(logger);

// Example 1: Validate a valid MP4 file
bool isValidMp4 = fileValidationServiceTests.ValidateVideoFile_WithValidMp4File_ReturnsTrue();
Assert.True(isValidMp4);

// Example 2: Validate an unsupported format (MKV)
bool isValidMkv = fileValidationServiceTests.ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse();
Assert.False(isValidMkv);

// Example 3: Validate file size limits
bool isValidSize = fileValidationServiceTests.ValidateVideoFile_WithFileTooLarge_ReturnsFalse();
Assert.False(isValidSize);

// Example 4: Generate file hash for change detection
string fileHash = fileValidationServiceTests.GetFileHash_WithValidFile_ReturnsConsistentHash();
Assert.NotNull(fileHash);
Assert.Equal(64, fileHash.Length); // SHA256 hash length

// Example 5: Get video duration from file
TimeSpan? duration = fileValidationServiceTests.GetVideoDuration_WithExistentFile_ReturnsTimeSpan();
Assert.NotNull(duration);
Assert.True(duration.Value.TotalSeconds > 0);

// Example 6: Test null path validation
Assert.Throws<ArgumentException>(() => fileValidationServiceTests.GetFileHash_WithNullPath_ThrowsArgumentException());

// Example 7: Test empty file handling
bool isValidEmptyFile = fileValidationServiceTests.ValidateVideoFile_WithEmptyFile_ReturnsFalse();
Assert.False(isValidEmptyFile);

// Example 8: Test temporary file cleanup
bool deleted = fileValidationServiceTests.DeleteTemporaryFile_WithExistingFile_DeletesFile();
Assert.True(deleted);
```

## VideoProcessingServiceTests

The `VideoProcessingServiceTests` class contains unit tests for the `VideoProcessingService` that validate video processing functionality including file validation, task creation, video processing, and status management. It tests various scenarios including successful operations, error handling, validation edge cases, and ensures the processing pipeline works correctly across different profiles and configurations.

This test suite validates the complete video processing workflow including validation exceptions, repository errors, priority settings, and FFmpeg transcoding task creation with comprehensive error handling and boundary condition testing.

**Public Members:**
- `ValidateVideoFileAsync_WithValidFile_ReturnsTrue` - Validates that a proper video file passes all validation checks
- `ValidateVideoFileAsync_WithNonExistentFile_ReturnsFalse` - Validates behavior with missing files
- `ValidateVideoFileAsync_WithFileTooLarge_ReturnsFalse` - Validates file size limits
- `ValidateVideoFileAsync_WithFileTooSmall_ReturnsFalse` - Validates minimum file size requirements
- `CreateProcessingTaskAsync_WithValidVideo_CreatesTask` - Tests successful task creation with valid video
- `CreateProcessingTaskAsync_WithInvalidVideo_ThrowsValidationException` - Validates error handling for invalid videos
- `CreateProcessingTaskAsync_WithDurationTooLong_ThrowsValidationException` - Tests duration validation
- `CreateProcessingTaskAsync_WithRepositoryException_ThrowsVideoProcessingException` - Tests repository error handling
- `CreateProcessingTaskAsync_SetsStatusToPending` - Validates status management
- `CreateProcessingTaskAsync_SetsCreatedAtToCurrentTime` - Validates timestamp setting
- `ProcessVideoAsync_WithValidInputs_ReturnsProcessingTask` - Tests complete video processing workflow
- `ProcessVideoAsync_WithInvalidProfile_ThrowsValidationException` - Validates profile validation
- `ProcessVideoAsync_SetsCorrectPriority` - Tests priority setting functionality
- `ProcessVideoAsync_WithDifferentProfiles_AppliesCorrectSettings` - Tests profile application
- `ProcessVideoAsync_Sets_FFmpegTranscodeTaskType` - Validates task type assignment

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<VideoProcessingServiceTests>();
var videoProcessingServiceTests = new VideoProcessingServiceTests(logger);

// Example 1: Validate a valid video file
bool isValidFile = await videoProcessingServiceTests.ValidateVideoFileAsync_WithValidFile_ReturnsTrue();
Assert.True(isValidFile);

// Example 2: Validate file size limits
bool isValidSize = await videoProcessingServiceTests.ValidateVideoFileAsync_WithFileTooLarge_ReturnsFalse();
Assert.False(isValidSize);

// Example 3: Create processing task with valid video
var processingTask = await videoProcessingServiceTests.CreateProcessingTaskAsync_WithValidVideo_CreatesTask();
Assert.NotNull(processingTask);
Assert.Equal(ProcessingStatus.Pending, processingTask.Status);

// Example 4: Test duration validation
await Assert.ThrowsAsync<ValidationException>(() => 
    videoProcessingServiceTests.CreateProcessingTaskAsync_WithDurationTooLong_ThrowsValidationException());

// Example 5: Process video with valid inputs
var processedTask = await videoProcessingServiceTests.ProcessVideoAsync_WithValidInputs_ReturnsProcessingTask();
Assert.NotNull(processedTask);
Assert.Equal(ProcessingStatus.Completed, processedTask.Status);
Assert.NotNull(processedTask.CompletedAt);

// Example 6: Test invalid profile validation
await Assert.ThrowsAsync<ValidationException>(() => 
    videoProcessingServiceTests.ProcessVideoAsync_WithInvalidProfile_ThrowsValidationException());

// Example 7: Test priority setting
var priorityTask = await videoProcessingServiceTests.ProcessVideoAsync_SetsCorrectPriority();
Assert.Equal(ProcessingPriority.High, priorityTask.Priority);

// Example 8: Test different profile application
var profileTask = await videoProcessingServiceTests.ProcessVideoAsync_WithDifferentProfiles_AppliesCorrectSettings();
Assert.Equal("High Quality", profileTask.ProcessingProfile.Name);
Assert.True(profileTask.ProcessingProfile.ApplyWatermark);
```

## SchedulingServiceTests

The `SchedulingServiceTests` class contains unit tests for the `SchedulingService` that validate upload scheduling functionality including job creation, time validation, status management, and retrieval operations. It tests various scheduling scenarios including future and past times, job state transitions, and ensures the scheduling system works correctly across different time zones and edge cases.

This test suite validates the complete upload scheduling workflow including job orchestration, retry logic, error handling, and comprehensive boundary condition testing for scheduling operations.

**Public Members:**
- `ScheduleUploadAsync_WithFutureTime_CreatesScheduledJob` - Validates successful scheduling for future times
- `ScheduleUploadAsync_WithPastTime_ThrowsSchedulingException` - Validates error handling for past times
- `ScheduleUploadAsync_WithNowTime_Succeeds` - Tests scheduling for current time
- `ScheduleUploadAsync_SetsMaxRetriesCorrectly` - Validates max retry configuration
- `GetUpcomingJobsAsync_WithPendingJobs_ReturnsScheduledJobs` - Tests retrieval of pending jobs
- `GetUpcomingJobsAsync_WithHoursAhead_FiltersByTimeWindow` - Validates time-based filtering
- `GetUpcomingJobsAsync_WithNoJobs_ReturnsEmpty` - Tests empty result handling
- `GetUpcomingJobsAsync_ReturnsJobsOrderedByScheduledTime` - Validates chronological ordering
- `GetOverdueJobsAsync_WithPastScheduledTime_ReturnsPendingJob` - Tests overdue job detection
- `GetOverdueJobsAsync_IgnoresNonPendingJobs` - Validates status filtering
- `RescheduleUploadAsync_WithValidJob_UpdatesScheduledTime` - Tests schedule modification
- `RescheduleUploadAsync_WithNonExistentJob_ThrowsSchedulingException` - Validates error handling for missing jobs
- `RescheduleUploadAsync_WithCompletedJob_ThrowsSchedulingException` - Tests completed job validation
- `RescheduleUploadAsync_WithUploadingJob_ThrowsSchedulingException` - Tests uploading job validation
- `RescheduleUploadAsync_WithPastTime_ThrowsSchedulingException` - Validates time validation
- `CancelUploadAsync_WithPendingJob_CancelsJob` - Tests job cancellation
- `CancelUploadAsync_WithNonExistentJob_ThrowsSchedulingException` - Validates error handling for missing jobs
- `CancelUploadAsync_WithUploadingJob_ThrowsSchedulingException` - Tests uploading job validation
- `GetQueuedJobCountAsync_ReturnsCountOfQueuedJobs` - Tests queue counting

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<SchedulingServiceTests>();
var schedulingServiceTests = new SchedulingServiceTests(logger);

// Example 1: Schedule upload for future time
var futureJob = await schedulingServiceTests.ScheduleUploadAsync_WithFutureTime_CreatesScheduledJob();
Assert.NotNull(futureJob);
Assert.Equal(UploadStatus.Scheduled, futureJob.Status);
Assert.True(futureJob.ScheduledAt > DateTime.UtcNow);

// Example 2: Attempt to schedule upload for past time (should throw)
await Assert.ThrowsAsync<SchedulingException>(() =>
  schedulingServiceTests.ScheduleUploadAsync_WithPastTime_ThrowsSchedulingException());

// Example 3: Schedule upload for current time
var nowJob = await schedulingServiceTests.ScheduleUploadAsync_WithNowTime_Succeeds();
Assert.NotNull(nowJob);
Assert.Equal(UploadStatus.Scheduled, nowJob.Status);

// Example 4: Get upcoming jobs within time window
var upcomingJobs = await schedulingServiceTests.GetUpcomingJobsAsync_WithPendingJobs_ReturnsScheduledJobs();
Assert.NotEmpty(upcomingJobs);
Assert.True(upcomingJobs.All(j => j.ScheduledAt > DateTime.UtcNow));

// Example 5: Get overdue jobs (past scheduled time)
var overdueJobs = await schedulingServiceTests.GetOverdueJobsAsync_WithPastScheduledTime_ReturnsPendingJob();
Assert.NotEmpty(overdueJobs);
Assert.True(overdueJobs.All(j => j.ScheduledAt < DateTime.UtcNow && j.Status == UploadStatus.Scheduled));

// Example 6: Reschedule an existing job
var rescheduledJob = await schedulingServiceTests.RescheduleUploadAsync_WithValidJob_UpdatesScheduledTime();
Assert.True(rescheduledJob.ScheduledAt > DateTime.UtcNow.AddHours(1));

// Example 7: Cancel a scheduled job
var cancelled = await schedulingServiceTests.CancelUploadAsync_WithPendingJob_CancelsJob();
Assert.True(cancelled);

// Example 8: Get queue statistics
var queuedCount = await schedulingServiceTests.GetQueuedJobCountAsync_ReturnsCountOfQueuedJobs();
Console.WriteLine($"Queued jobs: {queuedCount}");
```

## TitleOptimizationEngineTests

The `TitleOptimizationEngineTests` class contains unit tests for the `TitleOptimizationEngine` that validate title scoring, keyword extraction, optimization suggestions, and posting time recommendations. It tests various scenarios including edge cases, validation, and ensures the title optimization system works correctly across different title formats and content types.

This test suite validates the complete title optimization workflow including scoring algorithms, keyword extraction logic, optimization suggestion generation, hashtag recommendations, and posting time predictions with comprehensive error handling and boundary condition testing.

**Public Members:**
- `ScoreTitle_EmptyString_ReturnsZero` - Validates that empty strings return zero score
- `ScoreTitle_NullInput_ReturnsZero` - Validates that null inputs return zero score
- `ScoreTitle_ShortTitle_ReturnsLowerScoreThanOptimal` - Tests scoring for titles that are too short
- `ScoreTitle_TitleWithPowerWord_ReceivesBoost` - Validates that titles with power words receive score boosts
- `ScoreTitle_TitleWithQuestion_ReceivesBoost` - Validates that question-format titles receive score boosts
- `ScoreTitle_TitleWithNumber_ReceivesBoost` - Validates that titles with numbers receive score boosts
- `ScoreTitle_ReturnValueIsClamped` - Tests that score values are properly clamped between 0 and 1
- `ExtractKeywords_ExtractsNonTrivialWords` - Validates keyword extraction for meaningful words
- `ExtractKeywords_FiltersStopWords` - Tests that stop words are properly filtered out
- `ExtractKeywords_ReturnsAtMostTenKeywords` - Validates that maximum of 10 keywords are returned
- `ExtractKeywords_EmptyInputs_ReturnsEmptyArray` - Tests behavior with empty inputs
- `ExtractKeywords_ReturnsLowercaseWords` - Validates that extracted keywords are lowercase
- `OptimizeAsync_WithValidInput_ReturnsSuggestions` - Tests successful optimization with valid input
- `OptimizeAsync_SuggestionsHavePositiveConfidenceScore` - Validates that suggestions have positive confidence scores
- `OptimizeAsync_BestSuggestionIsHighestConfidence` - Tests that the best suggestion has the highest confidence
- `OptimizeAsync_RecommendedHashtagsIncludeShortsTag` - Validates that recommended hashtags include #shorts
- `OptimizeAsync_NullOrWhitespaceTitle_ThrowsArgumentException` - Tests error handling for null/whitespace titles
- `OptimizeAsync_OptimalPostingHourIsWithinValidRange` - Validates that optimal posting hours are within valid range
- `RecommendPostingTimesAsync_ReturnsRequestedCount` - Tests that the requested number of posting times are returned

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<TitleOptimizationEngineTests>();
var titleOptimizationEngineTests = new TitleOptimizationEngineTests(logger);

// Example 1: Test title scoring with various formats
var emptyScore = titleOptimizationEngineTests.ScoreTitle_EmptyString_ReturnsZero();
Assert.Equal(0, emptyScore);

var powerWordScore = titleOptimizationEngineTests.ScoreTitle_TitleWithPowerWord_ReceivesBoost();
Assert.True(powerWordScore > 0.5);

var questionScore = titleOptimizationEngineTests.ScoreTitle_TitleWithQuestion_ReceivesBoost();
Assert.True(questionScore > 0.6);

var numberScore = titleOptimizationEngineTests.ScoreTitle_TitleWithNumber_ReceivesBoost();
Assert.True(numberScore > 0.5);

// Example 2: Test keyword extraction
var keywords = titleOptimizationEngineTests.ExtractKeywords_ExtractsNonTrivialWords();
Assert.NotEmpty(keywords);
Assert.All(keywords, word => Assert.True(word.Length > 2)); // Filter out short words

// Example 3: Test optimization with valid input
var optimizationResult = await titleOptimizationEngineTests.OptimizeAsync_WithValidInput_ReturnsSuggestions();
Assert.NotNull(optimizationResult);
Assert.NotEmpty(optimizationResult.Suggestions);
Assert.All(optimizationResult.Suggestions, suggestion => Assert.True(suggestion.ConfidenceScore > 0));

// Example 4: Test posting time recommendations
var postingTimes = await titleOptimizationEngineTests.RecommendPostingTimesAsync_ReturnsRequestedCount();
Assert.Equal(5, postingTimes.Count);
Assert.All(postingTimes, time => Assert.InRange(time.Hour, 0, 23)); // Valid UTC hours

// Example 5: Test error handling - null title
Assert.Throws<ArgumentException>(() => titleOptimizationEngineTests.OptimizeAsync_NullOrWhitespaceTitle_ThrowsArgumentException());

// Example 6: Test score clamping
var clampedScore = titleOptimizationEngineTests.ScoreTitle_ReturnValueIsClamped();
Assert.InRange(clampedScore, 0, 1);

// Example 7: Test hashtag recommendations
var hashtags = optimizationResult.RecommendedHashtags;
Assert.Contains("#shorts", hashtags);
Assert.True(hashtags.Count > 0);

// Example 8: Test optimal posting hour range
var optimalHour = optimizationResult.OptimalPostingHour;
Assert.InRange(optimalHour, 0, 23);
```

## ThumbnailGeneratorServiceTests

The `ThumbnailGeneratorServiceTests` class contains unit tests for the `ThumbnailGeneratorService` that validate thumbnail generation functionality including frame extraction, aspect ratio handling, text overlay rendering, output format generation, and batch processing operations. It tests various scenarios including successful operations, error handling, validation edge cases, and ensures the thumbnail generation pipeline works correctly across different video formats and configurations.

This test suite validates the complete thumbnail generation workflow including dimension calculations, file validation, FFmpeg frame extraction commands, text rendering with drawtext filter, batch processing with evenly spaced frames, and comprehensive error handling for missing files, invalid formats, and boundary conditions.

**Public Members:**
- `GetDimensions_Vertical_Returns720x1280` - Validates vertical 9:16 aspect ratio returns 720×1280
- `GetDimensions_Horizontal_Returns1280x720` - Validates horizontal 16:9 aspect ratio returns 1280×720
- `GetDimensions_Square_Returns720x720` - Validates square 1:1 aspect ratio returns 720×720
- `GenerateFromVideoAsync_NullVideoPath_ThrowsArgumentException` - Validates null video path handling
- `GenerateFromVideoAsync_EmptyVideoPath_ThrowsArgumentException` - Validates empty video path handling
- `GenerateFromVideoAsync_NullRequest_ThrowsArgumentNullException` - Validates null request handling
- `GenerateFromVideoAsync_MissingVideoFile_ThrowsVideoProcessingException` - Validates missing file handling
- `GenerateFromVideoAsync_EmptyOutputDirectory_ThrowsValidationException` - Validates empty output directory handling
- `GenerateWithTextOverlayAsync_NullImagePath_ThrowsArgumentException` - Validates null image path handling
- `GenerateWithTextOverlayAsync_EmptyText_ThrowsArgumentException` - Validates empty text handling
- `GenerateWithTextOverlayAsync_MissingImageFile_ThrowsVideoProcessingException` - Validates missing image file handling
- `GenerateBatchAsync_ZeroFrameCount_ThrowsArgumentOutOfRangeException` - Validates zero frame count handling
- `GenerateBatchAsync_NegativeVideoDuration_ThrowsArgumentOutOfRangeException` - Validates negative duration handling
- `ThumbnailGenerationRequest_DefaultValues_AreCorrect` - Validates request default values
- `TextOverlayOptions_DefaultValues_AreCorrect` - Validates overlay options default values
- `ThumbnailGenerationResult_SuccessFalseByDefault` - Validates result default state
- `GenerateFromVideoAsync_CreatesOutputDirectoryIfMissing` - Validates directory creation

**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using Microsoft.Extensions.Logging;
using Xunit;

// Initialize test service with mock logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ThumbnailGeneratorServiceTests>();
var thumbnailGeneratorServiceTests = new ThumbnailGeneratorServiceTests(logger);

// Example 1: Test vertical video dimensions
var verticalDimensions = thumbnailGeneratorServiceTests.GetDimensions_Vertical_Returns720x1280();
Assert.Equal(720, verticalDimensions.Width);
Assert.Equal(1280, verticalDimensions.Height);

// Example 2: Test horizontal video dimensions
var horizontalDimensions = thumbnailGeneratorServiceTests.GetDimensions_Horizontal_Returns1280x720();
Assert.Equal(1280, horizontalDimensions.Width);
Assert.Equal(720, horizontalDimensions.Height);

// Example 3: Test square video dimensions
var squareDimensions = thumbnailGeneratorServiceTests.GetDimensions_Square_Returns720x720();
Assert.Equal(720, squareDimensions.Width);
Assert.Equal(720, squareDimensions.Height);

// Example 4: Generate thumbnail from video file
var request = new ThumbnailGenerationRequest
{
    VideoPath = "/videos/sample.mp4",
    OutputPath = "/thumbnails/sample.jpg",
    TimestampSeconds = 5.5,
    AspectRatio = AspectRatio.Vertical,
    Format = ThumbnailFormat.Jpeg,
    Quality = 90
};

var result = await thumbnailGeneratorServiceTests.GenerateFromVideoAsync_CreatesOutputDirectoryIfMissing(request);
Assert.True(result.Success);
Assert.NotNull(result.OutputPath);
Assert.True(File.Exists(result.OutputPath));

// Example 5: Generate thumbnail with text overlay
var overlayRequest = new ThumbnailGenerationRequest
{
    VideoPath = "/videos/tutorial.mp4",
    OutputPath = "/thumbnails/tutorial_with_text.jpg",
    TimestampSeconds = 10.0,
    AspectRatio = AspectRatio.Horizontal,
    Format = ThumbnailFormat.Jpeg,
    Quality = 85,
    TextOverlay = new TextOverlayOptions
    {
        Text = "Learn C# in 30 Days",
        FontSize = 48,
        FontColor = "#FFFFFF",
        BackgroundColor = "#00000080", // Semi-transparent black
        Position = TextPosition.TopCenter,
        ShadowEnabled = true
    }
};

var overlayResult = await thumbnailGeneratorServiceTests.GenerateWithTextOverlayAsync_MissingImageFile_ThrowsVideoProcessingException(overlayRequest);
Assert.True(overlayResult.Success);

// Example 6: Generate batch of thumbnails
var batchRequest = new ThumbnailBatchRequest
{
    VideoPath = "/videos/long_tutorial.mp4",
    OutputDirectory = "/thumbnails/batch/",
    FrameCount = 5,
    AspectRatio = AspectRatio.Square,
    Format = ThumbnailFormat.Png
};

var batchResult = await thumbnailGeneratorServiceTests.GenerateBatchAsync_ZeroFrameCount_ThrowsArgumentOutOfRangeException(batchRequest);
Assert.Equal(5, batchResult.GeneratedThumbnails.Count);

// Example 7: Test error handling - null video path
Assert.Throws<ArgumentException>(() =>
    thumbnailGeneratorServiceTests.GenerateFromVideoAsync_NullVideoPath_ThrowsArgumentException());

// Example 8: Test error handling - empty text
Assert.Throws<ArgumentException>(() =>
    thumbnailGeneratorServiceTests.GenerateWithTextOverlayAsync_EmptyText_ThrowsArgumentException());
```

## UploadJobModelTests

The `UploadJobModelTests` class contains unit tests for the `UploadJob` model validation and processing logic. It verifies that upload job state management works correctly including retry logic, progress tracking, completion status, and error handling. This test suite validates the core business rules for upload jobs including retry limits, progress calculations, and state transitions throughout the upload lifecycle.

**Public Members:**
- `CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue` - Validates that failed jobs can retry when under the maximum retry limit
- `CanRetry_WhenAttemptCountMatchesMaxRetries_ReturnsFalse` - Validates that jobs cannot retry when attempt count equals max retries
- `UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage` - Tests progress calculation for partial uploads
- `MarkAsCompleted_AssignsVideoIdAndSetsProgressToFull` - Validates successful upload completion with video ID assignment
- `UploadedBytes_WhenSet_PreservesValueForResume` - Tests that uploaded byte count is preserved for resume operations


**Usage Example:**

```csharp
using YouTubeShortAutomator.Tests;
using YouTubeShortAutomator.Domain.Models;
using FluentAssertions;

// Example 1: Test retry logic for failed job under retry limit
var failedJob = new UploadJob
{
    VideoShortId = 42,
    Status = UploadStatus.Failed,
    AttemptCount = 1,
    MaxRetries = 3,
    ErrorMessage = "YouTube API rate limit exceeded"
};

var uploadJobModelTests = new UploadJobModelTests();
bool canRetry = uploadJobModelTests.CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue(failedJob);
canRetry.Should().BeTrue();

// Example 2: Test retry logic when max retries reached
var exhaustedJob = new UploadJob
{
    VideoShortId = 43,
    Status = UploadStatus.Failed,
    AttemptCount = 3,
    MaxRetries = 3,
    ErrorMessage = "Failed after 3 attempts"
};

bool cannotRetry = uploadJobModelTests.CanRetry_WhenAttemptCountMatchesMaxRetries_ReturnsFalse(exhaustedJob);
cannotRetry.Should().BeFalse();

// Example 3: Test progress calculation for partial upload
var partialUpload = new UploadJob
{
    VideoShortId = 44,
    Status = UploadStatus.Uploading,
    TotalBytes = 100000000, // 100 MB
    UploadedBytes = 50000000,  // 50 MB
    UploadProgressPercentage = 0
};

uploadJobModelTests.UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage(partialUpload);
partialUpload.UploadProgressPercentage.Should().Be(50.0);

// Example 4: Test successful upload completion
var completedUpload = new UploadJob
{
    VideoShortId = 45,
    Status = UploadStatus.Completed,
    UploadedBytes = 150000000, // 150 MB
    TotalBytes = 150000000,
    YouTubeVideoId = null,
    UploadProgressPercentage = 0
};

uploadJobModelTests.MarkAsCompleted_AssignsVideoIdAndSetsProgressToFull(completedUpload, "dQw4w9WgXcQ");
completedUpload.Status.Should().Be(UploadStatus.Completed);
completedUpload.YouTubeVideoId.Should().Be("dQw4w9WgXcQ");
completedUpload.UploadProgressPercentage.Should().Be(100.0);

// Example 5: Test uploaded bytes preservation for resume
var resumableUpload = new UploadJob
{
    VideoShortId = 46,
    Status = UploadStatus.Paused,
    TotalBytes = 200000000, // 200 MB
    UploadedBytes = 75000000,  // 75 MB
    UploadProgressPercentage = 37.5
};

uploadJobModelTests.UploadedBytes_WhenSet_PreservesValueForResume(resumableUpload, 125000000);
resumableUpload.UploadedBytes.Should().Be(125000000);
resumableUpload.UploadProgressPercentage.Should().Be(62.5);
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

## VideoShortRepository

The `VideoShortRepository` class provides data access operations for managing video short entities in the YouTube Shorts automation system. It implements the `IRepository<VideoShort>` interface and offers comprehensive CRUD operations for video shorts with specialized methods for querying by status, channel, and retrieving all records. The repository handles all database operations for video shorts including creation, retrieval, updating, and deletion, with support for tracking video processing status, quality metrics, and error handling.

**Public Members:**
- `GetByIdAsync()` - Retrieves a video short by its unique identifier
- `GetAllAsync()` - Retrieves all video shorts from the database
- `GetByStatusAsync()` - Retrieves video shorts filtered by their processing status
- `GetByChannelAsync()` - Retrieves all video shorts for a specific YouTube channel
- `AddAsync()` - Creates a new video short in the database
- `UpdateAsync()` - Updates an existing video short in the database
- `DeleteAsync()` - Deletes a video short from the database by its identifier
- `ExistsAsync()` - Checks if a video short with the specified identifier exists in the database
- `CountAsync()` - Returns the total count of video shorts in the database
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
var repository = serviceProvider.GetRequiredService<VideoShortRepository>();

// Example 1: Create a new video short
var videoShort = new VideoShort
{
    Title = "Learn C# in 30 Days",
    Description = "A comprehensive guide to mastering C# programming language fundamentals",
    FilePath = "/videos/learn-csharp-30-days.mp4",
    ThumbnailPath = "/thumbnails/learn-csharp-30-days.jpg",
    Duration = TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(32)),
    FileSizeBytes = 125432100, // ~125 MB
    Quality = VideoQuality.HD1080,
    Status = ProcessingStatus.Queued,
    ProcessingProfileId = 1,
    YouTubeChannelId = 1,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

await repository.AddAsync(videoShort);
Console.WriteLine($"Created video short with ID: {videoShort.Id}");

// Example 2: Get a video short by ID
var existingVideo = await repository.GetByIdAsync(videoShort.Id);
if (existingVideo != null)
{
    Console.WriteLine($"Found video: {existingVideo.Title}");
    Console.WriteLine($" - Duration: {existingVideo.Duration:mm\\:ss}");
    Console.WriteLine($" - Status: {existingVideo.Status}");
    Console.WriteLine($" - Quality: {existingVideo.Quality}");
}

// Example 3: Get all video shorts
var allVideos = await repository.GetAllAsync();
Console.WriteLine($"Total video shorts: {allVideos.Count()}");

// Example 4: Get video shorts by processing status
var queuedVideos = await repository.GetByStatusAsync(ProcessingStatus.Queued);
Console.WriteLine($"Queued videos: {queuedVideos.Count()}");

var processedVideos = await repository.GetByStatusAsync(ProcessingStatus.Processed);
Console.WriteLine($"Processed videos: {processedVideos.Count()}");

// Example 5: Get video shorts by YouTube channel
var channelVideos = await repository.GetByChannelAsync(channelId: 1);
Console.WriteLine($"Videos for channel 1: {channelVideos.Count()}");

foreach (var video in channelVideos)
{
    Console.WriteLine($"- Video {video.Id}: {video.Title}");
    Console.WriteLine($"  Status: {video.Status}, Quality: {video.Quality}");
}

// Example 6: Update a video short status
if (existingVideo != null)
{
    existingVideo.Status = ProcessingStatus.Processed;
    existingVideo.ProcessedAt = DateTime.UtcNow;
    existingVideo.UpdatedAt = DateTime.UtcNow;
    
    await repository.UpdateAsync(existingVideo);
    Console.WriteLine("Updated video short status to Processed");
}

// Example 7: Check if video exists and get total count
bool videoExists = await repository.ExistsAsync(videoShort.Id);
int totalVideos = await repository.CountAsync();
Console.WriteLine($"Video {videoShort.Id} exists: {videoExists}");
Console.WriteLine($"Total videos in database: {totalVideos}");

// Example 8: Delete a video short (e.g., after successful upload)
bool deleted = await repository.DeleteAsync(videoShort.Id);
Console.WriteLine($"Deleted video {videoShort.Id}: {(deleted ? "Success" : "Failed")}");

// Don't forget to save changes after batch operations
await repository.SaveChangesAsync();
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

## ContentCalendarOptions

The `ContentCalendarOptions` class provides configuration settings for the content calendar system that manages scheduling, optimization, and analytics for YouTube Shorts uploads. These options control title length constraints, posting time optimization, keyword weighting, and engagement-based scheduling algorithms to maximize video performance.

This configuration class is typically registered with the dependency injection container and accessed by services that handle content planning, title optimization, and upload scheduling.

**Public Members:**
- `DefaultLookAheadDays` - Default number of days to look ahead when planning content (typically 7-30 days)
- `MaxTitleLength` - Maximum allowed title length in characters (YouTube limit: 100 characters)
- `OptimalTitleMinLength` - Minimum recommended title length for optimal engagement (typically 40-60 characters)
- `OptimalTitleMaxLength` - Maximum recommended title length for optimal engagement (typically 60-80 characters)
- `MaxDescriptionLength` - Maximum allowed description length in characters (YouTube limit: 5000 characters)
- `MaxTagCount` - Maximum number of tags allowed per video (YouTube limit: 500 tags, recommended: 10-15)
- `OptimizationSuggestionCount` - Number of optimization suggestions to generate per video (typically 1-5)
- `OptimalPostingHoursUtc` - Array of optimal UTC hours for posting videos based on historical engagement data
- `KeywordWeightMultiplier` - Weight multiplier applied to keywords when scoring titles and descriptions (typically 1.0-2.5)
- `EngagementScoreWeight` - Weight of engagement score in overall optimization calculations (typically 0.3-0.7)
- `MinSlotGapMinutes` - Minimum time gap in minutes between scheduled upload slots to avoid clustering (typically 60-180 minutes)
- `AutoOptimizeOnCreate` - Whether to automatically optimize titles and descriptions when content is created
- `HighEngagementKeywords` - Array of keywords that historically perform well for this channel
- `TrendingHashtags` - Array of trending hashtags to append to video descriptions
- `HistoricalSampleSize` - Number of historical videos to analyze for optimization patterns (typically 50-200)
- `HighEngagementBonus` - Bonus multiplier applied to titles containing high-engagement keywords (typically 1.1-1.5)
- `EngagementRateThreshold` - Minimum engagement rate threshold for considering content high-performing (typically 0.05-0.15)



## AppSettings

The `AppSettings` class provides centralized configuration management for the YouTube Shorts Automator application. It encapsulates all application settings including database connections, file paths, processing limits, YouTube API credentials, and scheduling parameters. This class is typically accessed through dependency injection in ASP.NET Core applications or manually instantiated in console applications.

The configuration values control core application behavior such as concurrent processing limits, retry strategies, analytics synchronization frequency, and watermark application settings.

**Public Members:**
- `ConnectionString` - Database connection string for SQL Server access
- `DatabasePath` - File path to the SQLite database
- `LogDirectory` - Directory path for application log files
- `ProcessingDirectory` - Directory path for temporary processing files
- `OutputDirectory` - Directory path for final output files
- `MaxConcurrentUploads` - Maximum number of concurrent upload operations
- `MaxConcurrentProcessing` - Maximum number of concurrent video processing operations
- `DefaultRetryCount` - Default number of retry attempts for failed operations
- `UploadTimeoutSeconds` - Timeout duration for upload operations in seconds
- `ProcessingQueueLimit` - Maximum size of the processing queue
- `EnableAnalyticsSyncing` - Flag to enable/disable analytics synchronization
- `AnalyticsSyncIntervalHours` - Interval between analytics sync operations in hours
- `YouTubeApiKey` - API key for YouTube API access
- `YouTubeClientId` - Client ID for YouTube OAuth authentication
- `YouTubeClientSecret` - Client secret for YouTube OAuth authentication
- `ScheduleCheckIntervalSeconds` - Interval between schedule checks in seconds
- `EnableWatermark` - Flag to enable/disable watermark application
- `WatermarkImagePath` - File path to the watermark image (nullable)



**Usage Example:**

```csharp
using YouTubeShortAutomator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Example 1: Manual configuration with optimal values for a coding channel
var contentCalendarOptions = new ContentCalendarOptions
{
    DefaultLookAheadDays = 14,
    MaxTitleLength = 100,
    OptimalTitleMinLength = 50,
    OptimalTitleMaxLength = 70,
    MaxDescriptionLength = 5000,
    MaxTagCount = 15,
    OptimizationSuggestionCount = 3,
    OptimalPostingHoursUtc = new[] { 8, 12, 16, 20 }, // UTC hours
    KeywordWeightMultiplier = 1.8,
    EngagementScoreWeight = 0.5,
    MinSlotGapMinutes = 120,
    AutoOptimizeOnCreate = true,
    HighEngagementKeywords = new[] { "tutorial", "guide", "learn", "master", "complete" },
    TrendingHashtags = new[] { "#coding", "#programming", "#csharp", "#dotnet", "#tutorial" },
    HistoricalSampleSize = 100,
    HighEngagementBonus = 1.3,
    EngagementRateThreshold = 0.08
};

Console.WriteLine($"Content calendar configured with {contentCalendarOptions.OptimalPostingHoursUtc.Length} optimal posting hours");
Console.WriteLine($"High engagement keywords: {string.Join(", ", contentCalendarOptions.HighEngagementKeywords)}");
Console.WriteLine($"Trending hashtags: {string.Join(", ", contentCalendarOptions.TrendingHashtags)}");

// Example 2: Configuration binding from IConfiguration (ASP.NET Core)
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.Configure<ContentCalendarOptions>(configuration.GetSection("ContentCalendar"));
services.AddSingleton<ContentCalendarOptions>(sp =>
    sp.GetRequiredService<IOptions<ContentCalendarOptions>>().Value);

var serviceProvider = services.BuildServiceProvider();
var options = serviceProvider.GetRequiredService<ContentCalendarOptions>();

Console.WriteLine($"Loaded configuration - Look ahead days: {options.DefaultLookAheadDays}");
Console.WriteLine($"Optimal title range: {options.OptimalTitleMinLength}-{options.OptimalTitleMaxLength} characters");

// Example 3: Using with TitleOptimizationEngine
var titleOptimizationEngine = new TitleOptimizationEngine(
    options,
    new MockAnalyticsService(),
    new MockKeywordService()
);

var optimizationResult = await titleOptimizationEngine.OptimizeAsync(
    title: "Learn C# in 30 Days - Complete Programming Guide",
    description: "Master C# programming from beginner to advanced with practical examples and exercises",
    tags: new[] { "csharp", "programming", "tutorial", "dotnet" },
    channelId: 1
);

Console.WriteLine($"Optimized title: {optimizationResult.Suggestions[0].SuggestedTitle}");
Console.WriteLine($"Confidence score: {optimizationResult.Suggestions[0].ConfidenceScore:F2}");
Console.WriteLine($"Recommended posting time: {optimizationResult.OptimalPostingHour}:00 UTC");
```



**Usage Example:**

```csharp
using YouTubeShortAutomator.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Example 1: Manual instantiation with configuration binding
var appSettings = new AppSettings
{
    ConnectionString = "Server=localhost;Database=YouTubeShortsAutomator;User Id=sa;Password=YourPassword;",
    DatabasePath = "./data/youtube-shorts-automator.db",
    LogDirectory = "./logs",
    ProcessingDirectory = "./processing",
    OutputDirectory = "./output",
    MaxConcurrentUploads = 5,
    MaxConcurrentProcessing = 10,
    DefaultRetryCount = 3,
    UploadTimeoutSeconds = 300,
    ProcessingQueueLimit = 100,
    EnableAnalyticsSyncing = true,
    AnalyticsSyncIntervalHours = 24,
    YouTubeApiKey = "AIzaSyDqjKXlX...",
    YouTubeClientId = "1234567890-abcdefghijklmnopqrstuvwxyz.apps.googleusercontent.com",
    YouTubeClientSecret = "GOCSPX-abcdefghijklmnopqrstuvwxyz",
    ScheduleCheckIntervalSeconds = 60,
    EnableWatermark = true,
    WatermarkImagePath = "./assets/watermark.png"
};

Console.WriteLine($"Database path: {appSettings.DatabasePath}");
Console.WriteLine($"Max concurrent uploads: {appSettings.MaxConcurrentUploads}");
Console.WriteLine($"YouTube API enabled: {!string.IsNullOrEmpty(appSettings.YouTubeApiKey)}");

// Example 2: Configuration binding from IConfiguration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var appSettingsFromConfig = new AppSettings();
configuration.GetSection("AppSettings").Bind(appSettingsFromConfig);

Console.WriteLine($"Loaded configuration - Database: {appSettingsFromConfig.DatabasePath}");

// Example 3: Using with dependency injection (ASP.NET Core)
var services = new ServiceCollection();
services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
services.AddSingleton<AppSettings>(sp => 
    sp.GetRequiredService<IOptions<AppSettings>>().Value);

var serviceProvider = services.BuildServiceProvider();
var settings = serviceProvider.GetRequiredService<AppSettings>();

Console.WriteLine($"DI-injected settings - Log directory: {settings.LogDirectory}");
Console.WriteLine($"Analytics sync enabled: {settings.EnableAnalyticsSyncing}");
```

## ThumbnailAbTestService