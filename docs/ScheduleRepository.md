# ScheduleRepository

The `ScheduleRepository` class serves as the primary data access layer for managing YouTube Shorts upload schedules within the `youtube-shorts-automator` project. It provides asynchronous operations to retrieve, update, and query `UploadSchedule` and `ScheduledUpload` entities based on user identity, activation status, execution timing, frequency patterns, and pagination requirements. This repository abstracts the underlying persistence mechanism, ensuring that schedule management logic remains decoupled from storage implementation details.

## API

### `public ScheduleRepository`
Initializes a new instance of the `ScheduleRepository` class. This constructor typically injects necessary dependencies such as database contexts or data mappers required to perform asynchronous data operations.

### `public async Task<List<UploadSchedule>> GetByUserIdAsync`
Retrieves all upload schedules associated with a specific user identifier.
*   **Parameters**: Accepts a user identifier (typically `string` or `Guid`, depending on the domain model) to filter results.
*   **Return Value**: Returns a `Task` resulting in a `List<UploadSchedule>` containing all schedules owned by the specified user. The list may be empty if no schedules exist.
*   **Exceptions**: Throws a database-related exception if the underlying data store is unreachable or if the query fails due to integrity constraints.

### `public async Task<List<UploadSchedule>> GetActiveSchedulesAsync`
Fetches all schedules currently marked as active within the system, regardless of user ownership.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` resulting in a `List<UploadSchedule>` containing only schedules where the active flag is set to true.
*   **Exceptions**: Throws if the data retrieval operation fails or if the connection to the storage medium is lost.

### `public async Task<List<UploadSchedule>> GetDueSchedulesAsync`
Identifies and returns schedules that are currently due for execution based on their configured timing logic.
*   **Parameters**: None. Relies on internal system time comparison against schedule configurations.
*   **Return Value**: Returns a `Task` resulting in a `List<UploadSchedule>` representing items ready for processing.
*   **Exceptions**: Throws if the calculation of due dates fails or if a database timeout occurs during the query.

### `public async Task<List<UploadSchedule>> GetByFrequencyAsync`
Retrieves schedules filtered by a specific frequency configuration.
*   **Parameters**: Accepts a frequency enumeration or value object defining the recurrence pattern (e.g., Daily, Weekly).
*   **Return Value**: Returns a `Task` resulting in a `List<UploadSchedule>` matching the specified frequency.
*   **Exceptions**: Throws if the provided frequency parameter is invalid or if the underlying query execution fails.

### `public async Task<ScheduledUpload?> GetScheduledUploadAsync`
Fetches a specific scheduled upload entity by its unique identifier.
*   **Parameters**: Accepts the unique ID of the scheduled upload.
*   **Return Value**: Returns a `Task` resulting in a `ScheduledUpload` object if found, or `null` if no matching record exists.
*   **Exceptions**: Throws if the data store encounters an error during the lookup, excluding the case where the entity simply does not exist.

### `public async Task UpdateScheduledUploadAsync`
Persists changes made to an existing `ScheduledUpload` entity.
*   **Parameters**: Accepts the modified `ScheduledUpload` object containing updated state.
*   **Return Value**: Returns a `Task` that completes when the update operation is successfully committed.
*   **Exceptions**: Throws if the entity does not exist, if concurrency conflicts are detected, or if data validation fails during persistence.

### `public async Task<(List<UploadSchedule> Schedules, int Total)> GetPaginatedAsync`
Retrieves a subset of upload schedules for display purposes, along with the total count of matching records.
*   **Parameters**: Accepts pagination arguments such as page number, page size, and optional filter criteria.
*   **Return Value**: Returns a `Task` resulting in a tuple containing:
    *   `Schedules`: A `List<UploadSchedule>` for the current page.
    *   `Total`: An `int` representing the total number of records matching the filter across all pages.
*   **Exceptions**: Throws if pagination parameters are invalid (e.g., negative page size) or if the query execution fails.

## Usage

### Example 1: Retrieving and Updating a User's Schedules
This example demonstrates fetching all schedules for a specific user, modifying one, and persisting the change.

```csharp
public async Task ManageUserScheduleAsync(string userId, IRepositoryFactory factory)
{
    var repository = factory.CreateScheduleRepository();
    
    // Retrieve all schedules for the user
    var schedules = await repository.GetByUserIdAsync(userId);
    
    if (schedules.Count == 0)
    {
        Console.WriteLine("No schedules found for this user.");
        return;
    }

    // Modify the first schedule
    var targetSchedule = schedules[0];
    // Assume logic to modify properties of targetSchedule here
    
    // Note: Depending on implementation, you might need to map to ScheduledUpload 
    // or the repository might accept UploadSchedule directly if they share state.
    // Assuming a mapping step or direct cast based on domain design:
    var uploadEntity = MapToScheduledUpload(targetSchedule); 
    
    await repository.UpdateScheduledUploadAsync(uploadEntity);
    
    Console.WriteLine($"Updated schedule: {targetSchedule.Id}");
}
```

### Example 2: Processing Due Schedules with Pagination Fallback
This example illustrates fetching items due for execution and utilizing pagination for administrative reporting.

```csharp
public async Task ProcessDueItemsAsync(IRepositoryFactory factory)
{
    var repository = factory.CreateScheduleRepository();

    // Get items ready for immediate processing
    var dueSchedules = await repository.GetDueSchedulesAsync();
    
    foreach (var schedule in dueSchedules)
    {
        // Trigger upload logic
        await ExecuteUploadAsync(schedule);
    }

    // Generate admin report using pagination
    int currentPage = 1;
    int pageSize = 50;
    
    var result = await repository.GetPaginatedAsync(currentPage, pageSize);
    
    Console.WriteLine($"Showing {result.Schedules.Count} of {result.Total} total schedules.");
}
```

## Notes

*   **Null Handling**: The `GetScheduledUploadAsync` method explicitly returns `null` for missing entities rather than throwing an exception. Callers must handle the nullable return type (`ScheduledUpload?`) appropriately to avoid `NullReferenceException`.
*   **Empty Collections**: Methods returning lists (`GetByUserIdAsync`, `GetActiveSchedulesAsync`, etc.) will return an empty `List<UploadSchedule>` rather than `null` when no records match the criteria. Consumers should check `Count` rather than performing null checks on the list itself.
*   **Concurrency**: As this repository performs asynchronous I/O operations, it is not inherently thread-safe for stateful operations on a single instance if that instance holds mutable local state. However, standard usage patterns involve resolving a new instance per request or ensuring that the `UpdateScheduledUploadAsync` method handles optimistic concurrency checks internally.
*   **Data Consistency**: The `GetDueSchedulesAsync` method relies on the system clock at the time of execution. In distributed environments, ensure server time synchronization to prevent race conditions where a schedule is picked up multiple times or missed entirely.
*   **Pagination Logic**: The `GetPaginatedAsync` method returns the total count separately from the page data. This requires two logical queries (data + count) or a window function depending on the underlying provider; callers should be aware that high-frequency calls to this method with large datasets may impact performance.
