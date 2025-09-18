# YouTube Shorts Automator

An automated YouTube Shorts upload pipeline built with .NET 10, featuring FFmpeg processing, Google APIs integration, intelligent scheduling, and comprehensive analytics dashboard.

## Features

- **Automated Video Processing**: FFmpeg-based transcoding with customizable profiles
- **YouTube Integration**: Native Google APIs for seamless upload and metadata management
- **Smart Scheduling**: Schedule uploads at optimal times with automatic retry logic
- **Analytics Dashboard**: Real-time performance metrics and engagement tracking
- **Multi-Channel Support**: Manage multiple YouTube channels from a single platform
- **Batch Processing**: Process and upload multiple videos concurrently
- **Error Handling**: Comprehensive exception handling with automatic recovery
- **Extensible Architecture**: Clean separation of concerns with service-oriented design

## Architecture

### Domain Models
- `VideoShort`: Core video entity with metadata and processing status
- `UploadJob`: Manages upload scheduling and progress tracking
- `ProcessingTask`: Tracks individual processing operations
- `ProcessingProfile`: Video encoding configuration templates
- `YouTubeChannel`: Channel credentials and authentication
- `AnalyticsData`: Performance metrics and engagement data
- `UploadResult`: Upload completion results and metadata

### Services
- `VideoProcessingService`: FFmpeg transcoding, watermarking, color grading
- `YouTubeUploadService`: Upload management, metadata updates, publishing
- `SchedulingService`: Job scheduling, rescheduling, cancellation
- `AnalyticsService`: Metrics sync, report generation, performance analysis
- `JobOrchestrationService`: Pipeline coordination and workflow orchestration

### Data Access
- Generic `IRepository<T>` interface for CRUD operations
- Specialized repositories: `VideoShortRepository`, `UploadJobRepository`, `AnalyticsRepository`
- `DatabaseContext`: Connection management and query execution

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "AppSettings": {
    "ConnectionString": "Server=localhost;Database=YouTubeShortsAutomator;...",
    "MaxConcurrentUploads": 3,
    "MaxConcurrentProcessing": 2,
    "DefaultRetryCount": 3,
    "YouTubeApiKey": "your-api-key",
    "YouTubeClientId": "your-client-id",
    "YouTubeClientSecret": "your-client-secret"
  }
}
```

## Project Structure

```
src/
├── Domain/
│   ├── Models/          # Entity classes with business logic
│   └── Exceptions/      # Custom exception types
├── Services/            # Business logic layer
├── Data/                # Repository pattern implementation
├── Configuration/       # DI and settings
├── Constants/           # Enums and constants
└── Program.cs           # Application entry point
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server 2019 or later
- FFmpeg installed and in PATH
- YouTube API credentials

### Installation

```bash
# Clone repository
git clone https://github.com/yourusername/youtube-shorts-automator.git
cd youtube-shorts-automator

# Restore dependencies
dotnet restore

# Update appsettings.json with your configuration
# Build the project
dotnet build

# Run
dotnet run
```

## Usage Examples

### Creating a Processing Profile
```csharp
var profile = new ProcessingProfile
{
    Name = "High Quality",
    VideoWidth = 1080,
    VideoHeight = 1920,
    VideoBitrate = 4000,
    FrameRate = 30
};
```

### Running the Full Pipeline
```csharp
var result = await orchestrationService.ProcessFullPipelineAsync(
    videoShortId: 1,
    processingProfile: profile,
    channel: youtubeChannel,
    scheduledUploadTime: DateTime.UtcNow.AddHours(2)
);
```

### Processing Scheduled Uploads
```csharp
await orchestrationService.ProcessReadyUploadsAsync(youtubeChannel);
```

## Constants

Key configuration constants are defined in `Constants/Constants.cs`:
- `MAX_DURATION_SECONDS`: 60 (YouTube Shorts limit)
- `MAX_FILE_SIZE_MB`: 512
- `MAX_CONCURRENT_UPLOADS`: 3
- `DEFAULT_RETRY_COUNT`: 3
- `PROCESSING_QUEUE_LIMIT`: 100

## Error Handling

The system includes comprehensive exception handling:
- `VideoProcessingException`: FFmpeg processing failures
- `YouTubeApiException`: YouTube API errors with retry logic
- `UploadException`: Upload-related failures
- `ValidationException`: Data validation errors
- `SchedulingException`: Scheduling operation failures

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

## Author

**Vladyslav Zaiets**  
CTO & Software Architect  
https://sarmkadan.com

---

This project demonstrates enterprise-grade .NET architecture with clean code principles, comprehensive error handling, and production-ready implementations.
