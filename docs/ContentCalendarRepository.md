# ContentCalendarRepository

A repository abstraction for managing `ContentCalendarEntry` entities, providing asynchronous CRUD operations and querying capabilities for scheduling and organizing YouTube Shorts content.

## API

### `ContentCalendarRepository`

The primary interface for interacting with the content calendar data store. All operations are asynchronous and return `Task` or `ValueTask` to avoid blocking the caller.

### `public async Task<ContentCalendarEntry?> GetByIdAsync(int id)`

Retrieves a single `ContentCalendarEntry` by its unique identifier.

- **Parameters**: `id` – The unique identifier of the entry to retrieve.
- **Return value**: A `ContentCalendarEntry` instance if found; otherwise, `null`.
- **Exceptions**: Throws if the underlying data store fails (e.g., connection loss, constraint violation).

### `public async Task<IEnumerable<ContentCalendarEntry>> GetAllAsync()`

Retrieves all `ContentCalendarEntry` records from the calendar.

- **Return value**: An `IEnumerable<ContentCalendarEntry>` containing all entries, possibly empty.
- **Exceptions**: Throws on data access failure.

### `public async Task<IEnumerable<ContentCalendarEntry>> GetByDateRangeAsync(DateTime start, DateTime end)`

Retrieves entries whose scheduled date falls within the specified range (inclusive).

- **Parameters**:
  - `start` – The start of the date range.
  - `end` – The end of the date range.
- **Return value**: An `IEnumerable<ContentCalendarEntry>` of entries within the range.
- **Exceptions**: Throws if the date range is invalid (e.g., `start > end`) or on data access failure.

### `public async Task<IEnumerable<ContentCalendarEntry>> GetUpcomingAsync(int limit = 10)`

Retrieves the next scheduled entries, ordered by ascending date.

- **Parameters**:
  - `limit` – Maximum number of entries to return (default: 10).
- **Return value**: An `IEnumerable<ContentCalendarEntry>` of upcoming entries, possibly empty.
- **Exceptions**: Throws if `limit` is negative or on data access failure.

### `public async Task<ContentCalendarEntry> AddAsync(ContentCalendarEntry entry)`

Adds a new `ContentCalendarEntry` to the calendar.

- **Parameters**: `entry` – The entry to add (must be non-null and valid).
- **Return value**: The added `ContentCalendarEntry` (with updated identity or timestamps).
- **Exceptions**:
  - Throws `ArgumentNullException` if `entry` is `null`.
  - Throws on constraint violation (e.g., duplicate ID or invalid date).

### `public async Task<ContentCalendarEntry> UpdateAsync(ContentCalendarEntry entry)`

Updates an existing `ContentCalendarEntry` in the calendar.

- **Parameters**: `entry` – The entry to update (must be non-null and valid).
- **Return value**: The updated `ContentCalendarEntry`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entry` is `null`.
  - Throws if the entry does not exist or on concurrency conflict.

### `public async Task<bool> DeleteAsync(int id)`

Removes a `ContentCalendarEntry` by its identifier.

- **Parameters**: `id` – The identifier of the entry to remove.
- **Return value**: `true` if the entry existed and was deleted; otherwise, `false`.
- **Exceptions**: Throws on data access failure.

### `public async Task<bool> ExistsAsync(int id)`

Checks whether a `ContentCalendarEntry` with the given identifier exists.

- **Parameters**: `id` – The identifier to check.
- **Return value**: `true` if the entry exists; otherwise, `false`.
- **Exceptions**: Throws on data access failure.

### `public async Task<int> CountAsync()`

Counts the total number of `ContentCalendarEntry` records.

- **Return value**: The total count as an `int`.
- **Exceptions**: Throws on data access failure.

### `public Task SaveChangesAsync()`

Persists all pending changes to the underlying data store.

- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws if the save operation fails (e.g., validation errors, connection issues).

## Usage

### Example 1: Scheduling a new Short
