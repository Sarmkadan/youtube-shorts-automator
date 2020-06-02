// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using YouTubeShortsAutomator.Application.Services;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Extension methods for <see cref="ApiCredentialService"/> providing common credential operations
/// </summary>
public static class ApiCredentialServiceExtensions
{
    /// <summary>
    /// Gets the active credential for a user, or creates a new one if none exists
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <param name="credentialType">The type of credential to create if needed</param>
    /// <returns>The active credential or newly created credential</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<ApiCredential> GetOrCreateActiveCredentialAsync(
        this ApiCredentialService service,
        Guid userId,
        ApiCredentialType credentialType = ApiCredentialType.GoogleOAuth)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credential = await service.GetActiveCredentialAsync(userId);

        if (credential != null)
        {
            return credential;
        }

        // Create a minimal valid credential
        var newCredential = new ApiCredential
        {
            ClientId = Guid.NewGuid().ToString("N"),
            ClientSecret = Guid.NewGuid().ToString("N"),
            AccessToken = Guid.NewGuid().ToString("N"),
            RefreshToken = Guid.NewGuid().ToString("N"),
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            CredentialType = credentialType,
            IsValid = true,
            Status = CredentialStatus.Active
        };

        return await service.CreateCredentialAsync(userId, newCredential);
    }

    /// <summary>
    /// Gets all valid credentials for a user
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <returns>Read-only list of valid credentials</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<IReadOnlyList<ApiCredential>> GetValidCredentialsAsync(
        this ApiCredentialService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var allCredentials = await service.GetUserCredentialsAsync(userId);
        return allCredentials
            .Where(c => c.IsValid && c.Status == CredentialStatus.Active)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the credential with the specified type for a user
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <param name="credentialType">The credential type to find</param>
    /// <returns>The credential if found, otherwise null</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<ApiCredential?> GetCredentialByTypeAsync(
        this ApiCredentialService service,
        Guid userId,
        ApiCredentialType credentialType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credentials = await service.GetUserCredentialsAsync(userId);
        return credentials.FirstOrDefault(c => c.CredentialType == credentialType);
    }

    /// <summary>
    /// Checks if any credential for the user needs refresh (within 30 minutes of expiration)
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <returns>True if any credential needs refresh, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<bool> AnyCredentialNeedsRefreshAsync(
        this ApiCredentialService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credentials = await service.GetUserCredentialsAsync(userId);
        return credentials.Any(c => c.IsValid && c.Status == CredentialStatus.Active && c.NeedsRefresh());
    }

    /// <summary>
    /// Gets the time remaining until the first credential expires
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <returns>TimeSpan until expiration, or null if no active credentials</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<TimeSpan?> GetTimeUntilFirstExpirationAsync(
        this ApiCredentialService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credentials = await service.GetUserCredentialsAsync(userId);
        var activeCredentials = credentials
            .Where(c => c.IsValid && c.Status == CredentialStatus.Active)
            .ToList();

        if (activeCredentials.Count == 0)
        {
            return null;
        }

        var soonestExpiration = activeCredentials
            .Min(c => c.AccessTokenExpiresAt);

        var timeRemaining = soonestExpiration - DateTime.UtcNow;
        return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Refreshes all expired credentials for a user
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <returns>Number of successfully refreshed credentials</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<int> RefreshAllExpiredCredentialsAsync(
        this ApiCredentialService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credentials = await service.GetUserCredentialsAsync(userId);
        var expiredCredentials = credentials
            .Where(c => c.IsValid && c.Status == CredentialStatus.Active && c.IsAccessTokenExpired())
            .ToList();

        var refreshedCount = 0;

        foreach (var credential in expiredCredentials)
        {
            try
            {
                await service.RefreshAccessTokenAsync(credential.Id);
                refreshedCount++;
            }
            catch
            {
                // Skip credentials that can't be refreshed
                continue;
            }
        }

        return refreshedCount;
    }

    /// <summary>
    /// Gets credentials ordered by expiration time (soonest first)
    /// </summary>
    /// <param name="service">The credential service</param>
    /// <param name="userId">The user ID</param>
    /// <returns>Read-only list of credentials ordered by expiration</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or userId is null</exception>
    public static async Task<IReadOnlyList<ApiCredential>> GetCredentialsOrderedByExpirationAsync(
        this ApiCredentialService service,
        Guid userId)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(userId);

        var credentials = await service.GetUserCredentialsAsync(userId);
        return credentials
            .OrderBy(c => c.AccessTokenExpiresAt)
            .ToList()
            .AsReadOnly();
    }
}