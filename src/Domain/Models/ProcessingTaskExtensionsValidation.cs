// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation extension methods for ProcessingTaskExtensions to validate
// the results of extension method operations on ProcessingTask instances.
// =====================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ProcessingTaskExtensions"/> extension method operations.
/// </summary>
public static class ProcessingTaskExtensionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ProcessingTask"/> instance that was modified using
    /// extension methods from <see cref="ProcessingTaskExtensions"/>.
    /// </summary>
    /// <param name="value">The processing task to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateExtension(this ProcessingTask value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GetCompletionPercentage result (must be in range [0, 100])
        var completionPercentage = value.GetCompletionPercentage();
        if (completionPercentage < 0 || completionPercentage > 100)
        {
            errors.Add($"GetCompletionPercentage returned {completionPercentage}, which is outside the valid range [0, 100].");
        }

        // Validate GetDurationDisplay result (must not be null/empty/whitespace, reasonable length)
        var durationDisplay = value.GetDurationDisplay();
        if (string.IsNullOrWhiteSpace(durationDisplay))
        {
            errors.Add("GetDurationDisplay returned null, empty, or whitespace.");
        }
        else if (durationDisplay.Length > 100)
        {
            errors.Add($"GetDurationDisplay returned a string of length {durationDisplay.Length}, which exceeds the maximum allowed length of 100 characters.");
        }

        // Validate Priority range [1, 10] for AdjustPriority operations
        if (value.Priority < 1 || value.Priority > 10)
        {
            errors.Add($"Priority value {value.Priority} is outside the valid range [1, 10].");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProcessingTask"/> instance is valid
    /// after being modified using extension methods from <see cref="ProcessingTaskExtensions"/>.
    /// </summary>
    /// <param name="value">The processing task to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValidExtension(this ProcessingTask value)
    {
        return value.ValidateExtension().Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="ProcessingTask"/> instance and throws an <see cref="ArgumentException"/>
    /// if any validation errors are found, after being modified using extension methods
    /// from <see cref="ProcessingTaskExtensions"/>.
    /// </summary>
    /// <param name="value">The processing task to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a message listing all errors.</exception>
    public static void EnsureValidExtension(this ProcessingTask value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateExtension();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ProcessingTask is invalid after extension method operations. Errors:\n\n- {string.Join("\n- ", errors)}");
    }
}
