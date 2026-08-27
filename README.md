## FileSystemUtility

`FileSystemUtility` provides static methods for common file system operations, such as ensuring directories exist, validating video files, reading and writing files, and managing directories.

### Usage Example

```csharp
// Ensure the output directory exists
await FileSystemUtility.EnsureDirectoryExistsAsync("./output");

// Check if a file is a valid video file
bool isValid = FileSystemUtility.IsValidVideoFile("./video.mp4");

// Read a text file
string content = await FileSystemUtility.ReadFileAsStringAsync("./notes.txt");

// Write the content to a new file in the output directory
await FileSystemUtility.WriteFileAsync("./output/notes.txt", content);

// Get all text files in the output directory
string[] textFiles = FileSystemUtility.GetFilesWithExtension("./output", "txt");

// Get the total size of the output directory
long directorySize = FileSystemUtility.GetDirectorySizeBytes("./output");
```

## TimeoutUtility

`TimeoutUtility` provides static helper methods for managing timeouts, deadlines, and backoff strategies in asynchronous and synchronous operations. It simplifies the creation of cancellation tokens, checking expiration status, calculating remaining time, and implementing retry logic with exponential backoff and jitter.

### Usage Example

```csharp
// Define a deadline 30 seconds from now
DateTime deadline = TimeoutUtility.GetDeadline(TimeSpan.FromSeconds(30));

// Check if the deadline has passed
if (!TimeoutUtility.IsExpired(deadline))
{
    // Create a cancellation token tied to the deadline
    using var cts = TimeoutUtility.CreateCancellationToken(deadline);

    // Execute an async operation with a timeout
    var result = await TimeoutUtility.ExecuteWithTimeoutAsync(
        async ct => await Task.Delay(1000, ct),
        deadline,
        cts.Token);

    // Calculate remaining time
    TimeSpan remaining = TimeoutUtility.GetTimeRemaining(deadline);
}

// Implement retry logic with exponential backoff and jitter
TimeSpan delay = TimeoutUtility.CalculateBackoffDelayWithJitter(attempt: 3, baseDelay: TimeSpan.FromSeconds(1));
```

## EncodingUtility

`EncodingUtility` provides static helpers for encoding, decoding, and hashing data, supporting Base64, URL, and HTML transformations alongside SHA-256 and MD5 hashes with optional verification. It also generates cryptographically secure random strings, hex strings, and GUIDs, and includes helpers for parsing and building URL query strings.

### Usage Example

```csharp
// Encode and decode Base64 strings
string encoded = EncodingUtility.EncodeBase64("Hello, world!");
string decoded = EncodingUtility.DecodeBase64(encoded);

// Compute hashes
string sha256 = EncodingUtility.ComputeSha256Hash("sensitive data");
string md5 = EncodingUtility.ComputeMd5Hash("checksum payload");

// Verify input against a previously computed SHA-256 hash
bool isValid = EncodingUtility.VerifyHash("sensitive data", sha256);

// URL encode and decode
string urlEncoded = EncodingUtility.UrlEncode("search term & more");
string urlDecoded = EncodingUtility.UrlDecode(urlEncoded);

// Build and parse query strings
var queryParams = new Dictionary<string, string>
{
    ["page"] = "2",
    ["sort"] = "desc"
};
string query = EncodingUtility.BuildQueryString(queryParams);
Dictionary<string, string> parsed = EncodingUtility.ParseQueryString(query);

// Securely generate random values
string token = EncodingUtility.GenerateRandomString(32);
string hexToken = EncodingUtility.GenerateRandomHexString(16);
Guid secureId = EncodingUtility.GenerateSecureGuid();

// HTML encode and decode
string safeHtml = EncodingUtility.HtmlEncode("<script>alert('x')</script>");
string rawHtml = EncodingUtility.HtmlDecode(safeHtml);
```

## DateTimeUtility

