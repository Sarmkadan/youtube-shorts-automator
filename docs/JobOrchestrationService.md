# JobOrchestrationService

Orchestrates the end-to-end lifecycle of a YouTube Shorts video within the `youtube-shorts-automator` pipeline. It coordinates content generation, upload scheduling, status tracking, retry logic for failed jobs, and analytics synchronization for a single video entity tied to a specific channel.

## API

### Constructors

- **`public JobOrchestrationService`**  
  Instantiates the service, typically binding it to a specific video short and upload job context. Internal initialization wires up dependencies for pipeline execution, upload management, and analytics syncing.

### Methods

- **`public async Task<Pipeline> ProcessFullPipelineAsync`**  
  Executes the complete automation pipeline for the associated short: content creation, processing, and preparation for upload.  
  **Returns:** A `Pipeline` object representing the assembled processing graph and its final state.  
  **Throws:** May throw if any pipeline stage fails irrecoverably or if required external services are unavailable.

- **`public async Task<bool> ProcessReadyUploadsAsync`**  
  Processes all upload jobs that are in a ready state and whose `ScheduledUploadTime` has been reached. Publishes the video to the target channel.  
  **Returns:** `true` if at least one upload was successfully processed; `false` otherwise.  
  **Throws:** Can throw on authentication failures or platform API errors during the upload step.

- **`public async Task<bool> ProcessFailedRetriableJobsAsync`**  
  Inspects jobs marked with a failed status that are eligible for retry and re-enqueues or re-attempts them.  
  **Returns:** `true` if any retriable job was successfully re-processed; `false` if no eligible jobs existed or all retries failed.  
  **Throws:** May throw if the underlying job store is unreachable or if retry logic encounters an unrecoverable state.

- **`public async Task<SyncResult> SyncAnalyticsAsync`**  
  Pulls performance analytics (views, engagement metrics) from the platform for the uploaded short and persists them locally.  
  **Returns:** A `SyncResult` containing the number of synced records and any failure details.  
  **Throws:** Throws on network or API authorization errors; does not throw for partial sync failures (those are captured in the result).

### Properties

- **`public int VideoShortId`**  
  The unique identifier of the video short entity this service instance is managing.

- **`public int UploadJobId`**  
  The identifier of the upload job associated with the current pipeline run.

- **`public string Status`**  
  The current high-level status of the job (e.g., `"Pending"`, `"Processing"`, `"Ready"`, `"Uploaded"`, `"Failed"`).

- **`public string? Error`**  
  The last captured error message, if any. Null when no error has occurred.

- **`public bool ProcessingCompleted`**  
  Indicates whether the full pipeline (generation through upload) has finished. `true` once the terminal state is reached, regardless of success or failure.

- **`public DateTime ScheduledUploadTime`**  
  The intended publication time for the short. The upload step respects this timestamp when deciding to proceed.

- **`public DateTime CompletedAt`**  
  The timestamp when the job reached its final state (uploaded, permanently failed, or cancelled).

- **`public string Channel`**  
  The target YouTube channel identifier or handle for this short.

- **`public int SyncedCount`**  
  The number of analytics records successfully synchronized in the most recent `SyncAnalyticsAsync` call.

- **`public int FailedCount`**  
  The number of analytics records that failed to synchronize in the most recent `SyncAnalyticsAsync` call.

## Usage

### Example 1: Full Pipeline Execution with Analytics Sync

```csharp
var service = new JobOrchestrationService
{
    VideoShortId = 1042,
    UploadJobId = 207,
    Channel = "@CreatorHub",
    ScheduledUploadTime = DateTime.UtcNow.AddHours(2)
};

// Run the complete pipeline (content generation + upload prep)
Pipeline pipeline = await service.ProcessFullPipelineAsync();

if (service.Status == "Ready")
{
    bool uploaded = await service.ProcessReadyUploadsAsync();
    if (uploaded)
    {
        SyncResult analytics = await service.SyncAnalyticsAsync();
        Console.WriteLine($"Synced: {analytics.SyncedCount}, Failed: {analytics.FailedCount}");
    }
}
```

### Example 2: Retry Loop for Failed Jobs

```csharp
var service = new JobOrchestrationService
{
    VideoShortId = 2091,
    UploadJobId = 418,
    Channel = "@DailyShorts"
};

// Attempt to recover any previously failed retriable jobs
bool retried = await service.ProcessFailedRetriableJobsAsync();

if (retried && service.Status == "Ready")
{
    bool uploaded = await service.ProcessReadyUploadsAsync();
    if (!uploaded)
    {
        Console.WriteLine($"Upload failed. Error: {service.Error}");
    }
}
else if (!retried)
{
    Console.WriteLine("No retriable jobs found or all retries exhausted.");
}
```

## Notes

- **State mutability:** Properties such as `Status`, `Error`, `ProcessingCompleted`, `SyncedCount`, and `FailedCount` are mutated by the async methods and reflect the outcome of the most recent operation. Reading them concurrently with an in-flight method may yield intermediate values.
- **Thread safety:** The service is not designed to be thread-safe. A single instance should be owned and driven sequentially by one consumer. Parallel invocation of `ProcessFullPipelineAsync`, `ProcessReadyUploadsAsync`, or `ProcessFailedRetriableJobsAsync` on the same instance leads to race conditions on status and error state.
- **Scheduling semantics:** `ProcessReadyUploadsAsync` only acts when the current system time is at or past `ScheduledUploadTime`. Calling it earlier is a no-op and returns `false`.
- **Retry eligibility:** `ProcessFailedRetriableJobsAsync` relies on internal retry metadata (not exposed directly on this type). Jobs that have exceeded their retry count or are marked non-retriable are skipped silently.
- **Analytics partial failure:** `SyncAnalyticsAsync` captures per-record failures in `FailedCount` without throwing. A non-zero `FailedCount` does not imply the entire sync aborted; `SyncedCount` may still be positive.
- **`CompletedAt` population:** This timestamp is set only when the job enters a terminal state. It remains at its default value while the job is in progress or pending.
