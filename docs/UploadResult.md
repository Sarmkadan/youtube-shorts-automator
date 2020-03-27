# UploadResult

`UploadResult` encapsulates the outcome of a single video upload operation within the YouTube Shorts Automator pipeline. It stores identifiers, transfer statistics, timing information, and a reference to the parent `UploadJob`, allowing callers to inspect success status, retrieve formatted metrics, and update the result after the upload completes.

## API

### Id
- **Purpose:** Unique identifier for this upload result record.
- **Type:** `int`
- **Access:** Public get/set.
- **Remarks:** Typically assigned by the persistence layer when the result is saved.

### UploadJobId
- **Purpose:** Foreign key linking the result to its parent upload job.
- **Type:** `int`
- **Access:** Public get/set.
- **Remarks:** Set when the result is created; unchanged thereafter.

### VideoId
- **Purpose:** The YouTube video identifier assigned after a successful upload.
- **Type:** `string`
- **Access:** Public get/set.
- **Remarks:** Populated only when `IsSuccessful` is `true`; otherwise empty or `null`.

### VideoUrl
- **Purpose:** Direct URL to the uploaded video on YouTube.
- **Type:** `string`
- **Access:** Public get/set.
- **Remarks:** Valid only after a successful upload; may be empty or `null` on failure.

### IsSuccessful
- **Purpose:** Indicates whether the upload completed without error.
- **Type:** `bool`
- **Access:** Public get/set.
- **Remarks:** Defaults to `false`; flipped to `true` by `MarkAsSuccessful` or to `false` by `MarkAsFailed`.

### ErrorDetails
- **Purpose:** Holds diagnostic information when the upload fails.
- **Type:** `string?`
- **Access:** Public get/set.
- **Remarks:** `null` when `IsSuccessful` is `true`; contains exception messages or validation errors otherwise.

### UploadedBytes
- **Purpose:** Number of bytes successfully transferred so far.
- **Type:** `long`
- **Access:** Public get/set.
- **Remarks:** Updated incrementally during the upload; never exceeds `TotalBytes`.

### TotalBytes
- **Purpose:** Total size of the video file being uploaded.
- **Type:** `long`
- **Access:** Public get/set.
- **Remarks:** Set before the upload begins; remains constant for the lifetime of the instance.

### UploadDuration
- **Purpose:** Elapsed time for the upload operation.
- **Type:** `TimeSpan`
- **Access:** Public get/set.
- **Remarks:** Measured from start to completion; zero until the upload finishes.

### AverageUploadSpeed
- **Purpose:** Average transfer rate in bytes per second.
- **Type:** `double`
- **Access:** Public get/set.
- **Remarks:** Calculated as `UploadedBytes / UploadDuration.TotalSeconds`; zero when duration is zero.

### CompletedAt
- **Purpose:** Timestamp marking when the upload finished (success or failure).
- **Type:** `DateTime`
- **Access:** Public get/set.
- **Remarks:** Set by `MarkAsSuccessful` or `MarkAsFailed`; otherwise retains its default value.

### UploadJob
- **Purpose:** Navigation property to the parent upload job.
- **Type:** `UploadJob?`
- **Access:** Public get/set.
- **Remarks:** May be `null` if the result is detached or the job was not loaded.

### MarkAsSuccessful
- **Purpose:** Finalizes the result as a successful upload.
- **Parameters:** None
- **Return:** `void`
- **Throws:** `InvalidOperationException` if the result has already been marked (either successful or failed) to prevent state corruption.

### MarkAsFailed
- **Purpose:** Finalizes the result as a failed upload and records error information.
- **Parameters:** None
- **Return:** `void`
- **Throws:** `InvalidOperationException` if the result has already been marked; callers should supply `ErrorDetails` before invoking this method.

### GetUploadSpeedFormatted
- **Purpose:** Returns a human‑readable string representing the average upload speed.
- **Parameters:** None
- **Return:** `string` (e.g., `"12.5 MB/s"`)
- **Throws:** None; returns `"0 B/s"` when `AverageUploadSpeed` is zero.

### GetDurationFormatted
- **Purpose:** Returns a human‑readable string representing the upload duration.
- **Parameters:** None
- **Return:** `string` (e.g., `"00:02:34"`)
- **Throws:** None; returns `"00:00:00"` when `UploadDuration` is zero.

## Usage

```csharp
// Example 1: Inspecting a successful upload
UploadResult result = await uploadService.UploadAsync(videoFile);
if (result.IsSuccessful)
{
    Console.WriteLine($"Uploaded video {result.VideoId}");
    Console.WriteLine($"Speed: {result.GetUploadSpeedFormatted()}");
    Console.WriteLine($"Duration: {result.GetDurationFormatted()}");
}
else
{
    Console.WriteLine($"Upload failed: {result.ErrorDetails}");
}
```

```csharp
// Example 2: Manually marking a failure after catching an exception
try
{
    // ... upload logic ...
}
catch (Exception ex)
{
    var result = new UploadResult
    {
        UploadJobId = job.Id,
        ErrorDetails = ex.ToString()
    };
    result.MarkAsFailed(); // sets IsSuccessful call after populating ErrorDetails
    // persist or report result
}
```

## Notes

- `ErrorDetails` should be set **before** calling `MarkAsFailed`; leaving it `null` while marking as failed may obscure the root cause.
- `UploadedBytes` must never exceed `TotalBytes`; violating this invariant indicates a bug in the progress reporting logic.
- `AverageUploadSpeed` is derived from `UploadedBytes` and `UploadDuration`; if `UploadDuration` is zero the speed is reported as zero to avoid division‑by‑zero.
- `CompletedAt` is only populated by the `MarkAsSuccessful`/`MarkAsFailed` methods; reading it prior to those calls yields the default `DateTime` value.
- The `UploadJob` navigation property may be `null` if the result is retrieved without eager‑loading the parent job; accessing its members without a null check will throw a `NullReferenceException`.
- The type contains no synchronization primitives; concurrent calls to `MarkAsSuccessful` or `MarkAsFailed` from multiple threads can lead to race conditions and inconsistent state. External synchronization is required when sharing an instance across threads.
- All mutable properties are safe to read after the object has been fully initialized; however, modifying them after the upload has been marked as complete may produce misleading results and is discouraged.
