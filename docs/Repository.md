# Repository

Generic repository class providing a virtual abstraction over data persistence operations for entities within the `youtube-shorts-automator` project. It encapsulates common CRUD and query methods, allowing derived implementations to supply the underlying storage mechanism while exposing a consistent async API for adding, retrieving, updating, deleting, and inspecting entities.

## API

### `public Repository`

Base constructor for the repository. Initializes the internal state required by the virtual methods. Derived classes are expected to invoke this constructor when providing a concrete implementation.

### `public virtual async Task AddAsync`

Persists a single entity to the underlying store. The entity is tracked for insertion but not necessarily committed until `SaveChangesAsync` is called.

- **Parameters:** The entity instance to add. Must not be null.
- **Returns:** A `Task` representing the asynchronous add operation.
- **Throws:** `ArgumentNullException` if the entity is null. May throw storage-specific exceptions if the entity violates constraints or the underlying connection is unavailable.

### `public virtual async Task AddRangeAsync`

Persists a collection of entities in a single batch operation. Entities are tracked for insertion but not necessarily committed until `SaveChangesAsync` is called.

- **Parameters:** An `IEnumerable` of entity instances to add. Must not be null and should not contain null elements.
- **Returns:** A `Task` representing the asynchronous batch add operation.
- **Throws:** `ArgumentNullException` if the collection is null. May throw if any individual entity is null or violates constraints.

### `public virtual async Task<TEntity?> GetByIdAsync`

Retrieves an entity by its primary key identifier. Returns the entity if found, or `null` if no matching record exists.

- **Parameters:** The primary key value of the entity to retrieve. The type must match the entity's key type.
- **Returns:** The entity instance cast to `TEntity`, or `null` if not found.
- **Throws:** May throw if the key type is incompatible or the underlying store encounters an error.

### `public virtual async Task<List<TEntity>> GetAllAsync`

Retrieves all entities of the repository's type from the underlying store. Returns an empty list if no entities exist.

- **Parameters:** None.
- **Returns:** A `List<TEntity>` containing all persisted entities. Never returns null; returns an empty list when no records are present.
- **Throws:** May throw if the underlying store encounters a connection or query error.

### `public virtual async Task UpdateAsync`

Marks an existing entity as modified in the underlying change tracker. The update is not persisted until `SaveChangesAsync` is called.

- **Parameters:** The entity instance with updated values. Must not be null and must correspond to an existing record.
- **Returns:** A `Task` representing the asynchronous update operation.
- **Throws:** `ArgumentNullException` if the entity is null. May throw if the entity does not exist in the store or a concurrency conflict is detected during save.

### `public virtual async Task UpdateRangeAsync`

Marks a collection of existing entities as modified in a single batch. Updates are not persisted until `SaveChangesAsync` is called.

- **Parameters:** An `IEnumerable` of entity instances with updated values. Must not be null.
- **Returns:** A `Task` representing the asynchronous batch update operation.
- **Throws:** `ArgumentNullException` if the collection is null. May throw if any entity does not exist or concurrency conflicts arise.

### `public virtual async Task DeleteAsync`

Marks an entity for deletion from the underlying store. The removal is not finalized until `SaveChangesAsync` is called.

- **Parameters:** The entity instance to delete. Must not be null and must correspond to an existing tracked or persisted record.
- **Returns:** A `Task` representing the asynchronous delete operation.
- **Throws:** `ArgumentNullException` if the entity is null. May throw if the entity is not found or foreign key constraints prevent deletion.

### `public virtual async Task DeleteRangeAsync`

Marks a collection of entities for deletion in a single batch. Removals are not finalized until `SaveChangesAsync` is called.

- **Parameters:** An `IEnumerable` of entity instances to delete. Must not be null.
- **Returns:** A `Task` representing the asynchronous batch delete operation.
- **Throws:** `ArgumentNullException` if the collection is null. May throw if any entity is not found or constraints are violated.

### `public virtual async Task<bool> ExistsAsync`

Determines whether an entity with the specified primary key exists in the store.

- **Parameters:** The primary key value to check.
- **Returns:** `true` if an entity with the given key exists; otherwise `false`.
- **Throws:** May throw if the key type is incompatible or the store is unreachable.

### `public virtual async Task<int> CountAsync`

Returns the total number of entities of the repository's type currently persisted in the store.

- **Parameters:** None.
- **Returns:** A non-negative integer representing the entity count.
- **Throws:** May throw if the underlying store encounters a connection or query error.

### `public virtual async Task SaveChangesAsync`

Commits all pending tracked changes—adds, updates, and deletions—to the underlying persistent store in a single transaction.

- **Parameters:** None.
- **Returns:** A `Task` representing the asynchronous save operation.
- **Throws:** May throw if any pending change violates constraints, a concurrency conflict is detected, or the store is unavailable. Partial failures are not expected; the operation should be atomic.

## Usage

### Example 1: Adding and saving a new entity

```csharp
var repository = new ConcreteRepository<ShortEntity>();
var newShort = new ShortEntity
{
    Title = "My Automated Short",
    ScheduledTime = DateTime.UtcNow.AddHours(1)
};

await repository.AddAsync(newShort);
await repository.SaveChangesAsync();
```

### Example 2: Retrieving, updating, and deleting entities in a workflow

```csharp
var repository = new ConcreteRepository<ShortEntity>();

// Check existence and retrieve
bool exists = await repository.ExistsAsync(shortId);
if (exists)
{
    var short = await repository.GetByIdAsync(shortId);
    if (short != null)
    {
        short.Title = "Updated Title";
        await repository.UpdateAsync(short);
        await repository.SaveChangesAsync();
    }
}

// Batch delete completed shorts
var allShorts = await repository.GetAllAsync();
var completed = allShorts.Where(s => s.IsCompleted).ToList();
if (completed.Any())
{
    await repository.DeleteRangeAsync(completed);
    await repository.SaveChangesAsync();
}
```

## Notes

- All mutation methods (`AddAsync`, `UpdateAsync`, `DeleteAsync`, and their range variants) defer persistence until `SaveChangesAsync` is explicitly called. Failing to call `SaveChangesAsync` after modifications will result in lost changes.
- `GetByIdAsync` returns `null` for missing entities; callers must perform null checks before accessing returned instances.
- `GetAllAsync` returns an empty list rather than null when no entities exist, simplifying iteration logic.
- The virtual design allows derived repositories to override behavior for specific storage backends. The base implementations may throw storage-specific exceptions that callers should handle generically or allow to propagate.
- Thread safety is not guaranteed by the base class. Concurrent calls to mutation methods followed by a single `SaveChangesAsync` may produce unpredictable ordering or concurrency exceptions depending on the underlying store's change tracker. Callers should serialize write operations or rely on the concrete implementation's synchronization guarantees if provided.
- Batch methods (`AddRangeAsync`, `UpdateRangeAsync`, `DeleteRangeAsync`) expect non-null collections. Null elements within a collection may cause exceptions depending on the concrete implementation's validation.
