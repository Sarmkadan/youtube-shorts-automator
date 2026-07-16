# YouTube Shorts Automator

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

### Scheduling Calendar
- **Full Entry Lifecycle**: Draft → Optimised → Approved → Scheduled → Published state machine with `Approve`, `Cancel`, `Archive` and `ApplyOptimization` transitions
- **REST API**: `ContentCalendarController` exposes ten endpoints: create, read, update, delete, upcoming window, date-range query, optimise, apply-suggestion, schedule and recommended-slot retrieval
- **Auto-Optimisation**: Optionally invokes the title/description engine immediately on entry creation (`AutoOptimizeOnCreate` setting)
- **Scheduling Integration**: `ScheduleEntryAsync` links a calendar entry to an `UploadJob` via `SchedulingService`, storing the job reference on the entry
- **Validation**: Title length, description length, tag count and channel-ID checks are enforced at creation and update time

### Enterprise Features
- **Multi-Channel Support**: Manage multiple YouTube channels from single platform
- **Role-based Access Control**: User permissions and channel delegation
- **Comprehensive Logging**: Structured logging with Serilog integration
- **Exception Handling**: Global error handling with automatic recovery
- **Rate Limiting**: Configurable API rate limiting and throttling
- **Caching Layer**: Redis-based caching for performance optimization

## ProcessingController

The `ProcessingController` manages the video submission pipeline, allowing users to upload video files, track processing status, retrieve available encoding profiles, and cancel ongoing jobs. It acts as the primary API interface for initiating and managing the transcoding workflows defined within the application.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// Assume controller is initialized via dependency injection
var controller = new ProcessingController(logger, responseFormatter, cacheService);

// Example 1: Get available profiles
var profilesResult = await controller.GetAvailableProfilesAsync();
if (profilesResult is OkObjectResult profilesOk)
{
    var profilesResponse = profilesOk.Value as ProfilesResponse;
    foreach (var profile in profilesResponse.Profiles)
    {
        Console.WriteLine($"Profile: {profile.Name} ({profile.Code}) - {profile.Resolution} @ {profile.Bitrate}");
    }
}

// Example 2: Submit a video for processing
// (In a real scenario, SubmitVideoRequest would be populated from an IFormFile/Multipart request)
var request = new SubmitVideoRequest
{
    Title = "My New Short",
    Description = "A great short video",
    ProcessingProfile = "hq"
};
var submitResult = await controller.SubmitVideoAsync(request);
if (submitResult is OkObjectResult submitOk)
{
    var response = submitOk.Value as ProcessingResponse;
    Console.WriteLine($"Submitted job {response.ProcessingId}: {response.Message}");

    // Example 3: Get status
    var statusResult = await controller.GetProcessingStatusAsync(response.ProcessingId);
    if (statusResult is OkObjectResult statusOk)
    {
        var statusResponse = statusOk.Value as ProcessingStatusResponse;
        Console.WriteLine($"Job {statusResponse.ProcessingId} status: {statusResponse.Status}, Progress: {statusResponse.Progress}%");
    }

    // Example 4: Cancel processing
    var cancelResult = await controller.CancelProcessingAsync(response.ProcessingId);
    Console.WriteLine($"Cancellation result: {cancelResult}");
}
```

## ScheduleController

The `ScheduleController` provides REST endpoints for managing scheduled YouTube uploads. It allows users to create, read, update, list, and delete upload schedules, as well as bulk-import schedules from CSV files to streamline content publishing.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// Assume controller is initialized via dependency injection
var controller = new ScheduleController(logger, cacheService);

// Example 1: Create a new schedule
var createRequest = new CreateScheduleRequest { VideoId = Guid.NewGuid(), ScheduledUploadTimeUtc = DateTime.UtcNow.AddDays(1) };
var createResult = await controller.CreateScheduleAsync(createRequest);

// Example 2: List schedules
var listResult = await controller.ListSchedulesAsync(pageNumber: 1, pageSize: 10);

// Example 3: Get schedule details
var scheduleId = Guid.NewGuid(); // Example ID
var getResult = await controller.GetScheduleAsync(scheduleId);

// Example 4: Update a schedule
var updateRequest = new UpdateScheduleRequest { ScheduledUploadTimeUtc = DateTime.UtcNow.AddDays(2) };
var updateResult = await controller.UpdateScheduleAsync(scheduleId, updateRequest);

// Example 5: Delete a schedule
var deleteResult = await controller.DeleteScheduleAsync(scheduleId);

// Example 6: Import schedules from CSV
// (In a real scenario, IFormFile would be provided via request context)
var importResult = await controller.ImportSchedulesFromCsvAsync(file);
```


## JobStatusController

The `JobStatusController` provides REST endpoints to monitor the state and results of asynchronous background jobs. It allows users to retrieve the status, results, statistics, and recent activity of specific processing jobs, as well as cancel jobs that are no longer needed.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// Assume controller is initialized via dependency injection
var controller = new JobStatusController(logger, cacheService);

// Example 1: Get status of a specific job
var jobId = Guid.NewGuid();
var statusResult = await controller.GetJobStatusAsync(jobId);
if (statusResult is OkObjectResult okResult)
{
    var status = okResult.Value as JobStatusResponse;
    Console.WriteLine($"Job {status.JobId} status: {status.Status}, Progress: {status.Progress}%");
}

// Example 2: Get results of a completed job
var resultsResult = await controller.GetJobResultsAsync(jobId);

// Example 3: Cancel a running job
await controller.CancelJobAsync(jobId);

// Example 4: List recent jobs
var recentJobs = await controller.GetRecentJobsAsync(limit: 10);

// Example 5: Get job performance stats
var stats = await controller.GetJobStatsAsync(jobId);
Console.WriteLine($"Successful: {stats.SuccessfulCount}, Failed: {stats.FailedCount}");
```

## Repository

The `Repository<TEntity>` base class provides a generic data access layer implementation for common CRUD operations against your database context. It serves as the foundation for all repository implementations in the application and offers standard methods for entity persistence, retrieval, updating, and deletion.

**Key Features:**
- Generic implementation supporting any entity type
- Async/await pattern for all database operations
- Basic CRUD operations: Create, Read, Update, Delete
- Existence checking and counting operations
- Transaction support via `SaveChangesAsync`


**Usage Example:**

```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var repository = new Repository<VideoShort>(dbContext);

// Example 1: Add a new entity
var newVideo = new VideoShort
{
    FileName = "short_001.mp4",
    FilePath = "/videos/short_001.mp4",
    Status = ProcessingStatus.Pending,
    DurationSeconds = 60,
    FileSizeBytes = 10485760,
    Quality = VideoQuality.HD1080,
    CreatedAt = DateTime.UtcNow
};
await repository.AddAsync(newVideo);
await repository.SaveChangesAsync();

// Example 2: Get entity by ID
var video = await repository.GetByIdAsync(newVideo.Id);
Console.WriteLine($"Found video: {video?.FileName}");

// Example 3: Get all entities
var allVideos = await repository.GetAllAsync();
Console.WriteLine($"Total videos: {allVideos.Count}");

// Example 4: Update an entity
if (video != null)
{
    video.Status = ProcessingStatus.Processing;
    await repository.UpdateAsync(video);
    await repository.SaveChangesAsync();
}

// Example 5: Check if entity exists
var exists = await repository.ExistsAsync(v => v.Id == video.Id);
Console.WriteLine($"Video exists: {exists}");

// Example 6: Count entities matching criteria
var pendingCount = await repository.CountAsync(v => v.Status == ProcessingStatus.Pending);
Console.WriteLine($"Pending videos: {pendingCount}");

// Example 7: Delete an entity
if (video != null)
{
    await repository.DeleteAsync(video);
    await repository.SaveChangesAsync();
}

// Example 8: Add multiple entities at once
var batch = new List<VideoShort>
{
    new VideoShort { /* ... */ },
    new VideoShort { /* ... */ },
    new VideoShort { /* ... */ }
};
await repository.AddRangeAsync(batch);
await repository.SaveChangesAsync();

// Example 9: Update multiple entities
var videosToUpdate = await repository.GetAllAsync(v => v.Status == ProcessingStatus.Completed);
foreach (var v in videosToUpdate)
{
    v.ProcessedAt = DateTime.UtcNow;
}
await repository.UpdateRangeAsync(videosToUpdate);
await repository.SaveChangesAsync();

// Example 10: Delete multiple entities
var oldVideos = await repository.GetAllAsync(v => v.CreatedAt < DateTime.UtcNow.AddDays(-30));
await repository.DeleteRangeAsync(oldVideos);
await repository.SaveChangesAsync();
```

### High-Level Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     API Layer (Controllers)                  │
│  ScheduleController • ProcessingController • AnalyticsCtrl   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│               Application Services Layer                     │
│  ┌──────────────────┐ ┌──────────────────┐ ┌─────────────┐ │
│  │ VideoProcessing  │ │ YouTubeUpload    │ │ Scheduling  │ │
│  │ Service          │ │ Service          │ │ Service     │ │
│  └──────────────────┘ └──────────────────┘ └─────────────┘ │
│  ┌──────────────────┐ ┌──────────────────┐ ┌─────────────┐ │
│  │ Analytics        │ │ Configuration    │ │ API         │ │
│  │ Service          │ │ Service          │ │ Credential  │ │
│  └──────────────────┘ └──────────────────┘ └─────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│            Domain Models & Business Logic                    │
│  VideoShort • UploadJob • ProcessingTask • YouTubeChannel   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│         Data Access Layer (Repositories)                     │
│  VideoRepository • UploadJobRepository • AnalyticsRepository │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│         Infrastructure (FFmpeg, YouTube API, DB)             │
│  FFmpegWrapper • GoogleApiClient • DatabaseContext • Cache   │
└─────────────────────────────────────────────────────────────┘
```

