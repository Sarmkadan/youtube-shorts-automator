# VideoRepository

`VideoRepository` provides a dedicated data-access abstraction for querying `Video` entities from the underlying persistence store. It centralizes common retrieval patterns—filtering by user, status, YouTube identifier, or recency—and supports paginated result sets with total counts, enabling efficient list views and background processing in the `youtube-shorts-automator` pipeline.

## API

### `public VideoRepository`
Constructor. Initializes a new instance of the repository, typically accepting an injected database context or connection factory. Exact parameters depend on the internal implementation and are not part of the public surface documented here.

### `public async Task<List<Video>> GetByUserIdAsync`
Retrieves all videos associated with a specific user.

**Parameters:**  
- `userId` (inferred from signature context) — the unique identifier of the user whose videos should be fetched.

**Returns:**  
A `List<Video>` containing all matching videos; an empty list if the user has no videos.

**Throws:**  
- `ArgumentNullException` when `userId` is null or empty.  
- `DatabaseException` (or a custom persistence exception) when the underlying data store is unreachable or the query fails.

### `public async Task<List<Video>> GetByStatusAsync`
Returns all videos whose processing status matches the given value.

**Parameters:**  
- `status` — an enum or string representing the desired video status (e.g., `Pending`, `Processing`, `Published`).

**Returns:**  
A `List<Video>` filtered by the specified status; an empty list if none match.

**Throws:**  
- `ArgumentException` when an invalid or unrecognized status value is supplied.  
- `DatabaseException` on data-store failures.

### `public async Task<List<Video>> GetPendingVideosAsync`
Convenience method that fetches all videos currently in a pending state, ready for processing.

**Returns:**  
A `List<Video>` of pending videos; an empty list if the pending queue is empty.

**Throws:**  
- `DatabaseException` on data-store failures.

### `public async Task<List<Video>> GetRecentVideosAsync`
Returns videos ordered by recency, typically limited to a fixed window (e.g., last 24 hours or top N entries). The exact ordering and limit are determined by the repository implementation.

**Returns:**  
A `List<Video>` of recent videos; an empty list if no videos exist in the recent window.

**Throws:**  
- `DatabaseException` on data-store failures.

### `public async Task<List<Video>> GetByYouTubeIdAsync`
Looks up videos by their external YouTube video identifier.

**Parameters:**  
- `youTubeId` — the YouTube video ID string.

**Returns:**  
A `List<Video>` containing matching videos (typically zero or one element, but the signature allows multiple in case of duplicate records).

**Throws:**  
- `ArgumentNullException` when `youTubeId` is null or whitespace.  
- `DatabaseException` on data-store failures.

### `public async Task<(List<Video> Videos, int Total)> GetPaginatedAsync`
Returns a page of videos along with the total count of videos matching the filter criteria, enabling paginated UI or batch processing.

**Parameters:**  
- `page` — the one-based page number.  
- `pageSize` — the number of items per page.  
- Additional filter parameters (e.g., status, date range) may be present depending on the overload.

**Returns:**  
A tuple `(List<Video> Videos, int Total)` where `Videos` contains the requested page items and `Total` is the overall count of matching records before paging.

**Throws:**  
- `ArgumentOutOfRangeException` when `page` is less than 1 or `pageSize` is less than 1.  
- `DatabaseException` on data-store failures.

### `public async Task<(List<Video> Videos, int Total)> GetUserVideosPaginatedAsync`
Returns a page of videos belonging to a specific user, together with the total count of that user’s videos.

**Parameters:**  
- `userId` — the user identifier.  
- `page` — the one-based page number.  
- `pageSize` — the number of items per page.

**Returns:**  
A tuple `(List<Video> Videos, int Total)` scoped to the given user.

**Throws:**  
- `ArgumentNullException` when `userId` is null or empty.  
- `ArgumentOutOfRangeException` when `page` or `pageSize` is out of range.  
- `DatabaseException` on data-store failures.

## Usage

### Example 1: Fetching pending videos for a background processor

```csharp
var repository = new VideoRepository(dbContext);

try
{
    List<Video> pendingVideos = await repository.GetPendingVideosAsync();

    foreach (var video in pendingVideos)
    {
        // Submit to processing pipeline
        await processingService.EnqueueAsync(video);
    }
}
catch (DatabaseException ex)
{
    logger.LogError(ex, "Failed to retrieve pending videos");
}
```

### Example 2: Paginated user video list for a dashboard

```csharp
var repository = new VideoRepository(dbContext);
string userId = "user-abc-123";
int page = 1;
int pageSize = 20;

(List<Video> videos, int total) = await repository.GetUserVideosPaginatedAsync(userId, page, pageSize);

int totalPages = (int)Math.Ceiling(total / (double)pageSize);

foreach (var video in videos)
{
    Console.WriteLine($"[{video.Status}] {video.Title}");
}

Console.WriteLine($"Showing page {page} of {totalPages} ({total} total videos)");
```

## Notes

- **Empty results:** All `Get*` methods return an empty list rather than null when no records match. Callers should guard against empty collections but do not need null checks on the returned list.
- **Paginated total counts:** The `Total` value in paginated methods reflects the count of records matching the filter *before* paging is applied. When the filter matches zero records, `Total` is 0 and `Videos` is an empty list.
- **Thread safety:** This class is not guaranteed to be thread-safe. Instances are intended to be scoped per unit of work (e.g., per request or per background job). Concurrent calls on the same instance from multiple threads may lead to undefined behavior depending on the underlying database context lifetime.
- **Exception handling:** All methods that touch the data store can throw database-level exceptions. Production callers should wrap calls in try-catch blocks or rely on a global error-handling middleware.
- **YouTube ID duplicates:** `GetByYouTubeIdAsync` returns a `List<Video>` rather than a single entity. If the data store enforces uniqueness on YouTube IDs, the list will contain at most one element; if duplicates are possible (e.g., across different users or historical imports), multiple entries may be returned. Callers should handle both cases.
