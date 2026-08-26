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

## Notes

- **Null handling:** The validation methods treat `null` or missing required fields as validation failures and include appropriate messages in the returned list. `EnsureValid` will consequently throw.
- **Thread safety:** All members are static and stateless; they do not modify shared mutable state. Consequently, they are safe to call concurrently from multiple threads.
- **Extensibility:** If additional validation rules are required (e.g., format checks for API keys), they should be added inside the implementation of these methods so that `Validate`, `IsValid`, and `EnsureValid` remain consistent.
- **Performance:** The methods perform only lightweight checks (e.g., string emptiness, length constraints). They are intended to be called before expensive operations such as database writes or external API calls.