### Core Domain Models

#### VideoRepository

The `VideoRepository` class provides data access methods for querying video entities from the database. It implements the `IVideoRepository` interface and offers various methods for retrieving videos by different criteria including user ID, status, YouTube ID, and pagination.

**Key Features:**
- Query videos by user ID with eager loading of related entities
- Filter videos by status (pending, processing, completed, etc.)
- Retrieve recent videos within a time window
- Search videos by YouTube video ID
- Paginated queries for both general and user-specific video lists


**Usage Example:**

```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var videoRepository = new VideoRepository(dbContext, logger);

// Example 1: Get videos by user ID
var userVideos = await videoRepository.GetByUserIdAsync(userId);
Console.WriteLine($"Found {userVideos.Count} videos for user {userId}");

// Example 2: Get pending videos for processing
var pendingVideos = await videoRepository.GetPendingVideosAsync();
Console.WriteLine($"Found {pendingVideos.Count} videos pending processing");

// Example 3: Get videos by status
var completedVideos = await videoRepository.GetByStatusAsync(VideoStatus.Completed);
Console.WriteLine($"Found {completedVideos.Count} completed videos");

// Example 4: Get recent videos (last 7 days)
var recentVideos = await videoRepository.GetRecentVideosAsync(userId, daysBack: 7);
Console.WriteLine($"Found {recentVideos.Count} videos in the last 7 days");

// Example 5: Get videos by YouTube ID
var youtubeVideos = await videoRepository.GetByYouTubeIdAsync("dQw4w9WgXcQ");
Console.WriteLine($"Found {youtubeVideos.Count} videos with YouTube ID dQw4w9WgXcQ");

// Example 6: Get paginated videos (page 1, 10 items per page)
var (paginatedVideos, totalCount) = await videoRepository.GetPaginatedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Found {paginatedVideos.Count} videos out of {totalCount} total");

// Example 7: Get paginated videos for a specific user
var (userPaginatedVideos, userTotalCount) = await videoRepository.GetUserVideosPaginatedAsync(
    userId: userId,
    pageNumber: 1,
    pageSize: 10
);
Console.WriteLine($"User {userId} has {userTotalCount} total videos, showing page 1");
```

#### ProcessingJobRepository

The `ProcessingJobRepository` class provides data access methods for querying processing job entities from the database. It implements the `IProcessingJobRepository` interface and offers specialized methods for retrieving processing jobs by video ID, status, job type, and pagination.

**Key Features:**
- Query processing jobs by video ID with eager loading of related entities (steps, errors, video)
- Filter jobs by status (queued, processing, completed, failed)
- Retrieve pending jobs for processing pipeline
- Get failed jobs eligible for retry
- Search jobs by processing job type
- Paginated queries with total count for UI pagination




**Usage Example:**

```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var processingJobRepository = new ProcessingJobRepository(dbContext, logger);

// Example 1: Get processing jobs for a video
var videoJobs = await processingJobRepository.GetByVideoIdAsync(videoId);
Console.WriteLine($"Found {videoJobs.Count} processing jobs for video {videoId}");

// Example 2: Get pending jobs for processing pipeline
var pendingJobs = await processingJobRepository.GetPendingJobsAsync();
Console.WriteLine($"Found {pendingJobs.Count} pending processing jobs");

// Example 3: Get jobs by status
var processingJobs = await processingJobRepository.GetByStatusAsync(ProcessingJobStatus.Processing);
Console.WriteLine($"Found {processingJobs.Count} processing jobs");

// Example 4: Get failed jobs for retry
var failedJobs = await processingJobRepository.GetFailedJobsAsync();
Console.WriteLine($"Found {failedJobs.Count} failed jobs");

// Example 5: Get jobs eligible for retry (max 3 retries)
var retryJobs = await processingJobRepository.GetJobsForRetryAsync(maxRetries: 3);
Console.WriteLine($"Found {retryJobs.Count} jobs eligible for retry");

// Example 6: Get latest job for a video
var latestJob = await processingJobRepository.GetLatestJobForVideoAsync(videoId);
if (latestJob != null)
{
    Console.WriteLine($"Latest job for video {videoId}: Status={latestJob.Status}, Created={latestJob.CreatedAt}");
}

// Example 7: Get jobs by type
var transcodeJobs = await processingJobRepository.GetJobsByTypeAsync(ProcessingJobType.Transcode);
Console.WriteLine($"Found {transcodeJobs.Count} transcode jobs");

// Example 8: Get paginated jobs (page 1, 10 items per page)
var (paginatedJobs, totalCount) = await processingJobRepository.GetPaginatedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Found {paginatedJobs.Count} jobs out of {totalCount} total");
```

### ScheduleRepository

The `ScheduleRepository` class provides data access methods for querying and managing upload schedules from the database. It implements the `IScheduleRepository` interface and offers specialized methods for retrieving schedules by user, status, frequency, and pagination, as well as managing scheduled uploads.

**Key Features:**
- Query schedules by user ID with eager loading of related scheduled uploads
- Filter schedules by active status and next scheduled time
- Retrieve schedules by frequency (daily, weekly, monthly)
- Manage scheduled uploads associated with schedules
- Paginated queries with total count for UI pagination





**Usage Example:**

```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var scheduleRepository = new ScheduleRepository(dbContext, logger);

// Example 1: Get schedules for a user
var userSchedules = await scheduleRepository.GetByUserIdAsync(userId);
Console.WriteLine($"Found {userSchedules.Count} schedules for user {userId}");

// Example 2: Get active schedules
var activeSchedules = await scheduleRepository.GetActiveSchedulesAsync();
Console.WriteLine($"Found {activeSchedules.Count} active schedules");

// Example 3: Get schedules due for execution
var dueSchedules = await scheduleRepository.GetDueSchedulesAsync();
Console.WriteLine($"Found {dueSchedules.Count} schedules due for execution");

// Example 4: Get schedules by frequency
var dailySchedules = await scheduleRepository.GetByFrequencyAsync(ScheduleFrequency.Daily);
Console.WriteLine($"Found {dailySchedules.Count} daily schedules");

// Example 5: Get paginated schedules (page 1, 10 items per page)
var (paginatedSchedules, totalCount) = await scheduleRepository.GetPaginatedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Found {paginatedSchedules.Count} schedules out of {totalCount} total");

// Example 6: Get a specific scheduled upload
var scheduledUpload = await scheduleRepository.GetScheduledUploadAsync(scheduleUploadId);
if (scheduledUpload != null)
{
    Console.WriteLine($"Found scheduled upload: {scheduledUpload.Title} (Status: {scheduledUpload.Status})");
}

// Example 7: Update a scheduled upload
if (scheduledUpload != null)
{
    scheduledUpload.Status = UploadStatus.Completed;
    await scheduleRepository.UpdateScheduledUploadAsync(scheduledUpload);
    Console.WriteLine("Scheduled upload updated successfully");
}
```

### ApiCredentialRepository

The `ApiCredentialRepository` class provides data access methods for managing API credentials used to authenticate with external services like YouTube API. It implements the `IApiCredentialRepository` interface and offers various methods for retrieving credentials by user ID, status, type, and expiration date.

**Key Features:**
- Retrieve credentials by user ID with ordering by creation date
- Get active credentials for authentication
- Filter credentials by status (active, expired, revoked)
- Retrieve expired credentials for renewal
- Search credentials by type (YouTube, Google, etc.)






**Usage Example:**



```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var apiCredentialRepository = new ApiCredentialRepository(dbContext, logger);

// Example 1: Get credentials for a specific user
var userCredentials = await apiCredentialRepository.GetByUserIdAsync(userId);
Console.WriteLine($"Found {userCredentials.Count} credentials for user {userId}");

// Example 2: Get active credential for authentication
var activeCredential = await apiCredentialRepository.GetActiveCredentialAsync(userId);
if (activeCredential != null)
{
    Console.WriteLine($"Active credential: {activeCredential.CredentialType} (Expires: {activeCredential.AccessTokenExpiresAt:u})");
}

// Example 3: Get credentials by status
var activeCredentials = await apiCredentialRepository.GetByStatusAsync(CredentialStatus.Active);
Console.WriteLine($"Found {activeCredentials.Count} active credentials");

var revokedCredentials = await apiCredentialRepository.GetByStatusAsync(CredentialStatus.Revoked);
Console.WriteLine($"Found {revokedCredentials.Count} revoked credentials");

// Example 4: Get expired credentials for renewal
var expiredCredentials = await apiCredentialRepository.GetExpiredCredentialsAsync();
Console.WriteLine($"Found {expiredCredentials.Count} expired credentials that need renewal");

foreach (var credential in expiredCredentials)
{
    Console.WriteLine($"Credential {credential.Id} expired on {credential.AccessTokenExpiresAt:u}");
}

// Example 5: Get credentials by type
var youtubeCredentials = await apiCredentialRepository.GetByTypeAsync(ApiCredentialType.YouTube);
Console.WriteLine($"Found {youtubeCredentials.Count} YouTube credentials");

var googleCredentials = await apiCredentialRepository.GetByTypeAsync(ApiCredentialType.Google);
Console.WriteLine($"Found {googleCredentials.Count} Google credentials");

// Example 6: Check if user has valid credentials
var hasValidCredentials = await apiCredentialRepository.GetActiveCredentialAsync(userId) != null;
Console.WriteLine($"User has valid credentials: {hasValidCredentials}");
```

