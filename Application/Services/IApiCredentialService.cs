// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Service for managing API credentials
/// </summary>
public interface IApiCredentialService
{
    Task<ApiCredential?> GetActiveCredentialAsync(Guid userId);
    Task<ApiCredential> CreateCredentialAsync(Guid userId, ApiCredential credential);
    Task<ApiCredential> UpdateCredentialAsync(ApiCredential credential);
    Task<bool> RefreshAccessTokenAsync(Guid credentialId);
    Task<bool> ValidateCredentialAsync(Guid credentialId);
    Task<bool> RevokeCredentialAsync(Guid credentialId);
    Task<ApiCredential?> GetByIdAsync(Guid credentialId);
    Task<List<ApiCredential>> GetUserCredentialsAsync(Guid userId);
}
