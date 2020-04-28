# SchedulingServiceExtensions

Provides asynchronous extension methods for scheduling, querying, and managing upload jobs within the YouTube Shorts Automator pipeline. These methods operate on collections of `UploadJob` instances, enabling bulk operations such as scheduling, rescheduling, cancellation, and status checks against the underlying job orchestration service.

## API

### ScheduleUploadsAsync
```csharp
public static async Task<IEnumerable<UploadJob>> ScheduleUploadsAsync(
    this IEnumerable<UploadJob> jobs,
    DateTime scheduledTime,
    CancellationToken cancellationToken = default)
```
Schedules a batch of upload jobs for execution at the specified time. Each job is submitted to the orchestration service and returned with its updated scheduling metadata.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to schedule.
- `scheduledTime` — The `DateTime` at which the jobs should be executed.
- `cancellationToken` — Optional cancellation token.

**Returns:** The collection of `UploadJob` instances with their scheduling state updated.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `InvalidOperationException` if any job in the collection is already scheduled and cannot be rescheduled through this method.
- `TaskCanceledException` if the operation is canceled.

---

### GetUpcomingJobsAsync
```csharp
public static async Task<IEnumerable<UploadJob>> GetUpcomingJobsAsync(
    this IEnumerable<UploadJob> jobs,
    CancellationToken cancellationToken = default)
```
Filters the provided collection to return only jobs that are scheduled for future execution and have not yet been processed.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to filter.
- `cancellationToken` — Optional cancellation token.

**Returns:** An `IEnumerable<UploadJob>` containing only upcoming jobs.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

---

### GetOverdueJobsAsync
```csharp
public static async Task<IEnumerable<UploadJob>> GetOverdueJobsAsync(
    this IEnumerable<UploadJob> jobs,
    CancellationToken cancellationToken = default)
```
Filters the provided collection to return only jobs whose scheduled execution time has passed without completion.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to filter.
- `cancellationToken` — Optional cancellation token.

**Returns:** An `IEnumerable<UploadJob>` containing overdue jobs.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

---

### RescheduleUploadsAsync
```csharp
public static async Task<IEnumerable<bool>> RescheduleUploadsAsync(
    this IEnumerable<UploadJob> jobs,
    DateTime newScheduledTime,
    CancellationToken cancellationToken = default)
```
Attempts to reschedule each job in the collection to a new execution time. Returns a collection of boolean results indicating success or failure for each individual job.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to reschedule.
- `newScheduledTime` — The new `DateTime` for execution.
- `cancellationToken` — Optional cancellation token.

**Returns:** An `IEnumerable<bool>` where each element corresponds to a job in the input collection; `true` indicates the reschedule succeeded, `false` indicates it failed.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

---

### CancelUploadsAsync
```csharp
public static async Task<IEnumerable<bool>> CancelUploadsAsync(
    this IEnumerable<UploadJob> jobs,
    CancellationToken cancellationToken = default)
```
Attempts to cancel each job in the collection. Returns a collection of boolean results indicating success or failure for each individual cancellation.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to cancel.
- `cancellationToken` — Optional cancellation token.

**Returns:** An `IEnumerable<bool>` where each element corresponds to a job in the input collection; `true` indicates the cancellation succeeded, `false` indicates it failed.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

---

### HasOverdueJobsAsync
```csharp
public static async Task<bool> HasOverdueJobsAsync(
    this IEnumerable<UploadJob> jobs,
    CancellationToken cancellationToken = default)
```
Determines whether any job in the collection has passed its scheduled execution time without being processed.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to check.
- `cancellationToken` — Optional cancellation token.

**Returns:** `true` if at least one job is overdue; otherwise `false`.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

---

### GetUpcomingJobsCountAsync
```csharp
public static async Task<int> GetUpcomingJobsCountAsync(
    this IEnumerable<UploadJob> jobs,
    CancellationToken cancellationToken = default)
```
Counts the number of jobs in the collection that are scheduled for future execution.

**Parameters:**
- `jobs` — The collection of `UploadJob` instances to evaluate.
- `cancellationToken` — Optional cancellation token.

**Returns:** The integer count of upcoming jobs.

**Throws:**
- `ArgumentNullException` if `jobs` is null.
- `TaskCanceledException` if the operation is canceled.

## Usage

### Example 1: Scheduling a Batch and Checking for Overdue Items
```csharp
var pendingJobs = await jobRepository.GetPendingJobsAsync();
var scheduledJobs = await pendingJobs.ScheduleUploadsAsync(
    DateTime.UtcNow.AddHours(2));

bool hasOverdue = await scheduledJobs.HasOverdueJobsAsync();
if (hasOverdue)
{
    var overdue = await scheduledJobs.GetOverdueJobsAsync();
    await overdue.RescheduleUploadsAsync(DateTime.UtcNow.AddMinutes(30));
}
```

### Example 2: Canceling Upcoming Jobs Based on Count Threshold
```csharp
var allJobs = await jobRepository.GetAllJobsAsync();
int upcomingCount = await allJobs.GetUpcomingJobsCountAsync();

if (upcomingCount > 10)
{
    var upcomingJobs = await allJobs.GetUpcomingJobsAsync();
    var jobsToCancel = upcomingJobs.OrderBy(j => j.ScheduledTime).Take(upcomingCount - 10);
    var cancelResults = await jobsToCancel.CancelUploadsAsync();

    int failedCancellations = cancelResults.Count(r => !r);
    logger.LogWarning("{Count} jobs failed to cancel", failedCancellations);
}
```

## Notes

- All methods are extension methods on `IEnumerable<UploadJob>` and require the `SchedulingServiceExtensions` namespace to be imported.
- The `RescheduleUploadsAsync` and `CancelUploadsAsync` methods return per-job boolean results rather than throwing aggregate exceptions. Callers should inspect the returned collection to determine which individual operations failed.
- These methods are not atomic across the entire collection; each job is processed sequentially or concurrently at the discretion of the underlying service. Partial failure is possible, and the state of jobs after a partially failed operation is implementation-defined by the orchestration layer.
- Thread safety depends on the thread safety guarantees of the underlying `IEnumerable<UploadJob>` source and the job orchestration service. These extension methods do not introduce additional synchronization.
- Passing an empty collection to any method is valid and will return an empty collection or zero/false as appropriate.
- All methods accept an optional `CancellationToken`. If cancellation is requested mid-operation, a `TaskCanceledException` is thrown, and the state of partially processed jobs is not rolled back.
