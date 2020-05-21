// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for <see cref="TitleOptimizationEngine"/> to simplify common
/// optimisation tasks and data retrieval.
/// </summary>
public static class TitleOptimizationEngineExtensions
{
    /// <summary>
    /// Checks if a title is already considered highly optimised based on its score.
    /// </summary>
    /// <param name="engine">The optimisation engine instance.</param>
    /// <param name="title">The title to evaluate.</param>
    /// <returns><c>true</c> if the title score is 0.7 or higher; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static bool IsHighlyOptimised(this TitleOptimizationEngine engine, string title)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return engine.ScoreTitle(title) >= 0.7;
    }

    /// <summary>
    /// Extracts keywords and formats them as a comma-separated string for UI display or logging.
    /// </summary>
    /// <param name="engine">The optimisation engine instance.</param>
    /// <param name="title">The title to extract keywords from.</param>
    /// <param name="description">The description to extract keywords from.</param>
    /// <returns>A comma-separated string of extracted keywords.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static string GetFormattedKeywords(this TitleOptimizationEngine engine, string title, string description)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var keywords = engine.ExtractKeywords(title, description);
        return string.Join(", ", keywords);
    }

    /// <summary>
    /// Gets the next recommended posting slot as a formatted string.
    /// </summary>
    /// <param name="engine">The optimisation engine instance.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string representation of the next recommended posting time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static async Task<string> GetNextPostingSlotFormattedAsync(
        this TitleOptimizationEngine engine,
        int channelId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var slots = await engine.RecommendPostingTimesAsync(channelId, 1, cancellationToken);
        var slot = slots.FirstOrDefault();

        return slot == default
            ? "No slots available"
            : slot.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
