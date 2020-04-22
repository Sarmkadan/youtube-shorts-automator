# ProcessingJob

The `ProcessingJob` class represents a discrete unit of work within the `youtube-shorts-automator` pipeline, managing the state, progress, and lifecycle of video-related processing tasks. It acts as the central entity for tracking job execution from initial creation through successful completion or terminal failure, providing the necessary infrastructure for observability, retry logic, and progress monitoring.

## API

### Properties

*   **`Guid Id`**: The unique identifier for this specific processing job instance.
*   **`Guid VideoId`**: The unique identifier of the video associated with this processing job.
*   **`Video? Video`**: The associated `Video` entity. This may be `null` if the video context has not been loaded.
*   **`ProcessingJobType JobType`**: Indicates the type of processing task being performed (e.g., rendering, uploading).
*   **`ProcessingJobStatus Status`**: The current execution state of the job.
*   **`ProcessingStepType CurrentStep`**: The specific stage within the overall job workflow currently being executed.
*   **`DateTime CreatedAt`**: The timestamp when the job was initially created in the system.
*   **`DateTime? StartedAt`**: The timestamp when the job execution was officially started. Remains `null` until `Start()` is called.
*   **`DateTime? CompletedAt`**: The timestamp when the job finished processing (either success or failure). Remains `null` until `Complete()` or `Fail()` is called.
*   **`string OutputPath`**: The filesystem path or URI where the resulting processed content is stored.
*   **`List<ProcessingStep> Steps`**: A collection of individual steps comprising this job's workflow.
*   **`List<ProcessingError> Errors`**: A collection of errors encountered during the lifecycle of the job.
*   **`int RetryCount`**: The number of times this job has been retried after encountering a transient error.
*   **`int MaxRetries`**: The maximum allowed number of retries for this job before it is marked as permanently failed.
*   **`TimeSpan EstimatedDuration`**: The anticipated time required to complete the job based on historical data or workload analysis.
*   **`float ProgressPercentage`**: The current completion status, represented as a float from 0.0 to 100.0.
*   **`string? WorkerId`**: The identifier of the worker node or process currently executing or that last executed this job.

### Methods

*   **`void Start()`**: Marks the job as started, sets the `StartedAt` timestamp to the current time, and transitions the status accordingly.
*   **`void Complete()`**: Marks the job as successfully completed, sets the `CompletedAt` timestamp to the current time, sets `ProgressPercentage` to 100.0, and updates the status to completed.
*   **`void Fail()`**: Transitions the job to a failed state, updates the `CompletedAt` timestamp, and increments the `RetryCount`. If the failure causes `RetryCount` to exceed `MaxRetries`, the job is marked as permanently failed.

## Usage

### Creating and Starting a Job

```csharp
var job = new ProcessingJob
{
    Id = Guid.NewGuid(),
    VideoId = targetVideo.Id,
    JobType = ProcessingJobType.Rendering,
    MaxRetries = 3
};

// ... setup logic ...

job.Start();
Console.WriteLine($"Job started at: {job.StartedAt}");
```

### Handling Job Completion and Errors

```csharp
try
{
    // ... perform processing logic ...
    job.Complete();
}
catch (Exception ex)
{
    job.Errors.Add(new ProcessingError { Message = ex.Message });
    job.Fail();
    
    if (job.RetryCount >= job.MaxRetries)
    {
        Console.WriteLine("Job exceeded maximum retries.");
    }
}
```

## Notes

*   **Thread Safety**: The `ProcessingJob` class is not inherently thread-safe. If an instance is accessed or modified across multiple threads (e.g., in a concurrent worker environment), appropriate external synchronization mechanisms (such as `lock` statements or concurrent collections) must be applied to ensure data integrity, particularly when updating `Status`, `ProgressPercentage`, or the `Errors` list.
*   **Nullability**: Consumers must handle potentially `null` values for `Video`, `StartedAt`, and `CompletedAt`, as these depend on the lifecycle state of the specific job instance.
*   **Retry Logic**: The `Fail()` method manages internal retry state. Ensure that external worker logic respects the `RetryCount` and `MaxRetries` properties before attempting to requeue a job.
*   **Step Tracking**: The `Steps` and `CurrentStep` properties are intended for granular tracking. Ensure that updates to these properties are synchronized with actual execution progress to prevent misleading telemetry data.
