# UploadJobRepository

`UploadJobRepository` provides a persistence abstraction for `UploadJob` entities within the `youtube-shorts-automator` project. It encapsulates all data-access operations—CRUD, status-based queries, scheduled retrieval, and retryable failure detection—against the underlying storage mechanism, exposing asynchronous methods that return domain objects rather than storage-specific constructs.

## API

### `public UploadJobRepository`

Constructor. Initializes a new instance of the repository with the required storage context or connection. The specific dependencies are injected and are opaque to consumers of the public surface.

### `public async Task<UploadJob?> GetByIdAsync`

Retrieves a single `UploadJob` by its unique identifier.

- **Parameters:** `id` (the primary key, type inferred as `int` or `Guid` based on the entity definition).
- **Returns:** The matching `UploadJob` instance, or `null` if no record with the given identifier exists.
- **Throws:** `OperationCanceledException` if the cancellation token is signaled; storage-layer exceptions (e.g., timeout, connection failure) propagate asynchronously.

### `public async Task<IEnumerable<UploadJob>> GetAllAsync`

Returns all `UploadJob` records in the store, without filtering or ordering guarantees unless the underlying provider imposes a default.

- **Returns:** An `IEnumerable<UploadJob>` that may represent an in-memory collection or a deferred-execution query, depending on the implementation. Callers should materialize the result before leaving a scope where the storage context is disposed.
- **Throws:** Storage-layer exceptions on connection failure or query cancellation.

### `public async Task<IEnumerable<UploadJob>> GetByStatusAsync`

Returns all `UploadJob` records whose current status matches the supplied value.

- **Parameters:** `status` (an enum or string representing the job state, e.g., `Pending`, `Processing`, `Completed`, `Failed`).
- **Returns:** Filtered collection of `UploadJob` instances. Empty enumerable if no jobs match.
- **Throws:** `ArgumentException` or `ArgumentOutOfRangeException` if an invalid status value is supplied (implementation-dependent); storage-layer exceptions.

### `public async Task<IEnumerable<UploadJob>> GetScheduledForUploadAsync`

Returns all `UploadJob` records that are scheduled and eligible for immediate upload processing. The exact criteria (e.g., status `Scheduled` with a `ScheduledFor` timestamp in the past) are determined by the implementation.

- **Returns:** Collection of jobs ready for upload. May be empty.
- **Throws:** Storage-layer exceptions.

### `public async Task<IEnumerable<UploadJob>> GetRetryableFailedJobsAsync`

Returns all `UploadJob` records that are in a failed state but still eligible for retry. Eligibility typically depends on a retry-count threshold and possibly a cooldown period since the last attempt.

- **Returns:** Collection of failed jobs that have not exhausted their retry budget. Empty if none qualify.
- **Throws:** Storage-layer exceptions.

### `public async Task<UploadJob> AddAsync`

Persists a new `UploadJob` to the store and returns the entity with any store-generated values populated (e.g., auto-incremented primary key, default status, timestamps).

- **Parameters:** `job` (the `UploadJob` to insert; must not be null).
- **Returns:** The inserted `UploadJob` with its post-persistence state.
- **Throws:** `ArgumentNullException` if `job` is null; storage-layer exceptions including constraint violations (duplicate key, missing required fields).

### `public async Task<UploadJob> UpdateAsync`

Persists changes to an existing `UploadJob`. The entity must already exist in the store; the method overwrites the corresponding record with the supplied instance’s current values.

- **Parameters:** `job` (the modified `UploadJob`; must not be null).
- **Returns:** The updated `UploadJob` as it exists after the write.
- **Throws:** `ArgumentNullException` if `job` is null; `InvalidOperationException` or a storage-layer concurrency exception if the record no longer exists or has been modified by another operation since it was loaded; storage-layer exceptions.

### `public async Task<bool> DeleteAsync`

Removes an `UploadJob` from the store by its identifier.

- **Parameters:** `id` (the primary key of the job to delete).
- **Returns:** `true` if a record was found and deleted; `false` if no record with the given identifier existed.
- **Throws:** Storage-layer exceptions (foreign-key violations if the job is referenced by other entities, depending on schema).

### `public async Task<bool> ExistsAsync`

Checks whether a record with the given identifier exists in the store.

- **Parameters:** `id` (the primary key to check).
- **Returns:** `true` if a matching record exists; `false` otherwise.
- **Throws:** Storage-layer exceptions.

### `public async Task<int> CountAsync`

Returns the total number of `UploadJob` records in the store.

- **Returns:** A non-negative integer count.
- **Throws:** Storage-layer exceptions.

### `public async Task SaveChangesAsync`

Commits all pending changes tracked by the underlying unit-of-work or context to durable storage. This is typically called after one or more `AddAsync`, `UpdateAsync`, or `DeleteAsync` invocations when the implementation batches writes rather than executing them immediately.

- **Throws:** Storage-layer exceptions including concurrency conflicts and constraint violations that are detected only at commit time.

## Usage

### Example 1: Scheduling and persisting a new upload job

```csharp
var repository = new UploadJobRepository(dbContext);

var newJob = new UploadJob
{
    VideoPath = "/media/clip_001.mp4",
    Title = "Quick DIY Hack #shorts",
    ScheduledFor = DateTime.UtcNow.AddMinutes(30),
    Status = UploadStatus.Scheduled
};

UploadJob created = await repository.AddAsync(newJob);
await repository.SaveChangesAsync();

Console.WriteLine($"Job created with ID: {created.Id}");
```

### Example 2: Fetching retryable failures and re-queuing them

```csharp
var repository = new UploadJobRepository(dbContext);

IEnumerable<UploadJob> retryableJobs = await repository.GetRetryableFailedJobsAsync();

foreach (var job in retryableJobs)
{
    job.Status = UploadStatus.Scheduled;
    job.RetryCount += 1;
    job.LastAttemptedAt = DateTime.UtcNow;

    await repository.UpdateAsync(job);
}

await repository.SaveChangesAsync();

int count = await repository.CountAsync();
Console.WriteLine($"Re-queued {retryableJobs.Count()} failed jobs. Total jobs in store: {count}");
```

## Notes

- **Null returns:** `GetByIdAsync` returns `null` for missing identifiers; callers must guard against dereferencing a null result. `GetByStatusAsync`, `GetScheduledForUploadAsync`, and `GetRetryableFailedJobsAsync` return empty collections, never null.
- **Deferred execution:** Methods returning `IEnumerable<UploadJob>` may return a query that executes lazily. Enumerate the result inside the same scope where the storage context is alive, or call `.ToList()` to materialize it before passing the collection onward.
- **Thread safety:** The repository is not guaranteed to be thread-safe. Instances are typically scoped to a single unit-of-work (e.g., per-request in a web application). Concurrent calls on the same instance, especially mixing writes and `SaveChangesAsync`, can lead to race conditions or concurrency exceptions.
- **`SaveChangesAsync` semantics:** If the implementation uses an ORM with change tracking, `AddAsync`, `UpdateAsync`, and `DeleteAsync` may only mark entities in memory. Failing to call `SaveChangesAsync` after modifications will result in lost writes.
- **Retry eligibility:** `GetRetryableFailedJobsAsync` relies on implementation-defined thresholds. Callers should not assume a specific retry count or cooldown; they should inspect the returned jobs’ `RetryCount` and `LastAttemptedAt` fields before acting.
- **Concurrency conflicts:** `UpdateAsync` and `SaveChangesAsync` may throw when the underlying record has been modified externally between load and save. Callers should implement retry logic or conflict-resolution strategies where appropriate.
