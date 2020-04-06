# UploadHistoryRepository

Manages the persistence and retrieval of upload history records for the YouTube Shorts Automator. This repository abstracts the underlying data store, providing methods to ensure the required schema exists, insert new upload entries, and query past uploads by success status, recency, or source file name. It is the single point of contact for all upload-audit operations within the application.

## API

### `public UploadHistoryRepository`

**Purpose:** Constructs a new instance of the repository, typically accepting an injected storage connection or configuration.

**Parameters:** Constructor parameters are implementation-specific and not part of the documented public surface beyond the default instantiation path.

**Return value:** A ready-to-use `UploadHistoryRepository` instance.

**Throws:** May throw if the underlying connection is unavailable at construction time, depending on the storage provider’s initialization behavior.

---

### `public async Task EnsureTableExistsAsync`

**Purpose:** Idempotently creates the storage schema (table, collection, or equivalent) required by this repository if it does not already exist. Safe to call multiple times; subsequent calls are no-ops when the schema is present.

**Parameters:** None.

**Return value:** A `Task` representing the asynchronous operation.

**Throws:** Throws when the storage provider cannot be reached or the caller lacks the necessary permissions to perform schema-level operations.

---

### `public async Task<UploadHistoryEntry> AddAsync`

**Purpose:** Persists a new upload history record and returns the fully materialized entry, including any server-generated fields such as an identifier or timestamp.

**Parameters:**
- `UploadHistoryEntry entry`: The entry to persist. Expected to contain at minimum the file name, upload status, and any relevant metadata. Server-generated fields may be ignored on input.

**Return value:** The persisted `UploadHistoryEntry` with all fields populated as stored.

**Throws:** Throws when the entry is null, when a required field is missing, or when the storage operation fails (connection loss, write conflict, constraint violation).

---

### `public async Task<bool> HasSuccessfulUploadAsync`

**Purpose:** Determines whether a given file has at least one successful upload record in the history store. Used to prevent re-uploading content that has already been processed without errors.

**Parameters:**
- `string fileName`: The file name to check. Expected to be an exact, case-sensitive match depending on the underlying store’s collation.

**Return value:** `true` if a record exists with a successful status for the specified file name; `false` otherwise.

**Throws:** Throws when `fileName` is null or empty, or when the query cannot be executed against the storage layer.

---

### `public async Task<IEnumerable<UploadHistoryEntry>> GetRecentAsync`

**Purpose:** Retrieves a bounded set of the most recent upload history entries, ordered by upload time descending. Suitable for dashboards or recent-activity summaries.

**Parameters:**
- `int count`: The maximum number of entries to return. Must be a positive integer.

**Return value:** An `IEnumerable<UploadHistoryEntry>` containing up to `count` entries, ordered from most recent to oldest.

**Throws:** Throws when `count` is less than 1, or when the storage query fails.

---

### `public async Task<IEnumerable<UploadHistoryEntry>> GetByFileNameAsync`

**Purpose:** Returns all upload history records associated with a specific file name, regardless of their status. Useful for auditing the complete upload lifecycle of a single source file.

**Parameters:**
- `string fileName`: The file name whose history should be retrieved.

**Return value:** An `IEnumerable<UploadHistoryEntry>` containing zero or more records for the given file name. Ordering is implementation-defined unless otherwise specified by the underlying query.

**Throws:** Throws when `fileName` is null or empty, or when the storage query fails.

## Usage

### Example 1: Guarding Against Duplicate Uploads

```csharp
var repo = new UploadHistoryRepository(connection);

await repo.EnsureTableExistsAsync();

string videoFile = "my_short.mp4";

if (await repo.HasSuccessfulUploadAsync(videoFile))
{
    Console.WriteLine($"Skipping '{videoFile}' — already uploaded successfully.");
    return;
}

// Perform upload...
var entry = new UploadHistoryEntry
{
    FileName = videoFile,
    Status = UploadStatus.Success,
    UploadedAt = DateTimeOffset.UtcNow
};

await repo.AddAsync(entry);
Console.WriteLine("Upload recorded.");
```

### Example 2: Displaying Recent Activity

```csharp
var repo = new UploadHistoryRepository(connection);

await repo.EnsureTableExistsAsync();

IEnumerable<UploadHistoryEntry> recent = await repo.GetRecentAsync(10);

foreach (var item in recent)
{
    Console.WriteLine($"[{item.UploadedAt:yyyy-MM-dd HH:mm}] {item.FileName} — {item.Status}");
}
```

## Notes

- **Schema assurance:** Always call `EnsureTableExistsAsync` once during application startup or repository initialization before invoking any other methods. The method is designed to be idempotent and cheap when the schema already exists.
- **Case sensitivity:** `HasSuccessfulUploadAsync` and `GetByFileNameAsync` perform exact matching on the `fileName` parameter. Normalize file names (e.g., `ToLowerInvariant`) before calling if the storage layer uses case-insensitive collation; otherwise, mismatches will produce false negatives.
- **Empty results:** `GetRecentAsync` returns an empty enumerable when no history records exist. `GetByFileNameAsync` returns an empty enumerable when the file has no recorded uploads. Neither returns null.
- **Thread safety:** This repository does not guarantee thread-safe operations across concurrent calls to `AddAsync` for the same file name. External synchronization should be applied if multiple processes or threads may attempt to insert records for identical files simultaneously.
- **Storage-specific exceptions:** Methods may surface provider-specific exceptions (e.g., timeouts, connection refused) wrapped in the native exception types of the storage library. Callers should implement retry policies appropriate to the deployment environment.
