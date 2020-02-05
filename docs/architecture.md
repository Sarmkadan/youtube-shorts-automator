// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Guide

## System Overview

YouTube Shorts Automator is built on a layered, domain-driven architecture that separates concerns and enables maintainability, testability, and scalability.

## Architectural Layers

### 1. Presentation Layer (Controllers)

Controllers handle HTTP requests and orchestrate services.

**Key Controllers**:
- `ProcessingController`: Video upload and processing management
- `ScheduleController`: Upload scheduling operations
- `AnalyticsController`: Metrics and reporting endpoints
- `HealthController`: System health and status checks
- `VideoController`: Video management CRUD operations

**Responsibilities**:
- Parse incoming requests
- Validate input parameters
- Call application services
- Format and return responses

### 2. Application Layer (Services)

Services implement business logic and coordinate domain operations.

**Core Services**:

#### VideoProcessingService
Manages FFmpeg-based video transcoding and optimization.

```csharp
public class VideoProcessingService
{
    // Transcode video with specified profile
    public async Task<string> TranscodeVideoAsync(
        string inputPath,
        string outputPath,
        ProcessingProfile profile);

    // Apply watermark to video
    public async Task<string> ApplyWatermarkAsync(
        string inputPath,
        string outputPath,
        string watermarkPath,
        WatermarkPosition position);

    // Analyze video quality
    public async Task<VideoQualityAnalysis> AnalyzeVideoAsync(string videoPath);

    // Apply color grading
    public async Task<string> ApplyColorGradingAsync(
        string inputPath,
        string outputPath,
        ColorGradingProfile profile);
}
```

#### YouTubeUploadService
Handles YouTube API interactions for uploads.

```csharp
public class YouTubeUploadService
{
    // Upload video to YouTube
    public async Task<UploadResult> UploadVideoAsync(
        UploadJob job,
        YouTubeChannel channel);

    // Update video metadata
    public async Task<bool> UpdateMetadataAsync(
        string videoId,
        VideoMetadata metadata);

    // Add video to playlist
    public async Task<bool> AddToPlaylistAsync(
        string videoId,
        string playlistId);

    // Set video thumbnail
    public async Task<bool> SetThumbnailAsync(
        string videoId,
        byte[] thumbnailData);
}
```

#### SchedulingService
Manages job scheduling and retry logic.

```csharp
public class SchedulingService
{
    // Schedule upload job
    public async Task<int> ScheduleUploadAsync(UploadJob job);

    // Process ready uploads
    public async Task<int> ProcessReadyUploadsAsync(YouTubeChannel channel);

    // Retry failed job
    public async Task<bool> RetryJobAsync(int jobId);

    // Cancel scheduled job
    public async Task<bool> CancelJobAsync(int jobId);
}
```

#### AnalyticsService
Provides metrics synchronization and reporting.

```csharp
public class AnalyticsService
{
    // Get video metrics
    public async Task<VideoMetrics> GetVideoMetricsAsync(string youtubeVideoId);

    // Generate performance report
    public async Task<byte[]> GeneratePerformanceReportAsync(
        DateTime startDate,
        DateTime endDate,
        ReportFormat format);

    // Sync analytics from YouTube
    public async Task<int> SyncAnalyticsAsync(YouTubeChannel channel);

    // Get trending videos
    public async Task<List<VideoMetrics>> GetTrendingVideosAsync(
        int days,
        int limit);
}
```

#### JobOrchestrationService
Coordinates complete workflows across services.

```csharp
public class JobOrchestrationService
{
    // Execute complete pipeline
    public async Task<PipelineResult> ProcessFullPipelineAsync(
        int videoShortId,
        ProcessingProfile profile,
        YouTubeChannel channel,
        DateTime scheduledUploadTime);

    // Handle job completion
    public async Task<bool> CompleteJobAsync(int jobId, UploadResult result);

    // Handle job failure with recovery
    public async Task<bool> HandleJobFailureAsync(int jobId, Exception ex);
}
```

### 3. Domain Layer (Models and Business Logic)

Domain models represent core business concepts.

