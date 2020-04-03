# YouTubeUploadService

Provides functionality to upload videos to YouTube, manage upload jobs, and update video metadata. Handles asynchronous upload operations, retries for failed uploads, and job scheduling for automated video publishing workflows.

## API

### `YouTubeUploadService`
Initializes a new instance of the `YouTubeUploadService` with required dependencies for YouTube API interactions and job persistence.

### `async Task<UploadJob> CreateUploadJobAsync`
Creates a new upload job with the specified video file and metadata. The job is persisted but not yet scheduled for upload.

- **Parameters**:
  - `videoFilePath` (string): Absolute path to the video file to upload.
  - `title` (string): Title of the YouTube video.
  - `description` (string): Description of the YouTube video.
  - `tags` (IEnumerable<string>): List of tags for the video.
  - `privacyStatus` (string): YouTube privacy status (e.g., "public", "private", "unlisted").
  - `scheduledPublishTime` (DateTimeOffset?, optional): Scheduled publish time; if null, uploads immediately.
- **Returns**: An `UploadJob` representing the created job.
- **Throws**:
  - `ArgumentException`: If `videoFilePath` is null or empty, or if the file does not exist.
  - `InvalidOperationException`: If the job cannot be created due to invalid metadata or API constraints.

### `async Task<UploadJob> UploadVideoAsync`
Uploads a video file to YouTube using an existing upload job. The job must be in a valid state for upload.

- **Parameters**:
  - `jobId` (Guid): Unique identifier of the upload job.
- **Returns**: The updated `UploadJob` with upload status and YouTube video ID if successful.
- **Throws**:
  - `ArgumentException`: If `jobId` is empty.
  - `InvalidOperationException`: If the job is not in a valid state for upload.
  - `YouTubeApiException`: If the YouTube API request fails.

### `async Task<bool> RetryFailedUploadAsync`
Attempts to retry a previously failed upload job. Only jobs with a failed upload status are eligible.

- **Parameters**:
  - `jobId` (Guid): Unique identifier of the failed upload job.
- **Returns**: `true` if the retry was initiated successfully; otherwise, `false`.
- **Throws**:
  - `ArgumentException`: If `jobId` is empty.
  - `InvalidOperationException`: If the job is not in a failed state or is otherwise ineligible for retry.

### `async Task<bool> UpdateVideoMetadataAsync`
Updates metadata (title, description, tags, privacy status) of an existing YouTube video associated with an upload job.

- **Parameters**:
  - `jobId` (Guid): Unique identifier of the upload job.
  - `title` (string): New title for the video.
  - `description` (string): New description for the video.
  - `tags` (IEnumerable<string>): New list of tags for the video.
  - `privacyStatus` (string): New privacy status for the video.
- **Returns**: `true` if the metadata was updated successfully; otherwise, `false`.
- **Throws**:
  - `ArgumentException`: If `jobId` is empty or any metadata field is invalid.
  - `InvalidOperationException`: If the job has no associated YouTube video ID or is not in a publishable state.

### `async Task<bool> PublishVideoAsync`
Publishes a video that was previously uploaded but not yet made public. The video must be in a "processed" state on YouTube.

- **Parameters**:
  - `jobId` (Guid): Unique identifier of the upload job.
- **Returns**: `true` if the video was published successfully; otherwise, `false`.
- **Throws**:
  - `ArgumentException`: If `jobId` is empty.
  - `InvalidOperationException`: If the job is not in a state eligible for publishing.

### `async Task<UploadJob?> GetUploadJobAsync`
Retrieves an upload job by its unique identifier.

- **Parameters**:
  - `jobId` (Guid): Unique identifier of the upload job.
- **Returns**: The `UploadJob` if found; otherwise, `null`.
- **Throws**:
  - `ArgumentException`: If `jobId` is empty.

### `async Task<IEnumerable<UploadJob>> GetScheduledJobsAsync`
Retrieves all upload jobs that are scheduled for future upload or publication.

- **Parameters**: None.
- **Returns**: An enumerable of `UploadJob` objects representing scheduled jobs.
- **Throws**: None.

## Usage

### Example 1: Upload and publish a video immediately
