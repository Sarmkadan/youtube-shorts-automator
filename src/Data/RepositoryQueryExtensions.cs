// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Side-effect-free query helpers for any IRepository<T>, built only on the
// existing GetByIdAsync, GetAllAsync and CountAsync members
// =============================================================================

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Read-only convenience methods for <see cref="IRepository{T}"/>. Nothing here adds state
/// or writes to storage; every method delegates to the repository's existing members.
/// </summary>
public static class RepositoryQueryExtensions
{
    /// <summary>
    /// Looks up an entity by identifier and reports whether it was found, without forcing
    /// callers to null-check the result separately.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple where <c>Found</c> is <see langword="true"/> when an entity exists, and
    /// <c>Entity</c> holds it (or <see langword="null"/> when not found).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    public static async Task<(bool Found, T? Entity)> TryGetByIdAsync<T>(
        this IRepository<T> repository,
        int id,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(repository);

        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return (entity is not null, entity);
    }

    /// <summary>
    /// Returns one page of entities. Paging is applied to the sequence yielded by
    /// <see cref="IRepository{T}.GetAllAsync"/>, so the order is whatever that method returns.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entities on the requested page; empty when the page is beyond the last one.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> or <paramref name="pageSize"/> is less than 1.</exception>
    public static async Task<IReadOnlyList<T>> GetPageAsync<T>(
        this IRepository<T> repository,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var all = await repository.GetAllAsync(cancellationToken);
        return all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// Returns the number of pages needed to hold all entities at the given page size.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total page count; zero when the repository is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than 1.</exception>
    public static async Task<int> GetPageCountAsync<T>(
        this IRepository<T> repository,
        int pageSize,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var total = await repository.CountAsync(cancellationToken);
        return (int)Math.Ceiling(total / (double)pageSize);
    }
}