`DateTimeUtility` provides static helpers for everyday date and time work, including UTC conversions, timestamp formatting (UTC, ISO 8601, and relative), and duration calculation and formatting. It also makes it easy to compute calendar boundaries such as the start and end of a day, week, or month, and to answer quick questions like whether a moment falls within business hours or whether a year is a leap year.

### Usage Example

```csharp
var now = DateTime.Now;

// Format dates
string formatted = DateTimeUtility.FormatUtcDateTime(now);
string iso = DateTimeUtility.FormatIsoDateTime(now);
string relative = DateTimeUtility.FormatRelativeTime(now.AddMinutes(-10));

// Convert between local time and UTC
DateTime utc = DateTimeUtility.ConvertToUtc(now);
DateTime local = DateTimeUtility.ConvertFromUtc(utc);

// Business hours and calendar lookups
bool duringBusinessHours = DateTimeUtility.IsWithinBusinessHours(now);
int dayOfYear = DateTimeUtility.GetDayOfYear(now);
int weekOfYear = DateTimeUtility.GetWeekOfYear(now);

// Calendar boundaries
DateTime dayStart = DateTimeUtility.GetStartOfDay(now);
DateTime dayEnd = DateTimeUtility.GetEndOfDay(now);
DateTime weekStart = DateTimeUtility.GetStartOfWeek(now);
DateTime weekEnd = DateTimeUtility.GetEndOfWeek(now);
DateTime monthStart = DateTimeUtility.GetStartOfMonth(now);
DateTime monthEnd = DateTimeUtility.GetEndOfMonth(now);

// Leap year check
bool leapYear = DateTimeUtility.IsLeapYear(now.Year);

// Durations
TimeSpan elapsed = DateTimeUtility.CalculateDuration(dayStart, now);
string durationText = DateTimeUtility.FormatDuration(elapsed);
var components = DateTimeUtility.FormatDurationComponents(elapsed);
Console.WriteLine($"{components.Days}d {components.Hours}h {components.Minutes}m {components.Seconds}s");
```

## ValidationUtility

`ValidationUtility` provides a comprehensive set of static helper methods for validating common data types and formats, including emails, URLs, YouTube identifiers, video metadata, file paths, time spans, JSON strings, and schedule times. It simplifies input validation by returning structured results with clear success/failure indicators and optional error messages.

### Usage Example

```csharp
// Validate contact and link information
var (emailValid, emailError) = ValidationUtility.ValidateEmail("creator@example.com");
var (urlValid, urlError) = ValidationUtility.ValidateUrl("https://youtube.com/channel/UCxxxxx");

// Validate YouTube-specific identifiers
var (channelIdValid, channelIdError) = ValidationUtility.ValidateYouTubeChannelId("UCxxxxxxxxxxxxxxxxxxxx");
var (videoIdValid, videoIdError) = ValidationUtility.ValidateYouTubeVideoId("dQw4w9WgXcQ");

// Validate video metadata
var (titleValid, titleError) = ValidationUtility.ValidateVideoTitle("My Awesome Video");
var (descValid, descError) = ValidationUtility.ValidateVideoDescription("A detailed description of the video...");
var (tagsValid, tagsError) = ValidationUtility.ValidateVideoTags("vlog, tech, tutorial");

// Validate video file path
var (fileValid, fileError) = ValidationUtility.ValidateVideoFile("./videos/clip.mp4");

// Check utility formats
bool validTimeSpan = ValidationUtility.IsValidTimeSpan("00:30:00");
bool validJson = ValidationUtility.IsValidJsonString("{\"key\": \"value\"}");

// Validate a schedule time
var (scheduleValid, scheduleError) = ValidationUtility.ValidateScheduleTime("14:30");
```

## ContentCalendarController

