# VideoShort

The `VideoShort` class serves as the primary domain entity representing a single short-form video asset within the `youtube-shorts-automator` pipeline. It encapsulates all metadata required for processing, including file system locations, technical specifications like duration and quality, and relational data linking the video to specific processing profiles and YouTube channels. Additionally, it tracks the lifecycle state of the video through status flags, error reporting, and timestamps, while maintaining collections of associated upload jobs and processing tasks for orchestration purposes.

## API

The following members constitute the public interface of the `VideoShort` class:

### `Id`
*   **Type**: `int`
*   **Purpose**: Unique identifier for the video short record within the database.
*   **Parameters**: None.
*   **Return Value**: The integer primary key.
*   **Throws**: None.

### `Title`
*   **Type**: `string`
*   **Purpose**: Stores the display title of the video, used for metadata injection and YouTube upload titles.
*   **Parameters**: None.
*   **Return Value**: The video title string.
*   **Throws**: None.

### `Description`
*   **Type**: `string`
*   **Purpose**: Contains the full description text intended for the video metadata or YouTube description field.
*   **Parameters**: None.
*   **Return Value**: The description string.
*   **Throws**: None.

### `FilePath`
*   **Type**: `string`
*   **Purpose**: Absolute or relative path to the source video file on the local file system.
*   **Parameters**: None.
*   **Return Value**: The file path string.
*   **Throws**: None.

### `ThumbnailPath`
*   **Type**: `string`
*   **Purpose**: Path to the associated thumbnail image file used during the upload process.
*   **Parameters**: None.
*   **Return Value**: The thumbnail file path string.
*   **Throws**: None.

### `Duration`
*   **Type**: `TimeSpan`
*   **Purpose**: Represents the total playback duration of the video. Used to validate compliance with YouTube Shorts limits (typically under 60 seconds).
*   **Parameters**: None.
*   **Return Value**: The duration time span.
*   **Throws**: None.

### `FileSizeBytes`
*   **Type**: `long`
*   **Purpose**: Stores the size of the video file in bytes, utilized for storage quota checks and upload estimation.
*   **Parameters**: None.
*   **Return Value**: The file size as a 64-bit integer.
*   **Throws**: None.

### `Quality`
*   **Type**: `VideoQuality`
*   **Purpose**: Enumerated value indicating the resolution or quality tier of the video (e.g., 1080p, 4K).
*   **Parameters**: None.
*   **Return Value**: The `VideoQuality` enum value.
*   **Throws**: None.

### `Status`
*   **Type**: `ProcessingStatus`
*   **Purpose**: Indicates the current state of the video in the automation pipeline (e.g., Pending, Processing, Completed, Failed).
*   **Parameters**: None.
*   **Return Value**: The `ProcessingStatus` enum value.
*   **Throws**: None.

### `Tags`
*   **Type**: `string[]`
*   **Purpose**: Array of keyword tags associated with the video for categorization and search optimization.
*   **Parameters**: None.
*   **Return Value**: An array of tag strings.
*   **Throws**: None.

### `ProcessingProfileId`
*   **Type**: `int`
*   **Purpose**: Foreign key referencing the `ProcessingProfile` that defines the transformation rules applied to this video.
*   **Parameters**: None.
*   **Return Value**: The integer ID of the linked profile.
*   **Throws**: None.

### `YouTubeChannelId`
*   **Type**: `int`
*   **Purpose**: Foreign key referencing the target `YouTubeChannel` where this video is scheduled for upload.
*   **Parameters**: None.
*   **Return Value**: The integer ID of the linked channel.
*   **Throws**: None.

### `ErrorMessage`
*   **Type**: `string?`
*   **Purpose**: Contains the error message details if the `Status` indicates a failure; otherwise null.
*   **Parameters**: None.
*   **Return Value**: The error message string or null.
*   **Throws**: None.

### `CreatedAt`
*   **Type**: `DateTime`
*   **Purpose**: Timestamp marking when the `VideoShort` record was initially created in the system.
*   **Parameters**: None.
*   **Return Value**: The creation date and time.
*   **Throws**: None.

### `UpdatedAt`
*   **Type**: `DateTime`
*   **Purpose**: Timestamp marking the last modification to the `VideoShort` record.
*   **Parameters**: None.
*   **Return Value**: The last update date and time.
*   **Throws**: None.

### `ProcessedAt`
*   **Type**: `DateTime?`
*   **Purpose**: Optional timestamp indicating when the video processing pipeline successfully completed. Null if processing is pending or failed.
*   **Parameters**: None.
*   **Return Value**: The completion date and time, or null.
*   **Throws**: None.

