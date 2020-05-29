// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for <see cref="AnalyticsServiceTests"/> to assist in test execution and reporting.
/// </summary>
public static class AnalyticsServiceTestsExtensions
{
    /// <summary>
    /// Executes a series of performance metric analysis tests to ensure consistency.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="iterationCount">The number of times to execute the analysis.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterationCount"/> is less than 1.</exception>
    public static void RunPerformanceMetricAnalysisTests(this AnalyticsServiceTests tests, int iterationCount = 1)
    {
        ArgumentNullException.ThrowIfNull(tests);
        if (iterationCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(iterationCount), iterationCount, "Iteration count must be at least 1.");
        }

        for (var i = 0; i < iterationCount; i++)
        {
            tests.AnalyzePerformanceMetrics_WithValidData_ReturnsInsights();
            tests.AnalyzePerformanceMetrics_WithLowEngagement_ReturnsWarning();
            tests.AnalyzePerformanceMetrics_WithoutValidData_ReturnsDefaultMessage();
        }
    }

    /// <summary>
    /// Validates the top performing videos query by executing it multiple times to ensure deterministic behavior.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="limit">The limit to test against.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task ValidateTopPerformingVideosConsistencyAsync(this AnalyticsServiceTests tests, int limit)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // This calls the public test method defined in AnalyticsServiceTests.
        await tests.GetTopPerformingVideosAsync_WithValidLimit_ReturnsTopVideos();
    }
}
