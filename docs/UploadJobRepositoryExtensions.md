# UploadJobRepositoryExtensions

The `UploadJobRepositoryExtensions` class provides a set of static asynchronous extension methods designed to simplify common query and batch operations against the `IUploadJobRepository`. By encapsulating complex filtering logic, counting strategies, and bulk update/delete patterns, this utility reduces boilerplate code in service layers while ensuring consistent data access patterns for managing YouTube Shorts upload lifecycles.

## API

### GetByVideoShortIdAsync
Retrieves a single upload job associated with a specific video short identifier.
*   **Parameters**: `repository` (the target repository instance), `videoShortId` (the unique identifier of the video short).
*   **Returns**: `Task<UploadJob?>` containing the matching job or `null` if not found.
*   **Throws**: `ArgumentNullException` if the repository or ID is null; database-specific exceptions if the query fails.

### GetByStatusAsync
Fetches a collection of upload jobs filtered by a specific processing status.
*   **Parameters**: `repository`, `status` (the `UploadJobStatus` to filter by).
*   **Returns**: `Task<IEnumerable<UploadJob>>` containing all matching jobs.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### GetScheduledForUploadAsync
Retrieves all jobs currently marked as scheduled and ready for processing.
*   **Parameters**: `repository`.
*   **Returns**: `Task<IEnumerable<UploadJob>>` containing jobs in the scheduled state.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### GetRetryableFailedJobsAsync
Identifies jobs that have failed but meet the criteria for an automatic retry attempt.
*   **Parameters**: `repository`.
*   **Returns**: `Task<IEnumerable<UploadJob>>` containing failed jobs eligible for retry.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### CountByStatusAsync
Calculates the total number of jobs existing in a specific status.
*   **Parameters**: `repository`, `status`.
*   **Returns**: `Task<int>` representing the count.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### CountScheduledForUploadAsync
Returns the count of jobs currently waiting in the scheduled queue.
*   **Parameters**: `repository`.
*   **Returns**: `Task<int>` representing the count of scheduled jobs.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### CountRetryableFailedJobsAsync
Returns the count of failed jobs that are eligible for retry.
*   **Parameters**: `repository`.
*   **Returns**: `Task<int>` representing the count of retryable jobs.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### BatchUpdateAsync
Persists a collection of modified upload jobs in a single transactional operation.
*   **Parameters**: `repository`, `jobs` (the enumerable of `UploadJob` entities to update).
*   **Returns**: `Task` that completes when the update is finished.
*   **Throws**: `ArgumentNullException` if the repository or jobs collection is null; `DbUpdateException` if persistence fails.

### BatchDeleteAsync
Removes a collection of upload jobs from the storage medium.
*   **Parameters**: `repository`, `jobs` (the enumerable of `UploadJob` entities to delete).
*   **Returns**: `Task<int>` indicating the number of records successfully deleted.
*   **Throws**: `ArgumentNullException` if the repository or jobs collection is null; database-specific exceptions on failure.

### GetOldestScheduledJobAsync
Retrieves the single scheduled job with the earliest creation or scheduling timestamp.
*   **Parameters**: `repository`.
*   **Returns**: `Task<UploadJob?>` containing the oldest job or `null` if no scheduled jobs exist.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### GetNewestJobAsync
Retrieves the most recently created upload job regardless of status.
*   **Parameters**: `repository`.
*   **Returns**: `Task<UploadJob?>` containing the newest job or `null` if the repository is empty.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### AnyByStatusAsync
Determines whether at least one job exists with the specified status.
*   **Parameters**: `repository`, `status`.
*   **Returns**: `Task<bool>` returning `true` if a match exists, otherwise `false`.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

### GetQueuedOrPendingJobsAsync
Fetches jobs that are either in the "Queued" or "Pending" state, typically representing work awaiting immediate processing.
*   **Parameters**: `repository`.
*   **Returns**: `Task<IEnumerable<UploadJob>>` containing the combined result set.
*   **Throws**: `ArgumentNullException` if the repository is null; database-specific exceptions on failure.

## Usage

### Retrieving and Processing Retryable Jobs
The following example demonstrates how to fetch failed jobs eligible for retry, apply a reset logic, and persist the changes in a batch operation.

```csharp
public async Task ProcessRetriesAsync(IUploadJobRepository repository)
{
    var retryableJobs = await repository.GetRetryableFailedJobsAsync();
    
    if (!retryableJobs.Any())
    {
        return;
    }

    foreach (var job in retryableJobs)
    {
        job.ResetRetryState();
        job.Status = UploadJobStatus.Queued;
    }

    await repository.BatchUpdateAsync(retryableJobs);
}
```

### Checking Queue Capacity Before Scheduling
This example illustrates using count and existence checks to prevent over-scheduling when the system is already at capacity.

```csharp
public async Task<bool> CanScheduleNewJobAsync(IUploadJobRepository repository, int maxQueueSize)
{
    var scheduledCount = await repository.CountScheduledForUploadAsync();
    var hasPendingWork = await repository.AnyByStatusAsync(UploadJobStatus.Pending);

    if (hasPendingWork || scheduledCount >= maxQueueSize)
    {
        return false;
    }

    return true;
}
```

## Notes

*   **Null Safety**: All extension methods validate that the `repository` argument is not null. Most methods accepting specific IDs or collections will also throw `ArgumentNullException` if those specific arguments are null.
*   **Return Values**: Methods returning a single entity (e.g., `GetByVideoShortIdAsync`, `GetOldestScheduledJobAsync`) return `null` rather than throwing an exception when no record is found. Collection-returning methods return an empty `IEnumerable` rather than `null` if no matches are found.
*   **Thread Safety**: As these are stateless static extension methods relying on the passed `IUploadJobRepository` instance, thread safety is dependent on the underlying repository implementation. The repository instance itself should be treated as non-thread-safe unless explicitly documented otherwise by its implementation.
*   **Transaction Scope**: `BatchUpdateAsync` and `BatchDeleteAsync` typically operate within the scope of the repository's underlying database context. Callers should ensure these methods are invoked within an appropriate transaction scope if atomicity across multiple repositories is required.
*   **Performance**: Methods like `AnyByStatusAsync` and `CountByStatusAsync` are optimized to translate to SQL `EXISTS` or `COUNT` queries respectively, avoiding the overhead of loading full entity graphs into memory.
