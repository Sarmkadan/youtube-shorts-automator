# ThumbnailAbTestRepository

Repository responsible for persisting and querying `ThumbnailVariant` entities that represent the different thumbnails used in A/B tests for YouTube Shorts. It provides asynchronous CRUD operations and helper methods to retrieve active variants, winners, and counts.

## API

### ThumbnailAbTestRepository()
Initializes a new instance of the repository. The instance is not thread‑safe; external synchronization is required for concurrent access.

### GetByIdAsync
```csharp
public async Task<ThumbnailVariant?> GetByIdAsync(Guid id)
```
- **Purpose:** Returns the thumbnail variant with the specified identifier, or `null` if no such variant exists.  
- **Parameters:**  
  - `id`: The unique identifier of the variant to retrieve.  
- **Return value:** A `Task` that completes with the matching `ThumbnailVariant` or `null`.  
- **Exceptions:**  
  - `ArgumentException` if `id` is `Guid.Empty`.  
  - `ObjectDisposedException` if the repository has been disposed.  

### GetAllAsync
```csharp
public async Task<IEnumerable<ThumbnailVariant>> GetAllAsync()
```
- **Purpose:** Retrieves every thumbnail variant stored in the repository.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes with an enumerable of all `ThumbnailVariant` objects.  
- **Exceptions:**  
  - `ObjectDisposedException` if the repository has been disposed.  

### GetByVideoShortIdAsync
```csharp
public async Task<IEnumerable<ThumbnailVariant>> GetByVideoShortIdAsync(Guid videoShortId)
```
- **Purpose:** Returns all thumbnail variants associated with the given YouTube Short.  
- **Parameters:**  
  - `videoShortId`: The identifier of the Short whose variants are requested.  
- **Return value:** A `Task` that completes with an enumerable of `ThumbnailVariant` objects for the specified Short; may be empty if none exist.  
- **Exceptions:**  
  - `ArgumentException` if `videoShortId` is `Guid.Empty`.  
  - `ObjectDisposedException` if the repository has been disposed.  

### GetActiveVariantsAsync
```csharp
public async Task<IEnumerable<ThumbnailVariant>> GetActiveVariantsAsync()
```
- **Purpose:** Retrieves only those thumbnail variants that are currently marked as active (i.e., participating in the ongoing A/B test).  
- **Parameters:** None.  
- **Return value:** A `Task` that completes with an enumerable of active `ThumbnailVariant` objects.  
- **Exceptions:**  
  - `ObjectDisposedException` if the repository has been disposed.  

### GetWinnerAsync
```csharp
public async Task<ThumbnailVariant?> GetWinnerAsync()
```
- **Purpose:** Returns the variant that has been declared the winner of the A/B test, based on stored performance metrics, or `null` if no winner has been determined.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes with the winning `ThumbnailVariant` or `null`.  
- **Exceptions:**  
  - `ObjectDisposedException` if the repository has been disposed.  

### AddAsync
```csharp
public async Task<ThumbnailVariant> AddAsync(ThumbnailVariant variant)
```
- **Purpose:** Inserts a new thumbnail variant into the repository and returns the persisted entity (including any generated identifier).  
- **Parameters:**  
  - `variant`: The variant to add; must not be `null` and must have a valid `VideoShortId`.  
- **Return value:** A `Task` that completes with the added `ThumbnailVariant`.  
- **Exceptions:**  
  - `ArgumentNullException` if `variant` is `null`.  
  - `InvalidOperationException` if a variant with the same identifier already exists.  
  - `ObjectDisposedException` if the repository has been disposed.  

### UpdateAsync
```csharp
public async Task<ThumbnailVariant> UpdateAsync(ThumbnailVariant variant)
```
- **Purpose:** Updates an existing thumbnail variant with the supplied values and returns the updated entity.  
- **Parameters:**  
  - `variant`: The variant containing the updated data; must not be `null` and must represent an existing entity.  
- **Return value:** A `Task` that completes with the updated `ThumbnailVariant`.  
- **Exceptions:**  
  - `ArgumentNullException` if `variant` is `null`.  
  - `InvalidOperationException` if no variant with the given identifier exists.  
  - `ObjectDisposedException` if the repository has been disposed.  

