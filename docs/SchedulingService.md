# SchedulingService
The `SchedulingService` class is designed to manage the scheduling of upload jobs, providing methods to schedule, reschedule, and cancel uploads, as well as retrieve information about upcoming and overdue jobs. It also offers functionality to calculate the optimal upload time and determine if a given time falls within the optimal upload window.

## API
### Constructors
* `public SchedulingService`: Initializes a new instance of the `SchedulingService` class.

### Methods
* `public async Task<UploadJob> ScheduleUploadAsync`: Schedules an upload job. Returns the scheduled `UploadJob`. Throws if the scheduling process fails.
* `public async Task<IEnumerable<UploadJob>> GetUpcomingJobsAsync`: Retrieves a list of upcoming upload jobs. Returns a collection of `UploadJob` objects. Throws if the retrieval process fails.
* `public async Task<IEnumerable<UploadJob>> GetOverdueJobsAsync`: Retrieves a list of overdue upload jobs. Returns a collection of `UploadJob` objects. Throws if the retrieval process fails.
* `public async Task<bool> RescheduleUploadAsync`: Reschedules an upload job. Returns `true` if the rescheduling is successful, `false` otherwise. Throws if the rescheduling process fails.
* `public async Task<bool> CancelUploadAsync`: Cancels an upload job. Returns `true` if the cancellation is successful, `false` otherwise. Throws if the cancellation process fails.
* `public async Task<int> GetQueuedJobCountAsync`: Retrieves the number of jobs currently queued for upload. Returns the count of queued jobs. Throws if the retrieval process fails.
* `public TimeSpan CalculateOptimalUploadTime`: Calculates the optimal time for uploading a job. Returns a `TimeSpan` representing the optimal upload time.
* `public bool IsWithinOptimalUploadWindow`: Determines if a given time falls within the optimal upload window. Returns `true` if the time is within the window, `false` otherwise.

## Usage
The following examples demonstrate how to use the `SchedulingService` class:
```csharp
// Example 1: Scheduling an upload job
var schedulingService = new SchedulingService();
var uploadJob = await schedulingService.ScheduleUploadAsync();
Console.WriteLine($"Upload job scheduled: {uploadJob.Id}");

// Example 2: Retrieving and rescheduling overdue jobs
var schedulingService = new SchedulingService();
var overdueJobs = await schedulingService.GetOverdueJobsAsync();
foreach (var job in overdueJobs)
{
    await schedulingService.RescheduleUploadAsync();
    Console.WriteLine($"Rescheduled job: {job.Id}");
}
```

## Notes
* The `SchedulingService` class is designed to be thread-safe, allowing multiple threads to access its methods concurrently without fear of data corruption or other threading issues.
* When using the `RescheduleUploadAsync` and `CancelUploadAsync` methods, be aware that they may throw exceptions if the job is not found or if the rescheduling/cancellation process fails.
* The `CalculateOptimalUploadTime` method returns a `TimeSpan` representing the optimal upload time, which can be used to schedule uploads at the most efficient time.
* The `IsWithinOptimalUploadWindow` method can be used to determine if a given time falls within the optimal upload window, allowing for more efficient scheduling of uploads.
