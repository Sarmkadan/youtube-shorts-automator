## Getting started

This walkthrough uses the console pipeline in `src/` (namespace `YouTubeShortAutomator`). It converts a source video into a 9:16 Short with `VideoProcessor`, then generates a vertical thumbnail from that Short with `ThumbnailGeneratorService`. No YouTube credentials or database are needed.

### Prerequisites

- **.NET SDK 10.0.100 or later.** `global.json` pins `10.0.100` with `rollForward: latestMinor`. Check with `dotnet --version`.
- **FFmpeg and FFprobe on `PATH`.** FFprobe ships with FFmpeg. Check with `ffmpeg -version` and `ffprobe -version`. The processor encodes with `libx264` and `aac`, so your build must include both (distribution and static builds do).
- **A source video** that FFmpeg can read. The example expects `input/source.mp4` relative to the working directory.

### Project setup

Create a console project next to a checkout of this repository (the path in `ProjectReference` assumes the checkout is in `../youtube-shorts-automator`):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../youtube-shorts-automator/youtube-shorts-automator.csproj" />
  </ItemGroup>
</Project>
```

### Example

Save as `Program.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// 1. Create the processor. The FFmpeg and FFprobe paths default to "ffmpeg" and "ffprobe" (resolved from PATH).
var videoProcessor = new VideoProcessor(
    loggerFactory.CreateLogger<VideoProcessor>(),
    ffmpegPath: "ffmpeg",
    ffprobePath: "ffprobe");

// 2. Convert the source into a 9:16 Short: 1080x1920, H.264/AAC, trimmed to at most 59 seconds.
var sourcePath = Path.GetFullPath("input/source.mp4");
var shortPath = Path.GetFullPath("output/short.mp4");

await videoProcessor.ProcessAsync(sourcePath, shortPath);
Console.WriteLine($"Short written to {shortPath}");

// 3. Generate a 9:16 thumbnail from the first second of the Short.
// An empty configuration means FFmpeg is resolved from PATH; set "Processing:FFmpegPath" to override it.
var configuration = new ConfigurationBuilder().Build();

var thumbnails = new ThumbnailGeneratorService(
    configuration,
    loggerFactory.CreateLogger<ThumbnailGeneratorService>());

var thumbnail = await thumbnails.GenerateFromVideoAsync(
    shortPath,
    new ThumbnailGenerationRequest
    {
        CaptureTimestamp = TimeSpan.FromSeconds(1),
        OutputDirectory = Path.GetFullPath("output/thumbnails"),
        AspectRatio = ThumbnailAspectRatio.Vertical,
        Format = ThumbnailOutputFormat.Jpeg,
        Quality = 85
    });

if (!thumbnail.Success)
    throw new InvalidOperationException($"Thumbnail generation failed: {thumbnail.ErrorMessage}");

Console.WriteLine($"Thumbnail written to {thumbnail.OutputPath} ({thumbnail.Width}x{thumbnail.Height})");
```

Run it from the project directory:

```bash
dotnet run
```

### Expected output

| Path | Contents |
|------|----------|
| `output/short.mp4` | 1080x1920 H.264 video with AAC audio. Sources longer than 59 seconds are trimmed; shorter ones keep their length. Any existing file at this path is overwritten. |
| `output/thumbnails/thumb_<32 hex chars>.jpg` | 720x1280 JPEG captured at 1 second into the Short. The file name is a new GUID on each run, so read the path from `thumbnail.OutputPath`. |

The logger writes progress to the console. On failure, `ProcessAsync` throws `FileNotFoundException` when the source is missing, or `VideoProcessingException` when the input is not a readable video or FFmpeg fails. `GenerateFromVideoAsync` throws `VideoProcessingException` when the video file is missing, but returns `Success = false` with `ErrorMessage` set when FFmpeg fails to extract the frame.

## Uploading and scheduling Shorts (web API)

This section uses the web API in the repository root (namespace `YouTubeShortsAutomator`), not the `src/` console pipeline above. It walks through authentication, an immediate upload with `VideoUploader`, a queued upload with `ScheduleManager`, and reading stats back with `AnalyticsTracker`. The example uses SQLite, so it needs no database server.

> **Current limits.** The YouTube calls are simulated. `GoogleApiClient` returns fixed analytics (1500 views, 120 likes, 45 comments, 30 shares), and `YouTubeUploadService` invents a video ID and does not send title, description or tags to YouTube. The app has no OAuth client-secrets file loader and no token cache on disk. Credentials are stored as `ApiCredential` rows in the database.

### Prerequisites

- **.NET SDK 10.0.100 or later.**
- **Two 9:16 Shorts under 60 seconds** at `output/short.mp4` and `output/short-2.mp4`, for example from the pipeline in [Getting started](#getting-started). Copy one file to create the second.
- **A Google Cloud OAuth client** (Desktop app) with the YouTube Data API v3 enabled. Get tokens with the Google OAuth 2.0 Playground or your own consent flow. This repository does not include a consent flow.

Create a console project next to a checkout of this repository, then add the SQLite provider:

```bash
dotnet new console -n ShortsExample
cd ShortsExample
```

Replace the generated `ShortsExample.csproj` with the following. It references the root project and adds the SQLite provider:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.9" />
    <ProjectReference Include="../youtube-shorts-automator/YouTubeShortsAutomator.csproj" />
  </ItemGroup>
</Project>
```

### Authentication

The example reads its configuration from environment variables. Nothing is hard-coded.

| Variable | Used for |
|----------|----------|
| `YOUTUBE_CLIENT_ID`, `YOUTUBE_CLIENT_SECRET` | OAuth client from Google Cloud Console |
| `YOUTUBE_ACCESS_TOKEN`, `YOUTUBE_REFRESH_TOKEN` | Tokens from your consent flow |
| `YOUTUBE_CHANNEL_ID` | Channel ID stored on the `User` row |
| `YouTube__ApiKey` | Required by the `GoogleApiClient` constructor. Any non-empty value works while the client is simulated. |

