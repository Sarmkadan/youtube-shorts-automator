// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Linq.Expressions;
using YouTubeShortsAutomator.Application.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Extension methods for Repository to provide additional convenience operations
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Gets the first entity matching the specified predicate, or null if not found
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="predicate">Filter expression</param>
    /// <returns>Matching entity or null</returns>
    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(
        this IRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var allEntities = await repository.GetAllAsync();
        return allEntities.FirstOrDefault(predicate.Compile());
    }

    /// <summary>
    /// Gets the first entity matching the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="predicate">Filter expression</param>
    /// <returns>Matching entity</returns>
    /// <exception cref="InvalidOperationException">Thrown if no entity matches</exception>
    public static async Task<TEntity> FirstAsync<TEntity>(
        this IRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var allEntities = await repository.GetAllAsync();
        return allEntities.First(predicate.Compile());
    }

    /// <summary>
    /// Gets all entities matching the specified predicate
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="predicate">Filter expression</param>
    /// <returns>List of matching entities</returns>
    public static async Task<List<TEntity>> WhereAsync<TEntity>(
        this IRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var allEntities = await repository.GetAllAsync();
        return allEntities.Where(predicate.Compile()).ToList();
    }

    /// <summary>
    /// Gets a single entity matching the specified predicate, or null if not found
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="predicate">Filter expression</param>
    /// <returns>Single matching entity or null</returns>
    /// <exception cref="InvalidOperationException">Thrown if multiple entities match</exception>
    public static async Task<TEntity?> SingleOrDefaultAsync<TEntity>(
        this IRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var allEntities = await repository.GetAllAsync();
        return allEntities.SingleOrDefault(predicate.Compile());
    }
}