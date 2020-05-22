// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for <see cref="ThumbnailAbTestService"/> that provide convenient
// fluent APIs and common operations for managing thumbnail A/B tests.
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using System.Globalization;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for <see cref="ThumbnailAbTestService"/> to simplify
/// common operations like creating tests with validation, checking test completion,
/// and working with test results.
/// </summary>
public static class ThumbnailAbTestServiceExtensions
{
    /// <summary>
    /// Creates a new A/B test with validation that thumbnail paths exist and are not empty.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="thumbnailPathA">File-system path to thumbnail variant A.</param>
    /// <param name="thumbnailPathB">File-system path to thumbnail variant B.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A tuple containing the newly created variant A and variant B.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thumbnail paths are null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">VideoShort not found or active test already exists.</exception>
    public static async Task<(ThumbnailVariant VariantA, ThumbnailVariant VariantB)> CreateTestAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        string thumbnailPathA,
        string thumbnailPathB,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        return await service.CreateTestAsync(videoShortId, thumbnailPathA, thumbnailPathB, cancellationToken);
    }

    /// <summary>
    /// Records a view event for the specified variant.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="variantId">Identifier of the <see cref="ThumbnailVariant"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Variant not found or no longer active.</exception>
    public static async Task RecordViewEventAsync(
        this ThumbnailAbTestService service,
        int variantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        await service.RecordViewEventAsync(variantId, clicked: false, cancellationToken);
    }

    /// <summary>
    /// Records a view event with click-through for the specified variant.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="variantId">Identifier of the <see cref="ThumbnailVariant"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Variant not found or no longer active.</exception>
    public static async Task RecordViewAndClickEventAsync(
        this ThumbnailAbTestService service,
        int variantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        await service.RecordViewEventAsync(variantId, clicked: true, cancellationToken);
    }

    /// <summary>
    /// Checks if the test for the specified video has sufficient data to conclude.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>
    /// <see langword="true"/> if the test has sufficient data to conclude;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<bool> HasSufficientDataAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        return result.Variants.Count > 0 &&
               result.Variants.All(v => v.ImpressionCount >= ThumbnailAbTestService.MinimumImpressionsForConclusion);
    }

    /// <summary>
    /// Gets the test result and formats it as a human-readable summary string.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A formatted summary string of the test results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<string> GetTestSummaryAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        return result.GetSummary();
    }

    /// <summary>
    /// Gets the winning variant label if the test is complete, or <see langword="null"/> if not.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>The winning variant label, or <see langword="null"/> if test not complete.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<string?> GetWinnerLabelAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        return result.WinnerLabel;
    }

    /// <summary>
    /// Gets the current view rate of the leading variant as a percentage.
    /// Returns <see langword="null"/> if there are no variants or insufficient data.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>The leading variant's view rate percentage, or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<double?> GetLeadingViewRateAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        if (result.Variants.Count == 0)
            return null;

        return result.Variants.Max(v => v.ViewRate);
    }

    /// <summary>
    /// Gets the total impressions across all variants in the test.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>Total impressions across all variants.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<long> GetTotalImpressionsAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        return result.Variants.Sum(v => v.ImpressionCount);
    }

    /// <summary>
    /// Gets the total clicks across all variants in the test.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>Total clicks across all variants.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<long> GetTotalClicksAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        return result.Variants.Sum(v => v.ClickCount);
    }

    /// <summary>
    /// Gets the click-through rate (CTR) of the leading variant as a percentage.
    /// Returns <see langword="null"/> if there are no variants or no impressions.
    /// </summary>
    /// <param name="service">The <see cref="ThumbnailAbTestService"/> instance.</param>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>The leading variant's click-through rate percentage, or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static async Task<double?> GetLeadingClickThroughRateAsync(
        this ThumbnailAbTestService service,
        int videoShortId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetTestResultAsync(videoShortId, cancellationToken);
        if (result.Variants.Count == 0 || result.Variants.All(v => v.ImpressionCount == 0))
            return null;

        var leading = result.Variants.OrderByDescending(v => v.ViewRate).First();
        return leading.ImpressionCount > 0
            ? (double?)((leading.ClickCount * 100.0) / leading.ImpressionCount)
            : null;
    }
}