**Core Models**:

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
    public string? YouTubeVideoId { get; set; }
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
    public int MaxRetries { get; set; } = 3;
    public string? YouTubeVideoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Calculate if job is due
    public bool IsDue => ScheduledUploadTime <= DateTime.UtcNow;
    
    // Check if can retry
    public bool CanRetry => RetryCount < MaxRetries && Status == UploadStatus.Failed;
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
    public int Progress { get; set; } // 0-100
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Calculate elapsed time
    public TimeSpan Elapsed => 
        (CompletedAt ?? DateTime.UtcNow) - StartedAt;
}
```

#### YouTubeChannel
```csharp
public class YouTubeChannel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ChannelId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Check if token needs refresh
    public bool NeedsTokenRefresh => TokenExpiresAt < DateTime.UtcNow.AddMinutes(5);
}
```

### 4. Data Access Layer (Repositories)

Repositories abstract data storage and retrieval.

```csharp
// Generic repository interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}

// Specialized repositories
public interface IVideoShortRepository : IRepository<VideoShort>
{
    Task<List<VideoShort>> GetPendingProcessingAsync();
    Task<List<VideoShort>> GetByStatusAsync(ProcessingStatus status);
}

public interface IUploadJobRepository : IRepository<UploadJob>
{
    Task<List<UploadJob>> GetDueForUploadAsync();
    Task<List<UploadJob>> GetByStatusAsync(UploadStatus status);
    Task<int> GetFailedCountAsync();
}

public interface IAnalyticsRepository : IRepository<AnalyticsData>
{
    Task<AnalyticsData?> GetByVideoIdAsync(string youtubeVideoId);
    Task<List<AnalyticsData>> GetTrendingAsync(int days, int limit);
    Task<AggregatedMetrics> GetAggregatedMetricsAsync(
        DateTime startDate,
        DateTime endDate);
}
```

### 5. Infrastructure Layer

Handles external integrations and data persistence.

**Key Components**:

#### FFmpegWrapper
```csharp
public class FFmpegWrapper
{
    // Execute FFmpeg command
    public async Task<ProcessResult> ExecuteAsync(
        string arguments,
        CancellationToken cancellationToken);

    // Validate FFmpeg installation
    public bool ValidateInstallation();

    // Get video information
    public async Task<VideoInfo> GetVideoInfoAsync(string filePath);
}
```

#### GoogleApiClient
```csharp
public class GoogleApiClient
{
    // Authenticate with OAuth
    public async Task<bool> AuthenticateAsync(
        string clientId,
        string clientSecret,
        string redirectUri);

    // Get authenticated YouTube service
    public YouTubeService GetYouTubeService();

    // Refresh access token
    public async Task<bool> RefreshTokenAsync(YouTubeChannel channel);
}
```

#### DatabaseContext
```csharp
public class DatabaseContext : DbContext
{
    public DbSet<VideoShort> Videos { get; set; }
    public DbSet<UploadJob> UploadJobs { get; set; }
    public DbSet<ProcessingTask> ProcessingTasks { get; set; }
    public DbSet<YouTubeChannel> YouTubeChannels { get; set; }
    public DbSet<AnalyticsData> Analytics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints
        modelBuilder.Entity<UploadJob>()
            .HasOne(j => j.VideoShort)
            .WithMany()
            .HasForeignKey(j => j.VideoShortId);

        modelBuilder.Entity<ProcessingTask>()
            .HasOne(t => t.UploadJob)
            .WithMany()
            .HasForeignKey(t => t.UploadJobId);
    }
}
```

## Data Flow

### Upload Pipeline

```
┌─────────────────────┐
│  User Upload File   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────────────────┐
│  ProcessingController.Upload()   │
│  - Validate file                │
│  - Store in processing folder   │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│  VideoProcessingService         │
│  - Transcode with profile       │
│  - Apply watermark              │
│  - Quality analysis             │
│  - Save to output folder        │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│  VideoRepository.Add()          │
│  - Insert into database         │
│  - Set status: Processed        │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│  Return JobId to User           │
│  User can schedule upload       │
└─────────────────────────────────┘
```

### Scheduling Pipeline

```
┌─────────────────────┐
│  ScheduleController │
│  Create UploadJob   │
└──────────┬──────────┘
           │
           ▼
