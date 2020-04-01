# AnalyticsService

`AnalyticsService` aggregates and persists performance metrics for YouTube Shorts videos managed by the automator. It provides asynchronous methods to create local analytics records, synchronize data from the YouTube API, retrieve analytics for individual videos or top performers, generate periodic reports, and compute channel-level growth metrics. The service also exposes a snapshot of aggregate statistics for a configurable time window.

## API

### public AnalyticsService
Constructor. Initializes a new instance of the service, preparing any required API clients or data-access dependencies. No parameters are exposed publicly.

### public async Task\<AnalyticsData\> CreateAnalyticsRecordAsync
Creates and persists a new `AnalyticsData` record locally. The exact input is supplied through internal service configuration or method overloads not shown in the public surface. Returns the newly created record. Throws if the underlying storage operation fails or if required input data is missing.

### public async Task\<AnalyticsData?\> SyncAnalyticsFromYouTubeAsync
Fetches the latest analytics for a specific video from the YouTube API and updates or creates the corresponding local record. The video identifier is determined by the current execution context (e.g., the most recently processed Short). Returns the synchronized `AnalyticsData` record, or `null` if the YouTube API returns no data or the video cannot be found. Throws when the API request fails due to authentication, quota exhaustion, or network errors.

### public async Task\<AnalyticsData?\> GetVideoAnalyticsAsync
Retrieves a stored analytics record for a given video. The video identifier is resolved from the current context. Returns the matching `AnalyticsData` instance, or `null` if no record exists. Throws if the data store is unreachable.

### public async Task\<IEnumerable\<AnalyticsData\>\> GetTopPerformingVideosAsync
Returns a collection of the highest-performing video analytics records within the current reporting period, ranked by a configurable metric (typically views or engagement). The count or threshold is determined by service settings. Returns an empty enumerable if no videos meet the criteria. Throws if the data query fails.

### public async Task\<AnalyticsReport\> GeneratePeriodReportAsync
Compiles a structured `AnalyticsReport` for the time window defined by `PeriodStart` and `PeriodEnd`. The report aggregates all relevant metrics and includes summary statistics. Returns the populated report object. Throws if the period is invalid (e.g., `PeriodEnd` precedes `PeriodStart`) or if data aggregation encounters an error.

### public string AnalyzePerformanceMetrics
Synchronously evaluates the current aggregate metrics and returns a human-readable string summarizing performance (e.g., “Engagement is up 12% week-over-week”). The analysis logic uses the values of `TotalViews`, `AverageEngagementRate`, and related properties at the moment of invocation. Does not throw; returns a default message if insufficient data is available.

### public async Task\<double\> CalculateChannelGrowthAsync
Computes the channel’s growth rate as a percentage over the current period, based on subscriber change and view trends. Returns the growth value as a `double` (e.g., `3.42` for 3.42%). Throws if the necessary historical baseline data cannot be retrieved.

### public DateTime PeriodStart
Gets or sets the inclusive start of the reporting window. All aggregate properties and report generation use this boundary.

### public DateTime PeriodEnd
Gets or sets the inclusive end of the reporting window. Must be equal to or later than `PeriodStart`.

### public int TotalVideos
Gets the total number of videos published or processed within the current period.

### public long TotalViews
Gets the sum of views across all videos in the current period.

### public long TotalLikes
Gets the sum of likes across all videos in the current period.

### public long TotalComments
Gets the sum of comments across all videos in the current period.

### public long TotalShares
Gets the sum of shares across all videos in the current period.

### public double AverageEngagementRate
Gets the mean engagement rate (likes + comments + shares divided by views) across all videos in the current period. Returns `0.0` if `TotalViews` is zero.

### public int TotalSubscribersGained
Gets the net subscriber change attributed to videos in the current period.

### public DateTime GeneratedAt
Gets the timestamp when the current aggregate snapshot was last computed or refreshed.

## Usage

```csharp
// Synchronize analytics for the latest Short and print a performance summary
var analyticsService = new AnalyticsService();
analyticsService.PeriodStart = DateTime.UtcNow.AddDays(-7);
analyticsService.PeriodEnd = DateTime.UtcNow;

AnalyticsData? synced = await analyticsService.SyncAnalyticsFromYouTubeAsync();
if (synced is not null)
{
    string summary = analyticsService.AnalyzePerformanceMetrics;
    Console.WriteLine(summary);
}
```

```csharp
// Generate a weekly report and inspect top performers
var analyticsService = new AnalyticsService();
analyticsService.PeriodStart = new DateTime(2025, 6, 1);
analyticsService.PeriodEnd = new DateTime(2025, 6, 7);

AnalyticsReport report = await analyticsService.GeneratePeriodReportAsync();
Console.WriteLine($"Period: {report.PeriodStart:d} – {report.PeriodEnd:d}");

IEnumerable<AnalyticsData> topVideos = await analyticsService.GetTopPerformingVideosAsync();
foreach (var video in topVideos)
{
    Console.WriteLine($"{video.VideoTitle}: {video.Views} views");
}

double growth = await analyticsService.CalculateChannelGrowthAsync();
Console.WriteLine($"Channel growth: {growth:F2}%");
```

## Notes

- **Period boundaries:** Setting `PeriodEnd` to a value earlier than `PeriodStart` will cause `GeneratePeriodReportAsync` to throw. The aggregate properties (`TotalViews`, `AverageEngagementRate`, etc.) reflect the period at the time they are accessed; they do not automatically recalculate when the boundaries change unless a refresh method (not shown publicly) is invoked internally.
- **Null returns:** `SyncAnalyticsFromYouTubeAsync` and `GetVideoAnalyticsAsync` return `null` when no data exists for the requested video. Callers must guard against `null` before accessing members of `AnalyticsData`.
- **YouTube API dependency:** `SyncAnalyticsFromYouTubeAsync` is subject to YouTube Data API quota limits and latency. Repeated calls in quick succession may result in quota-exhaustion exceptions.
- **Thread safety:** The service is not guaranteed to be thread-safe. Concurrent modifications to `PeriodStart`/`PeriodEnd` while calling `GeneratePeriodReportAsync` or reading aggregate properties may produce inconsistent results. Callers should serialize access or use a single dedicated instance per operation scope.
- **Empty data sets:** `GetTopPerformingVideosAsync` returns an empty collection when no videos fall within the period or meet the ranking threshold. `AnalyzePerformanceMetrics` returns a neutral default string when `TotalViews` is zero, rather than throwing.
- **`GeneratedAt`:** This timestamp reflects the last time the aggregate snapshot was populated or updated. It does not change on property reads alone.