`ContentCalendarController` is an ASP.NET Core Web API controller that exposes REST endpoints under `api/content-calendar` for managing the video publishing calendar. It covers the full entry lifecycle — creating, reading (by id, upcoming window, or UTC date range), updating, and deleting entries — as well as running the title/description optimisation engine, applying suggestions, scheduling entries for upload, and recommending high-engagement posting slots per channel. Every action delegates persistence and business rules to `IContentCalendarService` and returns a consistent JSON envelope (`success`, `data`/`message`).

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YouTubeShortsAutomator.Controllers;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

public class ContentCalendarApiExample
{
    private readonly ContentCalendarController _calendarApi;

    // In production MVC activates the controller via dependency injection;
    // here it is constructed manually so its actions can be called directly.
    public ContentCalendarApiExample(IContentCalendarService calendarService, ILogger<ContentCalendarController> logger)
    {
        _calendarApi = new ContentCalendarController(calendarService, logger);
    }

    public async Task RunAsync()
    {
        // Create a new calendar entry
        IActionResult created = await _calendarApi.CreateEntry(new CreateCalendarEntryRequest
        {
            Title              = "Weekly dev vlog",
            Description        = "Behind the scenes of our sprint demo.",
            Tags               = ["devlog", "vlog"],
            Category           = ContentCategory.Other,
            ScheduledPublishAt = DateTime.UtcNow.AddDays(3),
            YouTubeChannelId   = 1,
            Notes              = "Confirm the thumbnail before publishing.",
            Keywords           = ["dotnet", "shorts"]
        });

        // Read entries back
        IActionResult entry    = await _calendarApi.GetEntry(entryId: 1);
        IActionResult upcoming = await _calendarApi.GetUpcoming(daysAhead: 14);
        IActionResult inRange  = await _calendarApi.GetInRange(
            from: DateTime.UtcNow.AddDays(-7),
            to:   DateTime.UtcNow.AddDays(7));

        // Update metadata
        IActionResult updated = await _calendarApi.UpdateEntry(entryId: 1, new UpdateCalendarEntryRequest
        {
            Title = "Weekly dev vlog — episode 2",
            Tags  = ["devlog", "vlog", "dotnet"]
        });

        // Optimise title/description, then apply the best suggestion
        await _calendarApi.OptimizeEntry(entryId: 1);
        IActionResult optimised = await _calendarApi.ApplyOptimization(entryId: 1, suggestionIndex: 0);

        // Schedule the entry for upload
        IActionResult scheduled = await _calendarApi.ScheduleEntry(
            entryId: 1,
            new ScheduleEntryRequest { ScheduledAt = DateTime.UtcNow.AddDays(3) });

        // Recommended posting slots for the channel
        IActionResult slots = await _calendarApi.GetRecommendedSlots(channelId: 1, count: 5);

        // Delete the entry when it is no longer needed
        IActionResult deleted = await _calendarApi.DeleteEntry(entryId: 1);
    }
}
```

## VideoController

`VideoController` is an ASP.NET Core Web API controller that exposes REST endpoints for managing videos throughout their lifecycle. It covers listing a user's videos, retrieving a single video, triggering processing, uploading new videos, reading analytics, checking background job status, and publishing finished videos. Every action returns a standard `IActionResult` response, delegating persistence and business rules to the injected services.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using YouTubeShortsAutomator.Controllers;

public class VideoApiExample
{
    private readonly VideoController _videoApi;

    // In production MVC activates the controller via dependency injection;
    // here it is resolved from a service provider so its actions can be called directly.
    public VideoApiExample(IServiceProvider services)
    {
        _videoApi = services.GetRequiredService<VideoController>();
    }

    public async Task RunAsync()
    {
        // List all videos belonging to a user
        IActionResult userVideos = await _videoApi.GetUserVideos(1);

        // Read a single video back
        IActionResult video = await _videoApi.GetVideo(42);

        // Trigger processing for a video
        IActionResult processed = await _videoApi.ProcessVideo(42);

        // Upload a new video
        IActionResult uploaded = await _videoApi.UploadVideo("./videos/clip.mp4");

        // Read analytics for a video
        IActionResult analytics = await _videoApi.GetAnalytics(42);

        // Check the status of a background job
        IActionResult jobStatus = await _videoApi.GetJobStatus(7);

        // Publish the video once processing has completed
        IActionResult published = await _videoApi.PublishVideo(42);
    }
}
```

