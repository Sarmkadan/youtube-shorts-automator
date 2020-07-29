// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="ProcessingProfile"/> class.
/// </summary>
public static class ProcessingProfileValidation
{
    /// <summary>
    /// Validates the specified processing profile.
    /// </summary>
    /// <param name="value">The processing profile to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingProfile value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 100)
        {
            errors.Add("Name length must not exceed 100 characters.");
        }

        // Validate Description (optional, no specific constraints)
        if (value.Description is not null && value.Description.Length > 500)
        {
            errors.Add("Description length must not exceed 500 characters.");
        }

        // Validate VideoWidth
        if (value.VideoWidth < 360 || value.VideoWidth > 1920)
        {
            errors.Add("VideoWidth must be between 360 and 1920 pixels inclusive.");
        }

        // Validate VideoHeight
        if (value.VideoHeight < 640 || value.VideoHeight > 1920)
        {
            errors.Add("VideoHeight must be between 640 and 1920 pixels inclusive.");
        }

        // Validate VideoBitrate
        if (value.VideoBitrate < 500 || value.VideoBitrate > 20000)
        {
            errors.Add("VideoBitrate must be between 500 and 20000 kbps inclusive.");
        }

        // Validate AudioBitrate
        if (value.AudioBitrate < 64 || value.AudioBitrate > 320)
        {
            errors.Add("AudioBitrate must be between 64 and 320 kbps inclusive.");
        }

        // Validate FrameRate
        if (value.FrameRate < 24 || value.FrameRate > 60)
        {
            errors.Add("FrameRate must be between 24 and 60 frames per second inclusive.");
        }

        // Validate VideoCodec
        if (string.IsNullOrWhiteSpace(value.VideoCodec))
        {
            errors.Add("VideoCodec cannot be null or whitespace.");
        }

        // Validate AudioCodec
        if (string.IsNullOrWhiteSpace(value.AudioCodec))
        {
            errors.Add("AudioCodec cannot be null or whitespace.");
        }

        // Validate Container
        if (string.IsNullOrWhiteSpace(value.Container))
        {
            errors.Add("Container cannot be null or whitespace.");
        }

        // Validate CompressionLevel
        if (value.CompressionLevel < 0 || value.CompressionLevel > 10)
        {
            errors.Add("CompressionLevel must be between 0 and 10 inclusive.");
        }

        // Validate CreatedAt (must not be default)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be a valid DateTime.");
        }

        // Validate UpdatedAt (must not be default)
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be a valid DateTime.");
        }

        // Validate WatermarkPath if ApplyWatermark is true
        if (value.ApplyWatermark && string.IsNullOrWhiteSpace(value.WatermarkPath))
        {
            errors.Add("WatermarkPath must be specified when ApplyWatermark is true.");
        }

        // Validate ColorGradingProfile if ApplyColorGrading is true
        if (value.ApplyColorGrading && string.IsNullOrWhiteSpace(value.ColorGradingProfile))
        {
            errors.Add("ColorGradingProfile must be specified when ApplyColorGrading is true.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified processing profile is valid.
    /// </summary>
    /// <param name="value">The processing profile to check.</param>
    /// <returns>true if the processing profile is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProcessingProfile value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified processing profile is valid.
    /// </summary>
    /// <param name="value">The processing profile to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the processing profile is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this ProcessingProfile value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingProfile is invalid. Errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}