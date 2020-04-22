# IMetricsCollector

The `IMetricsCollector` interface provides a standardized mechanism for tracking performance, telemetry, and operational data within the `youtube-shorts-automator` application. It enables components to register granular metrics—such as processing durations, upload outcomes, and external API interaction statistics—to facilitate comprehensive health monitoring, performance analysis, and debugging of automation workflows.

## API

### Methods

*   **`MetricsCollector()`**: Initializes a new instance of the `MetricsCollector` implementation.
*   **`void RecordProcessingDuration(...)`**: Records the duration and resource usage of a processing operation.
*   **`void RecordUploadSuccess(...)`**: Registers a successful completion of an upload operation.
*   **`void RecordUploadFailure(...)`**: Registers a failure encounter during an upload operation.
*   **`void RecordApiCall(...)`**: Logs the details of an external API request, including metadata relevant to throughput and error tracking.
*   **`async Task<MetricsSnapshot> GetMetricsAsync(...)`**: Asynchronously retrieves a point-in-time snapshot of the collected metrics.

### Snapshot Properties (`MetricsSnapshot`)

*   **`DateTime CapturedAtUtc`**: The UTC timestamp indicating when the metrics snapshot was generated.
*   **`List<ProcessingMetric> ProcessingMetrics`**: A collection of detailed performance metrics recorded for various processing operations.
*   **`Dictionary<string, int> ErrorCounts`**: A mapping of error identifiers or types to their respective occurrence frequencies.
*   **`List<ApiCallMetric> ApiCallMetrics`**: A collection of detailed metrics regarding external API interactions.
*   **`int TotalApiCalls`**: The aggregate count of all recorded API calls.

### Metric Detail Properties (`ProcessingMetric` / `ApiCallMetric`)

*   **`string ProcessType`**: The identifier or classification of the process being measured.
*   **`int Count`**: The number of times the associated operation has been recorded.
*   **`double TotalDurationMs`**: The cumulative duration of the operations in milliseconds.
*   **`double MinDurationMs`**: The minimum duration recorded for an operation of this type.
*   **`double MaxDurationMs`**: The maximum duration recorded for an operation of this type.
*   **`long TotalBytesProcessed`**: The total amount of data, in bytes, processed during these operations.
*   **`DateTime LastRecordedUtc`**: The UTC timestamp of the most recent recording for this metric.
*   **`string Endpoint`**: The destination endpoint for an API call metric.
*   **`int StatusCode`**: The HTTP status code or operational result code associated with the metric.

## Usage

### Recording Performance Metrics

```csharp
public void ProcessVideo(IMetricsCollector metrics, Video video)
{
    var stopwatch = Stopwatch.StartNew();
    
    // ... perform video processing ...
    
    stopwatch.Stop();
    metrics.RecordProcessingDuration("VideoTranscoding", stopwatch.ElapsedMilliseconds, video.FileSize);
}
```

### Retrieving and Analyzing Telemetry

```csharp
public async Task MonitorHealth(IMetricsCollector metrics)
{
    MetricsSnapshot snapshot = await metrics.GetMetricsAsync();
    
    Console.WriteLine($"Snapshot time: {snapshot.CapturedAtUtc}");
    Console.WriteLine($"Total API calls: {snapshot.TotalApiCalls}");
    
    foreach (var error in snapshot.ErrorCounts)
    {
        Console.WriteLine($"Error {error.Key} occurred {error.Value} times.");
    }
}
```

## Notes

*   **Thread Safety**: The implementation of `IMetricsCollector` is designed to be thread-safe, allowing multiple concurrent components to record metrics simultaneously without corrupting the internal state.
*   **Snapshot Immutability**: The `MetricsSnapshot` returned by `GetMetricsAsync` represents an immutable view of the data at the time of the request.
*   **Data Aggregation**: Metrics are aggregated based on their identifiers (e.g., `ProcessType` or `Endpoint`). Recording a metric for an existing identifier will update the cumulative stats, such as `TotalDurationMs`, `Count`, `MinDurationMs`, and `MaxDurationMs`.
*   **Edge Cases**: Operations with a duration of zero or negative values are generally treated as invalid and may be ignored by the implementation depending on configuration. Large numbers of unique `ProcessType` or `Endpoint` values can lead to increased memory usage within the `MetricsCollector`.
