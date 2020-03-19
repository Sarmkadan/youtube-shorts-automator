# JobStatusController

The `JobStatusController` class provides a set of HTTP API endpoints for managing and querying the status of background jobs in the YouTube Shorts automator system. It also exposes instance properties that represent the current job context (e.g., `JobId`, `Status`, `Progress`) which are used by the action methods to operate on a specific job. This design allows a single controller instance to be configured with a job identifier and then used to retrieve status, results, or cancellation for that job.

## API

### Properties

#### `public Guid JobId`
Gets or sets the unique identifier of the job that the controller instance is currently targeting. This value is used by all action methods to identify the job to operate on.

#### `public string Status`
Gets or sets the current status of the job (e.g., "Running", "Completed", "Failed", "Cancelled"). This property is typically set by the system when the job state changes.

#### `public int Progress`
Gets or sets the progress percentage of the job, ranging from 0 to 100. Updated as the job executes.

#### `public DateTime CreatedAtUtc`
Gets or sets the UTC timestamp when the job was created.

#### `public DateTime? StartedAtUtc`
Gets or sets the UTC timestamp when the job started execution. `null` if the job has not yet started.

#### `public DateTime? CompletedAtUtc`
Gets or sets the UTC timestamp when the job completed (successfully, with failure, or cancelled). `null` if the job is still running or has not started.

#### `public string? ErrorMessage`
Gets or sets the error message if the job failed. `null` if no error occurred.

#### `public int SuccessfulCount`
Gets or sets the number of successfully processed items in the job.

#### `public int FailedCount`
Gets or sets the number of items that failed processing.

#### `public int TotalCount`
Gets or sets the total number of items in the job.

#### `public List<JobResultItem> Items`
Gets or sets the list of individual result items produced by the job. Each `JobResultItem` contains details about a single processed item.

#### `public string Identifier`
Gets or sets a human-readable identifier for the job (e.g., a user-defined name or a short description).

### Methods

#### `public async Task<IActionResult> GetJobStatusAsync()`
Retrieves the current status of the job identified by the `JobId` property.  
**Returns:** An `IActionResult` containing a JSON object with the job’s status, progress, timestamps, and error message (if any).  
**Throws:** `InvalidOperationException` if `JobId` is `Guid.Empty` or if the job does not exist.

#### `public async Task<IActionResult> GetJobResultsAsync()`
Retrieves the detailed results of the job, including the list of processed items.  
**Returns:** An `IActionResult` containing a JSON object with `SuccessfulCount`, `FailedCount`, `TotalCount`, and the `Items` list.  
**Throws:** `InvalidOperationException` if `JobId` is empty or the job has not yet completed.

#### `public async Task<IActionResult> CancelJobAsync()`
Requests cancellation of the job identified by `JobId`.  
**Returns:** An `IActionResult` indicating whether the cancellation was accepted (HTTP 200) or if the job cannot be cancelled (e.g., already completed).  
**Throws:** `InvalidOperationException` if `JobId` is empty or the job does not exist.

#### `public async Task<IActionResult> GetRecentJobsAsync()`
Returns a list of recently created jobs, ordered by creation time descending.  
**Returns:** An `IActionResult` containing a JSON array of job summaries (each with `JobId`, `Status`, `Progress`, `CreatedAtUtc`, `Identifier`).  
**Throws:** None.

#### `public async Task<IActionResult> GetJobStatsAsync()`
Returns aggregate statistics across all jobs (e.g., total jobs, average duration, success rate).  
**Returns:** An `IActionResult` containing a JSON object with statistical data.  
**Throws:** None.

## Usage

### Example 1: Retrieve job status and results

```csharp
var controller = new JobStatusController
{
    JobId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890")
};

// Get current status
var statusResult = await controller.GetJobStatusAsync();
// statusResult is an IActionResult; cast to ObjectResult to inspect value
if (statusResult is ObjectResult objResult && objResult.Value is JobStatusDto status)
{
    Console.WriteLine($"Job {status.JobId} is {status.Status} at {status.Progress}%");
}

// Get results (only valid after job completes)
var resultsResult = await controller.GetJobResultsAsync();
if (resultsResult is ObjectResult resObj && resObj.Value is JobResultsDto results)
{
    Console.WriteLine($"Processed {results.SuccessfulCount} of {results.TotalCount} items");
}
```

### Example 2: Cancel a running job and list recent jobs

```csharp
var controller = new JobStatusController
{
    JobId = Guid.NewGuid() // assume this is set to a running job's ID
};

// Attempt cancellation
var cancelResult = await controller.CancelJobAsync();
if (cancelResult is OkResult)
{
    Console.WriteLine("Cancellation request accepted.");
}
else
{
    Console.WriteLine("Job could not be cancelled (already completed or not found).");
}

// List recent jobs
var recentResult = await controller.GetRecentJobsAsync();
if (recentResult is ObjectResult recentObj && recentObj.Value is IEnumerable<JobSummary> summaries)
{
    foreach (var job in summaries)
    {
        Console.WriteLine($"{job.Identifier}: {job.Status}");
    }
}
```

## Notes

- **Thread safety:** The `JobStatusController` instance properties (`JobId`, `Status`, etc.) are not thread-safe. If the same controller instance is accessed concurrently from multiple threads, external synchronization is required. In typical ASP.NET Core usage, controllers are scoped per request, so this is rarely an issue. However, if the controller is reused (e.g., as a singleton), ensure that properties are not modified while action methods are executing.
- **JobId requirement:** All action methods except `GetRecentJobsAsync` and `GetJobStatsAsync` rely on the `JobId` property being set to a non-empty GUID. Calling these methods with `JobId == Guid.Empty` will throw an `InvalidOperationException`.
- **State consistency:** The properties `Status`, `Progress`, `CreatedAtUtc`, `StartedAtUtc`, `CompletedAtUtc`, `ErrorMessage`, `SuccessfulCount`, `FailedCount`, `TotalCount`, `Items`, and `Identifier` are intended to be set by the system before invoking action methods. They are not automatically populated by the methods themselves; they represent the current state of the job that the controller is configured to manage.
- **Edge cases:** `GetJobResultsAsync` will throw if the job has not yet completed (i.e., `CompletedAtUtc` is `null`). `CancelJobAsync` may return a non-OK result if the job is already in a terminal state. `GetRecentJobsAsync` and `GetJobStatsAsync` operate on all jobs and do not depend on `JobId`.
