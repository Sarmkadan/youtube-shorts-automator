// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Manages A/B thumbnail split tests for <see cref="VideoShort"/> uploads, tracking
/// per-variant impression and click-through view rates to identify the best-performing thumbnail.
/// </summary>
public class ThumbnailAbTestService
{
    private readonly ThumbnailAbTestRepository _repository;
    private readonly VideoShortRepository _videoRepository;
    private readonly ILogger<ThumbnailAbTestService> _logger;

    /// <summary>Minimum impressions required per variant before a winner can be declared.</summary>
    public const long MinimumImpressionsForConclusion = 500;

    /// <summary>Initializes a new instance of <see cref="ThumbnailAbTestService"/>.</summary>
    /// <param name="repository">Repository for <see cref="ThumbnailVariant"/> persistence.</param>
    /// <param name="videoRepository">Repository used to verify video existence.</param>
    /// <param name="logger">Logger instance.</param>
    public ThumbnailAbTestService(
        ThumbnailAbTestRepository repository,
        VideoShortRepository videoRepository,
        ILogger<ThumbnailAbTestService> logger)
    {
        _repository    = repository      ?? throw new ArgumentNullException(nameof(repository));
        _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
        _logger        = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new A/B test by registering two thumbnail variants for the specified video.
    /// Both variants are activated immediately. Throws if the video does not exist or an
    /// active test is already running for it.
    /// </summary>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="thumbnailPathA">File-system path to thumbnail variant A.</param>
    /// <param name="thumbnailPathB">File-system path to thumbnail variant B.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A tuple containing the newly created variant A and variant B.</returns>
    public async Task<(ThumbnailVariant VariantA, ThumbnailVariant VariantB)> CreateTestAsync(
        int videoShortId,
        string thumbnailPathA,
        string thumbnailPathB,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(thumbnailPathA))
            throw new ValidationException("Thumbnail path A is required.", nameof(thumbnailPathA), thumbnailPathA ?? string.Empty);
        if (string.IsNullOrWhiteSpace(thumbnailPathB))
            throw new ValidationException("Thumbnail path B is required.", nameof(thumbnailPathB), thumbnailPathB ?? string.Empty);

        _ = await _videoRepository.GetByIdAsync(videoShortId, cancellationToken)
            ?? throw new InvalidOperationException($"VideoShort {videoShortId} not found.");

        var existing = await _repository.GetActiveVariantsAsync(videoShortId, cancellationToken);
        if (existing.Any())
            throw new InvalidOperationException(
                $"An active A/B test already exists for VideoShort {videoShortId}. Conclude it before starting a new one.");

        var now = DateTime.UtcNow;

        var variantA = await _repository.AddAsync(new ThumbnailVariant
        {
            VideoShortId = videoShortId,
            Label        = "A",
            ThumbnailPath = thumbnailPathA,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now
        }, cancellationToken);

        var variantB = await _repository.AddAsync(new ThumbnailVariant
        {
            VideoShortId = videoShortId,
            Label        = "B",
            ThumbnailPath = thumbnailPathB,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now
        }, cancellationToken);

        _logger.LogInformation(
            "Created A/B thumbnail test for VideoShort {VideoShortId}: variant A (id={VariantAId}), variant B (id={VariantBId}).",
            videoShortId, variantA.Id, variantB.Id);

        return (variantA, variantB);
    }

