// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ContentCalendarController
// Provides validation, IsValid, and EnsureValid methods for controller validation
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Provides validation extension methods for <see cref="ContentCalendarController"/>.
/// </summary>
public static class ContentCalendarControllerValidation
{
    /// <summary>
    /// Validates the <see cref="ContentCalendarController"/> instance and returns a collection of
    /// human‑readable problems. Because the controller does not expose any public state,
    /// the only validation performed is a null‑check.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages,
    /// or an empty list if the instance is considered valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this ContentCalendarController? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // No public properties/state to validate at the moment.
        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ContentCalendarController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ContentCalendarController? value) =>
        !value.Validate().Any();

    /// <summary>
    /// Ensures that the <see cref="ContentCalendarController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a list of the problems.
    /// </exception>
    public static void EnsureValid(this ContentCalendarController? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Any())
        {
            throw new ArgumentException(
                $"ContentCalendarController validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)
                }",
                nameof(value));
        }
    }
}