On first run, the example saves a `User` and a `GoogleOAuth` `ApiCredential` in the `ShortsExample` SQLite file (`shorts.db`). Later runs reuse them.

```bash
export YOUTUBE_CHANNEL_ID=UCxxxxxxxxxxxxxxxxxxxxxx
export YOUTUBE_CLIENT_ID=...apps.googleusercontent.com
export YOUTUBE_CLIENT_SECRET=...
export YOUTUBE_ACCESS_TOKEN=...
export YOUTUBE_REFRESH_TOKEN=...
export YouTube__ApiKey=unused-while-simulated
```

### Example

Replace `Program.cs` with the following:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;
using YouTubeShortsAutomator.Infrastructure.Repositories;
using YouTubeShortsAutomator.Integration;

static string Required(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set the {name} environment variable.");

// Environment variables such as YouTube__ApiKey map to the YouTube:ApiKey configuration key.
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddSingleton<IConfiguration>(configuration);
services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=shorts.db"));

// Repository and service registrations from AddInfrastructureServices, with SQLite in place of SQL Server.
YouTubeShortsAutomator.Infrastructure.Extensions.ServiceCollectionExtensions.AddRepositories(services);
YouTubeShortsAutomator.Infrastructure.Extensions.ServiceCollectionExtensions.AddApplicationServices(services);
YouTubeShortsAutomator.Extensions.ServiceCollectionExtensions.AddApplicationServices(services);

// These three are not registered by the app, so the example registers them.
services.AddScoped<IGoogleApiClient, GoogleApiClient>();
services.AddScoped<VideoUploader>();
services.AddScoped<IVideoUploader>(sp => sp.GetRequiredService<VideoUploader>());
services.AddScoped<ScheduleManager>();
services.AddScoped<AnalyticsTracker>();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var sp = scope.ServiceProvider;

await sp.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();

// 1. Authenticate: store the user and their YouTube OAuth credential in the database.
var db = sp.GetRequiredService<ApplicationDbContext>();
var email = "creator@example.com";
var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
if (user is null)
{
    user = new User
    {
        Id = Guid.NewGuid(),
        Email = email,
        DisplayName = "Shorts Creator",
        ChannelId = Required("YOUTUBE_CHANNEL_ID"),
        CreatedAt = DateTime.UtcNow,
        SubscriptionExpiresAt = DateTime.UtcNow.AddYears(1),
        StorageQuotaBytes = 10L * 1024 * 1024 * 1024
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
}

var credentials = sp.GetRequiredService<IApiCredentialService>();
if (await credentials.GetActiveCredentialAsync(user.Id) is null)
{
    await credentials.CreateCredentialAsync(user.Id, new ApiCredential
    {
        Id = Guid.NewGuid(),
        CredentialType = ApiCredentialType.GoogleOAuth,
        ClientId = Required("YOUTUBE_CLIENT_ID"),
        ClientSecret = Required("YOUTUBE_CLIENT_SECRET"),
        AccessToken = Required("YOUTUBE_ACCESS_TOKEN"),
        RefreshToken = Required("YOUTUBE_REFRESH_TOKEN"),
        // Set this to the real expiry of the access token you pasted in.
        AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
        Scope = "https://www.googleapis.com/auth/youtube.upload"
    });
}

// Adds a processed Short to the library. The file must already exist at filePath.
async Task<Video> AddShortAsync(string filePath, string title)
{
    var video = new Video
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        Title = title,
        Description = "A 30-second look at my morning routine.\n\n#shorts #routine",
        Tags = new[] { "shorts", "routine", "vlog" },
        FilePath = filePath,
        FileSizeBytes = new FileInfo(filePath).Length,
        DurationSeconds = 59,
        CreatedAt = DateTime.UtcNow
    };
    video.MarkAsProcessed();
    await sp.GetRequiredService<IVideoRepository>().AddAsync(video);
    return video;
}

// 2. Upload a Short now. VideoUploader is the upload entry point; it does not check file size, so validate first.
var uploader = sp.GetRequiredService<VideoUploader>();
var immediate = await AddShortAsync(Path.GetFullPath("output/short.mp4"), "Morning routine in 30 seconds");

var problems = await uploader.ValidateForUploadAsync(immediate.Id);
if (problems.Count > 0)
    throw new InvalidOperationException(string.Join("; ", problems));

var upload = await uploader.UploadWithRetryAsync(immediate.Id, user.Id);
Console.WriteLine($"Uploaded: {upload.YouTubeUrl}");

// 3. Schedule a second Short for a later publish time. ScheduleManager uploads it once that time has passed.
var schedule = new UploadSchedule
{
    Id = Guid.NewGuid(),
    UserId = user.Id,
    ScheduleName = "Shorts daily",
    Frequency = ScheduleFrequency.Daily,
    ScheduledTime = new TimeOnly(9, 0),
    TimeZoneId = "UTC",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};
schedule.NextScheduledTime = schedule.CalculateNextScheduledTime(DateTime.UtcNow);
await sp.GetRequiredService<IScheduleRepository>().AddAsync(schedule);

var scheduled = await AddShortAsync(Path.GetFullPath("output/short-2.mp4"), "Evening routine in 30 seconds");

var scheduleManager = sp.GetRequiredService<ScheduleManager>();
var queued = await scheduleManager.EnqueueUploadAsync(schedule.Id, scheduled.Id, DateTime.UtcNow.AddHours(2));
Console.WriteLine($"Queued {queued.VideoId} for {queued.ScheduledFor:O}");

// Call this from a timer or worker. It returns 0 until the queued time has passed.
var dispatched = await scheduleManager.ProcessDueSchedulesAsync();
Console.WriteLine($"Uploads dispatched now: {dispatched}");

// 4. Read back basic stats for the uploaded Short.
var analytics = sp.GetRequiredService<AnalyticsTracker>();
var metric = await analytics.SyncVideoAnalyticsAsync(immediate.Id);
if (metric is null)
{
    Console.WriteLine("No analytics available yet.");
}
else
{
    Console.WriteLine($"Views: {metric.ViewCount}, likes: {metric.LikeCount}, " +
                      $"comments: {metric.CommentCount}, shares: {metric.ShareCount}");
}
Console.WriteLine($"Total views across all Shorts: {await analytics.GetTotalViewsAsync()}");
```

Run it from the project directory:

```bash
dotnet run
```

The flow is:

1. **Authenticate.** Store the user and their OAuth credential.
2. **Upload now.** `VideoUploader.ValidateForUploadAsync` runs first, then `UploadWithRetryAsync` uploads. `VideoUploader` is the upload entry point. There is no `UploadManager` class in this repository.
3. **Schedule.** `ScheduleManager.EnqueueUploadAsync` queues a second Short for two hours from now. `ProcessDueSchedulesAsync` returns `0` until that time, then uploads through `VideoUploader`.
4. **Read stats.** `AnalyticsTracker.SyncVideoAnalyticsAsync` fetches the metrics for the uploaded Short and stores a snapshot. `GetTotalViewsAsync` sums the latest snapshot per video.

The app's DI setup registers the repositories and `IApiCredentialService`, but not `ScheduleManager`, `AnalyticsTracker`, `VideoUploader`, or `IGoogleApiClient`. The example registers those four itself.

### Common errors

| Error | Where it comes from | What to do |
|-------|---------------------|------------|
| `UploadException: Upload failed: The database operation was expected to affect 1 row(s)...` (inner `DbUpdateConcurrencyException`) | `YouTubeUploadService.UploadVideoAsync` → `VideoRepository.UpdateAsync` | **Known issue.** `Repository.UpdateAsync` calls `DbSet.Update`, which also marks the new `UploadResult` as modified, so the UPDATE matches no rows. The upload fails on every run. The same EF Core behaviour applies to SQL Server, so this is not specific to SQLite. The fix belongs in the code, not in this example. |
| Quota exceeded | YouTube Data API v3 returns a `quotaExceeded` error | `QuotaExceededException` exists in `Domain/Exceptions`, but no current code path throws it. A real quota error currently reaches you wrapped in `UploadException` ("Upload failed: ..."). Check usage under Google Cloud Console → APIs & Services → YouTube Data API v3 → Quotas. The quota resets at midnight Pacific Time. |
| Invalid credentials | `CredentialException` ("Invalid credential: Access token is required", "Credentials are invalid or expired", "No valid YouTube credentials found") or `OAuthTokenExpiredException` ("OAuth refresh token has expired; re-authorization required.") | Get new tokens and update the environment variables. Mark or delete the stale `ApiCredential` row, because the example creates a new one only when `GetActiveCredentialAsync` returns `null`. `VideoUploader` retries these exceptions, so expect a few seconds of delay before the error surfaces. |
| File too large | `ValidateForUploadAsync` returns `File size exceeds 4GB limit`. `Video.Validate()` returns `File size exceeds maximum limit of 4GB`. | `UploadWithRetryAsync` does not check file size, so call `ValidateForUploadAsync` first, as the example does. Re-export the Short at a lower bitrate or re-run the pipeline. A 60-second Short should never reach 4 GB, so a file this size is usually the wrong file. |

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

## LoggingUtility

`LoggingUtility` provides static methods for structured logging of operations, validation errors, API calls, and database operations. It also includes an `OperationLogger` class for tracking operation execution time and automatically logging success or failure.

### Usage Example

```csharp
using Microsoft.Extensions.Logging;
using YouTubeShortsAutomator.Utilities;

public class LoggingExample
{
    private readonly ILogger _logger;

    public LoggingExample(ILogger<LoggingExample> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        // Log the start of an operation
        LoggingUtility.LogOperationStart(_logger, "ProcessVideo", new { VideoId = 123 });

        // Using OperationLogger to automatically log success or failure
        using var operationLogger = LoggingUtility.CreateOperationLogger(_logger, "ProcessVideo");
        try
        {
            // ... operation logic ...
            var result = new { Status = "Processed" };
            operationLogger.LogSuccess(result);
        }
        catch (Exception ex)
        {
            operationLogger.LogFailure(ex);
        }

        // Log a validation error
        LoggingUtility.LogValidationError(_logger, "Title", "Title cannot be empty");

        // Log an API call
        LoggingUtility.LogApiCall(_logger, "GET", "/api/videos/123", 200, 150);

        // Log a database operation
        LoggingUtility.LogDatabaseOperation(_logger, "INSERT", "Videos", 1, 45);
    }
}
```

## CollectionUtility

`CollectionUtility` provides static methods for safe collection manipulation and transformation, offering null-safe operations on IEnumerable types with helpful batch processing capabilities.

### Usage Example

```csharp
using YouTubeShortsAutomator.Utilities;
using System.Collections.Generic;
using System.Linq;

public class CollectionUtilityExample
{
    public void Run()
    {
        // Sample data - some might be null
        List<string>? tags = null;
        List<string> videoTitles = new List<string> 
        { 
            "Hello World", 
            "Dotnet Tutorial", 
            "Hello World", // duplicate
            "Advanced C#",
            "Dotnet Tutorial" // duplicate
        };
        List<int> viewCounts = new List<int> { 100, 250, 100, 500, 250 };

        // Safe conversion (handles null)
        List<string> safeTags = CollectionUtility.ToSafeList(tags); // Returns empty list
        string[] safeTitlesArray = CollectionUtility.ToSafeArray(videoTitles);

        // Check if collections have elements
        bool tagsHasElements = CollectionUtility.HasElements(tags); // false
        bool titlesHasElements = CollectionUtility.HasElements(videoTitles); // true

        // Safe transformation with null handling
        List<int> titleLengths = CollectionUtility.SelectSafe(
            videoTitles, 
            title => title.Length
        ); // Returns [11, 14, 11, 13, 14]

        // Safe filtering
        List<string> longTitles = CollectionUtility.WhereSafe(
            videoTitles, 
            title => title.Length > 12
        ); // Returns ["Dotnet Tutorial", "Advanced C#", "Dotnet Tutorial"]

        // Remove duplicates while preserving order
        IEnumerable<string> distinctTitles = CollectionUtility.DistinctBy(
            videoTitles, 
            title => title.ToLower()
        ); // Returns ["Hello World", "Dotnet Tutorial", "Advanced C#"]

        // Group items by first letter
        Dictionary<char, List<string>> titlesByInitial = CollectionUtility.GroupByToDictionary(
            videoTitles.Where(t => !string.IsNullOrEmpty(t)),
            title => char.ToUpper(title[0])
        );
        // Returns { H: ["Hello World", "Hello World"], D: ["Dotnet Tutorial", "Dotnet Tutorial"], A: ["Advanced C#"] }

        // Split into chunks for batch processing
        List<List<string>> titleChunks = CollectionUtility.ChunkBy(videoTitles, 2);
        // Returns [["Hello World", "Dotnet Tutorial"], ["Hello World", "Advanced C#"], ["Dotnet Tutorial"]]

        // Get first/last with defaults
        string? firstTitle = CollectionUtility.FirstOrDefault(videoTitles, "No titles");
        string? lastTitle = CollectionUtility.LastOrDefault(videoTitles, "No titles");
        string? firstEmpty = CollectionUtility.FirstOrDefault(tags, "No tags"); // Returns "No tags"

        // Check if collection contains specific values
        bool hasDotnet = CollectionUtility.ContainsAny(videoTitles, "Dotnet Tutorial", "Java");
        bool hasBoth = CollectionUtility.ContainsAll(videoTitles, "Hello World", "Dotnet Tutorial");

        // Shuffle for random ordering
        List<string> shuffledTitles = CollectionUtility.Shuffle(videoTitles);

        // Take random sample
        IEnumerable<string> randomSample = CollectionUtility.TakeRandom(videoTitles, 2);

        // Calculate statistics
        int totalViews = CollectionUtility.Sum(viewCounts); // 1200
        decimal averageViews = CollectionUtility.Average(
            viewCounts.Select(v => (decimal)v).ToList()
        ); // 240
    }
}
```

## StringUtility

`StringUtility` provides static methods for string manipulation and formatting, including truncation, case conversion, validation, and text transformation operations.

### Usage Example

```csharp
using YouTubeShortsAutomator.Utilities;

public class StringUtilityExample
{
    public void Run()
    {
        string text = "  Hello World! This is a TEST string  ";
        
        // Truncate text to 10 characters
        string truncated = StringUtility.Truncate(text, 10); // "  Hello W..."
        
        // Convert to different cases
        string camelCase = StringUtility.ToCamelCase(text); // "helloWorldThisIsATestString"
        string pascalCase = StringUtility.ToPascalCase(text); // "HelloWorldThisIsATestString"
        string snakeCase = StringUtility.ToSnakeCase(text); // "hello_world_this_is_a_test_string"
        
        // Create URL-friendly slug
        string slug = StringUtility.ToSlug(text); // "hello-world-this-is-a-test-string"
        
        // Validate string content
        bool isNumeric = StringUtility.IsNumeric("123"); // true
        bool isAlphabetic = StringUtility.IsAlphabetic("abc"); // true
        bool isAlphanumeric = StringUtility.IsAlphanumeric("abc123"); // true
        
        // Clean up whitespace and special characters
        string noWhitespace = StringUtility.RemoveWhitespace(text); // "HelloWorld!Thisis aTESTstring"
        string normalized = StringUtility.NormalizeWhitespace(text); // "Hello World! This is a TEST string"
        string clean = StringUtility.RemoveSpecialCharacters(text); // "  Hello World This is a TEST string  "
        
        // Other utility methods
        int count = StringUtility.CountOccurrences("hello world hello", "hello"); // 2
        string[] chunks = StringUtility.SplitByLength(text, 5); // ["  Hel", "lo Wo", "rld! ", "Thi", "s i", "s a ", "TEST", " str", "ing  "]
        string reversed = StringUtility.ReverseString(text); // "  tset  tset a si !dlroW olleH  "
        bool containsAny = StringUtility.ContainsAny(text, "world", "test"); // true
        bool containsAll = StringUtility.ContainsAll(text, "hello", "world"); // false
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

## HttpClientUtility

`HttpClientUtility` provides static helpers for creating and using HttpClient with common patterns: configuring timeouts, adding auth headers, making JSON requests, and interpreting status codes.

### Usage Example

```csharp
// Create a client with a 20-second timeout and custom User-Agent
var client = HttpClientUtility.CreateConfiguredClient(
    timeoutSeconds: 20,
    userAgent: "YouTubeShortsAutomator/1.0");

// Add an API key header
HttpClientUtility.AddApiKeyHeader(client, "your-api-key-here");

try
{
    // GET and deserialize JSON response (throws on failure)
    var data = await HttpClientUtility.GetAsJsonAsync<Dictionary<string, object>>(client, "https://api.example.com/data");
    Console.WriteLine($"Received keys: {string.Join(", ", data.Keys)}");
}
catch (YoutubeShortsAutomatorException ex)
{
    Console.Error.WriteLine($"Request failed: {ex.Message}");
}

// POST JSON content and check status code manually
var newPost = new { title = "My Short", description = "A quick update" };
var postResponse = await HttpClientUtility.PostAsJsonAsync(client, "https://api.example.com/posts", newPost);
if (HttpClientUtility.IsSuccessStatusCode((int)postResponse.StatusCode))
{
    Console.WriteLine("Post created successfully");
}
else
{
    Console.Error.WriteLine($"Post failed: {HttpClientUtility.GetStatusCodeDescription((int)postResponse.StatusCode)}");
}
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for the full text.

## IRepository

`IRepository<T>` is the generic repository contract for the console pipeline's data layer (`src/Data/IRepository.cs`, namespace `YouTubeShortAutomator.Data`). It constrains `T` to reference types and provides the basic CRUD operations, existence and count checks, and an explicit `SaveChangesAsync` for persisting pending changes. Concrete repositories such as `UploadJobRepository` implement it, and callers should depend on the interface rather than the implementation.

### Usage Example

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;

public class UploadJobService
{
    private readonly IRepository<UploadJob> _repository;

    public UploadJobService(IRepository<UploadJob> repository)
    {
        _repository = repository;
    }

    public async Task<UploadJob?> GetJobAsync(int id, CancellationToken cancellationToken = default)
    {
        // Returns null when no entity with the given id exists
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<UploadJob> CreateJobAsync(UploadJob job, CancellationToken cancellationToken = default)
    {
        // Stage the new entity, then persist it explicitly
        UploadJob added = await _repository.AddAsync(job, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return added;
    }

    public async Task<bool> RemoveJobAsync(int id, CancellationToken cancellationToken = default)
    {
        // Returns false when the entity was not found
        bool deleted = await _repository.DeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return deleted;
    }

    public async Task<int> GetTotalJobsAsync(CancellationToken cancellationToken = default)
    {
        bool exists = await _repository.ExistsAsync(1, cancellationToken);
        Console.WriteLine($"Job 1 exists: {exists}");

        return await _repository.CountAsync(cancellationToken);
    }
}
```

## Constants

`Constants` is a static class of compile-time constants shared by the console pipeline (`src/Constants/Constants.cs`, namespace `YouTubeShortAutomator.Constants`). It holds video constraints, YouTube metadata limits, processing defaults, FFmpeg encoding defaults, directory names, and timeouts. Values are `const`, so they can be used in attributes and switch labels, and they should be changed in one place rather than hardcoded elsewhere.

### Constant reference

| Group | Constant | Value | Description |
|-------|----------|-------|-------------|
| Video constraints | `MAX_DURATION_SECONDS` | `60` | Maximum Short duration |
| | `MIN_DURATION_SECONDS` | `1` | Minimum video duration |
| | `MAX_FILE_SIZE_MB` | `512` | Maximum input file size in megabytes |
| | `MAX_FILE_SIZE_BYTES` | `512 * 1024 * 1024` | Maximum file size in bytes (derived from `MAX_FILE_SIZE_MB`) |
| | `MIN_FILE_SIZE_BYTES` | `1024` | Minimum file size in bytes (1 KB) |
| | `MIN_WIDTH_PIXELS` / `MIN_HEIGHT_PIXELS` | `360` / `640` | Minimum frame dimensions |
| | `MAX_WIDTH_PIXELS` / `MAX_HEIGHT_PIXELS` | `1920` / `1080` | Maximum frame dimensions |
| YouTube limits | `MAX_TITLE_LENGTH` | `100` | Maximum title length in characters |
| | `MAX_DESCRIPTION_LENGTH` | `5000` | Maximum description length in characters |
| | `MAX_TAGS_COUNT` | `500` | Maximum number of tags |
| | `MAX_TAG_LENGTH` | `30` | Maximum length of a single tag |
| Processing | `DEFAULT_RETRY_COUNT` | `3` | Default number of retries for a failed job |
| | `DEFAULT_UPLOAD_TIMEOUT_SECONDS` | `7200` | Upload timeout (2 hours) |
| | `PROCESSING_QUEUE_LIMIT` | `100` | Maximum number of queued processing items |
| | `BATCH_PROCESSING_SIZE` | `5` | Number of items handled per batch |
| | `MAX_CONCURRENT_UPLOADS` | `3` | Maximum parallel uploads |
| | `MAX_CONCURRENT_PROCESSING_TASKS` | `2` | Maximum parallel processing tasks |
| FFmpeg defaults | `DEFAULT_VIDEO_CODEC` | `"libx264"` | Video codec |
| | `DEFAULT_AUDIO_CODEC` | `"aac"` | Audio codec |
| | `DEFAULT_CONTAINER` | `"mp4"` | Container format |
| | `DEFAULT_BITRATE_KBPS` | `2000` | Video bitrate in kbps |
| | `DEFAULT_AUDIO_BITRATE_KBPS` | `128` | Audio bitrate in kbps |
| | `DEFAULT_FPS` | `30` | Frame rate |
| Paths and formats | `LOG_DIRECTORY` | `"logs"` | Log directory name |
| | `PROCESSING_DIRECTORY` | `"processing"` | Working directory for in-progress videos |
| | `OUTPUT_DIRECTORY` | `"output"` | Directory for processed output |
| | `TEMP_DIRECTORY` | `"temp"` | Temporary file directory |
| | `SUPPORTED_INPUT_FORMATS` | `"mp4,avi,mov,mkv,flv,wmv"` | Comma-separated list of accepted input extensions |
| | `OUTPUT_FORMAT` | `"mp4"` | Output file format |
| Timeouts | `API_TIMEOUT_SECONDS` | `30` | External API request timeout |
| | `DATABASE_TIMEOUT_SECONDS` | `30` | Database command timeout |
| | `FFMPEG_TIMEOUT_SECONDS` | `3600` | FFmpeg process timeout (1 hour) |
| Scheduling | `SCHEDULE_CHECK_INTERVAL_SECONDS` | `60` | How often scheduled uploads are checked |
| | `ANALYTICS_SYNC_INTERVAL_HOURS` | `6` | Interval between analytics syncs |
| | `TOKEN_REFRESH_BUFFER_HOURS` | `1` | Refresh OAuth tokens this many hours before expiry |
| Caching | `CACHE_EXPIRATION_MINUTES` | `30` | Default cache entry lifetime |

Some constants, such as `MAX_TITLE_LENGTH`, `MAX_FILE_SIZE_BYTES`, `DEFAULT_RETRY_COUNT`, and `TEMP_DIRECTORY`, are referenced directly by the pipeline. Others are defined for future or shared use, so check the usages before relying on one. The descriptions above are based on each constant's name.

### Usage Example

```csharp
using YouTubeShortAutomator.Constants;

// Reject videos longer than the Shorts limit
if (duration.TotalSeconds > Constants.MAX_DURATION_SECONDS)
{
    throw new InvalidOperationException(
        $"Video must be at most {Constants.MAX_DURATION_SECONDS} seconds long.");
}

// Validate the file size against the byte limit
if (fileInfo.Length > Constants.MAX_FILE_SIZE_BYTES)
{
    Console.Error.WriteLine($"File exceeds {Constants.MAX_FILE_SIZE_MB} MB.");
}

// Split the supported input extensions into an array
string[] formats = Constants.SUPPORTED_INPUT_FORMATS.Split(',');

// Build the output path from the configured directory and format
string outputPath = Path.Combine(Constants.OUTPUT_DIRECTORY, $"short.{Constants.OUTPUT_FORMAT}");

// Use a timeout constant instead of a magic number
var timeout = TimeSpan.FromSeconds(Constants.FFMPEG_TIMEOUT_SECONDS);
```

## ThumbnailAbTestService

`ThumbnailAbTestService` runs A/B split tests on thumbnails for a `VideoShort` (`src/Services/ThumbnailAbTestService.cs`, namespace `YouTubeShortAutomator.Services`). It registers two thumbnail variants labelled `A` and `B`, records impressions and click-throughs for each variant, and declares the variant with the highest view rate as the winner once both have at least `MinimumImpressionsForConclusion` (500) impressions. The losing variant is deactivated. The service is registered as scoped in `src/Configuration/DependencyInjection.cs`, so resolve it from the container rather than constructing it by hand.

### Public members

| Member | Returns | Description |
|--------|---------|-------------|
| `MinimumImpressionsForConclusion` | `long` | Impressions each variant needs before a winner can be declared (`500`) |
| `CreateTestAsync(videoShortId, thumbnailPathA, thumbnailPathB, ct)` | `(ThumbnailVariant VariantA, ThumbnailVariant VariantB)` | Creates both variants as active. Throws `ValidationException` for an empty path, `InvalidOperationException` if the video does not exist or an active test already exists |
| `RecordViewEventAsync(variantId, clicked = false, ct)` | `Task` | Records one impression, plus one click when `clicked` is `true`. Throws `InvalidOperationException` if the variant does not exist or is inactive |
| `SyncAnalyticsAsync(videoShortId, analyticsData, ct)` | `Task` | Overwrites impression and click totals for each active variant whose label appears in `analyticsData`. Throws `ArgumentNullException` for a `null` dictionary. Does nothing if no active variants exist |
| `EvaluateAndConcludeAsync(videoShortId, ct)` | `Task<ThumbnailVariant?>` | Returns the winner, or `null` if there are no active variants or any variant is below the impression minimum. Otherwise it marks the highest view-rate variant as the winner and deactivates the rest |
| `GetTestResultAsync(videoShortId, ct)` | `Task<ThumbnailAbTestResult>` | Returns a snapshot of all variants for the video, including concluded ones, with the winner label and completion flag |

`ThumbnailAbTestResult` is the snapshot type returned by `GetTestResultAsync`. Its `GetSummary()` method returns a human-readable report with per-variant impressions, clicks, view rate, and the view-rate lead of the top variant over the runner-up.

### Usage Example

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

public class ThumbnailTestRunner
{
    private readonly ThumbnailAbTestService _abTestService;

    public ThumbnailTestRunner(ThumbnailAbTestService abTestService)
    {
        _abTestService = abTestService;
    }

    public async Task StartTestAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        // Register two thumbnail candidates; both variants start active
        var (variantA, variantB) = await _abTestService.CreateTestAsync(
            videoShortId,
            "thumbnails/short-42-a.jpg",
            "thumbnails/short-42-b.jpg",
            cancellationToken);
    }

    public async Task RecordViewAsync(int variantId, bool clicked, CancellationToken cancellationToken = default)
    {
        // Count one impression, and a click if the viewer clicked through
        await _abTestService.RecordViewEventAsync(variantId, clicked, cancellationToken);
    }

    public async Task ImportAnalyticsAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        // Keys are variant labels; values are (Impressions, Clicks) totals from the analytics provider
        var analytics = new Dictionary<string, (long Impressions, long Clicks)>
        {
            ["A"] = (1_250, 48),
            ["B"] = (1_310, 71)
        };

        await _abTestService.SyncAnalyticsAsync(videoShortId, analytics, cancellationToken);
    }

    public async Task<ThumbnailVariant?> TryConcludeAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        // Returns null until both variants reach MinimumImpressionsForConclusion
        ThumbnailVariant? winner = await _abTestService.EvaluateAndConcludeAsync(videoShortId, cancellationToken);
        return winner;
    }

    public async Task<string> ReportAsync(int videoShortId, CancellationToken cancellationToken = default)
    {
        ThumbnailAbTestResult result = await _abTestService.GetTestResultAsync(videoShortId, cancellationToken);

        // Inspect the outcome programmatically or print the formatted summary
        if (result.IsComplete)
        {
            System.Console.WriteLine($"Winner: variant {result.WinnerLabel}");
        }

        return result.GetSummary();
    }
}
```

## TitleOptimizationEngine

`TitleOptimizationEngine` produces title and description suggestions for a channel by comparing the current metadata against its historical performance (`src/Services/TitleOptimizationEngine.cs`, namespace `YouTubeShortAutomator.Services`). It implements `ITitleOptimizationEngine` and is registered as scoped in `src/Configuration/DependencyInjection.cs`, so resolve it from the container rather than constructing it by hand. The scoring and suggestion logic is deterministic: the only external call is `AnalyticsService.GetTopPerformingVideosAsync`, which supplies the sample of top-performing videos (`HistoricalSampleSize`, default 25).

### Method reference

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `TitleOptimizationEngine(AnalyticsService, ContentCalendarOptions, ILogger<TitleOptimizationEngine>)` | Creates the engine. Throws `ArgumentNullException` if any argument is `null`. |
| `OptimizeAsync` | `Task<TitleOptimizationResult> OptimizeAsync(string title, string description, string[] tags, int channelId, CancellationToken cancellationToken = default)` | Fetches the channel's top-performing videos, builds up to `OptimizationSuggestionCount` (default 3) suggestions ordered by `ConfidenceScore`, and returns them with the optimal posting hour and recommended hashtags. Throws `ArgumentNullException` when `title` is `null` and `ArgumentException` when it is empty or whitespace. |
| `RecommendPostingTimesAsync` | `Task<IEnumerable<DateTime>> RecommendPostingTimesAsync(int channelId, int count = 5, CancellationToken cancellationToken = default)` | Returns up to `count` UTC upload slots, starting tomorrow at the optimal hour. Sundays are skipped, and consecutive slots are at least `MinSlotGapMinutes` (default 240) apart. The search covers at most `count * 4` days, so fewer slots than requested can be returned. Throws `ArgumentOutOfRangeException` when `count` is zero or negative. |
| `ScoreTitle` | `double ScoreTitle(string title)` | Returns a heuristic quality score clamped to the range 0 to 1. Returns `0` for a `null` or whitespace title. See the scoring table below. |
| `ExtractKeywords` | `string[] ExtractKeywords(string title, string description)` | Returns up to 10 lowercase keywords from the combined title and description, ordered by frequency. Only words of at least 4 letters that are not stop words are kept. `null` arguments are treated as empty strings. |

`OptimizeAsync` and `RecommendPostingTimesAsync` both pass the sample through the same private pattern builder, so their results depend on the same analytics data.

### ScoreTitle scoring

`ScoreTitle` starts at a base score of `0.4` and adds or subtracts the following components. The final value is clamped to `[0, 1]`.

| Component | Condition | Adjustment |
|-----------|-----------|------------|
| Length | Length is between `OptimalTitleMinLength` (40) and `OptimalTitleMaxLength` (70) | `+0.2` |
| Length | Length is under 20 or over 90 characters | `-0.15` |
| Length | Any other length | `+0.05` |
| Power word | Title contains one of `Ultimate`, `Essential`, `Complete`, `Proven`, `Simple`, `Fast`, `Best`, `Top`, `Easy`, `Quick` (case-insensitive) | `+0.15` |
| Question | Title starts with `How to`, `Why does`, `What is`, `When should`, or `Which is better`, or ends with `?` | `+0.1` |
| Digits | Title contains at least one digit | `+0.1` |
| Engagement keyword | Title contains any of `HighEngagementKeywords` | `+0.05 * KeywordWeightMultiplier` (applied once) |

The length thresholds of 20 and 90 are fixed in the code. Only the optimal range and the keyword settings come from `ContentCalendarOptions`.

### OptimizeAsync output

Each suggestion carries the same description and tags, built from the input:

- **Power-word injection** is added only when the title has no power word. The word is picked from the title length, and it is inserted after `How to` when the title starts with that phrase.
- **Question format** is added only when the title does not already end with `?`. It turns the title into a `How to ...?` question.
- **Keyword alignment** is always added. It appends the first top-performer or extracted keyword that is missing from the title.

`ConfidenceScore` is capped at `1.0`. `SuggestedDescription` is the original description followed by `TrendingHashtags`. `RecommendedHashtags` contains the trending hashtags plus up to five extracted keywords as `#keyword` tags, with a maximum of 10 entries.

