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
        ValidateString(value.Name, nameof(value.Name), 100, errors);

        // Validate Description (optional, no specific constraints)
        ValidateString(value.Description, nameof(value.Description), 500, errors);

        // Validate VideoWidth
        ValidateRange(value.VideoWidth, nameof(value.VideoWidth), 360, 1920, errors);

        // Validate VideoHeight
        ValidateRange(value.VideoHeight, nameof(value.VideoHeight), 640, 1920, errors);

        // Validate VideoBitrate
        ValidateRange(value.VideoBitrate, nameof(value.VideoBitrate), 500, 20000, errors);

        // Validate AudioBitrate
        ValidateRange(value.AudioBitrate, nameof(value.AudioBitrate), 64, 320, errors);

        // Validate FrameRate
        ValidateRange(value.FrameRate, nameof(value.FrameRate), 24, 60, errors);

        // Validate VideoCodec
        ValidateString(value.VideoCodec, nameof(value.VideoCodec), null, errors);

        // Validate AudioCodec
        ValidateString(value.AudioCodec, nameof(value.AudioCodec), null, errors);

        // Validate Container
        ValidateString(value.Container, nameof(value.Container), null, errors);

        // Validate CompressionLevel
        ValidateRange(value.CompressionLevel, nameof(value.CompressionLevel), 0, 10, errors);

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

    private static void ValidateString(string? value, string paramName, int? maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{paramName} cannot be null or whitespace.");
        }
        else if (maxLength.HasValue && value.Length > maxLength.Value)
        {
            errors.Add($"{paramName} length must not exceed {maxLength.Value} characters.");
        }
    }

    private static void ValidateRange(int value, string paramName, int min, int max, List<string> errors)
    {
        if (value < min || value > max)
        {
            errors.Add($"{paramName} must be between {min} and {max} inclusive.");
        }
    }
}