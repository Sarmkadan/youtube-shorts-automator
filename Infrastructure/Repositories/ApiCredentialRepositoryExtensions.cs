using System;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Extension methods for <see cref="ApiCredentialRepository"/> providing convenience operations for API credential queries.
/// </summary>
public static class ApiCredentialRepositoryExtensions
{
    /// <summary>
    /// Gets the count of credentials with a specific status.
    /// </summary>
    /// <param name="repository">The credential repository instance.</param>
    /// <param name="status">The credential status to filter by.</param>
    /// <returns>The count of credentials matching the specified status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    public static async Task<int> CountByStatusAsync(this ApiCredentialRepository repository, CredentialStatus status)
    {
        ArgumentNullException.ThrowIfNull(repository);
        var credentials = await repository.GetByStatusAsync(status);
        return credentials.Count;
    }

    /// <summary>
    /// Checks if a user has an active credential.
    /// </summary>
    /// <param name="repository">The credential repository instance.</param>
    /// <param name="userId">The user identifier to check for active credentials.</param>
    /// <returns><see langword="true"/> if the user has an active credential; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    public static async Task<bool> HasActiveCredentialAsync(this ApiCredentialRepository repository, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(repository);
        var credential = await repository.GetActiveCredentialAsync(userId);
        return credential != null;
    }

    /// <summary>
    /// Gets the count of expired credentials.
    /// </summary>
    /// <param name="repository">The credential repository instance.</param>
    /// <returns>The count of credentials that have expired.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    public static async Task<int> GetExpiredCountAsync(this ApiCredentialRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        var credentials = await repository.GetExpiredCredentialsAsync();
        return credentials.Count;
    }
}
