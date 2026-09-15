// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides side-effect-free helper methods for comparing <see cref="ThumbnailVariant"/> instances.
/// </summary>
public static class ThumbnailVariantHelperExtensions
{
    /// <summary>
    /// Compares the score of one thumbnail variant with another, using
    /// <see cref="ThumbnailVariant.ViewRate"/> as the score.
    /// </summary>
    /// <param name="variant">The thumbnail variant to compare.</param>
    /// <param name="other">The thumbnail variant to compare against.</param>
    /// <returns>
    /// A value less than zero when <paramref name="variant"/> has a lower score;
    /// zero when both scores are equal; or a value greater than zero when
    /// <paramref name="variant"/> has a higher score.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variant"/> or <paramref name="other"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static int CompareScoreTo(this ThumbnailVariant variant, ThumbnailVariant other)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(other);

        return variant.ViewRate.CompareTo(other.ViewRate);
    }

    /// <summary>
    /// Determines whether a thumbnail variant has a higher score than another variant,
    /// using <see cref="ThumbnailVariant.ViewRate"/> as the score.
    /// </summary>
    /// <param name="variant">The thumbnail variant to evaluate.</param>
    /// <param name="other">The thumbnail variant to compare against.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="variant"/> has a higher score than
    /// <paramref name="other"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variant"/> or <paramref name="other"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static bool HasHigherScoreThan(this ThumbnailVariant variant, ThumbnailVariant other)
    {
        return variant.CompareScoreTo(other) > 0;
    }
}
