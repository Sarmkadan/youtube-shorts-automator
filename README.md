[![Build](https://github.com/sarmkadan/youtube-shorts-automator/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/youtube-shorts-automator/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# YouTube Shorts Automator

> **Automated YouTube Shorts upload pipeline built with .NET 10**  
> FFmpeg processing • Google APIs integration • Smart scheduling • Real-time analytics dashboard

An enterprise-grade solution for automating the entire YouTube Shorts lifecycle: from video processing and optimization to scheduled uploads and performance analytics. Built with clean architecture principles, comprehensive error handling, and production-ready implementation patterns.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [System Requirements](#system-requirements)
- [Installation](#installation)
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

### Enterprise Features
- **Multi-Channel Support**: Manage multiple YouTube channels from single platform
- **Role-based Access Control**: User permissions and channel delegation
- **Comprehensive Logging**: Structured logging with Serilog integration
- **Exception Handling**: Global error handling with automatic recovery
- **Rate Limiting**: Configurable API rate limiting and throttling
- **Caching Layer**: Redis-based caching for performance optimization

## Architecture

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
- **FFmpeg 4.0+** - [Download](https://ffmpeg.org/download.html)
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

```bash
# Build Docker image
docker build -t youtube-shorts-automator .

# Run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
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

## API Reference

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

**GET** `/api/metrics`
- Get system metrics
- Response: `{ processingQueue: int, failedJobs: int, uptime: int }`

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