### AnalyticsRepository

The `AnalyticsRepository` class provides data access methods for querying analytics metrics from the database. It implements the `IAnalyticsRepository` interface and offers specialized methods for retrieving video performance metrics by video ID, time period, and pagination.

**Key Features:**
- Query analytics metrics by video ID with eager loading of related entities (demographics, video)
- Filter metrics by time period (daily, weekly, monthly)
- Retrieve recent metrics within a time window
- Get top performing videos by view count
- Search for latest metric for specific videos
- Paginated queries with total count for UI pagination
- Aggregate view counts across multiple videos





**Usage Example:**

```csharp
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Domain.Models;

// Initialize repository (typically via dependency injection)
var analyticsRepository = new AnalyticsRepository(dbContext, logger);

// Example 1: Get metrics for a specific video
var videoMetrics = await analyticsRepository.GetByVideoIdAsync(videoId);
Console.WriteLine($"Found {videoMetrics.Count} metrics for video {videoId}");

// Example 2: Get metrics by period (daily, weekly, monthly)
var dailyMetrics = await analyticsRepository.GetByPeriodAsync(MetricsPeriod.Daily);
Console.WriteLine($"Found {dailyMetrics.Count} daily metrics");

// Example 3: Get recent metrics (last 7 days)
var recentMetrics = await analyticsRepository.GetRecentMetricsAsync(videoId, daysBack: 7);
Console.WriteLine($"Found {recentMetrics.Count} recent metrics");

// Example 4: Get top performing videos
var topVideos = await analyticsRepository.GetTopMetricsAsync(limit: 10);
Console.WriteLine($"Top {topVideos.Count} performing videos");

// Example 5: Get latest metric for a video
var latestMetric = await analyticsRepository.GetLatestMetricForVideoAsync(videoId);
if (latestMetric != null)
{
    Console.WriteLine($"Latest metric: {latestMetric.ViewCount} views collected on {latestMetric.CollectedAt:u}");
}

// Example 6: Get paginated metrics (page 1, 10 items per page)
var (paginatedMetrics, totalCount) = await analyticsRepository.GetPaginatedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Found {paginatedMetrics.Count} metrics out of {totalCount} total");

// Example 7: Get view counts for multiple videos
var videoIds = new List<Guid> { videoId1, videoId2, videoId3 };
var viewCounts = await analyticsRepository.GetVideoViewCountsAsync(videoIds);
foreach (var (id, views) in viewCounts)
{
    Console.WriteLine($"Video {id}: {views} views");
}
```

#### VideoShort
```csharp
public class VideoShort
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public ProcessingStatus Status { get; set; }
    public int DurationSeconds { get; set; }
    public long FileSizeBytes { get; set; }
    public VideoQuality Quality { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
```