## HealthController

`HealthController` is an ASP.NET Core Web API controller that exposes REST endpoints for monitoring the application's health and system status. It provides endpoints for checking overall status, system information, readiness, and liveness, returning detailed health data including database connectivity, configuration state, and version information.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using YouTubeShortsAutomator.Controllers;

public class HealthApiExample
{
    private readonly HealthController _healthApi;

    public HealthApiExample(IServiceProvider services)
    {
        _healthApi = services.GetRequiredService<HealthController>();
    }

    public async Task RunAsync()
    {
        // Check overall health status
        IActionResult status = await _healthApi.GetStatus();

        // Retrieve detailed system information
        IActionResult systemInfo = _healthApi.GetSystemInfo();

        // Check application readiness
        IActionResult readiness = await _healthApi.GetReadiness();

        // Check application liveness
        IActionResult liveness = _healthApi.GetLiveness();

        // Access health data properties
        string statusText = _healthApi.Status;
        string dbStatus = _healthApi.Database;
        string configStatus = _healthApi.Configuration;
        List<string>? configErrors = _healthApi.ConfigurationErrors;
        DateTime timestamp = _healthApi.Timestamp;
        string version = _healthApi.Version;
    }
}
```

## CsvExportFormatter

`CsvExportFormatter` turns in-memory collections into CSV documents returned as encoded bytes, ready to be written to disk or streamed back from a Web API download endpoint. It can export selected or all properties of strongly-typed rows as well as raw dictionary rows, automatically escaping delimiters, quotes, and line breaks so values never corrupt the output. Lower-level helpers (`EscapeCsvField`, `BuildCsvHeaderRow`, `BuildCsvDataRow`) are also exposed for composing custom CSV documents field by field.

### Usage Example

```csharp
using System.Collections.Generic;
using YouTubeShortsAutomator.Formatters;

public class CsvExportExample
{
    private sealed class VideoStat
    {
        public string Title { get; set; } = "";
        public string Channel { get; set; } = "";
        public int Views { get; set; } = 0;
    }

    public void Run()
    {
        var formatter = new CsvExportFormatter(); // defaults: ',' delimiter, UTF-8

        var stats = new List<VideoStat>
        {
            new() { Title = "Sprint 42 demo", Channel = "devlog", Views = 12400 },
            new() { Title = "Release notes, \"part 2\"", Channel = "devlog", Views = 8150 }
        };

        // Export only the columns you care about
        byte[] summary = formatter.ExportToCsv(stats, new[] { nameof(VideoStat.Title), nameof(VideoStat.Views) });

        // Or export every property of each row
        byte[] full = formatter.ExportToCsvWithAllProperties(stats);

        // Export untyped rows straight from a dynamic query result
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["Title"] = "Q&A shorts", ["Views"] = 450 },
            new() { ["Title"] = "Behind the scenes", ["Views"] = 1205 }
        };
        byte[] dictionaryCsv = formatter.ExportDictionariesToCsv(rows, new[] { "Title", "Views" });

        // Compose a CSV document manually with the low-level helpers
        string header = formatter.BuildCsvHeaderRow(new[] { "Title", "Channel" });
        string safeTitle = formatter.EscapeCsvField("Live coding, \"part 3\"");
        string dataRow = formatter.BuildCsvDataRow(new object[] { safeTitle, "devlog" });
    }
}
```

## JsonResponseFormatter

`JsonResponseFormatter` builds the consistent JSON envelopes returned by the API: success responses wrapping a typed payload with a message, structured error responses carrying an error code plus optional details, and paginated responses pairing a page of results with its `PaginationInfo` metadata. Beyond the envelopes, it also covers everyday JSON plumbing — serializing objects, deserializing them back into strongly-typed instances, and pretty-printing indented JSON for logs or debugging.

### Usage Example

```csharp
using System.Collections.Generic;
using YouTubeShortsAutomator.Formatters;

