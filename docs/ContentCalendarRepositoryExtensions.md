# ContentCalendarRepositoryExtensions

The `ContentCalendarRepositoryExtensions` class provides a set of static asynchronous extension methods designed to simplify complex querying and data retrieval operations for the `ContentCalendarEntry` entity within the `youtube-shorts-automator` project. By encapsulating common filtering, sorting, and inclusion logic directly onto the repository interface, this class reduces boilerplate code in service layers and ensures consistent data access patterns for scheduling, optimization, and status tracking workflows.

## API

### GetFirstUpcomingByStatusAsync
Retrieves the single earliest `ContentCalendarEntry` that matches a specific status and is scheduled for a future date relative to the current time.
*   **Parameters**: Accepts the repository instance, a `ContentStatus` enum value, and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<ContentCalendarEntry?>`. The result is the matching entry if found, or `null` if no upcoming entries exist for the specified status.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered. May throw database-related exceptions if the underlying query fails.

### GetByChannelIdAsync
Fetches all calendar entries associated with a specific YouTube channel identifier.
*   **Parameters**: Accepts the repository instance, a `string` representing the Channel ID, and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result is an enumerable collection of entries; the collection is empty if no matches are found.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered. Throws `ArgumentNullException` if the provided Channel ID is null or empty.

### GetByStatusAsync
Retrieves all calendar entries currently assigned a specific processing or publication status.
*   **Parameters**: Accepts the repository instance, a `ContentStatus` enum value, and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result contains all entries matching the status.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetByDateAsync
Fetches all calendar entries scheduled for a specific date.
*   **Parameters**: Accepts the repository instance, a `DateTime` object (typically normalized to UTC date only), and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result includes all entries falling within the specified 24-hour period.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetNextEntryAsync
Identifies the immediate subsequent entry in the content calendar sequence relative to a given reference entry or date.
*   **Parameters**: Accepts the repository instance, an optional reference `ContentCalendarEntry` or `DateTime`, and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<ContentCalendarEntry?>`. The result is the next chronological entry, or `null` if the current entry is the last in the sequence.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetPreviousEntryAsync
Identifies the immediate preceding entry in the content calendar sequence relative to a given reference entry or date.
*   **Parameters**: Accepts the repository instance, an optional reference `ContentCalendarEntry` or `DateTime`, and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<ContentCalendarEntry?>`. The result is the previous chronological entry, or `null` if the current entry is the first in the sequence.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetWithOptimizationAppliedAsync
Retrieves a collection of calendar entries where automated optimization rules (such as title A/B testing selections or thumbnail adjustments) have already been processed and applied.
*   **Parameters**: Accepts the repository instance and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result is a filtered list of entries ready for final review or scheduling.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetWithVideoShortsAsync
Fetches calendar entries that are explicitly linked to generated video short assets, ensuring the related media entities are loaded or validated.
*   **Parameters**: Accepts the repository instance and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result includes only entries with associated video content.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

### GetWithUploadJobsAsync
Retrieves calendar entries that have associated background upload jobs attached, allowing for monitoring of the ingestion pipeline.
*   **Parameters**: Accepts the repository instance and an optional `CancellationToken`.
*   **Return Value**: Returns a `Task<IEnumerable<ContentCalendarEntry>>`. The result includes entries where an upload job state exists.
*   **Exceptions**: Throws `OperationCanceledException` if the cancellation token is triggered.

## Usage

The following examples demonstrate how to utilize these extensions within a service layer to manage content scheduling and retrieval.

### Retrieving Upcoming Content for Processing
This example shows how to fetch the next pending item in the queue for a specific status to trigger a processing workflow.

```csharp
public async Task ProcessNextPendingItemAsync(IContentCalendarRepository repository, CancellationToken ct)
{
    // Retrieve the very next entry marked as 'PendingGeneration'
    var nextEntry = await repository.GetFirstUpcomingByStatusAsync(
        ContentStatus.PendingGeneration, 
        ct
    );

    if (nextEntry is null)
    {
        // No work currently available
        return;
    }

    // Proceed with generation logic using the retrieved entry
    await _generationService.GenerateShortAsync(nextEntry, ct);
}
```

### Aggregating Channel Data with Related Entities
This example illustrates fetching a full schedule for a channel, specifically filtering for entries that already have optimization data applied and associated upload jobs.

```csharp
public async Task<IEnumerable<ContentCalendarEntry>> GetReadyToPublishScheduleAsync(
    IContentCalendarRepository repository, 
    string channelId, 
    CancellationToken ct)
{
    // Fetch entries for the channel that have optimization applied
    var optimizedEntries = await repository.GetWithOptimizationAppliedAsync(ct);
    
    // Filter locally or combine with channel specific query depending on repository implementation
    // Here we assume a need to cross-reference with channel ID and upload jobs
    var channelEntries = await repository.GetByChannelIdAsync(channelId, ct);
    
    var readyEntries = channelEntries
        .Where(e => optimizedEntries.Any(o => o.Id == e.Id))
        .ToList();

    // Further refine to ensure upload jobs are present
    var entriesWithJobs = await repository.GetWithUploadJobsAsync(ct);
    
    return readyEntries.Where(e => entriesWithJobs.Any(j => j.Id == e.Id));
}
```

## Notes

*   **Nullability**: Methods returning a single entity (`GetFirstUpcomingByStatusAsync`, `GetNextEntryAsync`, `GetPreviousEntryAsync`) explicitly return `null` when no matching record is found. Callers must perform null checks before accessing properties of the returned entity.
*   **Empty Collections**: Methods returning `IEnumerable<ContentCalendarEntry>` will return an empty collection rather than `null` if no matches are found. Consumers should not check for null on these return values but may check for `Any()`.
*   **Cancellation**: All methods accept a `CancellationToken`. It is critical to pass the token from the calling context to ensure that long-running database queries can be aborted promptly during application shutdown or request timeouts.
*   **Thread Safety**: As this class consists entirely of static extension methods operating on stateless logic and deferring execution to the underlying repository instance, the class itself is thread-safe. However, thread safety of the actual data retrieval depends on the implementation of the `IContentCalendarRepository` instance passed to these methods.
*   **Date Handling**: When using `GetByDateAsync`, ensure the input `DateTime` is normalized (e.g., UTC date with time set to 00:00:00) to avoid mismatches caused by time component discrepancies, as the underlying query likely filters based on date ranges.