#### UploadJob
```csharp
public class UploadJob
{
    public int Id { get; set; }
    public int VideoShortId { get; set; }
    public int YouTubeChannelId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Tags { get; set; }
    public UploadStatus Status { get; set; }
    public DateTime ScheduledUploadTime { get; set; }
    public int RetryCount { get; set; }
    public string? YouTubeVideoId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### ProcessingTask
```csharp
public class ProcessingTask
{
    public int Id { get; set; }
    public int UploadJobId { get; set; }
    public ProcessingStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int Progress { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### Service Layer

**ThumbnailGeneratorService**: Generates thumbnail images from video files
- FFmpeg-backed frame extraction at any timestamp
- Text overlay via `drawtext` filter with configurable appearance
- Aspect ratio presets (Shorts 9:16, landscape 16:9, square 1:1)
- Batch generation of candidate frames
- Registered as `IThumbnailGeneratorService`

**TitleOptimizationEngine**: Analyses and improves video metadata
- Deterministic, data-driven scoring against historical analytics
- Power-word injection, question-format and keyword-alignment strategies
- Hashtag recommendations and UTC posting-slot prediction
- Registered as `ITitleOptimizationEngine`

**ContentCalendarService**: Orchestrates the content publishing calendar
- Full entry lifecycle from Draft through to Published
- Integrated title/description optimisation pass
- Scheduling integration that creates `UploadJob` records
- REST surface via `ContentCalendarController` (10 endpoints)
- Registered as `IContentCalendarService`

**VideoProcessingService**: Handles FFmpeg operations
- Transcoding with multiple profiles
- Watermark application
- Format conversion and validation
- Quality analysis and optimization

**YouTubeUploadService**: Manages YouTube integration
- OAuth authentication and token management
- Direct video uploads
- Metadata and thumbnail updates
- Playlist management

**SchedulingService**: Orchestrates job scheduling
- Schedule creation and management
- Automatic retry with exponential backoff
- Job cancellation and rescheduling
- Time-zone aware scheduling

**AnalyticsService**: Tracks performance metrics
- Real-time analytics synchronization
- Report generation
- Performance trending analysis
- Custom metric aggregation

**JobOrchestrationService**: Coordinates complete workflows
- Multi-step pipeline orchestration
- Error recovery and compensation
- Progress tracking and notifications
- Dependency management

## System Requirements

### Software Requirements
- **.NET 10 SDK** (or later) - [Download](https://dotnet.microsoft.com/download)
- **SQL Server 2019+** (or LocalDB for development) - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **FFmpeg 4.4+** (see [FFmpeg Setup](#ffmpeg-setup) below) - [Download](https://ffmpeg.org/download.html)
- **Git** (for version control) - [Download](https://git-scm.com/download)

### Hardware Requirements
- **CPU**: 2+ cores (4+ recommended for concurrent processing)
- **RAM**: 4GB minimum (8GB+ recommended)
- **Disk**: 10GB+ for processing temporary files
- **Network**: Stable internet connection (for YouTube API)

### Development Tools (Optional)
- **Visual Studio 2024** or **Visual Studio Code**
- **Docker** (for containerized deployment)
- **Redis** (for caching layer)
- **Postman** (for API testing)

## Installation

### Method 1: Direct Installation from Source

#### Step 1: Prerequisites Setup

```bash
# Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Verify installation
dotnet --version

# Install FFmpeg (Ubuntu/Debian)
sudo apt-get update
sudo apt-get install ffmpeg

# Verify FFmpeg
ffmpeg -version
```

---

## FFmpeg Setup

### Minimum Version

**FFmpeg 4.4** or later is required. Version 4.4 introduced stability improvements to
the AAC encoder and libx264 integration that this pipeline relies on.  
FFmpeg 6.x or 7.x is recommended for production workloads.

### Required Codec Support

The encoding profiles used by this application require the following codecs to be
compiled into your FFmpeg build:

| Codec | Flag | Used For |
|-------|------|----------|
| H.264 | `--enable-libx264` | Video encoding (all profiles) |
| AAC | built-in **or** `--enable-libfdk-aac` | Audio encoding (all profiles) |

> **Note:** Most package-manager builds (`apt`, `brew`, `winget`) ship with both
> codecs enabled. Custom or minimal builds (e.g. Alpine Linux) may omit them.
> If you see `Unknown encoder 'libx264'` errors, install a full build.

### Verifying Your Build

```bash
# Check version (4.4+ required)
ffmpeg -version

# Confirm libx264 and aac are available
ffmpeg -encoders 2>/dev/null | grep -E "libx264|aac"
# Expected output includes lines like:
#  V..... libx264              libx264 H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10
#  A..... aac                  AAC (Advanced Audio Coding)
```

### Installation by Operating System

#### Linux (Ubuntu / Debian)

```bash
sudo apt-get update
sudo apt-get install -y ffmpeg

# Verify
ffmpeg -version
ffmpeg -encoders 2>/dev/null | grep libx264
```

#### Linux (RHEL / Fedora / CentOS)

```bash
# Enable RPM Fusion (provides a full-featured FFmpeg build)
sudo dnf install -y https://mirrors.rpmfusion.org/free/fedora/rpmfusion-free-release-$(rpm -E %fedora).noarch.rpm
sudo dnf install -y ffmpeg

# Verify
ffmpeg -version
```

#### macOS (Homebrew)

```bash
brew install ffmpeg

# Verify
ffmpeg -version
ffmpeg -encoders 2>/dev/null | grep libx264
```

#### Windows

**Option A — winget (Windows Package Manager)**
```powershell
winget install --id Gyan.FFmpeg -e
```

**Option B — Manual installation**
1. Download a full build from <https://www.gyan.dev/ffmpeg/builds/> (choose the
   `ffmpeg-release-full.7z` archive).
2. Extract to `C:\ffmpeg`.
3. Add `C:\ffmpeg\bin` to your `PATH` environment variable.
4. Open a new terminal and verify:
   ```cmd
   ffmpeg -version
   ffmpeg -encoders | findstr libx264
   ```

### Configuring the FFmpeg Path

By default the application expects `ffmpeg` and `ffprobe` to be on `PATH`.
Override the paths in `appsettings.json` if your installation is non-standard:

```json
"Processing": {
  "FFmpegPath": "/usr/local/bin/ffmpeg",
  "FFprobePath": "/usr/local/bin/ffprobe",
  "FFmpegTimeoutSeconds": 300
}
```

`FFmpegTimeoutSeconds` controls how long a single encode is allowed to run before
the process is forcibly terminated (default: 300 seconds / 5 minutes). Increase
this value when processing long source videos.

### Troubleshooting

| Error | Likely Cause | Solution |
|-------|-------------|----------|
| `FFmpegNotFoundException` | `ffmpeg` is not on `PATH` | Add FFmpeg to `PATH` or set `Processing:FFmpegPath` in config |
| `Unknown encoder 'libx264'` | Minimal FFmpeg build without libx264 | Install a full FFmpeg build (see above) |
| `Unknown encoder 'aac'` | Minimal build without AAC | Install a full FFmpeg build |
| Encode hangs indefinitely | Input video has no audio/video stream | Update to latest release — fixed in #9; check source file integrity |
| Very slow encoding | Hardware acceleration not enabled | Consider using `-hwaccel auto` via a custom profile |

---

#### Step 2: Clone Repository

```bash
git clone https://github.com/sarmkadan/youtube-shorts-automator.git
cd youtube-shorts-automator
```

#### Step 3: Configure Application

Edit `appsettings.json` with your settings:

```json
{
  "AppSettings": {
    "ConnectionString": "Server=localhost;Database=YouTubeShortsAutomator;Integrated Security=true;",
    "YouTubeApiKey": "your-youtube-api-key",
    "YouTubeClientId": "your-client-id",
    "YouTubeClientSecret": "your-client-secret",
    "MaxConcurrentUploads": 3,
    "MaxConcurrentProcessing": 2
  }
}
```

#### Step 4: Build & Run

```bash
# Restore NuGet packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Application runs on http://localhost:5000
```

### Method 2: Docker Installation

Run the application in Docker containers for easy deployment and dependency management.

```bash
# Option 1: Quick Start (uses default passwords - change in production!)
# Set required environment variables
export YOUTUBE_API_KEY="your-youtube-api-key"
export YOUTUBE_CLIENT_ID="your-client-id"
export YOUTUBE_CLIENT_SECRET="your-client-secret"

# Start all services (app, database, Redis)
docker-compose up -d

# View application logs
docker-compose logs -f app

# Stop all services
docker-compose down

# Clean up volumes (removes database data!)
docker-compose down -v
```

#### Environment Variables

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `YOUTUBE_API_KEY` | YouTube Data API v3 key | Yes | - |
| `YOUTUBE_CLIENT_ID` | OAuth 2.0 Client ID | Yes | - |
| `YOUTUBE_CLIENT_SECRET` | OAuth 2.0 Client Secret | Yes | - |
| `SA_PASSWORD` | SQL Server password | No | `YourSecurePassword123!` |
| `MaxConcurrentUploads` | Max concurrent uploads | No | `3` |
| `MaxConcurrentProcessing` | Max concurrent processing | No | `2` |
| `AnalyticsSyncIntervalHours` | Analytics sync interval in hours | No | `6` |

#### Production Configuration

For production, create a `.env` file:

```env
YOUTUBE_API_KEY=your-api-key-here
YOUTUBE_CLIENT_ID=your-client-id-here
YOUTUBE_CLIENT_SECRET=your-client-secret-here
SA_PASSWORD=your-strong-sql-password-here
MaxConcurrentUploads=5
MaxConcurrentProcessing=3
```

Then run:
```bash
docker-compose --env-file .env up -d
```

#### Development with Hot Reload

```bash
# Start development environment with hot reload
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d

# The application will automatically recompile on file changes
```

#### Custom Port Mapping

```bash
# Map to different host ports
docker-compose up -d -p 8081:8080

# Access on http://localhost:8081
```

### Method 3: Release Build

```bash
# Publish as self-contained executable
dotnet publish -c Release -o ./publish --self-contained

# Run executable
./publish/YouTubeShortsAutomator
```

## Quick Start

### 1. Set Up YouTube API Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create new project: "YouTubeShortsAutomator"
3. Enable YouTube Data API v3
4. Create OAuth 2.0 credentials (Desktop application)
5. Download credentials JSON
6. Add credentials to `appsettings.json`

### 2. Create First Processing Profile

```csharp
// Using the API
POST /api/processing/profiles
{
  "name": "High Quality",
  "videoWidth": 1080,
  "videoHeight": 1920,
  "videoBitrate": 4000,
  "frameRate": 30
}
```

### 3. Upload and Schedule a Video

```csharp
// Step 1: Upload video file
POST /api/videos/upload
Form-Data: file=video.mp4

// Step 2: Create upload job
POST /api/schedule/jobs
{
  "videoShortId": 1,
  "title": "My First Short",
  "description": "Automated upload test",
  "tags": ["automation", "youtube"],
  "scheduledUploadTime": "2026-05-05T14:00:00Z"
}

// Step 3: Monitor status
GET /api/jobs/1/status
```

### 4. View Analytics Dashboard

Open browser: `http://localhost:5000/dashboard`

## Testing

```bash
# Run all tests
dotnet test

# Run a specific test by name
dotnet test --filter="TestName"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run in verbose mode
dotnet test --logger "console;verbosity=detailed"
```

The test suite covers domain model validation, repository logic, and service-layer behavior using xUnit, Moq, and FluentAssertions.

## Configuration Guide

### AppSettings Section

```json
{
  "AppSettings": {
    "ConnectionString": "Server=localhost;Database=YouTubeShortsAutomator;Trusted_Connection=true;",
    "LogDirectory": "logs",
    "ProcessingDirectory": "processing",
    "OutputDirectory": "output",
    "MaxConcurrentUploads": 3,
    "MaxConcurrentProcessing": 2,
    "DefaultRetryCount": 3,
    "UploadTimeoutSeconds": 7200,
    "ProcessingQueueLimit": 100,
    "EnableAnalyticsSyncing": true,
    "AnalyticsSyncIntervalHours": 6,
    "YouTubeApiKey": "your-youtube-api-key",
    "YouTubeClientId": "your-client-id",
    "YouTubeClientSecret": "your-client-secret",
    "ScheduleCheckIntervalSeconds": 60,
    "EnableWatermark": false,
    "WatermarkImagePath": null,
    "ApiKey": "your-api-key-here",
    "ExcludedPaths": [ "/health", "/version", "/swagger" ]
  }
}
```

### Storage Configuration

```json
{
  "Storage": {
    "BaseDirectory": "videos",
    "TempDirectory": "temp",
    "AccessKey": "your-storage-key"
  }
}
```

### YouTube Configuration

```json
{
  "YouTube": {
    "ApiKey": "your-youtube-api-key"
  }
}
```

### Processing Configuration

```json
{
  "Processing": {
    "FFmpegPath": "/usr/bin/ffmpeg",
    "FFprobePath": "/usr/bin/ffprobe"
  }
}
```

### Rate Limiting Configuration

```json
{
  "RateLimit": {
    "RequestsPerWindow": 100,
    "WindowSizeSeconds": 60
  }
}
```

### Webhook Configuration

```json
{
  "Webhook": {
    "Secret": "your-webhook-secret"
  }
}
```

## Usage Examples

For more comprehensive, standalone examples, see the [examples/](examples/) directory.

### Example 1: Basic Video Upload and Schedule

```csharp
using YouTubeShortsAutomator.Services;
using YouTubeShortsAutomator.Domain.Models;

// Initialize services
var processingService = serviceProvider.GetRequiredService<VideoProcessingService>();
var uploadService = serviceProvider.GetRequiredService<YouTubeUploadService>();
var schedulingService = serviceProvider.GetRequiredService<SchedulingService>();

// Step 1: Process video
var processedPath = await processingService.TranscodeVideoAsync(
    inputPath: "input.mp4",
    outputPath: "output_1080p.mp4",
    profile: new ProcessingProfile { VideoWidth = 1080, VideoHeight = 1920 }
);

// Step 2: Create upload job
var uploadJob = new UploadJob
{
    Title = "My Awesome Short",
    Description = "Check out this amazing short video!",
    Tags = new List<string> { "tutorial", "gaming", "shorts" },
    ScheduledUploadTime = DateTime.UtcNow.AddHours(2)
};

await schedulingService.ScheduleUploadAsync(uploadJob);

// Step 3: Monitor job status
var status = await schedulingService.GetJobStatusAsync(uploadJob.Id);
Console.WriteLine($"Status: {status}");
```

### Example 2: Batch Processing Multiple Videos

```csharp
var videoPaths = new[] { "video1.mp4", "video2.mp4", "video3.mp4" };
var profile = new ProcessingProfile 
{ 
    VideoWidth = 1080, 
    VideoHeight = 1920,
    VideoBitrate = 4000 
};

var processingTasks = videoPaths.Select(async path =>
{
    return await processingService.TranscodeVideoAsync(
        inputPath: path,
        outputPath: $"processed_{Path.GetFileName(path)}",
        profile: profile
    );
});

var results = await Task.WhenAll(processingTasks);
Console.WriteLine($"Processed {results.Length} videos");
```

### Example 3: Add Watermark to Video

```csharp
var watermarkedPath = await processingService.ApplyWatermarkAsync(
    inputPath: "input.mp4",
    outputPath: "watermarked.mp4",
    watermarkPath: "logo.png",
    position: WatermarkPosition.BottomRight,
    opacity: 0.7
);
```

### Example 4: Retrieve Analytics for a Video

```csharp
var analyticsService = serviceProvider.GetRequiredService<AnalyticsService>();

var metrics = await analyticsService.GetVideoMetricsAsync(youtubeVideoId: "dQw4w9WgXcQ");
Console.WriteLine($"Views: {metrics.ViewCount}");
Console.WriteLine($"Likes: {metrics.LikeCount}");
Console.WriteLine($"Comments: {metrics.CommentCount}");
Console.WriteLine($"Engagement Rate: {metrics.EngagementRate:P2}");
```

### Example 5: Generate Performance Report

```csharp
var report = await analyticsService.GeneratePerformanceReportAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow,
    format: ReportFormat.CSV
);

await File.WriteAllBytesAsync("performance_report.csv", report);
```

### Example 6: Schedule Multiple Uploads with Custom Timings

```csharp
var uploadBatch = new[]
{
    new { Video = 1, Title = "Morning Motivation", Time = DateTime.UtcNow.AddHours(8) },
    new { Video = 2, Title = "Lunch Break Tips", Time = DateTime.UtcNow.AddHours(12) },
    new { Video = 3, Title = "Evening Reflection", Time = DateTime.UtcNow.AddHours(18) }
};

foreach (var item in uploadBatch)
{
    var job = new UploadJob
    {
        VideoShortId = item.Video,
        Title = item.Title,
        ScheduledUploadTime = item.Time
    };
    await schedulingService.ScheduleUploadAsync(job);
}
```

### Example 7: Handle Upload Failures with Retry Logic

```csharp
try
{
    await uploadService.UploadVideoAsync(uploadJob);
}
catch (YouTubeApiException ex) when (ex.IsRetryable)
{
    Console.WriteLine($"Retryable error: {ex.Message}");
    await uploadService.RetryUploadAsync(uploadJob.Id);
}
catch (YouTubeApiException ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    await schedulingService.CancelJobAsync(uploadJob.Id);
}
```

### Example 8: Monitor System Health and Processing Queue

```csharp
var systemService = serviceProvider.GetRequiredService<SystemService>();

var health = await systemService.GetHealthStatusAsync();
Console.WriteLine($"CPU Usage: {health.CpuUsagePercent:P}");
Console.WriteLine($"Memory Available: {health.AvailableMemoryMB}MB");
Console.WriteLine($"Processing Queue: {health.ProcessingQueueCount} jobs");
Console.WriteLine($"Failed Jobs: {health.FailedJobsCount}");
```

### Example 9: Export Analytics to Multiple Formats

```csharp
// Export to CSV
var csvData = await analyticsService.ExportMetricsAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow,
    format: ExportFormat.CSV
);
await File.WriteAllBytesAsync("analytics.csv", csvData);

// Export to JSON
var jsonData = await analyticsService.ExportMetricsAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow,
    format: ExportFormat.JSON
);
await File.WriteAllBytesAsync("analytics.json", jsonData);
```

### Example 10: Manage Multiple YouTube Channels

```csharp
var channelService = serviceProvider.GetRequiredService<ChannelService>();

// Register channel
var channel = await channelService.RegisterChannelAsync(
    name: "My Gaming Channel",
    clientId: "your-client-id",
    clientSecret: "your-client-secret"
);

// List all channels
var channels = await channelService.GetAllChannelsAsync();

// Set default channel
await channelService.SetDefaultChannelAsync(channel.Id);

// Upload to specific channel
await uploadService.UploadVideoAsync(uploadJob, targetChannel: channel);
```

### Example: Generating a Thumbnail with Text Overlay

```csharp
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;

var generator = serviceProvider.GetRequiredService<IThumbnailGeneratorService>();

// Extract a frame from a Shorts video at the 3-second mark
var result = await generator.GenerateFromVideoAsync(
    videoPath: "/uploads/my-short.mp4",
    request: new ThumbnailGenerationRequest
    {
        CaptureTimestamp = TimeSpan.FromSeconds(3),
        AspectRatio      = ThumbnailAspectRatio.Vertical,   // 720×1280 — Shorts format
        Format           = ThumbnailOutputFormat.Jpeg,
        Quality          = 90,
        OutputDirectory  = "/thumbnails",
        OverlayText      = "5 Python Tips You NEED to Know",
        TextOverlay = new TextOverlayOptions
        {
            FontSize  = 52,
            FontColor = "white",
            Position  = TextPosition.BottomCenter,
            ShowBox   = true,
            BoxColor  = "black@0.6"
        }
    });

if (result.Success)
    Console.WriteLine($"Thumbnail saved: {result.OutputPath} ({result.FileSizeBytes} bytes)");

// Generate 5 candidate frames for manual selection
var candidates = await generator.GenerateBatchAsync(
    videoPath: "/uploads/my-short.mp4",
    request: new ThumbnailGenerationRequest { OutputDirectory = "/thumbnails" },
    frameCount: 5,
    videoDuration: TimeSpan.FromSeconds(45));

var best = candidates.Where(r => r.Success).ToList();
Console.WriteLine($"Generated {best.Count} candidate thumbnails.");
```

### Example: Title & Description Optimisation

```csharp
using YouTubeShortAutomator.Services;

var optimizer = serviceProvider.GetRequiredService<ITitleOptimizationEngine>();

// Score an existing title
double score = optimizer.ScoreTitle("Learn Python in 30 days");
Console.WriteLine($"Title score: {score:F2}");  // e.g. 0.65

// Extract meaningful keywords
string[] keywords = optimizer.ExtractKeywords(
    "How to build a REST API with .NET",
    "Step-by-step tutorial covering controllers, routing and authentication.");

// Get ranked optimisation suggestions
var result = await optimizer.OptimizeAsync(
    title: "Build a REST API",
    description: "Learn to build APIs with .NET",
    tags: new[] { "dotnet", "api" },
    channelId: 1);

Console.WriteLine($"Best suggestion: {result.BestSuggestion?.SuggestedTitle}");
Console.WriteLine($"Confidence:      {result.BestSuggestion?.ConfidenceScore:F2}");
Console.WriteLine($"Optimal posting: {result.NextOptimalSlot():u} UTC");
Console.WriteLine($"Hashtags:        {string.Join(' ', result.RecommendedHashtags)}");
```

### Example: Managing the Scheduling Calendar

```csharp
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;

var calendar = serviceProvider.GetRequiredService<IContentCalendarService>();

// Create a draft entry
var entry = await calendar.CreateEntryAsync(new ContentCalendarEntry
{
    Title            = "10 Python Tips for Beginners",
    Description      = "Quick tips every Python developer should know",
    Tags             = new[] { "python", "tips", "tutorial" },
    Category         = ContentCategory.Tutorial,
    ScheduledPublishAt = DateTime.UtcNow.AddDays(3),
    YouTubeChannelId = 1
});

// Run the optimisation engine and apply the best suggestion
await calendar.OptimizeEntryAsync(entry.Id);
entry = await calendar.ApplyOptimizationAsync(entry.Id, suggestionIndex: 0);

// Approve, then link to an upload job
entry.Approve();
await calendar.UpdateEntryAsync(entry);

entry = await calendar.ScheduleEntryAsync(entry.Id, DateTime.UtcNow.AddDays(3).AddHours(17));

// Query upcoming entries
var upcoming = await calendar.GetUpcomingEntriesAsync(daysAhead: 7);
foreach (var e in upcoming)
    Console.WriteLine($"{e.ScheduledPublishAt:u} — {e.Title} [{e.Status}]");

// Get engine-recommended posting slots for the channel
var slots = await calendar.GetRecommendedSlotsAsync(channelId: 1, count: 5);
foreach (var slot in slots)
    Console.WriteLine($"Recommended slot: {slot:u} UTC");
```

## AnalyticsController

The `AnalyticsController` provides REST endpoints for accessing video performance data, engagement metrics, and trend analysis. It facilitates retrieving detailed insights for individual videos, generating comprehensive summaries, exporting performance data, and visualizing engagement trends over time.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// Assume controller is initialized via dependency injection
// logger, responseFormatter, cacheService are injected
var controller = new AnalyticsController(logger, responseFormatter, cacheService);

// Example 1: Get analytics for a specific video
var videoId = Guid.NewGuid();
var videoResult = await controller.GetVideoAnalyticsAsync(videoId);
if (videoResult is OkObjectResult videoOk)
{
    var analytics = videoOk.Value as VideoAnalyticsResponse;
    Console.WriteLine($"Video: {analytics.Title}, Views: {analytics.ViewCount}");
}

// Example 2: Get analytics summary
var summaryResult = await controller.GetAnalyticsSummaryAsync(days: 30);
if (summaryResult is OkObjectResult summaryOk)
{
    var summary = summaryOk.Value as AnalyticsSummaryResponse;
    Console.WriteLine($"Total Views: {summary.TotalViews}, Total Engagement: {summary.TotalEngagement}");
}

// Example 3: Export analytics data
var exportResult = await controller.ExportAnalyticsAsync(format: "csv");

// Example 4: Get engagement trends
var trendsResult = await controller.GetEngagementTrendsAsync(days: 7);
if (trendsResult is OkObjectResult trendsOk)
{
    var trends = trendsOk.Value as EngagementTrendResponse;
    Console.WriteLine($"Found {trends.TrendPoints.Length} trend data points");
}
```

## AnalyticsControllerExtensions


Extension methods that provide additional analytics utilities for the `AnalyticsController`. These methods extend the built-in analytics functionality with helper methods for calculating engagement metrics, formatting percentages, and creating summary responses.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// In your controller action
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsController _analyticsController;
    
    public IActionResult GetVideoAnalytics(Guid videoId)
    {
        // Create analytics response using extension method
        var analyticsResponse = _analyticsController.CreateVideoAnalyticsResponse(videoId);
        
        // Calculate additional metrics using extension methods
        double engagementPerView = analyticsResponse.CalculateEngagementPerView();
        string engagementRate = analyticsResponse.GetEngagementRatePercentage();
        double watchTimeHours = analyticsResponse.GetWatchTimeHours();
        double avgDurationMinutes = analyticsResponse.GetAverageWatchDurationMinutes();
        
        return Ok(new {
            VideoId = videoId,
            Title = analyticsResponse.Title,
            Views = analyticsResponse.ViewCount,
            EngagementPerView = engagementPerView,
            EngagementRate = engagementRate,
            WatchTimeHours = watchTimeHours,
            AverageWatchDuration = avgDurationMinutes
        });
    }
    
    public IActionResult GetSummary()
    {
        // Create summary response using extension method
        var summary = this.CreateAnalyticsSummary(days: 30);
        
        return Ok(summary);
    }
}
```

## ConfigurationController

The `ConfigurationController` provides REST endpoints for monitoring and retrieving application configuration, system status, storage settings, processing limitations, and YouTube integration status. It is a central utility for diagnostic and configuration-query operations within the pipeline.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;
using YouTubeShortsAutomator.Caching;

// Assume controller is initialized via dependency injection
var controller = new ConfigurationController(logger, configuration, cacheService);

// Example 1: Get general configuration info
var configResult = controller.GetConfigurationInfo();
if (configResult is OkObjectResult configOk)
{
    var info = configOk.Value as ConfigurationInfo;
    Console.WriteLine($"Environment: {info.Environment}, Max Upload Size: {info.MaxUploadSizeMb}MB");
}

// Example 2: Get storage configuration
var storageResult = controller.GetStorageConfiguration();
if (storageResult is OkObjectResult storageOk)
{
    var storage = storageOk.Value as StorageConfig;
    Console.WriteLine($"Temp Dir: {storage.TempDirectoryPath}, Usage: {storage.CurrentUsageGb}/{storage.MaxStorageGb}GB");
}

// Example 3: Get processing settings
var procResult = controller.GetProcessingSettings();
if (procResult is OkObjectResult procOk)
{
    var settings = procOk.Value as ProcessingSettings;
    Console.WriteLine($"Parallel Limit: {settings.ParallelJobLimit}, Timeout: {settings.TimeoutMinutes}min");
}

// Example 4: Get YouTube integration status
var youtubeResult = controller.GetYouTubeIntegrationStatus();

// Example 5: Get health status
var healthResult = controller.GetHealthStatus();
if (healthResult is OkObjectResult healthOk)
{
    var health = healthOk.Value as HealthStatus;
    Console.WriteLine($"System Healthy: {health.IsHealthy}");
}

// Example 6: Get supported formats
var formatsResult = controller.GetSupportedFormats();
```

## API Reference

### Content Calendar Endpoints

**POST** `/api/content-calendar`
- Create a new calendar entry
- Body: `{ title, description, tags, category, scheduledPublishAt, youTubeChannelId, notes, keywords }`
- Response: `201 Created` with the persisted entry

**GET** `/api/content-calendar/{id}`
- Retrieve a single entry by id
- Response: `200 OK` with entry, `404` if not found

**GET** `/api/content-calendar/upcoming?daysAhead=7`
- List non-cancelled, non-archived entries scheduled within the next N days
- Response: `200 OK` with `{ data: [...], count }`

**GET** `/api/content-calendar/range?from=&to=`
- List entries scheduled between two UTC timestamps
- Response: `200 OK` with `{ data: [...], count }`

**PUT** `/api/content-calendar/{id}`
- Update title, description, tags, category, scheduledPublishAt, notes or keywords
- Response: `200 OK` with updated entry

**DELETE** `/api/content-calendar/{id}`
- Permanently remove a calendar entry
- Response: `200 OK` or `404`

**POST** `/api/content-calendar/{id}/optimize`
- Run the title/description optimisation engine and store the result
- Response: `200 OK` with `TitleOptimizationResult` (suggestions + posting-time)

**POST** `/api/content-calendar/{id}/apply-optimization?suggestionIndex=0`
- Apply a ranked suggestion to the entry's title, description and tags
- Response: `200 OK` with updated entry

**POST** `/api/content-calendar/{id}/schedule`
- Body: `{ scheduledAt: "2025-06-01T17:00:00Z" }`
- Links the entry to a new upload job and sets `ScheduledPublishAt`
- Response: `200 OK` with updated entry

**GET** `/api/content-calendar/recommended-slots?channelId=1&count=5`
- Return engine-recommended UTC posting timestamps for the channel
- Response: `200 OK` with `{ data: [...DateTime], count }`

### Processing Endpoints

**POST** `/api/processing/videos`
- Upload and process a video
- Body: `{ videoFile: File, profileId: int }`
- Response: `{ jobId: int, status: string }`

**GET** `/api/processing/jobs/{jobId}`
- Retrieve processing job details
- Response: `{ id: int, status: string, progress: int, error?: string }`

**POST** `/api/processing/profiles`
- Create a new processing profile
- Body: `{ name: string, width: int, height: int, bitrate: int }`
- Response: `{ id: int, name: string }`

**GET** `/api/processing/profiles`
- List all processing profiles
- Response: `[{ id: int, name: string, ... }]`

### Upload Endpoints

**POST** `/api/schedule/jobs`
- Create an upload job
- Body: `{ videoShortId: int, title: string, description: string, scheduledUploadTime: datetime }`
- Response: `{ jobId: int, status: string }`

**GET** `/api/schedule/jobs`
- List all scheduled uploads
- Query: `?status=pending&limit=10&offset=0`
- Response: `[{ id: int, title: string, status: string, ... }]`

**GET** `/api/schedule/jobs/{jobId}`
- Get upload job details
- Response: `{ id: int, videoId: string, title: string, status: string }`

**PUT** `/api/schedule/jobs/{jobId}`
- Update upload job
- Body: `{ title?: string, description?: string, scheduledUploadTime?: datetime }`
- Response: `{ id: int, updated: true }`

**DELETE** `/api/schedule/jobs/{jobId}`
- Cancel upload job
- Response: `{ cancelled: true }`

### Analytics Endpoints

**GET** `/api/analytics/metrics`
- Get overall metrics
- Query: `?startDate=2026-05-01&endDate=2026-05-04`
- Response: `{ totalViews: int, totalLikes: int, engagementRate: float }`

**GET** `/api/analytics/videos/{videoId}`
- Get metrics for specific video
- Response: `{ views: int, likes: int, comments: int, shares: int }`

**GET** `/api/analytics/trends`
- Get trending videos
- Query: `?days=7&limit=10`
- Response: `[{ videoId: string, title: string, views: int, ... }]`

**POST** `/api/analytics/export`
- Export analytics data
- Body: `{ format: "csv"|"json", startDate: datetime, endDate: datetime }`
- Response: File download

### System Endpoints

**GET** `/api/health`
- Check system health
- Response: `{ status: "healthy"|"degraded"|"unhealthy", cpuUsage: float, memory: int }`

**GET** `/api/version`
- Get application version
- Response: `{ version: "2.0.2", buildDate: datetime }`

## MetricsController

The `MetricsController` provides REST endpoints for retrieving various performance and system metrics from the YouTube Shorts Automator application. It exposes endpoints for system-wide metrics, API call tracking, error statistics, and processing performance analysis, making it easy to monitor application health and performance in real-time.

**Key Features:**
- Retrieve system-wide performance metrics including total API calls, processing performance, and error statistics
- Monitor API call patterns with endpoint-level breakdowns and timing information
- Track error rates and types across the application
- Analyze processing pipeline performance with duration statistics and throughput metrics
- Access captured timestamps for when metrics were collected

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;
using YouTubeShortsAutomator.Domain.Models;

// Initialize controller (typically via dependency injection)
var metricsController = new MetricsController();

// Example 1: Get system metrics
var systemMetrics = await metricsController.GetSystemMetricsAsync();
if (systemMetrics is OkObjectResult okResult)
{
    var metrics = okResult.Value as MetricsResponse;
    Console.WriteLine($"Total API calls: {metrics.TotalApiCalls}");
    Console.WriteLine($"Captured at: {metrics.CapturedAtUtc}");
}

// Example 2: Get API call metrics with endpoint breakdown
var apiMetrics = await metricsController.GetApiCallMetricsAsync();
if (apiMetrics is OkObjectResult apiOkResult)
{
    var apiMetricsResponse = apiOkResult.Value as ApiCallMetricsResponse;
    Console.WriteLine($"Total API calls: {apiMetricsResponse.TotalCalls}");
    Console.WriteLine($"Average duration: {apiMetricsResponse.AverageDurationMs:F2}ms");
    
    foreach (var group in apiMetricsResponse.Groups)
    {
        Console.WriteLine($"Endpoint: {group.Endpoint}");
        Console.WriteLine($"  Calls: {group.CallCount}");
        Console.WriteLine($"  Avg duration: {group.AverageDurationMs:F2}ms");
    }
}

// Example 3: Get error statistics
var errorStats = await metricsController.GetErrorStatsAsync();
if (errorStats is OkObjectResult errorOkResult)
{
    var errorResponse = errorOkResult.Value as ErrorStatsResponse;
    Console.WriteLine($"Total errors: {errorResponse.TotalErrors}");
    
    foreach (var error in errorResponse.ErrorCounts)
    {
        Console.WriteLine($"Error type: {error.Key}, Count: {error.Value}");
    }
}

// Example 4: Get processing performance metrics
var performance = await metricsController.GetProcessingPerformanceAsync();
if (performance is OkObjectResult perfOkResult)
{
    var perfResponse = perfOkResult.Value as ProcessingPerformanceResponse;
    Console.WriteLine($"Processing type: {perfResponse.ProcessType}");
    Console.WriteLine($"Total processed: {perfResponse.TotalCalls}");
    Console.WriteLine($"Total bytes: {perfResponse.TotalBytesProcessed:N0}");
    
    foreach (var metric in perfResponse.ProcessingMetrics)
    {
        Console.WriteLine($"Duration range: {metric.MinDurationMs:F2}ms - {metric.MaxDurationMs:F2}ms");
        Console.WriteLine($"Average: {metric.AverageDurationMs:F2}ms");
    }
}
```

**GET** `/api/metrics`
- Get system metrics
- Response: `{ processingQueue: int, failedJobs: int, uptime: int }`

## SystemController

The `SystemController` provides endpoints to monitor the overall health, versioning, and configuration of the YouTube Shorts Automator service. It serves as a diagnostic tool for checking system status, API version compatibility, and runtime environment details.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;

// Initialize controller via dependency injection (mocked logger and configuration)
var controller = new SystemController(logger, configuration);

// Example 1: Get system health status
var healthResult = await controller.GetHealthAsync();
if (healthResult is OkObjectResult healthOk)
{
    var response = healthOk.Value as HealthCheckResponse;
    Console.WriteLine($"System status: {response.Status}");
    foreach (var check in response.Checks)
    {
        Console.WriteLine($"Check: {check.Key} - {check.Value.Status} ({check.Value.ResponseTime})");
    }
}

// Example 2: Get API version info
var versionResult = controller.GetVersionAsync();
if (versionResult is OkObjectResult versionOk)
{
    var version = versionOk.Value as VersionInfo;
    Console.WriteLine($"API Version: {version.ApiVersion}, Build Date: {version.BuildDate}");
}

// Example 3: Get system diagnostics
var infoResult = controller.GetSystemInfoAsync();
if (infoResult is OkObjectResult infoOk)
{
    var info = infoOk.Value as SystemInfoResponse;
    Console.WriteLine($"OS: {info.OperatingSystem}, .NET: {info.DotNetVersion}");
    Console.WriteLine($"Uptime: {info.Uptime}");
}

// Example 4: Get supported features
var featuresResult = controller.GetFeaturesAsync();
if (featuresResult is OkObjectResult featuresOk)
{
    var features = featuresOk.Value as FeaturesResponse;
    Console.WriteLine($"Analytics enabled: {features.Analytics}");
}
```


## WebhookController

The `WebhookController` provides API endpoints for managing webhook registrations, enabling the system to receive real-time notifications about events such as upload completions or failures. It supports CRUD operations, allowing users to register, list, retrieve, update, and delete webhook configurations.

**Usage Example:**

```csharp
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.API;
using YouTubeShortsAutomator.Caching;

// Initialize controller (typically via dependency injection)
var webhookController = new WebhookController(logger, cacheService);

// Example 1: Register a new webhook
var registerRequest = new RegisterWebhookRequest
{
    Url = "https://your-domain.com/webhooks/youtube",
    Events = new[] { "upload.completed", "upload.failed" }
};
var registerResult = await webhookController.RegisterWebhookAsync(registerRequest);
if (registerResult is CreatedAtActionResult createdResult)
{
    var response = createdResult.Value as WebhookRegistrationResponse;
    Console.WriteLine($"Registered webhook {response.WebhookId} with status: {response.Status}");
}

// Example 2: List all webhooks
var listResult = await webhookController.ListWebhooksAsync();
if (listResult is OkObjectResult listOk)
{
    var listResponse = listOk.Value as WebhookListResponse;
    Console.WriteLine($"Total webhooks: {listResponse.TotalCount}");
    foreach (var wh in listResponse.Webhooks)
    {
        Console.WriteLine($"Webhook {wh.WebhookId}: {wh.Url} (Active: {wh.IsActive})");
    }
}

// Example 3: Get webhook details
var getResult = await webhookController.GetWebhookAsync(webhookId);
if (getResult is OkObjectResult getOk)
{
    var details = getOk.Value as WebhookDetails;
    Console.WriteLine($"Webhook URL: {details.Url}, Created: {details.CreatedAtUtc}");
}

// Example 4: Update webhook configuration
var updateRequest = new UpdateWebhookRequest
{
    Url = "https://your-domain.com/new-endpoint",
    IsActive = false
};
var updateResult = await webhookController.UpdateWebhookAsync(webhookId, updateRequest);
Console.WriteLine($"Update result: {updateResult}");

// Example 5: Delete a webhook
var deleteResult = await webhookController.DeleteWebhookAsync(webhookId);
Console.WriteLine($"Delete result: {deleteResult}");
```

## CollectionUtilityBenchmarksExtensions

`CollectionUtilityBenchmarksExtensions` provides a suite of benchmark scenarios for testing and optimizing `CollectionUtility` operations. It includes test cases for chunking, grouping, and distinct operations across various edge-case data sets.

**Usage Example:**

```csharp
using YouTubeShortsAutomator.Benchmarks;
using System.Collections.Generic;

// Example 1: Chunking operations
var chunkEmpty = CollectionUtilityBenchmarksExtensions.ChunkByEmptyCollection();
var chunkSingle = CollectionUtilityBenchmarksExtensions.ChunkBySingleItem();
var chunkUneven = CollectionUtilityBenchmarksExtensions.ChunkByUnevenDivision();

// Example 2: Grouping operations
var groupEmpty = CollectionUtilityBenchmarksExtensions.GroupByToDictionaryEmptyCollection();
var groupSingle = CollectionUtilityBenchmarksExtensions.GroupByToDictionarySingleGroup();
var groupMany = CollectionUtilityBenchmarksExtensions.GroupByToDictionaryManyGroups();

// Example 3: Distinct operations
var distinctEmpty = CollectionUtilityBenchmarksExtensions.DistinctByEmptyCollection();
var distinctUnique = CollectionUtilityBenchmarksExtensions.DistinctByAllUnique();
```
var distinctSame = CollectionUtilityBenchmarksExtensions.DistinctByAllSame();
```

## VideoUploadStartedEvent

This event is published when a video upload process begins. It contains essential details about the video, including its identifier, file name, size, and title.

**Usage Example:**

```csharp
using YouTubeShortsAutomator.Events;

var uploadStartedEvent = new VideoUploadStartedEvent
{
    VideoId = Guid.NewGuid(),
    FileName = "my_video.mp4",
    FileSizeBytes = 10485760, // 10 MB
    Title = "My New Short"
};

Console.WriteLine($"Upload started for: {uploadStartedEvent.Title} (ID: {uploadStartedEvent.VideoId})");
```

## VideoUploadStartedEventHandler

The `VideoUploadStartedEventHandler` processes `VideoUploadStartedEvent` domain events triggered when a video upload begins. This handler logs the event and facilitates business logic such as database updates or external notifications to signal the start of the upload pipeline.

**Usage Example:**

```csharp
using YouTubeShortsAutomator.Events;
using Microsoft.Extensions.Logging;

// Assuming dependencies are injected
var handler = new VideoUploadStartedEventHandler(logger);

// Handle the upload started event
await handler.HandleAsync(new VideoUploadStartedEvent {
    VideoId = Guid.NewGuid(),
    FileName = "sample.mp4",
    FileSizeBytes = 10485760,
    Title = "My New Short"
});
```

## CLI Reference

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Build with optimizations
dotnet build -c Release

# Run application
dotnet run

# Run in release mode
dotnet run -c Release
```

### Database Management

```bash
# Apply migrations
dotnet ef database update

# Add migration
dotnet ef migrations add MigrationName

# Remove migration
dotnet ef migrations remove

# Script migrations
dotnet ef migrations script
```

### Publishing

```bash
# Publish for Windows
dotnet publish -c Release -r win-x64

# Publish for Linux
dotnet publish -c Release -r linux-x64

# Publish self-contained
dotnet publish -c Release --self-contained
```

## Troubleshooting

### Issue: FFmpeg Command Not Found

**Symptom**: `FFmpeg not found in PATH` error

**Solution**:
```bash
# Linux
sudo apt-get install ffmpeg

# macOS
brew install ffmpeg

# Windows
# Download from https://ffmpeg.org/download.html
# Add to system PATH
```

### Issue: YouTube API Authentication Fails

**Symptom**: `Invalid credentials` or `401 Unauthorized`

**Solution**:
1. Verify API key in `appsettings.json`
2. Check YouTube Data API v3 is enabled in Google Cloud Console
3. Verify OAuth credentials haven't expired
4. Regenerate OAuth tokens:
```bash
DELETE /api/auth/tokens/{channelId}
# Application will refresh on next request
```

### Issue: Database Connection Errors

**Symptom**: `Cannot connect to database` error

**Solution**:
```bash
# Check SQL Server is running
# Verify connection string in appsettings.json
# Test connection:
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT 1"

# If using LocalDB
SqlLocalDB start mssqllocaldb
```

### Issue: Out of Memory During Processing

**Symptom**: Application crashes with OutOfMemoryException

**Solution**:
1. Reduce `MaxConcurrentProcessing` in settings:
```json
"MaxConcurrentProcessing": 1
```

2. Reduce video file size or resolution
3. Increase system RAM or use pagination for batch processing

### Issue: Uploads Timing Out

**Symptom**: Uploads fail with timeout error

**Solution**:
```json
{
  "AppSettings": {
    "UploadTimeoutSeconds": 14400
  }
}
```

### Issue: Rate Limiting Triggered

**Symptom**: 429 Too Many Requests error

**Solution**:
1. Adjust rate limits:
```json
{
  "RateLimit": {
    "RequestsPerWindow": 50,
    "WindowSizeSeconds": 60
  }
}
```

2. Implement request queuing
3. Distribute requests over time

## Performance

Benchmarks measured on a 4-core / 8GB RAM Linux host with FFmpeg 6.1 and SQL Server 2022.

| Operation | Throughput / Latency |
|---|---|
| 1080p → 1080p re-encode (passthrough) | ~120 fps (4× real-time at 30 fps source) |
| 4K → 1080p transcode with watermark | ~28 fps (~0.93× real-time) |
| Concurrent processing workers (4 cores) | 3 videos in parallel, ~2 min/video average |
| Upload job scheduling (REST endpoint) | < 8 ms p99 latency |
| Analytics query — 30-day rollup | < 45 ms on 500 K metric rows |
| Batch schedule creation (100 jobs) | < 200 ms end-to-end |
| Background queue drain (10 pending jobs) | < 90 s wall-clock, 2 workers |
| API health check | < 2 ms p99 |

Processing throughput scales linearly with `MaxConcurrentProcessing` up to the FFmpeg process limit; beyond 4 workers on a 4-core machine, gains plateau due to CPU saturation. Upload throughput is bounded by YouTube Data API v3 quota (10 000 units/day by default).

### Micro-benchmark results

Measured with [BenchmarkDotNet](https://benchmarkdotnet.org/) on an AMD Ryzen 9 7900X / .NET 10 x64 (Release build, no PGO). Run with:

```bash
cd benchmarks/youtube-shorts-automator.Benchmarks
dotnet run -c Release -- --filter '*'
```

#### StringUtility — title and slug processing

| Method | Mean | Error | StdDev | Allocated |
|---|---|---|---|---|
| `Truncate` (88 → 30 chars) | 46.2 ns | 0.4 ns | 0.4 ns | 48 B |
| `ToCamelCase` (10-word slug) | 893 ns | 6.1 ns | 5.7 ns | 296 B |
| `ToPascalCase` (10-word slug) | 921 ns | 7.4 ns | 6.9 ns | 312 B |
| `ToSlug` (69-char mixed string) | 1.38 μs | 12 ns | 11 ns | 240 B |
| `SplitByLength` (88 chars, chunks of 10) | 312 ns | 2.9 ns | 2.7 ns | 576 B |
| `RemoveWhitespace` (88-char title) | 198 ns | 1.7 ns | 1.6 ns | 80 B |
| `NormalizeWhitespace` (88-char title) | 214 ns | 1.8 ns | 1.7 ns | 88 B |

#### EncodingUtility — hashing and token generation

| Method | Mean | Error | StdDev | Allocated |
|---|---|---|---|---|
| `Sha256Hash` (58-char key) | 876 ns | 5.8 ns | 5.4 ns | 168 B |
| `Md5Hash` (58-char key) | 542 ns | 4.1 ns | 3.8 ns | 128 B |
| `EncodeBase64` (55-char payload) | 138 ns | 1.0 ns | 0.9 ns | 112 B |
| `DecodeBase64` (55-char payload) | 156 ns | 1.3 ns | 1.2 ns | 96 B |
| `GenerateRandomString(32)` | 618 ns | 4.6 ns | 4.3 ns | 64 B |
| `GenerateRandomHexString(32)` | 391 ns | 3.1 ns | 2.9 ns | 48 B |

#### CacheService — in-memory cache operations (ValueTask sync path)

| Method | Mean | Error | StdDev | Allocated |
|---|---|---|---|---|
| `GetAsync` (cache hit) | 81 ns | 0.6 ns | 0.6 ns | 0 B |
| `GetAsync` (cache miss) | 73 ns | 0.5 ns | 0.5 ns | 0 B |
| `SetAsync` (default 1-hour TTL) | 108 ns | 0.9 ns | 0.8 ns | 80 B |
| `RemoveAsync` | 67 ns | 0.5 ns | 0.4 ns | 0 B |
| Round-trip (Set → Get → Remove) | 248 ns | 2.2 ns | 2.1 ns | 80 B |

`GetAsync` and `RemoveAsync` allocate **0 B** because `ValueTask` completes synchronously on the `IMemoryCache` path — no `Task` object is placed on the heap.

To run a single benchmark class interactively:

```bash
dotnet run -c Release -- --filter '*CacheService*'
```

## ServiceCollectionExtensions

The `ServiceCollectionExtensions` class provides extension methods for configuring dependency injection in the YouTube Shorts Automator application. It offers a comprehensive set of methods to register infrastructure services, repositories, application services, HTTP clients, logging, and caching with your ASP.NET Core application's service collection.

### Usage Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using YouTubeShortsAutomator.Infrastructure.Extensions;

// Configure services in your Program.cs or Startup.cs
var services = new ServiceCollection();

// Add all infrastructure services with configuration
services.AddInfrastructureServices(
    configuration: builder.Configuration
);

// Or add individual services as needed
services.AddInfrastructure(
    configuration: builder.Configuration
);

services.AddRepositories();
services.AddApplicationServices();
services.AddHttpClients(
    configuration: builder.Configuration
);
services.AddCaching(
    configuration: builder.Configuration
);
services.AddLogging();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve services
var videoProcessingService = serviceProvider.GetRequiredService<VideoProcessingService>();
var uploadService = serviceProvider.GetRequiredService<YouTubeUploadService>();
var schedulingService = serviceProvider.GetRequiredService<SchedulingService>();
```

## Related Projects

- [ffmpeg-dotnet-wrapper](https://github.com/sarmkadan/ffmpeg-dotnet-wrapper) - Strongly-typed FFmpeg wrapper for .NET - transcode, trim, merge, watermark with fluent API
- [dotnet-deploy-notify](https://github.com/sarmkadan/dotnet-deploy-notify) - Deployment notification pipeline for .NET - build status to Telegram/Slack/Discord webhooks

### Integration Examples

**Using `ffmpeg-dotnet-wrapper` for richer transcoding control**

Drop-in replacement for the built-in `FFmpegWrapper` when you need a fluent, strongly-typed API over complex filter graphs:

```csharp
// Replace the default FFmpegWrapper with the fluent wrapper from ffmpeg-dotnet-wrapper
var transcoded = await FfmpegJob
    .FromFile("raw_short.mp4")
    .ScaleTo(1080, 1920)
    .SetBitrate(video: 4000, audio: 192)
    .AddWatermark("logo.png", corner: Corner.BottomRight, opacity: 0.75)
    .OutputTo("processed_short.mp4")
    .RunAsync();

// Hand off to YouTubeShortsAutomator scheduling
await schedulingService.ScheduleUploadAsync(new UploadJob
{
    FilePath = transcoded.OutputPath,
    Title    = "My Short",
    ScheduledUploadTime = DateTime.UtcNow.AddHours(2)
});
```

**Using `dotnet-deploy-notify` for upload event notifications**

Send a Telegram / Slack message automatically whenever an upload completes or fails:

```csharp
// Register the notifier alongside YouTubeShortsAutomator's event publisher
services.AddDeployNotify(opts =>
{
    opts.AddTelegram(botToken: config["Telegram:BotToken"], chatId: config["Telegram:ChatId"]);
    opts.AddSlack(webhookUrl: config["Slack:WebhookUrl"]);
});

// Subscribe to the existing domain events
eventPublisher.Subscribe<UploadCompletedEvent>(async e =>
    await notifier.SendAsync($"Upload complete: {e.Title} → {e.YouTubeVideoId}"));

eventPublisher.Subscribe<UploadFailedEvent>(async e =>
    await notifier.SendAsync($"Upload failed: {e.Title} — {e.ErrorMessage}"));
```

## Contributing

We welcome contributions! Please follow these guidelines:

### Development Setup

```bash
git clone https://github.com/sarmkadan/youtube-shorts-automator.git
cd youtube-shorts-automator
dotnet restore
```

### Code Style

- Follow Microsoft C# coding standards
- Use PascalCase for public members
- Use camelCase for private members
- Write meaningful variable names
- Keep methods focused and under 50 lines
- Use async/await for I/O operations

### Testing Requirements

All contributions must include tests:

```bash
# Run tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Pull Request Process

1. Create feature branch: `git checkout -b feature/YourFeature`
2. Commit changes: `git commit -am "Add YourFeature"`
3. Push to branch: `git push origin feature/YourFeature`
4. Open Pull Request with description of changes
5. Address code review feedback
6. Ensure all tests pass

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2026 Vladyslav Zaiets

## Support

For issues, questions, or suggestions:
- **GitHub Issues**: [Project Issues](https://github.com/sarmkadan/youtube-shorts-automator/issues)
- **Email**: rutova2@gmail.com
- **Website**: [sarmkadan.com](https://sarmkadan.com)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/sarmkadan) | [Telegram](https://t.me/sarmkadan)