### `ProcessingProfile`
*   **Type**: `ProcessingProfile?`
*   **Purpose**: Navigation property providing access to the related `ProcessingProfile` entity.
*   **Parameters**: None.
*   **Return Value**: The related `ProcessingProfile` object or null if not loaded.
*   **Throws**: None.

### `YouTubeChannel`
*   **Type**: `YouTubeChannel?`
*   **Purpose**: Navigation property providing access to the related `YouTubeChannel` entity.
*   **Parameters**: None.
*   **Return Value**: The related `YouTubeChannel` object or null if not loaded.
*   **Throws**: None.

### `UploadJobs`
*   **Type**: `ICollection<UploadJob>`
*   **Purpose**: Collection of `UploadJob` entities associated with this video, tracking individual upload attempts or schedules.
*   **Parameters**: None.
*   **Return Value**: A collection of `UploadJob` objects.
*   **Throws**: None.

### `ProcessingTasks`
*   **Type**: `ICollection<ProcessingTask>`
*   **Purpose**: Collection of `ProcessingTask` entities representing specific steps executed against this video during the processing phase.
*   **Parameters**: None.
*   **Return Value**: A collection of `ProcessingTask` objects.
*   **Throws**: None.

## Usage

### Example 1: Inspecting Video Metadata and Status
This example demonstrates how to retrieve a video short and validate its readiness for upload by checking its duration and processing status.

```csharp
public void ValidateShortForUpload(VideoShort videoShort)
{
    if (videoShort.Status != ProcessingStatus.Completed)
    {
        Console.WriteLine($"Video '{videoShort.Title}' is not ready. Current status: {videoShort.Status}");
        if (!string.IsNullOrEmpty(videoShort.ErrorMessage))
        {
            Console.WriteLine($"Error details: {videoShort.ErrorMessage}");
        }
        return;
    }

    if (videoShort.Duration.TotalSeconds > 60)
    {
        Console.WriteLine($"Warning: Video '{videoShort.Title}' exceeds 60 seconds ({videoShort.Duration.TotalSeconds}s).");
    }

    Console.WriteLine($"Video ready: {videoShort.FilePath} ({videoShort.FileSizeBytes} bytes)");
    Console.WriteLine($"Target Channel ID: {videoShort.YouTubeChannelId}");
}
```

### Example 2: Accessing Related Entities and Tasks
This example illustrates navigating the relationships to access the processing profile settings and iterating over historical processing tasks.

```csharp
public void AuditProcessingHistory(VideoShort videoShort)
{
    if (videoShort.ProcessingProfile == null)
    {
        throw new InvalidOperationException("Processing profile data not loaded.");
    }

    Console.WriteLine($"Video: {videoShort.Title}");
    Console.WriteLine($"Applied Profile: {videoShort.ProcessingProfile.Name}");
    Console.WriteLine($"Quality Target: {videoShort.Quality}");

    foreach (var task in videoShort.ProcessingTasks)
    {
        Console.WriteLine($"- Task: {task.TaskType} | Status: {task.Status} | Completed: {task.CompletedAt}");
    }

    if (videoShort.UploadJobs.Any())
    {
        var latestJob = videoShort.UploadJobs.OrderByDescending(j => j.CreatedAt).First();
        Console.WriteLine($"Latest Upload Job ID: {latestJob.Id}");
    }
}
```

## Notes

*   **Nullability**: Navigation properties (`ProcessingProfile`, `YouTubeChannel`) and the `ProcessedAt` timestamp are nullable. Consumers must ensure these properties are explicitly loaded (e.g., via eager loading in an ORM context) before accessing members of the related objects to avoid `NullReferenceException`.
*   **Collection Initialization**: The `UploadJobs` and `ProcessingTasks` collections are exposed as interfaces (`ICollection<T>`). While typically initialized by the ORM or constructor, callers should verify the collection is not null before adding items if operating outside of a tracked context.
*   **Thread Safety**: This class is a Plain Old CLR Object (POCO) and does not implement internal locking mechanisms. It is not thread-safe for concurrent writes. If multiple threads need to update properties such as `Status`, `ErrorMessage`, or `UpdatedAt`, external synchronization or atomic update operations via the repository layer are required.
*   **File System Dependencies**: The `FilePath` and `ThumbnailPath` properties contain string paths only. The class does not validate the existence of these files on disk; callers must perform file system checks before attempting I/O operations using these paths.
*   **Data Integrity**: The `Duration` and `FileSizeBytes` properties are expected to be populated during the initial ingestion or scanning phase. Relying on these values without prior validation may lead to incorrect logic in upload throttling or format compliance checks.