public class JsonResponseExample
{
    private sealed class VideoSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Views { get; set; } = 0;
    }

    public void Run()
    {
        var formatter = new JsonResponseFormatter();

        var video = new VideoSummary { Id = 42, Title = "Sprint 42 demo", Views = 12400 };

        // Wrap a payload in the standard success envelope
        string success = formatter.FormatSuccessResponse(video, "Video loaded successfully");

        // Return a structured error envelope
        string error = formatter.FormatErrorResponse("VIDEO_NOT_FOUND", "No video exists with id 99");

        // Wrap a page of results together with its pagination metadata
        var videos = new List<VideoSummary>
        {
            new() { Id = 1, Title = "Q&A shorts", Views = 450 },
            new() { Id = 2, Title = "Behind the scenes", Views = 1205 }
        };
        string paginated = formatter.FormatPaginatedResponse(videos, new PaginationInfo());

        // Round-trip objects through JSON
        string json = formatter.SerializeToJson(video);
        VideoSummary? restored = formatter.DeserializeJson<VideoSummary>(json);

        // Pretty-print JSON for humans
        string indented = formatter.FormatIndentedJson(video);
    }
}
```

## Notes

- **Null handling:** The validation methods treat `null` or missing required fields as validation failures and include appropriate messages in the returned list. `EnsureValid` will consequently throw.
- **Thread safety:** All members are static and stateless; they do not modify shared mutable state. Consequently, they are safe to call concurrently from multiple threads.
- **Extensibility:** If additional validation rules are required (e.g., format checks for API keys), they should be added inside the implementation of these methods so that `Validate`, `IsValid`, and `EnsureValid` remain consistent.
- **Performance:** The methods perform only lightweight checks (e.g., string emptiness, length constraints). They are intended to be called before expensive operations such as database writes or external API calls.

## ICacheService

`ICacheService` provides an in-memory caching mechanism with support for setting, getting, and removing cache entries, including pattern-based removal and expiration handling.

### Usage Example

```csharp
using YouTubeShortsAutomator.Caching;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class CacheExample
{
    private class UserDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private readonly ICacheService _cacheService;

    public CacheExample(IServiceProvider services)
    {
        _cacheService = services.GetRequiredService<ICacheService>();
    }

    public async Task RunAsync()
    {
        // Set a value in the cache with a 30-second expiration
        _cacheService.Set("user:123", new UserDto { Name = "John", Age = 30 }, TimeSpan.FromSeconds(30));

        // Retrieve the value synchronously
        var user = _cacheService.Get<UserDto>("user:123");
        Console.WriteLine($"User: {user?.Name}, Age: {user?.Age}");

        // Asynchronously retrieve the same value
        var userAsync = await _cacheService.GetAsync<UserDto>("user:123");
        Console.WriteLine($"Async User: {userAsync?.Name}");

        // Check if a key exists
        if (_cacheService.Exists("user:123"))
        {
            Console.WriteLine("Key exists in cache.");
        }

        // Remove a specific key
        _cacheService.Remove("user:123");

        // Remove all keys matching a pattern (e.g., all user keys)
        _cacheService.Set("user:456", new UserDto { Name = "Jane" }, TimeSpan.FromMinutes(10));
        _cacheService.Set("user:789", new UserDto { Name = "Bob" }, TimeSpan.FromMinutes(10));
        _cacheService.RemoveByPattern("user:"); // Removes both user:456 and user:789

        // Asynchronous removal
        await _cacheService.RemoveAsync("user:999"); // Safe even if key doesn't exist
    }
}
```

## ProcessingError

`ProcessingError` represents an error that occurred during a video processing job, capturing details such as the error type, message, severity, and retry information. It is used to track and manage errors in the processing pipeline, including marking errors as resolved and tracking retry attempts.

### Usage Example

```csharp
using YouTubeShortsAutomator.Domain.Models;
using System;

