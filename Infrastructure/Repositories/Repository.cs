// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for CRUD operations
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<Repository<TEntity>> _logger;

    public Repository(ApplicationDbContext context, ILogger<Repository<TEntity>> logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger;
    }

    /// <summary>
    /// Adds a single entity
    /// </summary>
    public virtual async Task AddAsync(TEntity entity)
    {
        _logger.LogDebug($"Adding entity of type {typeof(TEntity).Name}");
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        _logger.LogDebug($"Adding {entities.Count()} entities of type {typeof(TEntity).Name}");
        await _dbSet.AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Gets entity by ID
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all entities
    /// </summary>
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Updates an entity
    /// </summary>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        _logger.LogDebug($"Updating entity of type {typeof(TEntity).Name}");
        _dbSet.Update(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        _logger.LogDebug($"Updating {entities.Count()} entities of type {typeof(TEntity).Name}");
        _dbSet.UpdateRange(entities);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Deletes entity by ID
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        _logger.LogDebug($"Deleting entity of type {typeof(TEntity).Name} with ID {id}");
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    public virtual async Task DeleteRangeAsync(IEnumerable<Guid> ids)
    {
        var entities = new List<TEntity>();
        foreach (var id in ids)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                entities.Add(entity);
        }

        _logger.LogDebug($"Deleting {entities.Count} entities of type {typeof(TEntity).Name}");
        _dbSet.RemoveRange(entities);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Checks if entity exists
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    /// <summary>
    /// Gets total count of entities
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// <summary>
    /// Saves changes to database
    /// </summary>
    public virtual async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogDebug("Changes saved to database");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }
}
