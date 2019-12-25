// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a single thumbnail variant used in an A/B split test for a <see cref="VideoShort"/>.
/// </summary>
public class ThumbnailVariant
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the video this variant belongs to.</summary>
    public int VideoShortId { get; set; }

    /// <summary>Gets or sets the variant label, e.g. "A" or "B".</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the file-system path to the thumbnail image.</summary>
    public string ThumbnailPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of times this thumbnail was shown to viewers.</summary>
    public long ImpressionCount { get; set; }

    /// <summary>Gets or sets the total number of click-throughs recorded for this thumbnail.</summary>
    public long ClickCount { get; set; }

    /// <summary>Gets or sets the click-through / view rate as a percentage (0–100).</summary>
    public double ViewRate { get; set; }

    /// <summary>Gets or sets a value indicating whether this variant is currently being served.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets a value indicating whether this variant was declared the test winner.</summary>
    public bool IsWinner { get; set; }

    /// <summary>Gets or sets when this variant was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets when this variant was last modified.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the parent <see cref="VideoShort"/>.</summary>
    public VideoShort? VideoShort { get; set; }

    /// <summary>Records a single impression event and recalculates <see cref="ViewRate"/>.</summary>
    public void RecordImpression()
    {
        ImpressionCount++;
        RecalculateViewRate();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Records a click-through event and recalculates <see cref="ViewRate"/>.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no impression has been recorded yet.</exception>
    public void RecordClick()
    {
        if (ImpressionCount == 0)
            throw new InvalidOperationException("Cannot record a click without a prior impression.");
        ClickCount++;
        RecalculateViewRate();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bulk-updates impression and click counts sourced from an external analytics provider
    /// and recalculates <see cref="ViewRate"/>.
    /// </summary>
    /// <param name="impressions">Total impressions reported by the analytics source.</param>
    /// <param name="clicks">Total clicks reported by the analytics source.</param>
    public void UpdateFromAnalytics(long impressions, long clicks)
    {
        if (impressions < 0) throw new ArgumentOutOfRangeException(nameof(impressions));
        if (clicks < 0) throw new ArgumentOutOfRangeException(nameof(clicks));
        ImpressionCount = impressions;
        ClickCount = Math.Min(clicks, impressions);
        RecalculateViewRate();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marks this variant as the A/B test winner.</summary>
    public void DeclareWinner()
    {
        IsWinner = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the variant has accumulated enough impressions
    /// for a statistically useful comparison.
    /// </summary>
    /// <param name="minimumImpressions">Impression threshold before drawing conclusions.</param>
    public bool HasSufficientData(long minimumImpressions = 500) => ImpressionCount >= minimumImpressions;

    private void RecalculateViewRate()
    {
        ViewRate = ImpressionCount > 0 ? (double)ClickCount / ImpressionCount * 100.0 : 0;
    }
}