The optimal posting hour is currently fixed at `17` UTC. `AnalyticsData` does not include upload timestamps, so the pattern builder cannot derive the hour from the data yet. If `17` is not in `OptimalPostingHoursUtc`, the engine uses the middle entry of that array instead.

### Usage Example

```csharp
using YouTubeShortAutomator.Services;

// Resolve the engine from the container (registered as scoped)
ITitleOptimizationEngine engine = scope.ServiceProvider.GetRequiredService<ITitleOptimizationEngine>();

// Score a candidate title before scheduling
double score = engine.ScoreTitle("How to Edit Shorts Fast in 2 Minutes");
Console.WriteLine($"Title score: {score:F2}");

// Generate suggestions for the channel's current metadata
TitleOptimizationResult result = await engine.OptimizeAsync(
    title: "How to edit Shorts",
    description: "A quick tutorial on editing vertical video.",
    tags: new[] { "shorts", "editing" },
    channelId: 1);

// Use the highest-confidence suggestion, if there is one
OptimizationSuggestion? best = result.BestSuggestion;
if (best is not null)
{
    Console.WriteLine($"{best.SuggestedTitle} ({best.ConfidenceScore:F2}): {best.Rationale}");
}

// Pick five upcoming UTC slots, skipping Sundays
IEnumerable<DateTime> slots = await engine.RecommendPostingTimesAsync(channelId: 1, count: 5);

// Extract seed keywords for a new description
string[] keywords = engine.ExtractKeywords("Edit Shorts faster", "Batch editing tips for creators");
```

