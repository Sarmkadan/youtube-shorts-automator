using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ThumbnailVariant"/> instances.
/// </summary>
public static class ThumbnailVariantValidation
{
    /// <summary>
    /// Validates the specified thumbnail variant and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The thumbnail variant to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the thumbnail variant is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ThumbnailVariant value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id (must be positive for a valid entity)
        if (value.Id < 0)
        {
            errors.Add("Id must be a non-negative integer.");
        }

        // Validate VideoShortId (must be positive for a valid relationship)
        if (value.VideoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        // Validate Label (must not be null or whitespace, typically "A" or "B")
        if (string.IsNullOrWhiteSpace(value.Label))
        {
            errors.Add("Label cannot be null or whitespace.");
        }
        else if (value.Label.Length > 50)
        {
            errors.Add("Label cannot exceed 50 characters.");
        }

        // Validate ThumbnailPath (must not be null or whitespace, must be a valid file path)
        if (string.IsNullOrWhiteSpace(value.ThumbnailPath))
        {
            errors.Add("ThumbnailPath cannot be null or whitespace.");
        }
        else if (value.ThumbnailPath.Length > 500)
        {
            errors.Add("ThumbnailPath cannot exceed 500 characters.");
        }

        // Validate ImpressionCount (must not be negative)
        if (value.ImpressionCount < 0)
        {
            errors.Add("ImpressionCount cannot be negative.");
        }

        // Validate ClickCount (must not be negative, cannot exceed ImpressionCount)
        if (value.ClickCount < 0)
        {
            errors.Add("ClickCount cannot be negative.");
        }
        else if (value.ClickCount > value.ImpressionCount)
        {
            errors.Add("ClickCount cannot exceed ImpressionCount.");
        }

        // Validate ViewRate (must be between 0 and 100 inclusive)
        if (value.ViewRate < 0 || value.ViewRate > 100)
        {
            errors.Add("ViewRate must be between 0 and 100 inclusive.");
        }

        // Validate IsActive (no specific validation needed, just a boolean flag)
        // Validate IsWinner (no specific validation needed, just a boolean flag)

        // Validate CreatedAt (must not be default DateTime)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }

        // Validate UpdatedAt (must not be default DateTime)
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be the default DateTime value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified thumbnail variant is valid.
    /// </summary>
    /// <param name="value">The thumbnail variant to check.</param>
    /// <returns><see langword="true"/> if the thumbnail variant is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ThumbnailVariant value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified thumbnail variant is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The thumbnail variant to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the thumbnail variant is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this ThumbnailVariant value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ThumbnailVariant is invalid. Validation errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}
