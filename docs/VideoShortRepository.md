# VideoShortRepository

The `VideoShortRepository` class serves as the primary data access layer for managing `VideoShort` entities within the `youtube-shorts-automator` project. It provides an asynchronous interface for performing standard CRUD operations, allowing the application to retrieve, create, update, and delete video short records while abstracting the underlying persistence mechanism. This repository ensures that all database interactions are non-blocking, supporting scalable handling of video metadata and processing states.

## API

### Constructor
**`public VideoShortRepository()`**
Initializes a new instance of the `VideoShortRepository` class. This constructor typically sets up the required database context or data connection needed to perform subsequent operations.

### GetByIdAsync
**`public async Task<VideoShort?> GetByIdAsync`**
Retrieves a single `VideoShort` entity by its unique identifier.
*   **Parameters**: Accepts the unique ID of the video short (type inferred as `string`, `int`, or `Guid` based on project conventions).
*   **Return Value**: Returns a `Task` that resolves to the `VideoShort` object if found, or `null` if no matching record exists.
*   **Exceptions**: May throw exceptions related to database connectivity or serialization errors.

### GetAllAsync
**`public async Task<IEnumerable<VideoShort>> GetAllAsync`**
Fetches all `VideoShort` records stored in the repository.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` resolving to an enumerable collection of `VideoShort` objects. Returns an empty collection if no records exist.
*   **Exceptions**: Throws if the underlying data source is unavailable.

### GetByStatusAsync
**`public async Task<IEnumerable<VideoShort>> GetByStatusAsync`**
Retrieves a list of `VideoShort` entities filtered by their current processing or publication status.
*   **Parameters**: Accepts a status value (e.g., `ProcessingStatus` enum or string) to filter results.
*   **Return Value**: Returns a `Task` resolving to an enumerable collection of `VideoShort` objects matching the specified status.
*   **Exceptions**: Throws if the status parameter is invalid or if a database error occurs.

### GetByChannelAsync
**`public async Task<IEnumerable<VideoShort>> GetByChannelAsync`**
Fetches all `VideoShort` records associated with a specific YouTube channel.
*   **Parameters**: Accepts the channel identifier (e.g., Channel ID).
*   **Return Value**: Returns a `Task` resolving to an enumerable collection of `VideoShort` objects linked to the provided channel.
*   **Exceptions**: Throws if the channel identifier format is invalid or on data access failure.

### AddAsync
**`public async Task<VideoShort> AddAsync`**
Persists a new `VideoShort` entity to the data store.
*   **Parameters**: Accepts the `VideoShort` object to be inserted.
*   **Return Value**: Returns a `Task` resolving to the created `VideoShort` object, typically including generated fields like the primary key or timestamps.
*   **Exceptions**: Throws if the entity already exists (constraint violation), if the input object is null, or if validation fails.

### UpdateAsync
**`public async Task<VideoShort> UpdateAsync`**
Updates an existing `VideoShort` entity in the data store.
*   **Parameters**: Accepts the modified `VideoShort` object.
*   **Return Value**: Returns a `Task` resolving to the updated `VideoShort` object.
*   **Exceptions**: Throws if the entity does not exist, if concurrency conflicts are detected, or if the input is null.

### DeleteAsync
**`public async Task<bool> DeleteAsync`**
Removes a `VideoShort` entity from the data store.
*   **Parameters**: Accepts the unique identifier of the video short to delete.
*   **Return Value**: Returns a `Task` resolving to `true` if the deletion was successful, or `false` if the record was not found.
*   **Exceptions**: Throws if a database constraint prevents deletion (e.g., foreign key relationships).

### ExistsAsync
**`public async Task<bool> ExistsAsync`**
Checks whether a `VideoShort` entity with a specific identifier exists in the repository.
*   **Parameters**: Accepts the unique identifier to check.
*   **Return Value**: Returns a `Task` resolving to `true` if the record exists, otherwise `false`.
*   **Exceptions**: Throws on database connectivity issues.

### CountAsync
**`public async Task<int> CountAsync`**
Returns the total number of `VideoShort` records currently in the repository.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` resolving to an integer representing the count.
*   **Exceptions**: Throws if the count operation fails due to data source errors.

### SaveChangesAsync
**`public async Task SaveChangesAsync`**
Commits any pending changes tracked by the repository's underlying unit of work or context to the database.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` that completes when the transaction is successfully committed.
*   **Exceptions**: Throws if the transaction fails, rolls back, or encounters validation errors during commit.

## Usage

### Example 1: Creating and Persisting a New Video Short
This example demonstrates instantiating a new `VideoShort` object, adding it to the repository, and explicitly saving the changes.

```csharp
var repository = new VideoShortRepository();

var newShort = new VideoShort
{
    Title = "Amazing Nature Facts",
    ChannelId = "UC_123456789",
    Status = ProcessingStatus.Queued,
    SourceUrl = "https://example.com/video.mp4"
};

// Add the entity to the tracking context
var createdShort = await repository.AddAsync(newShort);

// Commit the transaction to the database
await repository.SaveChangesAsync();

Console.WriteLine($"Created short with ID: {createdShort.Id}");
```

### Example 2: Retrieving and Updating Status
This example retrieves all videos queued for processing for a specific channel, updates their status, and persists the changes.

```csharp
var repository = new VideoShortRepository();
string targetChannelId = "UC_123456789";

// Fetch videos by channel and status
var queuedVideos = await repository.GetByChannelAsync(targetChannelId);
var processingItems = queuedVideos.Where(v => v.Status == ProcessingStatus.Queued);

foreach (var item in processingItems)
{
    item.Status = ProcessingStatus.Processing;
    item.LastModified = DateTime.UtcNow;
    
    // Stage the update
    await repository.UpdateAsync(item);
}

// Commit all updates in a single batch
await repository.SaveChangesAsync();
```

## Notes

*   **Null Handling**: Methods returning a single entity (`GetByIdAsync`) return `null` if the record is not found, whereas collection methods (`GetAllAsync`, `GetByStatusAsync`, etc.) return an empty enumerable rather than `null`. Callers should handle null checks appropriately for singular returns.
*   **Transaction Management**: While individual methods like `AddAsync` and `UpdateAsync` stage changes, they may not immediately commit to the database depending on the underlying implementation. It is recommended to call `SaveChangesAsync` after a batch of operations to ensure data consistency and performance.
*   **Thread Safety**: As with most Entity Framework or ORM-backed repositories, instances of `VideoShortRepository` are generally not thread-safe. A new instance should be created per request or per unit of work to avoid concurrency conflicts within the underlying database context.
*   **Exception Propagation**: Database-specific exceptions (e.g., connection timeouts, constraint violations) are not swallowed and will propagate to the caller. Implementations should wrap calls in appropriate try-catch blocks for robust error handling.
*   **Existence Checks**: Use `ExistsAsync` instead of `GetByIdAsync` when only the presence of a record is required, as this may result in more efficient database queries (e.g., `SELECT 1` vs `SELECT *`).
