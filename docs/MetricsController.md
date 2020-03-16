# MetricsController

The `MetricsController` is an ASP.NET Core controller that exposes endpoints for retrieving various telemetry and performance data gathered by the YouTube Shorts Automator application. It provides aggregated system metrics, API call statistics, error summaries, and processing performance details in a format suitable for monitoring dashboards or diagnostic tools.

## API

### MetricsController()
Parameterless constructor. Creates a new instance of the controller with no dependencies injected. The instance is ready to handle action invocations once placed within the ASP.NET Core pipeline.

### GetSystemMetricsAsync()
```csharp
public async Task<IActionResult> GetSystemMetricsAsync()
```
**Purpose:** Returns a snapshot of overall system metrics (e.g., uptime, memory usage, thread counts).  
**Parameters:** None.  
**Return Value:** An `IActionResult` that, on success, yields an `OkObjectResult` containing a DTO with the system metrics; on failure, returns an appropriate error status (e.g., `500 Internal Server Error`).  
**Throws:** May throw `IOException` if reading performance counters fails, or `ObjectDisposedException` if the underlying metrics provider has been disposed.

### GetApiCallMetricsAsync()
```csharp
public async Task<IActionResult> GetApiCallMetricsAsync()
```
**Purpose:** Returns aggregated statistics for external API calls made by the automator (total calls, success/failure ratios, latency).  
**Parameters:** None.  
**Return Value:** An `IActionResult` yielding an `OkObjectResult` with a DTO that includes `TotalApiCalls`, `Groups`, and related timing information; otherwise an error status.  
**Throws:** May throw `InvalidOperationException` if the call‑tracking service is not initialized, or `IOException` on I/O errors while retrieving persisted call logs.

### GetErrorStatsAsync()
```csharp
public async Task<IActionResult> GetErrorStatsAsync()
```
**Purpose:** Provides a breakdown of recorded errors by category or message.  
**Parameters:** None.  
**Return Value:** An `IActionResult` yielding an `OkObjectResult` containing a dictionary (`ErrorCounts`) mapping error identifiers to their occurrence counts; otherwise an error status.  
**Throws:** May throw `NullReferenceException` if the error store has not been instantiated, or `IOException` when reading error persisted storage.

### GetProcessingPerformanceAsync()
```csharp
public async Task<IActionResult> GetProcessingPerformanceAsync()
```
**Purpose:** Returns performance metrics for the core processing pipeline (duration, bytes processed, throughput).  
**Parameters:** None.  
**Return Value:** An `IActionResult` yielding an `OkObjectResult` with a DTO that includes `ProcessingMetrics`, `TotalBytesProcessed`, and timing aggregates; otherwise an error status.  
**Throws:** May throw `IOException` if accessing processing logs fails, or `InvalidOperationException` if processing state is inconsistent.

### CapturedAtUtc
```csharp
public DateTime CapturedAtUtc
```
**Purpose:** Timestamp indicating when the metrics snapshot was taken, expressed in UTC.  
**Remarks:** Set by each action method before returning the result; defaults to `DateTime.MinValue` if not explicitly assigned.

### TotalApiCalls
```csharp
public int TotalApiCalls
```
**Purpose:** Cumulative count of API calls made since the application started or since the last reset.  
**Remarks:** Updated by the API call tracking component; zero indicates no calls have been recorded.

### ProcessingMetrics
```csharp
public List<ProcessingMetricResponse> ProcessingMetrics
```
**Purpose:** Collection of detailed processing metric entries (e.g., per‑job or per‑stage timings).  
**Remarks:** May be empty if no processing has occurred; each element provides `ProcessType`, `Count`, `AverageDurationMs`, `MinDurationMs`, `MaxDurationMs`, and `TotalBytesProcessed`.

### ErrorCounts
```csharp
public Dictionary<string, int> ErrorCounts
```
**Purpose:** Mapping of error identifiers (e.g., exception types or error codes) to the number of times each has been observed.  
**Remarks:** Empty dictionary signals no recorded errors.

### ProcessType
```csharp
public string ProcessType
```
**Purpose:** Label describing the type of processing job to which a set of metrics belongs (e.g., “VideoDownload”, “ThumbnailGeneration”).  
**Remarks:** Populated within each `ProcessingMetricResponse` instance.

### Count
```csharp
public int Count
```
**Purpose:** Number of individual processing units represented by the enclosing metric entry.  
**Remarks:** Used to calculate averages; zero would indicate no samples for that entry.

### AverageDurationMs (first occurrence)
```csharp
public double AverageDurationMs
```
**Purpose:** Mean duration of the processing operations described by the enclosing metric entry, in milliseconds.  
**Remarks:** Derived from the sum of durations divided by `Count`; may be `0.0` when `Count` is zero.