## JobOrchestrationService

`JobOrchestrationService` coordinates the full video pipeline for YouTube Shorts: processing and validation, thumbnail generation, analytics record creation, upload scheduling, upload and retry runs, and analytics sync (`src/Services/JobOrchestrationService.cs`, namespace `YouTubeShortAutomator.Services`). It is sealed and has no interface. Resolve it from the service provider, as `src/Program.cs` does, rather than constructing it by hand. The constructor throws `ArgumentNullException` for any `null` dependency.

The public methods below catch their exceptions and report failures through their return values or result objects, so they do not throw for pipeline or upload errors. Each one logs its failures.

### Public members

| Member | Returns | Description |
|--------|---------|-------------|
| `ProcessFullPipelineAsync(videoShortId, processingProfile, channel, scheduledUploadTime, ct)` | `Task<Pipeline>` | Runs the pipeline for one video. Steps: load the video, validate its file, process it, generate a thumbnail at 2 seconds, apply the watermark and colour grading when the profile enables them, mark it processed, create an analytics record, and create an upload job for `scheduledUploadTime`. Returns `Status = "Failed"` with `Error` set if the video is missing, validation fails, processing does not complete, or an exception is thrown. Returns `Status = "Success"` with `UploadJobId`, `ScheduledUploadTime`, and `CompletedAt` set otherwise. `channel` is accepted but not used by the method body |
| `ProcessReadyUploadsAsync(channel, ct)` | `Task<bool>` | Uploads up to `Constants.MAX_CONCURRENT_UPLOADS` (3) scheduled jobs. Jobs whose video no longer exists are skipped. Each job is saved after its attempt. A failure on one job is logged and does not stop the others. Returns `true` if at least one upload completed; `false` if none did or if the initial query failed |
| `ProcessFailedRetriableJobsAsync(channel, ct)` | `Task<bool>` | Retries up to 3 failed upload jobs that are still eligible for retry. A job is saved only when its retry succeeds. Returns `true` when the batch finishes, even if no retry succeeded. Returns `false` only if the initial query failed |
| `SyncAnalyticsAsync(channel, ct)` | `Task<SyncResult>` | Syncs analytics for every video in the channel by calling `AnalyticsService.SyncAnalyticsFromYouTubeAsync`. Counts successes in `SyncedCount` and failures in `FailedCount`. Sets `Error` if the video query fails. Sets `CompletedAt` when the loop finishes. `channel` is dereferenced before the `try` block, so a `null` channel throws `NullReferenceException` |

