// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="ThumbnailVariant"/> to enhance functionality
/// with common operations for thumbnail A/B testing scenarios.
/// </summary>
public static class ThumbnailVariantExtensions
{
    /// <summary>
    /// Calculates the conversion rate (clicks per impression) as a percentage.
    /// </summary>
    /// <param name="variant">The thumbnail variant to calculate conversion for.</param>
    /// <returns>The conversion rate percentage (0-100), or 0 if no impressions recorded.</returns>
    public static double GetConversionRate(this ThumbnailVariant variant)
    {
        if (variant.ImpressionCount == 0)
            return 0;

        return (double)variant.ClickCount / variant.ImpressionCount * 100.0;
    }

    /// <summary>
    /// Determines if this variant is performing better than another variant based on view rate.
    /// </summary>
    /// <param name="variant">The current variant.</param>
    /// <param name="other">The variant to compare against.</param>
    /// <returns>True if this variant has a higher view rate than the other.</returns>
    public static bool IsBetterThan(this ThumbnailVariant variant, ThumbnailVariant other)
    {
        if (other == null)
            return true;

        return variant.ViewRate > other.ViewRate;
    }

    /// <summary>
    /// Gets the performance status based on the view rate and data sufficiency.
    /// </summary>
    /// <param name="variant">The thumbnail variant.</param>
    /// <param name="minimumImpressions">Minimum impressions threshold for meaningful analysis.</param>
    /// <returns>A string representing the performance status: "Excellent", "Good", "Needs More Data", or "Poor".</returns>
    public static string GetPerformanceStatus(this ThumbnailVariant variant, long minimumImpressions = 500)
    {
        if (!variant.HasSufficientData(minimumImpressions))
            return "Needs More Data";

        if (variant.ViewRate >= 5.0)
            return "Excellent";

        if (variant.ViewRate >= 2.5)
            return "Good";

        return "Poor";
    }

    /// <summary>
    /// Calculates the relative improvement percentage compared to another variant.
    /// </summary>
    /// <param name="variant">The current variant.</param>
    /// <param name="baseline">The baseline variant to compare against.</param>
    /// <returns>The relative improvement percentage, or 0 if baseline has no data.</returns>
    public static double GetRelativeImprovement(this ThumbnailVariant variant, ThumbnailVariant baseline)
    {
        if (baseline == null || baseline.ImpressionCount == 0 || baseline.ViewRate == 0)
            return 0;

        return ((variant.ViewRate - baseline.ViewRate) / baseline.ViewRate) * 100.0;
    }
}