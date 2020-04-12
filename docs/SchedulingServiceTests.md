# SchedulingServiceTests

The `SchedulingServiceTests` class contains unit tests for the `SchedulingService` component of the `youtube-shorts-automator` project. It validates the scheduling, rescheduling, cancellation, and querying of upload jobs, covering both successful operations and expected failure conditions such as invalid times, non‑existent jobs, or jobs in incompatible states.

## API

### `public SchedulingServiceTests()`
Default constructor. No special initialization is required; test dependencies are typically injected via a test framework (e.g., xUnit’s `IClassFixture` or constructor injection).

### `public async Task ScheduleUploadAsync_WithFutureTime_CreatesScheduledJob`
Verifies that scheduling an upload with a future date/time successfully creates a new scheduled job and returns a valid job identifier.

### `public async Task ScheduleUploadAsync_WithPastTime_ThrowsSchedulingException`
Ensures that attempting to schedule an upload with a time in the past throws a `SchedulingException`.

### `public async Task ScheduleUploadAsync_WithNowTime_Succeeds`
Confirms that scheduling an upload for the current time (or a very near‑future time) completes without error.

### `public async Task ScheduleUploadAsync_SetsMaxRetriesCorrectly`
Checks that the newly created scheduled job has the maximum retry count set to the expected default or configured value.

### `public async Task GetUpcomingJobsAsync_WithPendingJobs_ReturnsScheduledJobs`
Tests that the service returns a collection of pending jobs when there are jobs scheduled in the future.

### `public async Task GetUpcomingJobsAsync_WithHoursAhead_FiltersByTimeWindow`
Validates that the `GetUpcomingJobsAsync` method correctly filters results to only include jobs whose scheduled time falls within a specified look‑ahead window (e.g., next 24 hours).

### `public async Task GetUpcomingJobsAsync_WithNoJobs_ReturnsEmpty`
Ensures that an empty collection is returned when no pending jobs exist.

### `public async Task GetUpcomingJobsAsync_ReturnsJobsOrderedByScheduledTime`
Confirms that the returned jobs are sorted in ascending order by their scheduled time.

### `public async Task GetOverdueJobsAsync_WithPastScheduledTime_ReturnsPendingJob`
Verifies that jobs with a scheduled time in the past and a status of `Pending` are returned as overdue.

### `public async Task GetOverdueJobsAsync_IgnoresNonPendingJobs`
Ensures that jobs in states other than `Pending` (e.g., `Completed`, `Uploading`, `Failed`) are excluded from the overdue results.

### `public async Task RescheduleUploadAsync_WithValidJob_UpdatesScheduledTime`
Tests that rescheduling an existing pending job to a new future time updates the job’s scheduled time and returns the updated job.

### `public async Task RescheduleUploadAsync_WithNonExistentJob_ThrowsSchedulingException`
Ensures that attempting to reschedule a job that does not exist throws a `SchedulingException`.

### `public async Task RescheduleUploadAsync_WithCompletedJob_ThrowsSchedulingException`
Verifies that rescheduling a job that has already been completed throws a `SchedulingException`.

### `public async Task RescheduleUploadAsync_WithUploadingJob_ThrowsSchedulingException`
Confirms that rescheduling a job that is currently being uploaded throws a `SchedulingException`.

### `public async Task RescheduleUploadAsync_WithPastTime_ThrowsSchedulingException`
Ensures that providing a past time to the reschedule operation throws a `SchedulingException`.

### `public async Task CancelUploadAsync_WithPendingJob_CancelsJob`
Tests that cancelling a pending job changes its status to `Cancelled` and returns the updated job.

### `public async Task CancelUploadAsync_WithNonExistentJob_ThrowsSchedulingException`
Verifies that cancelling a non‑existent job throws a `SchedulingException`.

### `public async Task CancelUploadAsync_WithUploadingJob_ThrowsSchedulingException`
Ensures that attempting to cancel a job that is currently uploading throws a `SchedulingException`.

### `public async Task GetQueuedJobCountAsync_ReturnsCountOfQueuedJobs`
Confirms that the method returns the correct number of jobs in the `Queued` state (i.e., jobs that are ready to be processed).

## Usage

The following examples demonstrate how the `SchedulingService` is used in the scenarios covered by the tests. These snippets assume the service has been properly instantiated with its dependencies (e.g., a repository and a clock).

### Example 1: Scheduling an upload and retrieving upcoming jobs

```csharp
// Arrange
var service = new SchedulingService(repository, clock);
var futureTime = DateTime.UtcNow.AddHours(2);
var uploadInfo = new UploadInfo { VideoPath = "/videos/short.mp4", Title = "My Short" };

// Act
var job = await service.ScheduleUploadAsync(uploadInfo, futureTime);

// Assert
Assert.NotNull(job);
Assert.Equal(JobStatus.Pending, job.Status);
Assert.Equal(futureTime, job.ScheduledTimeUtc);

// Retrieve upcoming jobs
var upcoming = await service.GetUpcomingJobsAsync(TimeSpan.FromHours(24));
Assert.Contains(job.Id, upcoming.Select(j => j.Id));
```

### Example 2: Rescheduling and cancelling a job

```csharp
// Arrange
var service = new SchedulingService(repository, clock);
var originalTime = DateTime.UtcNow.AddDays(1);
var job = await service.ScheduleUploadAsync(uploadInfo, originalTime);

// Act – reschedule to a later time
var newTime = DateTime.UtcNow.AddDays(2);
var rescheduled = await service.RescheduleUploadAsync(job.Id, newTime);
Assert.Equal(newTime, rescheduled.ScheduledTimeUtc);

// Act – cancel the job
var cancelled = await service.CancelUploadAsync(job.Id);
Assert.Equal(JobStatus.Cancelled, cancelled.Status);
```

## Notes

- **Edge cases**: All operations that accept a `DateTime` parameter reject past times (including `DateTime.MinValue` or times more than a few seconds in the past) by throwing `SchedulingException`. Jobs in non‑modifiable states (`Completed`, `Uploading`) cannot be rescheduled or cancelled. Non‑existent job IDs always result in a `SchedulingException`.
- **Thread safety**: The tests are designed to be run independently and can be executed in parallel if the underlying `SchedulingService` implementation is thread‑safe. The service itself is expected to handle concurrent access to its job store (e.g., via database transactions or in‑memory locks). No test‑specific synchronization is required.
- **Time precision**: Tests that involve “now” or “future” times use a configurable clock abstraction to avoid flakiness due to system clock drift. The `SchedulingService` should rely on an `IClock` or similar interface to obtain the current time.
- **State transitions**: The service enforces a strict state machine: only `Pending` jobs can be rescheduled or cancelled; `Queued` jobs are immutable until picked up by the upload worker; `Uploading` jobs are locked; `Completed` and `Failed` jobs are terminal.