### DeleteAsync
```csharp
public async Task<bool> DeleteAsync(Guid id)
```
- **Purpose:** Attempts to remove the thumbnail variant with the specified identifier. Returns `true` if the entity was found and deleted; otherwise `false`.  
- **Parameters:**  
  - `id`: The identifier of the variant to delete.  
- **Return value:** A `Task` that completes with a `bool` indicating whether the deletion succeeded.  
- **Exceptions:**  
  - `ArgumentException` if `id` is `Guid.Empty`.  
  - `ObjectDisposedException` if the repository has been disposed.  

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(Guid id)
```
- **Purpose:** Checks whether a thumbnail variant with the given identifier exists in the repository.  
- **Parameters:**  
  - `id`: The identifier to test.  
- **Return value:** A `Task` that completes with `true` if a variant with that identifier exists; otherwise `false`.  
- **Exceptions:**  
  - `ArgumentException` if `id` is `Guid.Empty`.  
  - `ObjectDisposedException` if the repository has been disposed.  

### CountAsync
```csharp
public async Task<int> CountAsync()
```
- **Purpose:** Returns the total number of thumbnail variants stored in the repository.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes with an `int` representing the count of variants.  
- **Exceptions:**  
  - `ObjectDisposedException` if the repository has been disposed.  

### SaveChangesAsync
```csharp
public async Task SaveChangesAsync()
```
- **Purpose:** Persists all pending additions, updates, and deletions to the underlying data store.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the save operation finishes.  
- **Exceptions:**  
  - `ObjectDisposedException` if the repository has been disposed.  
  - `DbUpdateException` (or similar) if the commit fails due to constraints or other store‑specific errors.  

## Usage

### Retrieving active variants and determining a winner
```csharp
using var repo = new ThumbnailAbTestRepository();

// Get all variants that are currently active for a specific Short
var active = await repo.GetActiveVariantsAsync();
foreach (var v in active)
{
    Console.WriteLine($"Variant {v.Id}: {v.Url}");
}

// Determine whether a winner has been declared
var winner = await repo.GetWinnerAsync();
if (winner != null)
{
    Console.WriteLine($"Winning variant: {winner.Id}");
}
else
{
    Console.WriteLine("No winner yet.");
}
```

### Adding a new variant and saving changes
```csharp
await using var repo = new ThumbnailAbTestRepository();

var newVariant = new ThumbnailVariant
{
    VideoShortId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
    Url = "https://example.com/thumbs/variantA.jpg",
    IsActive = true
};

// Add the variant to the context
var added = await repo.AddAsync(newVariant);

// Persist the change
await repo.SaveChangesAsync();

Console.WriteLine($"Added variant with ID {added.Id}");
```

## Notes

- The repository does **not** enforce thread safety; concurrent calls from multiple threads may lead to race conditions, especially when mixing read and write operations. External synchronization (e.g., locking or using a single‑threaded context) is required for safe concurrent use.  
- All methods that accept an identifier validate that the supplied `Guid` is not empty; passing `Guid.Empty` results in an `ArgumentException`.  
- Passing `null` for any reference‑type parameter (`ThumbnailVariant` in `AddAsync`/`UpdateAsync`) throws an `ArgumentNullException`.  
- If the repository instance has been disposed (via a `Dispose` method not listed here but assumed to exist), any subsequent operation throws an `ObjectDisposedException`.  
- `SaveChangesAsync` may throw store‑specific exceptions (e.g., `DbUpdateException`) when constraints such as duplicate keys or foreign‑key violations are violated; callers should handle these exceptions according to their error‑handling strategy.  
- The `GetByIdAsync`, `GetByVideoShortIdAsync`, `GetActiveVariantsAsync`, `GetWinnerAsync`, and `GetAllAsync` methods are safe to call concurrently for read‑only scenarios, provided the underlying data store supports concurrent reads.  
- After a successful `DeleteAsync` call, the entity is considered removed; subsequent calls to `ExistsAsync` or `GetByIdAsync` for the same identifier will return `false` or `null`, respectively.  
- The `CountAsync` method reflects the state of the repository **before** any pending changes are persisted; to obtain a count that includes unsaved additions, call `SaveChangesAsync` first.