Two public result types come with the service:

- `Pipeline` holds `VideoShortId`, `UploadJobId`, `Status`, `Error`, `ProcessingCompleted`, `ScheduledUploadTime`, and `CompletedAt`.
- `SyncResult` holds `Channel`, `SyncedCount`, `FailedCount`, `Error`, and `CompletedAt`.

The `JobOrchestrationServiceExtensions` class adds per-video helpers for single-video calls: `ProcessReadyUploadForVideoShortAsync`, `ProcessFailedRetriableJobForVideoShortAsync`, `SyncAnalyticsForVideoShortAsync`, and `GetUploadJobIdForVideoShortAsync`.

### Usage Example

```csharp
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

// Resolve the orchestrator from the container
JobOrchestrationService orchestrator = scope.ServiceProvider.GetRequiredService<JobOrchestrationService>();

// Process one video and schedule its upload
Pipeline pipeline = await orchestrator.ProcessFullPipelineAsync(
    videoShortId: 42,
    processingProfile: profile,
    channel: channel,
    scheduledUploadTime: DateTime.UtcNow.AddHours(4));

if (pipeline.Status == "Success")
{
    Console.WriteLine($"Upload job {pipeline.UploadJobId} scheduled for {pipeline.ScheduledUploadTime:u}");
}
else
{
    Console.WriteLine($"Pipeline failed: {pipeline.Error}");
}

// Upload due jobs, then retry failed ones
bool uploaded = await orchestrator.ProcessReadyUploadsAsync(channel);
bool retried = await orchestrator.ProcessFailedRetriableJobsAsync(channel);

// Refresh analytics for the whole channel
SyncResult sync = await orchestrator.SyncAnalyticsAsync(channel);
Console.WriteLine($"Synced {sync.SyncedCount}, failed {sync.FailedCount}");
```

