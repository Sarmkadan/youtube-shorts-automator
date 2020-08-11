// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ProcessingJobRequest domain model
// Provides comprehensive validation for video processing job requests
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Provides validation methods for <see cref="ProcessingJobRequest"/> instances.
/// </summary>
public static class ProcessingJobRequestValidation
{
    /// <summary>
    /// Validates a <see cref="ProcessingJobRequest"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The processing job request to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if the request is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingJobRequest? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate RequestId
        if (value.RequestId == Guid.Empty)
        {
            errors.Add("RequestId must be a non-empty GUID.");
        }

        // Validate VideoFilePath
        if (string.IsNullOrWhiteSpace(value.VideoFilePath))
        {
            errors.Add("VideoFilePath cannot be null, empty, or whitespace.");
        }
        else if (!System.IO.Path.IsPathRooted(value.VideoFilePath))
        {
            errors.Add("VideoFilePath must be an absolute file path.");
        }
        else if (!System.IO.File.Exists(value.VideoFilePath))
        {
            errors.Add("VideoFilePath must point to an existing file.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Title cannot be null, empty, or whitespace.");
        }
        else if (value.Title.Length > 100)
        {
            errors.Add("Title cannot exceed 100 characters.");
        }

        // Validate Description
        if (value.Description.Length > 5000)
        {
            errors.Add("Description cannot exceed 5000 characters.");
        }

        // Validate Tags
        if (value.Tags is null)
        {
            errors.Add("Tags collection cannot be null.");
        }
        else if (value.Tags.Length > 50)
        {
            errors.Add("Tags collection cannot exceed 50 items.");
        }
        else
        {
            for (int i = 0; i < value.Tags.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(value.Tags[i]))
                {
                    errors.Add($"Tags[{i}] cannot be null, empty, or whitespace.");
                }
                else if (value.Tags[i].Length > 100)
                {
                    errors.Add($"Tags[{i}] cannot exceed 100 characters.");
                }
            }
        }

        // Validate ProcessingProfile
        if (string.IsNullOrWhiteSpace(value.ProcessingProfile))
        {
            errors.Add("ProcessingProfile cannot be null, empty, or whitespace.");
        }
        else if (value.ProcessingProfile.Length > 50)
        {
            errors.Add("ProcessingProfile cannot exceed 50 characters.");
        }

        // Validate Options (null check handled by property initialization)
        if (value.Options is null)
        {
            errors.Add("Options cannot be null.");
        }
        else
        {
            ValidateProcessingOptions(value.Options, errors);
        }

        // Validate CreatedAtUtc
        if (value.CreatedAtUtc == default)
        {
            errors.Add("CreatedAtUtc must be a valid DateTime.");
        }
        else if (value.CreatedAtUtc > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAtUtc cannot be in the future.");
        }

        // Validate RequestedBy
        if (string.IsNullOrWhiteSpace(value.RequestedBy))
        {
            errors.Add("RequestedBy cannot be null, empty, or whitespace.");
        }
        else if (value.RequestedBy.Length > 100)
        {
            errors.Add("RequestedBy cannot exceed 100 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ProcessingJobRequest"/> instance is valid.
    /// </summary>
    /// <param name="value">The processing job request to check.</param>
    /// <returns>True if the request is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProcessingJobRequest? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ProcessingJobRequest"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The processing job request to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the request contains validation errors.</exception>
    public static void EnsureValid(this ProcessingJobRequest? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingJobRequest is invalid. Errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates ProcessingOptions and appends errors to the provided list.
    /// </summary>
    /// <param name="options">The processing options to validate.</param>
    /// <param name="errors">The list to append validation errors to.</param>
    private static void ValidateProcessingOptions(ProcessingOptions options, List<string> errors)
    {
        if (options.EnableWatermark && string.IsNullOrWhiteSpace(options.WatermarkImagePath))
        {
            errors.Add("WatermarkImagePath must be specified when EnableWatermark is true.");
        }
        else if (options.EnableWatermark && !string.IsNullOrWhiteSpace(options.WatermarkImagePath))
        {
            if (!System.IO.Path.IsPathRooted(options.WatermarkImagePath))
            {
                errors.Add("WatermarkImagePath must be an absolute file path when watermark is enabled.");
            }
            else if (!System.IO.File.Exists(options.WatermarkImagePath))
            {
                errors.Add("WatermarkImagePath must point to an existing file when watermark is enabled.");
            }
        }

        // Validate MaxWidth
        if (options.MaxWidth <= 0)
        {
            errors.Add("MaxWidth must be a positive integer.");
        }
        else if (options.MaxWidth > 7680) // 8K UHD
        {
            errors.Add("MaxWidth cannot exceed 7680 pixels (8K UHD).");
        }

        // Validate MaxHeight
        if (options.MaxHeight <= 0)
        {
            errors.Add("MaxHeight must be a positive integer.");
        }
        else if (options.MaxHeight > 4320) // 8K UHD
        {
            errors.Add("MaxHeight cannot exceed 4320 pixels (8K UHD).");
        }

        // Validate BitrateKbps
        if (options.BitrateKbps <= 0)
        {
            errors.Add("BitrateKbps must be a positive integer.");
        }
        else if (options.BitrateKbps > 100000) // 100 Mbps
        {
            errors.Add("BitrateKbps cannot exceed 100000 kbps (100 Mbps).");
        }
    }
}