    /// <summary>
    /// Records an impression event and optionally a click-through event for a specific variant.
    /// Throws if the variant does not exist or is no longer active.
    /// </summary>
    /// <param name="variantId">Identifier of the <see cref="ThumbnailVariant"/>.</param>
    /// <param name="clicked"><see langword="true"/> if the viewer clicked through after the impression.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    public async Task RecordViewEventAsync(int variantId, bool clicked = false,
        CancellationToken cancellationToken = default)
    {
        var variant = await _repository.GetByIdAsync(variantId, cancellationToken)
            ?? throw new InvalidOperationException($"ThumbnailVariant {variantId} not found.");

        if (!variant.IsActive)
            throw new InvalidOperationException($"ThumbnailVariant {variantId} is no longer active.");

        variant.RecordImpression();
        if (clicked) variant.RecordClick();

        await _repository.UpdateAsync(variant, cancellationToken);

        _logger.LogDebug(
            "Recorded view event for variant {VariantId} (clicked={Clicked}). ViewRate={ViewRate:F2}%.",
            variantId, clicked, variant.ViewRate);
    }

    /// <summary>
    /// Bulk-updates impression and click counts for all active variants of a video using data
    /// sourced from an external analytics provider. Each entry in <paramref name="analyticsData"/>
    /// maps a variant label (e.g. "A", "B") to its impression and click totals.
    /// </summary>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="analyticsData">
    /// Dictionary keyed by variant label containing <c>(Impressions, Clicks)</c> tuples.
    /// </param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    public async Task SyncAnalyticsAsync(
        int videoShortId,
        IDictionary<string, (long Impressions, long Clicks)> analyticsData,
        CancellationToken cancellationToken = default)
    {
        if (analyticsData == null) throw new ArgumentNullException(nameof(analyticsData));

        var variants = (await _repository.GetActiveVariantsAsync(videoShortId, cancellationToken)).ToList();
        if (!variants.Any())
        {
            _logger.LogWarning("No active thumbnail variants found for VideoShort {VideoShortId}.", videoShortId);
            return;
        }

        foreach (var variant in variants)
        {
            if (!analyticsData.TryGetValue(variant.Label, out var data))
                continue;

            variant.UpdateFromAnalytics(data.Impressions, data.Clicks);
            await _repository.UpdateAsync(variant, cancellationToken);
        }

        _logger.LogInformation(
            "Synced analytics for {Count} thumbnail variant(s) of VideoShort {VideoShortId}.",
            variants.Count, videoShortId);
    }

    /// <summary>
    /// Evaluates all active variants for a video and, once every variant has surpassed
    /// <see cref="MinimumImpressionsForConclusion"/>, declares the highest view-rate variant
    /// as the winner and deactivates the others.
    /// </summary>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>
    /// The winning <see cref="ThumbnailVariant"/>, or <see langword="null"/> if there is
    /// insufficient data to conclude the test.
    /// </returns>
    public async Task<ThumbnailVariant?> EvaluateAndConcludeAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        var variants = (await _repository.GetActiveVariantsAsync(videoShortId, cancellationToken)).ToList();

        if (!variants.Any())
        {
            _logger.LogWarning("No active variants for VideoShort {VideoShortId} to evaluate.", videoShortId);
            return null;
        }

        if (variants.Any(v => !v.HasSufficientData(MinimumImpressionsForConclusion)))
        {
            _logger.LogInformation(
                "Insufficient data to conclude A/B test for VideoShort {VideoShortId}. " +
                "Minimum {Minimum} impressions required per variant.",
                videoShortId, MinimumImpressionsForConclusion);
            return null;
        }

        var winner = variants.OrderByDescending(v => v.ViewRate).First();
        winner.DeclareWinner();
        await _repository.UpdateAsync(winner, cancellationToken);

        foreach (var loser in variants.Where(v => v.Id != winner.Id))
        {
            loser.IsActive  = false;
            loser.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(loser, cancellationToken);
        }

        _logger.LogInformation(
            "A/B test concluded for VideoShort {VideoShortId}. " +
            "Winner: variant {Label} with {ViewRate:F2}% view rate over {Impressions} impressions.",
            videoShortId, winner.Label, winner.ViewRate, winner.ImpressionCount);

        return winner;
    }

    /// <summary>
    /// Retrieves the current test result summary for a video, including per-variant metrics
    /// and the winner if one has already been declared.
    /// </summary>
    /// <param name="videoShortId">Identifier of the target <see cref="VideoShort"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation signals.</param>
    /// <returns>A <see cref="ThumbnailAbTestResult"/> snapshot.</returns>
    public async Task<ThumbnailAbTestResult> GetTestResultAsync(int videoShortId,
        CancellationToken cancellationToken = default)
    {
        var variants = (await _repository.GetByVideoShortIdAsync(videoShortId, cancellationToken)).ToList();
        var winner   = variants.FirstOrDefault(v => v.IsWinner);

        return new ThumbnailAbTestResult
        {
            VideoShortId = videoShortId,
            Variants     = variants,
            WinnerLabel  = winner?.Label,
            IsComplete   = winner != null,
            GeneratedAt  = DateTime.UtcNow
        };
    }
}

/// <summary>Encapsulates a point-in-time snapshot of a thumbnail A/B test's results.</summary>
public class ThumbnailAbTestResult
{
    /// <summary>Gets or sets the video identifier this test belongs to.</summary>
    public int VideoShortId { get; set; }

    /// <summary>Gets or sets all variants (active and concluded) recorded for this test.</summary>
    public IReadOnlyList<ThumbnailVariant> Variants { get; set; } = [];

    /// <summary>Gets or sets the label of the winning variant, or <see langword="null"/> if undecided.</summary>
    public string? WinnerLabel { get; set; }

    /// <summary>Gets or sets a value indicating whether the test has been concluded.</summary>
    public bool IsComplete { get; set; }

    /// <summary>Gets or sets when this result snapshot was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Returns a human-readable summary of the test outcome, including per-variant metrics
    /// and the relative view-rate advantage of the leading variant.
    /// </summary>
    public string GetSummary()
    {
        if (!Variants.Any())
            return "No variants found for this test.";

        var sb = new StringBuilder();
        sb.AppendLine($"A/B Thumbnail Test — VideoShort {VideoShortId}");
        sb.AppendLine(IsComplete
            ? $"Status  : Complete (Winner: {WinnerLabel})"
            : "Status  : In progress");

        foreach (var v in Variants.OrderBy(v => v.Label))
        {
            var winTag = v.IsWinner ? " ★ WINNER" : string.Empty;
            sb.AppendLine(
                $"  Variant {v.Label}: {v.ImpressionCount:N0} impressions, " +
                $"{v.ClickCount:N0} clicks, {v.ViewRate:F2}% view rate{winTag}");
        }

        if (Variants.Count >= 2)
        {
            var ordered = Variants.OrderByDescending(v => v.ViewRate).ToList();
            var delta   = ordered[0].ViewRate - ordered[1].ViewRate;
            sb.AppendLine($"  Leading edge: +{delta:F2}% view rate over runner-up");
        }

        return sb.ToString();
    }
}