// Example: Creating and handling a processing error
var error = new ProcessingError
{
    Id = Guid.NewGuid(),
    JobId = Guid.NewGuid(),
    ErrorType = ProcessingErrorType.VideoEncodingFailed,
    ErrorMessage = "Failed to encode video due to unsupported codec.",
    Severity = ErrorSeverity.High,
    OccurredAt = DateTime.UtcNow
};

// Check if the error is critical or retryable
if (error.IsCritical)
{
    Console.WriteLine("Critical error requiring immediate attention.");
}

if (error.IsRetryable)
{
    error.RecordRetryAttempt();
    Console.WriteLine($"Retry attempt {error.RetryAttemptCount} recorded.");
}

// After resolving the error
error.MarkAsResolved("Fixed by updating the codec pack.");
Console.WriteLine($"Error resolved at {error.ResolvedAt}");
```

## User

`User` represents a YouTube Shorts Automator account, storing profile information, subscription details, storage usage, and associated videos. It provides methods to validate user data, manage storage quota, and check subscription and upload eligibility.

### Usage Example

```csharp
using YouTubeShortsAutomator.Domain.Models;

// Create a new user
var user = new User
{
    Id = Guid.NewGuid(),
    Email = "creator@example.com",
    DisplayName = "Awesome Creator",
    ChannelId = "UCxxxxxxxxxxxxxxxxxxxx",
    StorageQuotaBytes = 10L * 1024 * 1024 * 1024, // 10 GB
    SubscriptionTier = UserSubscriptionTier.Pro,
    SubscriptionExpiresAt = DateTime.UtcNow.AddDays(30),
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

// Validate user data
var (isValid, errors) = user.Validate();
if (!isValid)
{
    Console.WriteLine("Validation errors: " + string.Join(", ", errors));
    return;
}

// Check storage and add usage
if (!user.IsStorageFull())
{
    user.AddStorageUsage(50 * 1024 * 1024); // Add 50 MB
}

// Update last activity
user.UpdateLastActivity();

// Check if user can upload a new video
if (user.CanUploadNewVideo())
{
    Console.WriteLine("User can upload a new video.");
}
else
{
    Console.WriteLine("User cannot upload a new video due to limits.");
}
```

## ProcessingJob

`ProcessingJob` represents a video processing job in the YouTube Shorts Automator system, tracking the job's status, progress, steps, and errors throughout the processing pipeline.

### Usage Example

```csharp
using YouTubeShortsAutomator.Domain.Models;
using System;

// Create a new processing job for video encoding
var job = new ProcessingJob
{
    Id = Guid.NewGuid(),
    VideoId = Guid.NewGuid(),
    JobType = ProcessingJobType.Encoding,
    OutputPath = "./processed/video.mp4",
    CreatedAt = DateTime.UtcNow
};

// Start the job
job.Start();

// Update progress as processing advances
job.ProgressPercentage = 45f;

// Complete the job successfully
job.Complete();

// Check final job status
Console.WriteLine($"Job {job.Id} finished with status: {job.Status}");
```

## ConfigurationService

`ConfigurationService` manages application configuration and settings, providing typed access to configuration values, YouTube API settings, connection strings, and feature flags. It includes caching for performance and validation methods to ensure required settings are present.

### Usage Example

```csharp
using YouTubeShortsAutomator.Application.Services;
using Microsoft.Extensions.DependencyInjection;

// Example usage with dependency injection
public class ConfigurationExample
{
    private readonly ConfigurationService _configService;
    
    public ConfigurationExample(IServiceProvider services)
    {
        _configService = services.GetRequiredService<ConfigurationService>();
    }
    
    public void Run()
    {
        // Get YouTube API configuration
        var youtubeConfig = _configService.GetYouTubeApiConfig();
        
        // Get individual settings with defaults
        var maxFileSize = _configService.GetMaxFileSize();
        var processingTimeout = _configService.GetProcessingTimeout();
        var defaultTimeZone = _configService.GetDefaultTimeZone();
        
        // Check feature flags
        bool isUploadEnabled = _configService.IsFeatureEnabled("VideoUpload");
        var enabledFeatures = _configService.GetEnabledFeatures();
        
        // Validate configuration
        var (isValid, errors) = _configService.ValidateConfiguration();
        if (!isValid)
        {
            Console.WriteLine("Configuration errors: " + string.Join(", ", errors));
        }
        
        // Clear cache when needed (e.g., after configuration changes)
        _configService.ClearCache();
    }
}
```

## Video

`Video` represents a YouTube video in the automation system, containing metadata, processing status, and relationships to users, analytics, and upload results. It provides validation methods and state transition helpers for the video lifecycle.

### Usage Example

```csharp
using YouTubeShortsAutomator.Domain.Models;
using System;

// Create a new video
var video = new Video
{
    Id = Guid.NewGuid(),
    Title = "My Awesome Short",
    Description = "This is an amazing YouTube short about programming",
    FilePath = "./videos/my-short.mp4",
    Tags = new[] { "shorts", "programming", "tutorial" },
    ThumbnailPath = "./thumbnails/my-short.jpg",
    FileSizeBytes = 15_728_640, // 15MB
    DurationSeconds = 58,
    UserId = Guid.NewGuid(),
    CreatedAt = DateTime.UtcNow
};

// Validate the video before processing
var (isValid, errors) = video.Validate();
if (!isValid)
{
    Console.WriteLine("Validation errors: " + string.Join(", ", errors));
    return;
}

// Process the video
video.MarkAsProcessed();

// Upload the video
video.MarkAsUploaded("dQw4w9WgXcQ");

// Check final status
Console.WriteLine($"Video status: {video.Status}");
Console.WriteLine($"YouTube ID: {video.YouTubeVideoId}");
```

## ErrorHandlingMiddleware

`ErrorHandlingMiddleware` is an ASP.NET Core middleware component that catches all unhandled exceptions in the application pipeline and returns consistent JSON error responses to clients. It logs the full exception details while providing sanitized error messages with appropriate HTTP status codes based on exception type.

### Usage Example

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using YouTubeShortsAutomator.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add middleware to the pipeline (typically in Program.cs)
var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Logger.LogError("Error handling middleware configured");

// The middleware automatically handles exceptions and returns responses like:
// {
//   "message": "Required parameter is missing",
//   "errorCode": "INVALID_PARAMETER",
//   "details": "parameterName",
//   "timestamp": "2026-08-27T10:30:00Z"
// }
```

## CliArgumentParser

`CliArgumentParser` parses command-line arguments for CLI operations, supporting flags, options, and positional arguments with validation. Commands are registered with a description, required and optional options, and an async handler, then `TryParseArguments` routes the raw arguments to the matching command and produces a `ParsedCliArguments` result with typed accessors for reading option values.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeShortsAutomator.CLI;

public class CliExample
{
    public static async Task<int> RunAsync(string[] args)
    {
        var parser = new CliArgumentParser();

        // Register a command with its description, options, and handler
        parser.RegisterCommand("process", new CliCommand
        {
            Description = "Process a video file",
            RequiredOptions = new List<string> { "input" },
            OptionalOptions = new List<string> { "output", "fast" },
            Handler = async parsed =>
            {
                string input = parsed.GetOption("input");
                string output = parsed.GetOption("output", "./output.mp4");
                bool fast = parsed.GetBoolOption("fast");

                Console.WriteLine($"Processing {input} -> {output} (fast: {fast})");
                await Task.Delay(100);
                return 0;
            }
        });

        // Parse the raw command-line arguments
        if (!parser.TryParseArguments(args, out ParsedCliArguments result))
        {
            return 1;
        }

        Console.WriteLine($"Command: {result.Command}");
        foreach (var positional in result.PositionalArguments)
        {
            Console.WriteLine($"Positional: {positional}");
        }

        if (result.TryGetOption("input", out string input))
        {
            Console.WriteLine($"Input option: {input}");
        }

        return result.Handler is null ? 0 : await result.Handler(result);
    }
}
```

## IMetricsCollector

`IMetricsCollector` is the contract for collecting application metrics and performance data, tracking processing durations, upload outcomes, error rates, and outbound API calls. The concrete `MetricsCollector` implementation aggregates these records in memory and exposes them through a `MetricsSnapshot` (with `CapturedAtUtc`, `ProcessingMetrics`, `ErrorCounts`, `ApiCallMetrics`, and `TotalApiCalls`) that can be surfaced by a health or monitoring endpoint. It is thread-safe and intended to be registered as a singleton and injected wherever telemetry is recorded.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YouTubeShortsAutomator.Metrics;

public class MetricsExample
{
    private readonly IMetricsCollector _metrics;

    // In production the collector is resolved via dependency injection.
    public MetricsExample(ILogger<MetricsCollector> logger)
    {
        _metrics = new MetricsCollector(logger);
    }

    public async Task RunAsync()
    {
        // Record how long a processing step took
        _metrics.RecordProcessingDuration("encoding", TimeSpan.FromSeconds(12.5));

        // Record a successful upload, including the file size
        _metrics.RecordUploadSuccess(fileSizeBytes: 15_728_640, TimeSpan.FromSeconds(3.2));

        // Record an upload failure with its error code
        _metrics.RecordUploadFailure("UPLOAD_TIMEOUT");

        // Record an outbound API call with its endpoint and status code
        _metrics.RecordApiCall("/api/videos", statusCode: 200, TimeSpan.FromMilliseconds(240));

        // Retrieve a snapshot of all recorded metrics
        MetricsSnapshot snapshot = await _metrics.GetMetricsAsync();

        Console.WriteLine($"Captured at: {snapshot.CapturedAtUtc}");
        Console.WriteLine($"Total API calls: {snapshot.TotalApiCalls}");

        foreach (ProcessingMetric metric in snapshot.ProcessingMetrics)
        {
            Console.WriteLine($"{metric.ProcessType}: count={metric.Count}, " +
                              $"avg={metric.AverageDurationMs:F1}ms, total={metric.TotalDurationMs:F1}ms");
        }

        foreach (var error in snapshot.ErrorCounts)
        {
            Console.WriteLine($"Error {error.Key}: {error.Value} occurrence(s)");
        }
    }
}
```

## ConversionUtility

`ConversionUtility` provides static methods for safe type conversion with fallback defaults, handling nullable types and culture-aware parsing for common data types including numbers, booleans, dates, GUIDs, enums, and JSON serialization.

### Usage Example

```csharp
// Convert string to int with default value
int count = ConversionUtility.ToInt("42"); // returns 42
int safeCount = ConversionUtility.ToInt("invalid", -1); // returns -1

// Convert to boolean
bool isEnabled = ConversionUtility.ToBoolean("true"); // returns true
bool isActive = ConversionUtility.ToBoolean("0"); // returns false

// Parse JSON
string json = "{\"name\":\"test\",\"value\":123}";
var data = ConversionUtility.JsonDeserialize<MyData>(json);

// Convert object to dictionary
var user = new { Name = "John", Age = 30 };
var dict = ConversionUtility.ObjectToDictionary(user);
// dict contains {"Name": "John", "Age": 30}
```