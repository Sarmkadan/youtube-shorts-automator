# UploadStatus

The `UploadStatus` enum describes the lifecycle state of an upload job in the console pipeline (`YouTubeShortAutomator` namespace, `src/Constants/UploadStatus.cs`). It is stored on `UploadJob.Status` and used by the scheduler, the upload service, and the job orchestration code to decide what happens to a job next.

The underlying type is `int`, and the members are assigned explicit values from `0` to `7`. Those values are persisted, so they must not be reordered or renumbered.

## API

*   **`Pending = 0`**
    The job has been created and is waiting to be scheduled. It is the initial state assigned when an upload job is created, and the scheduler picks up pending jobs whose scheduled time has arrived.
*   **`Queued = 1`**
    The job has been selected for upload and is waiting for an upload slot. Set by `UploadJob.MarkAsQueued()`.
*   **`Uploading = 2`**
    The video is being transferred to YouTube. Set by `UploadJob.MarkAsUploading()`. Jobs in this state are treated as in progress and cannot be cancelled.
*   **`Completed = 3`**
    The upload finished successfully and the job has a YouTube video ID. Set by `UploadJob.MarkAsCompleted(videoId)`. This is a terminal state.
*   **`Failed = 4`**
    The upload attempt ended with an error. Set by `UploadJob.MarkAsFailed(error)`. A failed job can be retried while its attempt count is below its maximum retry count.
*   **`Cancelled = 5`**
    The job was cancelled by the scheduling service. Cancelling is refused only while the job is `Uploading`. Nothing in the current code moves a cancelled job back to another state.
*   **`Paused = 6`**
    Reserved for a job that has been temporarily suspended. No code path in the current codebase assigns this value, so treat it as not yet in use.
*   **`Retrying = 7`**
    A retry of a previously failed job has started and the upload is running again. Set by the upload service at the start of a retry attempt. Retrying jobs count as active, like `Queued` and `Uploading`.

## Usage

### Checking whether a job is still active
A job is active while it is queued, uploading, or retrying. `UploadJobExtensions.IsActive()` implements this check.

```csharp
bool isActive = job.IsActive();
```

### Finding jobs that are ready to run
The scheduler selects `Pending` jobs whose scheduled time has passed.

```csharp
var due = jobs.Where(j => j.ScheduledAt <= now && j.Status == UploadStatus.Pending);
```

## Notes

*   **Underlying type**: The enum does not declare a base type, so it uses `int`. Members have explicit values `0` to `7`. Do not renumber existing members or insert new ones in the middle; append new members with the next free value.
*   **Retry flow**: A job moves from `Failed` to `Retrying` only when `CanRetry()` is true (status `Failed` and attempt count below the maximum retry count).
*   **Cancellation and rescheduling**: Cancelling a job that is `Uploading` throws a `SchedulingException`. Rescheduling refuses jobs that are `Completed` or `Uploading`.
