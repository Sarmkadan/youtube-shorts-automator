# ProcessingJobRepository

The `ProcessingJobRepository` class provides data access operations for managing and querying `ProcessingJob` entities within the youtube-shorts-automator system. It abstracts persistence logic for job lifecycle management, enabling retrieval of jobs by various criteria including video association, status, type, and pagination parameters.

## API

### ProcessingJobRepository()

Initializes a new instance of the `ProcessingJobRepository` class.

---

### GetByVideoIdAsync(string videoId)

Retrieves all processing jobs associated with a specific video identifier.

**Parameters**
- `videoId` (string): The unique identifier of the video to filter jobs by.

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of jobs linked to the specified video.

**Exceptions**
- `ArgumentException`: Thrown when `videoId` is null or empty.
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetByStatusAsync(JobStatus status)

Fetches all processing jobs matching a specified job status.

**Parameters**
- `status` (JobStatus): The status value to filter jobs by (e.g., Pending, Completed, Failed).

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of jobs with the specified status.

**Exceptions**
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetPendingJobsAsync()

Retrieves all processing jobs currently in a pending state.

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of pending jobs.

**Exceptions**
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetFailedJobsAsync()

Fetches all processing jobs marked as failed.

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of failed jobs.

**Exceptions**
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetJobsForRetryAsync(int maxRetryCount)

Retrieves jobs eligible for retry based on their retry count.

**Parameters**
- `maxRetryCount` (int): The maximum number of retry attempts allowed for a job to be considered retryable.

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of retryable jobs.

**Exceptions**
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetLatestJobForVideoAsync(string videoId)

Obtains the most recent processing job for a specific video.

**Parameters**
- `videoId` (string): The unique identifier of the video to retrieve the latest job for.

**Returns**
- `Task<ProcessingJob?>`: A task representing the asynchronous operation, containing the latest job or null if none exists.

**Exceptions**
- `ArgumentException`: Thrown when `videoId` is null or empty.
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetJobsByTypeAsync(JobType type)

Fetches all processing jobs of a specified type.

**Parameters**
- `type` (JobType): The job type to filter by (e.g., VideoProcessing, ThumbnailGeneration).

**Returns**
- `Task<List<ProcessingJob>>`: A task representing the asynchronous operation, containing a list of jobs of the specified type.

**Exceptions**
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

### GetPaginatedAsync(int page, int pageSize)

Retrieves a paginated subset of processing jobs along with the total count.

**Parameters**
- `page` (int): The 1-based page index to retrieve.
- `pageSize` (int): The number of jobs per page.

**Returns**
- `Task<(List<ProcessingJob> Jobs, int Total)>`: A task representing the asynchronous operation, containing the list of jobs for the requested page and the total number of jobs across all pages.

**Exceptions**
- `ArgumentException`: Thrown when `page` or `pageSize` are less than 1.
- `DatabaseException`: Thrown when database connectivity or query execution fails.

---

## Usage

```csharp
// Retrieve all pending jobs for processing
var repository = new ProcessingJobRepository();
List<ProcessingJob> pendingJobs = await repository.GetPendingJobsAsync();

foreach (var job in pendingJobs)
{
    Console.WriteLine($"Processing job {job.Id} for video {job.VideoId}");
}
```

```csharp
// Fetch first page of failed jobs with 10 items per page
var repository = new ProcessingJobRepository();
var (jobs, total) = await repository.GetPaginatedAsync(1, 10);

Console.WriteLine($"Retrieved {jobs.Count} jobs out of {total} total failed jobs.");
```

---

## Notes

- All methods perform asynchronous I/O operations and should be awaited to prevent blocking.
- Methods returning `List<ProcessingJob>` may return empty lists if no matching records exist; they do not throw exceptions for missing data.
- `GetLatestJobForVideoAsync` returns null when no jobs are found for the specified video ID.
- Pagination parameters in `GetPaginatedAsync` use 1-based indexing; passing 0 or negative values will result in `ArgumentException`.
- Thread safety depends on the underlying database context implementation; concurrent calls to the same instance may require external synchronization.
- Custom `DatabaseException` is thrown for infrastructure-related failures, allowing centralized error handling strategies.
