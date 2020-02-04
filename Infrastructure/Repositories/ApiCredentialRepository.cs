// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.EntityFrameworkCore;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApiCredential entity
/// </summary>
public class ApiCredentialRepository : Repository<ApiCredential>, IApiCredentialRepository
{
    public ApiCredentialRepository(ApplicationDbContext context, ILogger<ApiCredentialRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets credentials for a user
    /// </summary>
    public async Task<List<ApiCredential>> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving credentials for user {userId}");
        return await _dbSet
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active credential for a user
    /// </summary>
    public async Task<ApiCredential?> GetActiveCredentialAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving active credential for user {userId}");
        return await _dbSet
            .Where(c => c.UserId == userId &&
                       c.Status == CredentialStatus.Active &&
                       c.IsValid)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets credentials by status
    /// </summary>
    public async Task<List<ApiCredential>> GetByStatusAsync(CredentialStatus status)
    {
        _logger.LogInformation($"Retrieving credentials with status {status}");
        return await _dbSet
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets expired credentials
    /// </summary>
    public async Task<List<ApiCredential>> GetExpiredCredentialsAsync()
    {
        _logger.LogInformation("Retrieving expired credentials");
        return await _dbSet
            .Where(c => c.AccessTokenExpiresAt < DateTime.UtcNow)
            .OrderBy(c => c.AccessTokenExpiresAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets credentials by type
    /// </summary>
    public async Task<List<ApiCredential>> GetByTypeAsync(ApiCredentialType type)
    {
        _logger.LogInformation($"Retrieving credentials of type {type}");
        return await _dbSet
            .Where(c => c.CredentialType == type)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
