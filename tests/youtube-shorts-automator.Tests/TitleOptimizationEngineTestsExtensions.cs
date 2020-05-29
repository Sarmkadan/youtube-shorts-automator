// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="TitleOptimizationEngineTests"/>.
/// </summary>
public static class TitleOptimizationEngineTestsExtensions
{
    /// <summary>
    /// Asserts that the suggestion has at least the minimum required confidence score.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="suggestion">The suggestion to check.</param>
    /// <param name="minConfidence">The minimum acceptable confidence score.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="suggestion"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the confidence score is below the minimum.</exception>
    public static void AssertSuggestionConfidence(this TitleOptimizationEngineTests tests, OptimizationSuggestion suggestion, double minConfidence)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(suggestion);

        if (suggestion.ConfidenceScore < minConfidence)
        {
            throw new InvalidOperationException($"Expected confidence score >= {minConfidence}, but found {suggestion.ConfidenceScore}");
        }
    }

    /// <summary>
    /// Asserts that the optimal posting hour matches the expected value.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="result">The optimization result to check.</param>
    /// <param name="expectedHour">The expected optimal posting hour.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="result"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the optimal posting hour does not match.</exception>
    public static void AssertOptimalPostingHour(this TitleOptimizationEngineTests tests, TitleOptimizationResult result, int expectedHour)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(result);

        if (Math.Abs(result.OptimalPostingHour - expectedHour) > 0.001)
        {
            throw new InvalidOperationException($"Expected optimal posting hour {expectedHour}, but found {result.OptimalPostingHour}");
        }
    }

    /// <summary>
    /// Asserts that the actual keywords match the expected keywords.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="actualKeywords">The actual keywords to check.</param>
    /// <param name="expectedKeywords">The expected keywords.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/>, <paramref name="actualKeywords"/> or <paramref name="expectedKeywords"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the keywords do not match.</exception>
    public static void AssertKeywordsMatch(this TitleOptimizationEngineTests tests, IReadOnlyList<string> actualKeywords, IEnumerable<string> expectedKeywords)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(actualKeywords);
        ArgumentNullException.ThrowIfNull(expectedKeywords);

        var expectedList = expectedKeywords.ToList();
        if (actualKeywords.Count != expectedList.Count || !actualKeywords.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).SequenceEqual(expectedList.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Keywords do not match. Expected: {string.Join(", ", expectedList)}, but found: {string.Join(", ", actualKeywords)}");
        }
    }
}