## ChannelService

`ChannelService` checks and updates the status and credentials of a `YouTubeChannel` (`src/Services/ChannelService.cs`, namespace `YouTubeShortAutomator.Services`). It is sealed and has no interface. It takes an `ILogger<ChannelService>` and throws `ArgumentNullException` if the logger is `null`. It is not registered in the DI container, so construct it directly with a logger. Extension methods for batch operations are in `ChannelServiceExtensions`.

Only `UpdateChannelStatus` and `ValidateChannelCredentials` write to the log. The other methods have no side effects.

### Public members

| Member | Returns | Description |
|--------|---------|-------------|
| `IsChannelTokenValid(channel)` | `bool` | Returns `true` when the access token has not expired and the channel is active. Returns `false` for a `null` channel instead of throwing |
| `NeedsTokenRefresh(channel)` | `bool` | Returns `true` when the channel's token should be refreshed proactively, as decided by `YouTubeChannel.NeedsTokenRefresh()`. Throws `ArgumentNullException` for a `null` channel |
| `UpdateChannelStatus(channel, isActive)` | `void` | Activates or deactivates the channel through `Activate()` or `Deactivate()`, then logs the change at information level. Throws `ArgumentNullException` for a `null` channel |
| `GetChannelStatusSummary(channel)` | `string` | Returns a multi-line report with the channel name, active and verified flags, subscriber, view, and video counts, token state (`Valid` or `Expired`), and the last sync time. Throws `ArgumentNullException` for a `null` channel |
| `ValidateChannelCredentials(channel)` | `bool` | Returns `true` only if `YouTubeChannel.IsValid()` passes and both the access token and the refresh token are non-blank. Logs a warning naming the failed check and returns `false` otherwise. Throws `ArgumentNullException` for a `null` channel |

### Usage Example

```csharp
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Services;

// ChannelService is not registered in DI, so create it with a logger
ChannelService channelService = new ChannelService(loggerFactory.CreateLogger<ChannelService>());

if (!channelService.ValidateChannelCredentials(channel))
{
    channelService.UpdateChannelStatus(channel, isActive: false);
}
else if (channelService.NeedsTokenRefresh(channel))
{
    Console.WriteLine("Token should be refreshed before the next upload.");
}

if (channelService.IsChannelTokenValid(channel))
{
    Console.WriteLine(channelService.GetChannelStatusSummary(channel));
}
```
