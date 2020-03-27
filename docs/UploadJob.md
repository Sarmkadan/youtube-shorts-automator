# UploadJob

Represents a single upload task for a YouTube Short within the automator system. It tracks the job’s identifier, scheduling information, current status, progress metrics, retry logic, and links to the associated `VideoShort` entity.

## API

### Id
- **Purpose:** Primary key of the upload job record in the database.
- **Type:** `int`
- **Remarks:** Set by the persistence layer; read‑only after creation.

### VideoShortId
- **Purpose:** Foreign key referencing the `VideoShort` that this job uploads.
- **Type:** `int`
- **Remarks:** Must correspond to an existing `VideoShort` entry.

### YouTubeVideoId
- **Purpose:** The YouTube‑assigned video identifier after a successful upload.
- **Type:** `string`
- **Remarks:** Populated only when `Status` is `Completed`; otherwise `null` or empty.

### Status
- **Purpose:** Current state of the upload job.
- **Type:** `UploadStatus` (enum: `Queued`, `Uploading`, `Completed`, `Failed`, `Cancelled`)
- **Remarks:** Updated via the `MarkAs*` methods; invalid transitions may raise exceptions.

### ScheduledAt
- **Purpose:** Date and time when the job is eligible to start processing.
- **Type:** `DateTime`
- **Remarks:** Jobs with a `ScheduledAt` in the past are picked up immediately by the scheduler.

### UploadedAt
- **Purpose:** Timestamp of when the upload finished successfully.
- **Type:** `DateTime?`
- **Remarks:** `null` while the job is pending or failed; set only on completion.

### AttemptCount
- **Purpose:** Number of times the upload has been attempted.
- **Type:** `int`
- **Remarks:** Starts at 0 and is incremented by `IncrementAttempt`.

### MaxRetries
- **Purpose:** Upper limit on retry attempts before the job is considered permanently failed.
- **Type:** `int`
- **Remarks:** Typically configured from application settings; must be non‑negative.

### ErrorMessage
- **Purpose:** Description of the last error encountered during an upload attempt.
- **Type:** `string?`
- **Remarks:** Populated when `Status` is `Failed`; cleared on successful retry or completion.

### UploadedBytes
- **Purpose:** Number of bytes that have been transferred to YouTube so far.
- **Type:** `long`
- **Remarks:** Updated during the upload process; reset to 0 when a new attempt begins.

### UploadProgressPercentage
- **Purpose:** Progress of the current upload attempt expressed as a percentage (0‑100).
- **Type:** `double`
- **Remarks:** Should stay within the 0‑100 range; values outside indicate a bug.

### EstimatedTimeRemaining
- **Purpose:** Approximate time left until the current upload finishes.
- **Type:** `TimeSpan`
- **Remarks:** Set to `TimeSpan.Zero` when no upload is in progress or when progress cannot be estimated.

### CreatedAt
- **Purpose:** Moment when the upload job record was first created.
- **Type:** `DateTime`
- **Remarks:** Set automatically by the data access layer; never changes.

### UpdatedAt
- **Purpose:** Moment when the upload job record was last modified.
- **Type:** `DateTime`
- **Remarks:** Updated automatically by the ORM on each change.

### VideoShort
- **Purpose:** Navigation property to the related `VideoShort` entity.
- **Type:** `VideoShort?`
- **Remarks:** May be `null` if the association has not been loaded or the referenced short was deleted.

### CanRetry
- **Purpose:** Indicates whether another upload attempt is permissible.
- **Type:** `bool` (read‑only)
- **Logic:** Returns `true` when `AttemptCount < MaxRetries` and `Status` is not `Completed`; otherwise `false`.
- **Remarks:** Computed from other fields; no side effects.

### IncrementAttempt
- **Purpose:** Increases the attempt counter and prepares the job for a new try.
- **Signature:** `public void IncrementAttempt()`
- **Parameters:** None
- **Return:** `void`
- **Throws:** 
  - `InvalidOperationException` if `AttemptCount` already equals `MaxRetries` (no retries left) or if the job is already in a terminal state (`Completed` or `Cancelled`).

### MarkAsQueued
- **Purpose:** Transitions the job to the queued state, resetting progress indicators.
- **Signature:** `public void MarkAsQueued()`
- **Parameters:** None
- **Return:** `void`
- **Throws:** 
  - `InvalidOperationException` if the job is currently `Completed` or `Cancelled`.

### MarkAsUploading
- **Purpose:** Signals that the upload process has started.
- **Signature:** `public void MarkAsUploading()`
- **Parameters:** None
- **Return:** `void`
- **Throws:** 
  - `InvalidOperationException` if the job is not in `Queued` state.

### MarkAsCompleted
- **Purpose:** Records a successful upload, storing the YouTube video ID and completion timestamp.
- **Signature:** `public void MarkAsCompleted(string youTubeVideoId)`
- **Parameters:** 
  - `youTubeVideoId`: The video ID returned by YouTube upon successful upload.
- **Return:** `void`
- **Throws:** 
  - `ArgumentNullException` if `youTubeVideoId` is `null` or empty.
  - `InvalidOperationException` if the job is not in `Uploading` state.

## Usage

```csharp
// Example 1: Creating a new upload job and preparing for a retry
var job = new UploadJob
{
    VideoShortId = 42,
    ScheduledAt = DateTime.UtcNow.AddMinutes(5),
    MaxRetries = 3
};

job.MarkAsQueued();          // Ready to be picked up by the worker
job.IncrementAttempt();      // First attempt will be counted as 1
bool can = job.CanRetry;     // true (1 < 3)
```

```csharp
// Example 2: Processing an upload attempt and handling outcomes
try
{
    job.MarkAsUploading();
    // … perform upload, updating UploadedBytes and UploadProgressPercentage …
    job.MarkAsCompleted(youTubeVideoId: "abc123XYZ");
}
catch (UploadException ex)
{
    job.ErrorMessage = ex.Message;
    // Status remains Uploading; worker will later call MarkAsFailed or retry
    if (job.CanRetry)
    {
        job.IncrementAttempt();
        job.MarkAsQueued();   // Schedule another try
    }
}
```

## Notes

- The type does **not** provide any internal synchronization; concurrent access from multiple threads can lead to race conditions on fields such as `AttemptCount`, `UploadedBytes`, and `Status`. Callers must enforce external locking or use a single‑threaded processing model (e.g., a dedicated background worker per job).
- `UploadProgressPercentage` and `EstimatedTimeRemaining` are intended for UI feedback; setting them outside of a valid upload sequence may produce misleading values.
- `CanRetry` is a computed property; it does **not** modify state. Relying on it for flow control is safe as long as the underlying fields are not changed concurrently.
- `ErrorMessage` should be cleared or overwritten only when transitioning out of the `Failed` state; leaving stale error text after a successful retry can confuse diagnostics.
- The `VideoShort` navigation property may be `null` if the related entity has been detached or deleted; code that accesses its members should check for null to avoid `NullReferenceException`.
- `ScheduledAt` values in the past cause the scheduler to treat the job as immediately eligible; there is no automatic adjustment of the timestamp.
- The `IncrementAttempt` method does **not** reset progress counters; callers should manually zero `UploadedBytes`, `UploadProgressPercentage`, and `EstimatedTimeRemaining` before starting a new attempt if such a reset is desired.