### MinDurationMs
```csharp
public double MinDurationMs
```
**Purpose:** Shortest observed duration for the processing operations in the enclosing metric entry, in milliseconds.  
**Remarks:** Defaults to `0.0` if no samples exist.

### MaxDurationMs
```csharp
public double MaxDurationMs
```
**Purpose:** Longest observed duration for the processing operations in the enclosing metric entry, in milliseconds.  
**Remarks:** Defaults to `0.0` if no samples exist.

### TotalBytesProcessed
```csharp
public long TotalBytesProcessed
```
**Purpose:** Cumulative volume of data processed by the operations represented by the enclosing metric entry, in bytes.  
**Remarks:** Useful for throughput calculations; zero indicates no data processed.

### TotalCalls
```csharp
public int TotalCalls
```
**Purpose:** Total number of invocations for a specific API endpoint group (see `ApiCallGroup`).  
**Remarks:** Part of the API call metrics payload; zero means no calls for that group.

### Groups
```csharp
public List<ApiCallGroup> Groups
```
**Purpose:** List of grouped API call statistics, each representing a distinct endpoint or service.  
**Remarks:** Each `ApiCallGroup` contains `Endpoint`, `CallCount`, and its own `AverageDurationMs`.

### Endpoint
```csharp
public string Endpoint
```
**Purpose:** The specific API endpoint or service identifier associated with a call group.  
**Remarks:** Populated from configuration or request logging; empty string indicates unknown.

### CallCount
```csharp
public int CallCount
```
**Purpose:** Number of calls made to the endpoint represented by the enclosing `ApiCallGroup`.  
**Remarks:** Mirrors `TotalCalls` within the group context; zero indicates no traffic.

### AverageDurationMs (second occurrence)
```csharp
public double AverageDurationMs
```
**Purpose:** Mean call duration for the endpoint group, in milliseconds.  
**Remarks:** Calculated from the sum of individual call latencies divided by `CallCount`; defaults to `0.0` when no calls have been recorded.

## Usage

### Example 1: Invoking a controller action directly (unit test or internal service)
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// Assume the controller is instantiated within the test harness
var controller = new MetricsController();

// Retrieve system metrics
IActionResult systemResult = await controller.GetSystemMetricsAsync();
if (systemResult is OkObjectResult ok && ok.Value is SystemMetricsDto systemMetrics)
{
    Console.WriteLine($"System captured at: {systemMetrics.CapturedAtUtc:O}");
    Console.WriteLine($"Total API calls: {systemMetrics.TotalApiCalls}");
}
else
{
    // Handle non‑success status (e.g., BadRequest, StatusCodeResult)
    Console.WriteLine($"Failed to get system metrics: {systemResult.StatusCode}");
}
```

### Example 2: Calling the endpoint via HTTP (typical client scenario)
```csharp
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

var http = new HttpClient { BaseAddress = new Uri("https://localhost:5001/api/") };

// Request processing performance data
HttpResponseMessage response = await http.GetAsync("metrics/processing-performance");
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();
var perf = JsonSerializer.Deserialize<ProcessingPerformanceDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

Console.WriteLine($"Average processing time: {perf.AverageDurationMs} ms");
Console.WriteLine($"Total bytes processed: {perf.TotalBytesProcessed}");
```

## Notes

- **Default values:** When no data is available, numeric properties (`TotalApiCalls`, `Count`, `TotalCalls`, `CallCount`) default to `0`, timing properties (`AverageDurationMs`, `MinDurationMs`, `MaxDurationMs`) default to `0.0`, and collections (`ProcessingMetrics`, `Groups`, `ErrorCounts`) are empty instances. The `CapturedAtUtc` field defaults to `DateTime.MinValue` if not explicitly set by the action method.
- **Thread safety:** The controller is designed to be instantiated per request by the ASP.NET Core core; its instance members are not guarded against concurrent access from multiple threads. If the controller is reused outside the typical request scope (e.g., as a singleton), callers must synchronize access to mutable properties or treat the instance as read‑only after initialization.
- **Error propagation:** Action methods do not swallow exceptions; they allow the framework to convert unhandled exceptions into `500 Internal Server Error` responses. Consumers should inspect the returned `IActionResult` status code to differentiate between successful payloads and error conditions.
- **Data consistency:** The metrics returned by different endpoints reflect snapshots taken at the moment each method is invoked. Rapid successive calls may show varying values because underlying counters are updated asynchronously by the application’s background services. For a consistent view across multiple metric types, consider invoking a single aggregated endpoint (if available) or locking the measurement window externally.
