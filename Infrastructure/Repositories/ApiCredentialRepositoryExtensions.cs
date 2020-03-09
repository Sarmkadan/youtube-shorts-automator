using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Extension methods for ApiCredentialRepository
/// </summary>
public static class ApiCredentialRepositoryExtensions
{
    /// <summary>
    /// Gets the count of credentials with a specific status
    /// </summary>
    public static async Task<int> CountByStatusAsync(this ApiCredentialRepository repository, CredentialStatus status)
    {
        var credentials = await repository.GetByStatusAsync(status);
        return credentials.Count;
    }

    /// <summary>
    /// Checks if a user has an active credential
    /// </summary>
    public static async Task<bool> HasActiveCredentialAsync(this ApiCredentialRepository repository, Guid userId)
    {
        var credential = await repository.GetActiveCredentialAsync(userId);
        return credential != null;
    }

    /// <summary>
    /// Gets the count of expired credentials
    /// </summary>
    public static async Task<int> GetExpiredCountAsync(this ApiCredentialRepository repository)
    {
        var credentials = await repository.GetExpiredCredentialsAsync();
        return credentials.Count;
    }
}
