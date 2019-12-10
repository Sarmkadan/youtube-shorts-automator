// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Repositories;

/// <summary>
/// Repository for ApiCredential entity operations
/// </summary>
public interface IApiCredentialRepository : IRepository<ApiCredential>
{
    Task<List<ApiCredential>> GetByUserIdAsync(Guid userId);
    Task<ApiCredential?> GetActiveCredentialAsync(Guid userId);
    Task<List<ApiCredential>> GetByStatusAsync(CredentialStatus status);
    Task<List<ApiCredential>> GetExpiredCredentialsAsync();
    Task<List<ApiCredential>> GetByTypeAsync(ApiCredentialType type);
}
