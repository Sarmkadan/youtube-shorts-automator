// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Generic data access contract for entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type persisted by the repository.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching entity, or <c>null</c> if no entity has the given identifier.</returns>
    Task<T?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities held by the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A sequence of all entities; empty if there are none.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The added entity, as returned by the underlying store.</returns>
    Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated entity, as returned by the underlying store.</returns>
    Task<T> UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if an entity was deleted; <c>false</c> if no entity had the given identifier.</returns>
    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier to look for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if an entity with the given identifier exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total number of entities held by the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists any pending changes to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the changes have been saved.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
