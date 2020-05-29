using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Data;

/// <summary>Provides extension methods for <see cref="ThumbnailAbTestRepository"/>.</summary>
public static class ThumbnailAbTestRepositoryExtensions
{
    /// <summary>
    /// Gets the variant with the highest view rate for a specific video.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The best performing variant, or null if none found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<ThumbnailVariant?> GetBestPerformingAsync(
        this ThumbnailAbTestRepository repository,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var variants = await repository.GetByVideoShortIdAsync(videoShortId, cancellationToken);
        return variants.OrderByDescending(v => v.ViewRate).FirstOrDefault();
    }

    /// <summary>
    /// Calculates the total number of impressions across all thumbnail variants for a specific video.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sum of all impressions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<long> GetTotalImpressionsAsync(
        this ThumbnailAbTestRepository repository,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var variants = await repository.GetByVideoShortIdAsync(videoShortId, cancellationToken);
        return variants.Sum(v => v.ImpressionCount);
    }

    /// <summary>
    /// Calculates the total number of clicks across all thumbnail variants for a specific video.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="videoShortId">The video identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sum of all clicks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static async Task<long> GetTotalClicksAsync(
        this ThumbnailAbTestRepository repository,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var variants = await repository.GetByVideoShortIdAsync(videoShortId, cancellationToken);
        return variants.Sum(v => v.ClickCount);
    }
}
