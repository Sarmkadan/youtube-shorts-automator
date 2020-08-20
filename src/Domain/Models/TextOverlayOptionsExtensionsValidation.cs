// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation extension methods for TextOverlayOptionsExtensions to validate
// the results of extension method operations on TextOverlayOptions instances.
// =====================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="TextOverlayOptionsExtensions"/> extension method operations.
/// </summary>
public static class TextOverlayOptionsExtensionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="TextOverlayOptions"/> instance that was modified using
    /// extension methods from <see cref="TextOverlayOptionsExtensions"/>.
    /// </summary>
    /// <param name="value">The text overlay options to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateExtension(this TextOverlayOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate FontSize (should be positive)
        if (value.FontSize <= 0)
        {
            errors.Add($"FontSize must be a positive integer, but was {value.FontSize}.");
        }

        // Validate FontColor (should not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.FontColor))
        {
            errors.Add("FontColor must not be null or whitespace.");
        }

        // Validate BoxColor (should not be null or whitespace)
        if (string.IsNullOrWhiteSpace(value.BoxColor))
        {
            errors.Add("BoxColor must not be null or whitespace.");
        }

        // Validate BoxPadding (should be non-negative)
        if (value.BoxPadding < 0)
        {
            errors.Add($"BoxPadding must be non-negative, but was {value.BoxPadding}.");
        }

        // Validate WrapWidth (should be non-negative; 0 means no wrapping)
        if (value.WrapWidth < 0)
        {
            errors.Add($"WrapWidth must be non-negative, but was {value.WrapWidth}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TextOverlayOptions"/> instance is valid
    /// after being modified using extension methods from <see cref="TextOverlayOptionsExtensions"/>.
    /// </summary>
    /// <param name="value">The text overlay options to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValidExtension(this TextOverlayOptions value)
    {
        return value.ValidateExtension().Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="TextOverlayOptions"/> instance and throws an <see cref="ArgumentException"/>
    /// if any validation errors are found, after being modified using extension methods
    /// from <see cref="TextOverlayOptionsExtensions"/>.
    /// </summary>
    /// <param name="value">The text overlay options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a message listing all errors.</exception>
    public static void EnsureValidExtension(this TextOverlayOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateExtension();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"TextOverlayOptions is invalid after extension method operations. Errors: {string.Join(" ", errors)}");
    }
}