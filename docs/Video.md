# Video

The `Video` class is the primary domain entity representing a media asset within the `youtube-shorts-automator` system. It encapsulates all necessary metadata, filesystem references, processing status, and relationships to associated users, processing jobs, and analytics data required to manage the lifecycle of a video from ingestion to upload on YouTube.

## API

### Properties

*   **`Id` (Guid)**: The unique identifier for the video instance.
*   **`Title` (string)**: The title of the video asset.
*   **`Description` (string)**: The description content associated with the video.
*   **`FilePath` (string)**: The absolute filesystem path to the source video file.
*   **`Tags` (string[])**: A collection of tags for metadata and categorization.
*   **`ThumbnailPath` (string)**: The absolute filesystem path to the video's thumbnail image.
*   **`FileSizeBytes` (long)**: The size of the video file in bytes.
*   **`DurationSeconds` (int)**: The duration of the video in seconds.
*   **`Status` (VideoStatus)**: An enumeration representing the current lifecycle state of the video (e.g., Pending, Processing, Processed, Uploaded, Failed).
*   **`UserId` (Guid)**: The unique identifier of the user who owns this video.
*   **`User` (User?)**: An optional navigation property to the owner of the video.
*   **`CreatedAt` (DateTime)**: The timestamp indicating when the video record was created in the system.
*   **`ProcessedAt` (DateTime?)**: The timestamp indicating when the video processing was successfully completed.
*   **`YouTubeVideoId` (string?)**: The identifier assigned to the video by YouTube upon successful upload.
*   **`ProcessingJobs` (List<ProcessingJob>)**: A list of associated processing jobs performed on this video.
*   **`Metrics` (List<AnalyticsMetric>)**: A list of analytics metrics recorded for this video.
*   **`UploadResult` (UploadResult?)**: The result object containing details of the upload attempt, if one has been made.

### Methods

*   **`Validate()`**
    *   **Returns**: `(bool IsValid, List<string> Errors)` - A tuple containing a boolean indicating if the video entity is in a valid state, and a list of error messages if validation fails.
*   **`MarkAsProcessed()`**
    *   **Purpose**: Transitions the video status to a processed state and updates `ProcessedAt` to the current system time.
*   **`MarkAsUploaded()`**
    *   **Purpose**: Transitions the video status to an uploaded state.

## Usage

### Validating a Video Entity
```csharp
var video = new Video { Title = "", FilePath = "path/to/file.mp4" };
var (isValid, errors) = video.Validate();

if (!isValid)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}
```

### Updating Status after Processing
```csharp
// Assuming video instance is retrieved from a repository
if (processingTask.IsCompletedSuccessfully)
{
    video.MarkAsProcessed();
    // Persist changes to database
    await context.SaveChangesAsync();
}
```

## Notes

*   **Thread Safety**: The `Video` entity is not inherently thread-safe. Instances should be managed within the scope of a single unit of work (e.g., a database context transaction) to prevent race conditions during state transitions.
*   **State Transitions**: `YouTubeVideoId` and `UploadResult` are only populated after successful interaction with the YouTube API. `ProcessedAt` is intended to be set only upon the successful conclusion of all required processing jobs.
*   **Validation**: The `Validate` method should be called before attempting to persist a new video or initiate a processing job to ensure all required metadata, such as `FilePath` and `Title`, are present and conform to system constraints.