┌──────────────────────────────┐
│  SchedulingService           │
│  - Validate job              │
│  - Calculate schedule time   │
│  - Store in database         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  ProcessingBackgroundService │
│  - Polls every minute        │
│  - Finds due jobs            │
│  - Invokes orchestration     │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  JobOrchestrationService     │
│  - Load video file           │
│  - Authenticate with YouTube │
│  - Upload video              │
│  - Set metadata              │
│  - Update job status         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  AnalyticsBackgroundService  │
│  - Schedule metrics sync     │
│  - Retrieve performance data │
└──────────────────────────────┘
```

## Background Services

### ProcessingBackgroundService
Runs continuously to process due uploads.

```csharp
public class ProcessingBackgroundService : BackgroundService
{
    // Executes every ScheduleCheckIntervalSeconds
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Find jobs scheduled for now
            var dueJobs = await _schedulingService
                .GetDueUploadsAsync();

            // Process each job
            foreach (var job in dueJobs)
            {
                await _orchestrationService
                    .ProcessJobAsync(job);
            }

            // Wait before next check
            await Task.Delay(
                _config.ScheduleCheckIntervalSeconds * 1000,
                stoppingToken);
        }
    }
}
```

### AnalyticsBackgroundService
Periodically synchronizes YouTube analytics.

```csharp
public class AnalyticsBackgroundService : BackgroundService
{
    // Executes every AnalyticsSyncIntervalHours
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Sync all channels
            var channels = await _channelRepository
                .GetAllAsync();

            foreach (var channel in channels)
            {
                await _analyticsService
                    .SyncAnalyticsAsync(channel);
            }

            // Wait before next sync
            await Task.Delay(
                _config.AnalyticsSyncIntervalHours * 3600000,
                stoppingToken);
        }
    }
}
```

## Exception Handling

Custom exceptions provide domain-specific error information.

```csharp
public class VideoProcessingException : Exception
{
    public string? VideoPath { get; set; }
    public string? FFmpegOutput { get; set; }
}

public class YouTubeApiException : Exception
{
    public bool IsRetryable { get; set; }
    public int? HttpStatusCode { get; set; }
}

public class UploadException : Exception
{
    public int UploadJobId { get; set; }
    public UploadStatus CurrentStatus { get; set; }
}

public class ValidationException : Exception
{
    public Dictionary<string, string> Errors { get; set; }
}
```

## Middleware Pipeline

```
Request Comes In
       │
       ▼
┌──────────────────────────────┐
│  ApiKeyValidationMiddleware  │
│  - Verify API key            │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  RateLimitingMiddleware      │
│  - Check request rate        │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  RequestLoggingMiddleware    │
│  - Log request details       │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Route to Controller         │
│  - Process request           │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  ErrorHandlingMiddleware     │
│  - Catch exceptions          │
│  - Format error response     │
└──────────┬───────────────────┘
           │
           ▼
Response Sent to Client
```

## Dependency Injection

Services are registered in `Program.cs`:

```csharp
// Register infrastructure services
builder.Services.AddScoped<DatabaseContext>();
builder.Services.AddScoped<FFmpegWrapper>();
builder.Services.AddScoped<GoogleApiClient>();
builder.Services.AddScoped<CacheService>();

// Register repositories
builder.Services.AddScoped<IVideoShortRepository, VideoShortRepository>();
builder.Services.AddScoped<IUploadJobRepository, UploadJobRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Register application services
builder.Services.AddScoped<VideoProcessingService>();
builder.Services.AddScoped<YouTubeUploadService>();
builder.Services.AddScoped<SchedulingService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<JobOrchestrationService>();

// Register background services
builder.Services.AddHostedService<ProcessingBackgroundService>();
builder.Services.AddHostedService<AnalyticsBackgroundService>();
builder.Services.AddHostedService<CleanupBackgroundService>();
```

## Performance Considerations

### Caching Strategy
- Cache processing profiles (1 hour TTL)
- Cache YouTube channel credentials (30 minutes TTL)
- Cache analytics aggregates (1 hour TTL)

### Concurrency Control
- `MaxConcurrentUploads`: Limits parallel YouTube uploads
- `MaxConcurrentProcessing`: Limits parallel FFmpeg jobs
- Database connection pooling for optimal resource usage

### Database Optimization
- Indexes on Status and CreatedAt columns
- Pagination for large result sets
- Connection pooling with configurable pool size

## Security Architecture

### API Authentication
- API Key in request headers
- Validated by ApiKeyValidationMiddleware
- Rate limiting to prevent abuse

### YouTube Authentication
- OAuth 2.0 flow
- Secure token storage
- Automatic token refresh
- No credentials stored in logs

### Data Protection
- Webhook secret validation
- HTTPS for all external communication
- SQL injection prevention via parameterized queries
- XSS protection in API responses
