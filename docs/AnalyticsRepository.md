# AnalyticsRepository

The `AnalyticsRepository` serves as the primary data access layer for retrieving YouTube Shorts analytics metrics within the `youtube-shorts-automator` project. It provides asynchronous methods to query performance data such as views, engagement, and retention, supporting filtering by specific video identifiers, time periods, and pagination requirements. This repository abstracts the underlying storage mechanism, ensuring consistent access patterns for analytics data across the application.

## API

### `AnalyticsRepository`
Initializes a new instance of the `AnalyticsRepository` class.
*   **Parameters**: None (assumes dependency injection or internal configuration).
*   **Returns**: A new `AnalyticsRepository` instance.
*   **Throws**: Throws exceptions related to configuration failures or database connection issues during initialization.

### `GetByVideoIdAsync`
Retrieves a list of analytics metrics associated with a specific video identifier.
*   **Parameters**: `Guid videoId` (The unique identifier of the video).
*   **Returns**: `Task<List<AnalyticsMetric>>` containing all recorded metrics for the specified video. Returns an empty list if no metrics exist.
*   **Throws**: Throws `ArgumentException` if the `videoId` is empty or invalid; throws data access exceptions if the underlying query fails.

### `GetByPeriodAsync`
Fetches analytics metrics recorded within a specific start and end time range.
*   **Parameters**: `DateTime startDate`, `DateTime endDate` (The inclusive time window for the query).
*   **Returns**: `Task<List<AnalyticsMetric>>` containing metrics falling within the specified period.
*   **Throws**: Throws `ArgumentException` if `startDate` is greater than `endDate`; throws data access exceptions on query failure.

### `GetRecentMetricsAsync`
Retrieves the most recently recorded analytics metrics across all videos, typically used for dashboards or latest activity feeds.
*   **Parameters**: None (implies a default limit or configuration-defined count).
*   **Returns**: `Task<List<AnalyticsMetric>>` ordered by recording timestamp descending.
*   **Throws**: Throws data access exceptions if the retrieval operation fails.

### `GetTopMetricsAsync`
Returns analytics metrics sorted by a primary performance indicator (e.g., view count or engagement rate) in descending order.
*   **Parameters**: None (implies default sorting criteria and count).
*   **Returns**: `Task<List<AnalyticsMetric>>` representing the highest-performing entries.
*   **Throws**: Throws data access exceptions if the sorting or retrieval operation fails.

### `GetLatestMetricForVideoAsync`
Fetches the single most recent analytics entry for a specific video.
*   **Parameters**: `Guid videoId` (The unique identifier of the video).
*   **Returns**: `Task<AnalyticsMetric?>` containing the latest metric or `null` if no data exists for the video.
*   **Throws**: Throws `ArgumentException` if the `videoId` is invalid; throws data access exceptions on query failure.

### `GetPaginatedAsync`
Retrieves a specific page of analytics metrics along with the total count of available records.
*   **Parameters**: `int pageNumber`, `int pageSize` (Pagination controls).
*   **Returns**: `Task<(List<AnalyticsMetric> Metrics, int Total)>` where `Metrics` is the current page subset and `Total` is the grand total of records matching the query criteria.
*   **Throws**: Throws `ArgumentOutOfRangeException` if page parameters are invalid (e.g., negative values); throws data access exceptions on failure.

### `GetVideoViewCountsAsync`
Aggregates and returns the total view counts for a collection of videos.
*   **Parameters**: None (assumes aggregation over all tracked videos or a predefined set).
*   **Returns**: `Task<Dictionary<Guid, long>>` mapping video IDs to their respective total view counts.
*   **Throws**: Throws data access exceptions if the aggregation query fails.

## Usage

### Example 1: Retrieving Performance Data for a Specific Video
This example demonstrates how to fetch the latest metric for a specific video and handle the case where data might not yet exist.

```csharp
public async Task DisplayVideoPerformance(Guid videoId, AnalyticsRepository repository)
{
    var latestMetric = await repository.GetLatestMetricForVideoAsync(videoId);

    if (latestMetric == null)
    {
        Console.WriteLine($"No analytics data available for video {videoId}.");
        return;
    }

    Console.WriteLine($"Video: {videoId}");
    Console.WriteLine($"Latest Views: {latestMetric.ViewCount}");
    Console.WriteLine($"Recorded At: {latestMetric.Timestamp}");
}
```

### Example 2: Generating a Paginated Report of Top Metrics
This example illustrates retrieving a specific page of top-performing metrics while utilizing the total count for UI pagination controls.

```csharp
public async Task GenerateMetricsReport(AnalyticsRepository repository, int page = 1, int pageSize = 20)
{
    // Retrieve paginated top metrics
    var result = await repository.GetPaginatedAsync(page, pageSize);
    var metrics = result.Metrics;
    var totalRecords = result.Total;

    Console.WriteLine($"Showing page {page} of {Math.Ceiling((double)totalRecords / pageSize)}");
    
    foreach (var metric in metrics)
    {
        Console.WriteLine($"[ID: {metric.VideoId}] Views: {metric.ViewCount} | Likes: {metric.LikeCount}");
    }
}
```

## Notes

*   **Null Handling**: The method `GetLatestMetricForVideoAsync` explicitly returns a nullable `AnalyticsMetric?`. Callers must perform null checks before accessing properties of the returned object to avoid `NullReferenceException`.
*   **Empty Collections**: Methods returning `List<AnalyticsMetric>` (e.g., `GetByVideoIdAsync`, `GetByPeriodAsync`) will return an empty list rather than `null` when no data is found. Consumers should check `Count` rather than performing null checks on the list itself.
*   **Thread Safety**: As an asynchronous repository pattern implementation, this class is designed to be stateless regarding request data. It is generally safe for concurrent use by multiple threads, provided the underlying database context or data provider injected into the constructor supports concurrent async operations.
*   **Date Validation**: When using `GetByPeriodAsync`, ensure the `startDate` precedes the `endDate`. Passing an inverted range will result in an exception rather than an automatic swap or empty result.
*   **Aggregation Scope**: `GetVideoViewCountsAsync` returns a dictionary. If the system tracks thousands of videos, the resulting dictionary may consume significant memory. Consider using filtered overloads if available in future iterations or applying pagination logic before calling aggregation methods in high-scale scenarios.
