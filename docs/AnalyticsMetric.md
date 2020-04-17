# AnalyticsMetric

The `AnalyticsMetric` class serves as a structured data model representing the performance statistics of a YouTube video over a defined period. It aggregates engagement data, such as views, likes, and comments, alongside demographic breakdowns and calculated performance indicators, facilitating comprehensive analysis within the `youtube-shorts-automator` system.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | The unique identifier for the analytics record. |
| `VideoId` | `Guid` | The foreign key identifier of the associated `Video`. |
| `Video` | `Video?` | The navigation property to the associated `Video` entity; may be `null` if not loaded. |
| `ViewCount` | `long` | The total number of views recorded for this period. |
| `LikeCount` | `long` | The total number of likes recorded for this period. |
| `CommentCount` | `long` | The total number of comments recorded for this period. |
| `ShareCount` | `long` | The total number of shares recorded for this period. |
| `SubscriberGainedCount`| `long` | The number of subscribers gained during this period. |
| `SubscriberLostCount` | `long` | The number of subscribers lost during this period. |
| `AverageViewDurationSeconds`| `double` | The average duration of video views in seconds. |
| `EngagementRatePercent`| `double` | The calculated engagement rate as a percentage. |
| `ClickThroughRatePercent`| `double` | The click-through rate for the video as a percentage. |
| `Period` | `MetricsPeriod` | The `MetricsPeriod` enum indicating the timeframe covered by this record. |
| `CollectedAt` | `DateTime` | The timestamp indicating when these metrics were collected. |
| `UpdatedAt` | `DateTime?` | The timestamp indicating when this record was last modified; `null` if never updated. |
| `TrafficSource` | `string?` | The primary traffic source for this metric set, if specified. |
| `DeviceType` | `string?` | The primary device type (e.g., 'mobile', 'desktop'), if specified. |
| `Demographics` | `List<DemographicMetric>`| A collection of `DemographicMetric` objects detailing audience breakdown. |
| `CalculateEngagementScore`| `double` | A read-only property returning the dynamically calculated engagement score based on current metrics. |
| `IsHighPerforming` | `bool` | A read-only property indicating if the video meets performance thresholds for the current period. |

## Usage

### Retrieving and Displaying Core Metrics
```csharp
public void PrintVideoPerformance(AnalyticsMetric metric)
{
    Console.WriteLine($"Metric ID: {metric.Id}");
    Console.WriteLine($"Views: {metric.ViewCount}");
    Console.WriteLine($"Engagement Rate: {metric.EngagementRatePercent}%");
    
    if (metric.Video != null)
    {
        Console.WriteLine($"Video Title: {metric.Video.Title}");
    }
}
```

### Filtering Data Using Calculated Properties
```csharp
public List<AnalyticsMetric> GetTopPerformers(List<AnalyticsMetric> allMetrics)
{
    // Filter metrics that are flagged as high performing
    return allMetrics
        .Where(m => m.IsHighPerforming)
        .OrderByDescending(m => m.CalculateEngagementScore)
        .ToList();
}
```

## Notes

- **Thread Safety**: This class is not thread-safe. Instances should not be shared across threads without external synchronization if updates to property values are occurring.
- **Nullable Members**: Several properties (`Video`, `UpdatedAt`, `TrafficSource`, `DeviceType`) are nullable. Ensure proper null-checking is performed before accessing these members to avoid `NullReferenceException`.
- **Calculated Properties**: The properties `CalculateEngagementScore` and `IsHighPerforming` are calculated dynamically based on the current state of the object. Modifying underlying numeric properties (such as `ViewCount` or `LikeCount`) will immediately affect the values returned by these accessors.
- **Demographics**: The `Demographics` property initializes as an empty list; ensure it is properly populated before iterating or performing data operations.
