// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Validation helpers for <see cref="ThumbnailGeneratorServiceTests"/> test cases.
/// </summary>
public static class ThumbnailGeneratorServiceTestsValidation
{
    /// <summary>
    /// Validates the specified test instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>An immutable list of validation messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ThumbnailGeneratorServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GetDimensions expected behavior
        // GetDimensions_Vertical_Returns720x1280 expects (720, 1280)
        // GetDimensions_Horizontal_Returns1280x720 expects (1280, 720)
        // GetDimensions_Square_Returns720x720 expects (720, 720)
        // These are validated by the test methods themselves

        // Validate default values from ThumbnailGenerationRequest
        var request = new YouTubeShortAutomator.Domain.Models.ThumbnailGenerationRequest();
        if (request.CaptureTimestamp != TimeSpan.FromSeconds(1))
            errors.Add("ThumbnailGenerationRequest default CaptureTimestamp should be 1 second");
        if (request.Format != YouTubeShortAutomator.Domain.Models.ThumbnailOutputFormat.Jpeg)
            errors.Add("ThumbnailGenerationRequest default Format should be Jpeg");
        if (request.AspectRatio != YouTubeShortAutomator.Domain.Models.ThumbnailAspectRatio.Vertical)
            errors.Add("ThumbnailGenerationRequest default AspectRatio should be Vertical");
        if (request.Quality != 85)
            errors.Add("ThumbnailGenerationRequest default Quality should be 85");
        if (request.OverlayText != null)
            errors.Add("ThumbnailGenerationRequest default OverlayText should be null");

        // Validate default values from TextOverlayOptions
        var textOverlay = new YouTubeShortAutomator.Domain.Models.TextOverlayOptions();
        if (textOverlay.FontSize != 48)
            errors.Add("TextOverlayOptions default FontSize should be 48");
        if (textOverlay.FontColor != "white")
            errors.Add("TextOverlayOptions default FontColor should be 'white'");
        if (!textOverlay.ShowBox)
            errors.Add("TextOverlayOptions default ShowBox should be true");
        if (textOverlay.BoxColor != "black@0.5")
            errors.Add("TextOverlayOptions default BoxColor should be 'black@0.5'");
        if (textOverlay.Position != YouTubeShortAutomator.Domain.Models.TextPosition.BottomCenter)
            errors.Add("TextOverlayOptions default Position should be BottomCenter");

        // Validate default values from ThumbnailGenerationResult
        var result = new YouTubeShortAutomator.Domain.Models.ThumbnailGenerationResult();
        if (result.Success)
            errors.Add("ThumbnailGenerationResult default Success should be false");
        if (!string.IsNullOrEmpty(result.OutputPath))
            errors.Add("ThumbnailGenerationResult default OutputPath should be empty");
        if (result.FileSizeBytes != 0)
            errors.Add("ThumbnailGenerationResult default FileSizeBytes should be 0");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified test instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ThumbnailGeneratorServiceTests value)
        => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified test instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation failures if it is not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, with a message listing all problems.</exception>
    public static void EnsureValid(this ThumbnailGeneratorServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            string.Join("\n", errors),
            nameof(value));
    }
}
