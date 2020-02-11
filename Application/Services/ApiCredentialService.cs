// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Exceptions;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Manages user API credentials and token refresh
/// </summary>
public class ApiCredentialService : IApiCredentialService
{
    private readonly ILogger<ApiCredentialService> _logger;
    private readonly IApiCredentialRepository _credentialRepository;

    public ApiCredentialService(
        ILogger<ApiCredentialService> logger,
        IApiCredentialRepository credentialRepository)
    {
        _logger = logger;
        _credentialRepository = credentialRepository;
    }

    /// <summary>
    /// Gets the active credential for a user
    /// </summary>
    public async Task<ApiCredential?> GetActiveCredentialAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving active credential for user {userId}");

        var credentials = await _credentialRepository.GetByUserIdAsync(userId);
        var activeCredential = credentials.FirstOrDefault(c =>
            c.Status == CredentialStatus.Active &&
            c.IsValid &&
            !c.IsAccessTokenExpired());

        if (activeCredential != null)
        {
            if (activeCredential.NeedsRefresh())
            {
                _logger.LogInformation($"Credential needs refresh, refreshing now");
                await RefreshAccessTokenAsync(activeCredential.Id);
            }

            return activeCredential;
        }

        // Try to find expired but otherwise valid credential and refresh it
        var expiredCredential = credentials.FirstOrDefault(c =>
            c.Status == CredentialStatus.Active &&
            c.IsValid &&
            c.IsAccessTokenExpired());

        if (expiredCredential != null)
        {
            if (expiredCredential.IsRefreshTokenExpired())
            {
                _logger.LogError(
                    "Refresh token for credential {CredentialId} (user {UserId}) has expired. " +
                    "Re-authorization is required.",
                    expiredCredential.Id, userId);
                throw new OAuthTokenExpiredException(expiredCredential.Id);
            }

            _logger.LogInformation($"Attempting to refresh expired credential");
            await RefreshAccessTokenAsync(expiredCredential.Id);
            return expiredCredential;
        }

        _logger.LogWarning($"No active credentials found for user {userId}");
        return null;
    }

    /// <summary>
    /// Creates a new credential for a user
    /// </summary>
    public async Task<ApiCredential> CreateCredentialAsync(Guid userId, ApiCredential credential)
    {
        _logger.LogInformation($"Creating new credential for user {userId}");

        var (isValid, errors) = credential.Validate();
        if (!isValid)
            throw new CredentialException($"Invalid credential: {string.Join(", ", errors)}");

        credential.UserId = userId;
        credential.CreatedAt = DateTime.UtcNow;
        credential.Status = CredentialStatus.Active;

        await _credentialRepository.AddAsync(credential);

        _logger.LogInformation($"Credential created successfully");
        return credential;
    }

    /// <summary>
    /// Updates an existing credential
    /// </summary>
    public async Task<ApiCredential> UpdateCredentialAsync(ApiCredential credential)
    {
        _logger.LogInformation($"Updating credential {credential.Id}");

        var existing = await _credentialRepository.GetByIdAsync(credential.Id)
            ?? throw new ResourceNotFoundException("Credential not found", credential.Id, "ApiCredential");

        var (isValid, errors) = credential.Validate();
        if (!isValid)
            throw new CredentialException($"Invalid credential: {string.Join(", ", errors)}");

        existing.ClientSecret = credential.ClientSecret;
        existing.AccessToken = credential.AccessToken;
        existing.RefreshToken = credential.RefreshToken;
        existing.AccessTokenExpiresAt = credential.AccessTokenExpiresAt;
        existing.UpdatedAt = DateTime.UtcNow;

        await _credentialRepository.UpdateAsync(existing);

        _logger.LogInformation($"Credential updated successfully");
        return existing;
    }

    /// <summary>
    /// Refreshes the access token
    /// </summary>
    public async Task<bool> RefreshAccessTokenAsync(Guid credentialId)
    {
        _logger.LogInformation($"Refreshing access token for credential {credentialId}");

        var credential = await _credentialRepository.GetByIdAsync(credentialId)
            ?? throw new ResourceNotFoundException("Credential not found", credentialId, "ApiCredential");

        if (string.IsNullOrWhiteSpace(credential.RefreshToken))
            throw new CredentialException("Refresh token is not available");

        try
        {
            // Simulate OAuth token refresh call.
            // In a real implementation this calls the OAuth token endpoint and inspects the
            // HTTP response.  A 400 with error=invalid_grant means the refresh token has
            // expired (common after 7 days in OAuth testing mode) and is not retryable.
            var newAccessToken = $"access_{Guid.NewGuid().ToString().Substring(0, 20)}";
            var newExpiresAt = DateTime.UtcNow.AddHours(1);

            credential.UpdateAccessToken(newAccessToken, newExpiresAt);
            credential.Status = CredentialStatus.Active;

            await _credentialRepository.UpdateAsync(credential);

            _logger.LogInformation($"Access token refreshed successfully");
            return true;
        }
        catch (OAuthTokenExpiredException)
        {
            // Refresh token itself has expired — mark immediately and do not retry.
            credential.MarkRefreshTokenExpired();
            await _credentialRepository.UpdateAsync(credential);
            _logger.LogError(
                "Refresh token for credential {CredentialId} has expired (invalid_grant). " +
                "Re-authorization is required. Uploads for this credential will be halted.",
                credentialId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refresh access token");
            credential.RecordRefreshAttempt();
            await _credentialRepository.UpdateAsync(credential);
            throw new CredentialException($"Token refresh failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a credential
    /// </summary>
    public async Task<bool> ValidateCredentialAsync(Guid credentialId)
    {
        _logger.LogInformation($"Validating credential {credentialId}");

        var credential = await _credentialRepository.GetByIdAsync(credentialId)
            ?? throw new ResourceNotFoundException("Credential not found", credentialId, "ApiCredential");

        if (credential.IsAccessTokenExpired())
        {
            _logger.LogWarning($"Access token expired");
            return false;
        }

        if (!credential.IsValid)
        {
            _logger.LogWarning($"Credential is marked invalid");
            return false;
        }

        _logger.LogInformation($"Credential is valid");
        return true;
    }

    /// <summary>
    /// Revokes a credential
    /// </summary>
    public async Task<bool> RevokeCredentialAsync(Guid credentialId)
    {
        _logger.LogInformation($"Revoking credential {credentialId}");

        var credential = await _credentialRepository.GetByIdAsync(credentialId)
            ?? throw new ResourceNotFoundException("Credential not found", credentialId, "ApiCredential");

        credential.Invalidate("Credential revoked by user");
        credential.Status = CredentialStatus.Revoked;

        await _credentialRepository.UpdateAsync(credential);

        _logger.LogInformation($"Credential revoked successfully");
        return true;
    }

    /// <summary>
    /// Gets a credential by ID
    /// </summary>
    public async Task<ApiCredential?> GetByIdAsync(Guid credentialId)
    {
        return await _credentialRepository.GetByIdAsync(credentialId);
    }

    /// <summary>
    /// Gets all credentials for a user
    /// </summary>
    public async Task<List<ApiCredential>> GetUserCredentialsAsync(Guid userId)
    {
        _logger.LogInformation($"Retrieving all credentials for user {userId}");
        return await _credentialRepository.GetByUserIdAsync(userId);
    }
}
