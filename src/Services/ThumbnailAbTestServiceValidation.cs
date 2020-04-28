// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ThumbnailAbTestService to ensure data integrity
// before operations that would persist to the database.
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="ThumbnailAbTestService"/> to ensure
/// that service operations receive valid input parameters before processing.
/// </summary>
public static class ThumbnailAbTestServiceValidation
{
    /// <summary>
    /// Validates a <see cref="ThumbnailAbTestService"/> instance and returns a list of human-readable
    /// validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ThumbnailAbTestService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate constructor dependencies (these are injected and should never be null in a valid instance)
        // Note: We can't validate the injected repositories without reflection or additional context,
        // so we focus on the service's own state

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ThumbnailAbTestService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ThumbnailAbTestService? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ThumbnailAbTestService"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed error message if it is not.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the service instance is not valid.</exception>
    public static void EnsureValid(this ThumbnailAbTestService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"ThumbnailAbTestService instance is not valid. Problems:\n{string.Join("\n", errors)}",
            nameof(value));
    }

    /// <summary>
    /// Validates parameters for <see cref="ThumbnailAbTestService.CreateTestAsync"/> method.
    /// </summary>
    /// <param name="videoShortId">The video short identifier.</param>
    /// <param name="thumbnailPathA">Path to thumbnail variant A.</param>
    /// <param name="thumbnailPathB">Path to thumbnail variant B.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateCreateTest(
        int videoShortId,
        string? thumbnailPathA,
        string? thumbnailPathB)
    {
        var errors = new List<string>();

        if (videoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(thumbnailPathA))
        {
            errors.Add("Thumbnail path A is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(thumbnailPathB))
        {
            errors.Add("Thumbnail path B is required and cannot be empty or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="ThumbnailAbTestService.RecordViewEventAsync"/> method.
    /// </summary>
    /// <param name="variantId">The thumbnail variant identifier.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRecordViewEvent(int variantId)
    {
        var errors = new List<string>();

        if (variantId <= 0)
        {
            errors.Add("VariantId must be a positive integer.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="ThumbnailAbTestService.SyncAnalyticsAsync"/> method.
    /// </summary>
    /// <param name="videoShortId">The video short identifier.</param>
    /// <param name="analyticsData">Analytics data dictionary.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateSyncAnalytics(
        int videoShortId,
        IDictionary<string, (long Impressions, long Clicks)>? analyticsData)
    {
        var errors = new List<string>();

        if (videoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        if (analyticsData == null)
        {
            errors.Add("Analytics data cannot be null.");
        }
        else if (analyticsData.Count == 0)
        {
            errors.Add("Analytics data cannot be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="ThumbnailAbTestService.EvaluateAndConcludeAsync"/> method.
    /// </summary>
    /// <param name="videoShortId">The video short identifier.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateEvaluateAndConclude(int videoShortId)
    {
        var errors = new List<string>();

        if (videoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="ThumbnailAbTestService.GetTestResultAsync"/> method.
    /// </summary>
    /// <param name="videoShortId">The video short identifier.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetTestResult(int videoShortId)
    {
        var errors = new List<string>();

        if (videoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="ThumbnailAbTestResult"/> instance and returns a list of human-readable
    /// validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="result">The test result to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ThumbnailAbTestResult? result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var errors = new List<string>();

        if (result.VideoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        if (result.Variants == null)
        {
            errors.Add("Variants collection cannot be null.");
        }
        else if (result.Variants.Count == 0)
        {
            errors.Add("Variants collection cannot be empty.");
        }

        if (result.GeneratedAt == default)
        {
            errors.Add("GeneratedAt must be a valid DateTime (cannot be default).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ThumbnailAbTestResult"/> instance is valid.
    /// </summary>
    /// <param name="result">The test result to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ThumbnailAbTestResult? result)
    {
        return Validate(result).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ThumbnailAbTestResult"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed error message if it is not.
    /// </summary>
    /// <param name="result">The test result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result instance is not valid.</exception>
    public static void EnsureValid(this ThumbnailAbTestResult? result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var errors = Validate(result);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"ThumbnailAbTestResult instance is not valid. Problems:\n{string.Join("\n", errors)}",
            nameof(result));
    }

    /// <summary>
    /// Validates a <see cref="ThumbnailVariant"/> instance and returns a list of human-readable
    /// validation problems. Returns an empty list if the instance is valid.
    /// </summary>
    /// <param name="variant">The variant to validate.</param>
    /// <returns>List of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ThumbnailVariant? variant)
    {
        ArgumentNullException.ThrowIfNull(variant);

        var errors = new List<string>();

        if (variant.VideoShortId <= 0)
        {
            errors.Add("VideoShortId must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(variant.Label))
        {
            errors.Add("Label is required and cannot be empty or whitespace.");
        }
        else if (variant.Label.Length > 10)
        {
            errors.Add("Label cannot exceed 10 characters.");
        }

        if (string.IsNullOrWhiteSpace(variant.ThumbnailPath))
        {
            errors.Add("ThumbnailPath is required and cannot be empty or whitespace.");
        }

        if (variant.ImpressionCount < 0)
        {
            errors.Add("ImpressionCount cannot be negative.");
        }

        if (variant.ClickCount < 0)
        {
            errors.Add("ClickCount cannot be negative.");
        }

        if (variant.ViewRate < 0 || variant.ViewRate > 100)
        {
            errors.Add("ViewRate must be between 0 and 100.");
        }

        if (variant.CreatedAt == default)
        {
            errors.Add("CreatedAt must be a valid DateTime (cannot be default).");
        }

        if (variant.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be a valid DateTime (cannot be default).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ThumbnailVariant"/> instance is valid.
    /// </summary>
    /// <param name="variant">The variant to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ThumbnailVariant? variant)
    {
        return Validate(variant).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ThumbnailVariant"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed error message if it is not.
    /// </summary>
    /// <param name="variant">The variant to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variant"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the variant instance is not valid.</exception>
    public static void EnsureValid(this ThumbnailVariant? variant)
    {
        ArgumentNullException.ThrowIfNull(variant);

        var errors = Validate(variant);
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"ThumbnailVariant instance is not valid. Problems:\n{string.Join("\n", errors)}",
            nameof(variant));
    }
}