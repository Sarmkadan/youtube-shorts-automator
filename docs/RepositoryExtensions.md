# RepositoryExtensions

Provides asynchronous extension methods over a repository abstraction to simplify common query patterns such as retrieving single entities, first matches, and filtered collections. These methods assume an underlying `IQueryable<TEntity>` source and execute queries asynchronously with cancellation support.

## API

### FirstOrDefaultAsync\<TEntity\>

```csharp
public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(
    this IRepository<TEntity> repository,
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
```

Returns the first entity that satisfies the predicate, or the default value of `TEntity` if no matching entity is found. The predicate is required; passing `null` throws `ArgumentNullException`. If the underlying query provider throws during execution, that exception propagates to the caller.

### FirstAsync\<TEntity\>

```csharp
public static async Task<TEntity> FirstAsync<TEntity>(
    this IRepository<TEntity> repository,
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
```

Returns the first entity that satisfies the predicate. Throws `InvalidOperationException` when no entity matches the predicate. The predicate parameter must not be `null`; otherwise `ArgumentNullException` is thrown. Exceptions from the query provider are not caught.

### WhereAsync\<TEntity\>

```csharp
public static async Task<List<TEntity>> WhereAsync<TEntity>(
    this IRepository<TEntity> repository,
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
```

Materializes all entities matching the predicate into a `List<TEntity>`. A `null` predicate throws `ArgumentNullException`. Returns an empty list when no entities match. Query provider failures surface as exceptions.

### SingleOrDefaultAsync\<TEntity\>

```csharp
public static async Task<TEntity?> SingleOrDefaultAsync<TEntity>(
    this IRepository<TEntity> repository,
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
```

Returns the single entity that satisfies the predicate, or the default value of `TEntity` if no match exists. Throws `InvalidOperationException` when more than one entity matches the predicate. Passing a `null` predicate throws `ArgumentNullException`. Underlying query provider exceptions propagate normally.

## Usage

```csharp
// Retrieve a specific Shorts video by its content hash, or null if absent
var existing = await repository.FirstOrDefaultAsync<ShortsVideo>(
    v => v.ContentHash == newHash,
    cancellationToken);

if (existing is null)
{
    // No duplicate — proceed with creation
    await repository.AddAsync(newVideo, cancellationToken);
}
```

```csharp
// Fetch all Shorts scheduled for publishing within the next hour
var upcoming = await repository.WhereAsync<ShortsVideo>(
    v => v.ScheduledPublishTime > DateTime.UtcNow
         && v.ScheduledPublishTime <= DateTime.UtcNow.AddHours(1),
    cancellationToken);

foreach (var shorts in upcoming)
{
    await publisher.EnqueueAsync(shorts, cancellationToken);
}
```

## Notes

- All methods accept a `CancellationToken`; if cancellation is requested before or during execution, the underlying async query will throw `OperationCanceledException` (or a derived `TaskCanceledException`).
- These extensions are not thread-safe by themselves. The safety guarantees depend entirely on the `IRepository<TEntity>` implementation and the underlying data source. Concurrent calls against the same repository instance may produce inconsistent results if the repository shares mutable state.
- `SingleOrDefaultAsync` enforces the "at most one" constraint in memory after materialization. If the underlying store can enforce uniqueness (e.g., a database unique index), consider combining both layers to avoid race conditions where two matching entities are inserted between the query and a subsequent write.
- `FirstAsync` and `SingleOrDefaultAsync` throw `InvalidOperationException` for empty and multiple-match cases respectively. Callers should handle these explicitly when the absence of a result is a recoverable condition rather than a programming error.
