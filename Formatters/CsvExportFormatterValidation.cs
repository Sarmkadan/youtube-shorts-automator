// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortsAutomator.Formatters;

/// <summary>
/// Provides validation helpers for <see cref="CsvExportFormatter"/>.
/// </summary>
public static class CsvExportFormatterValidation
{
    /// <summary>
    /// Validates the specified <see cref="CsvExportFormatter"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="CsvExportFormatter"/> to validate.</param>
    /// <returns>A list of human-readable problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(this CsvExportFormatter? value)
    {
        if (value is null)
            return new[] { "CsvExportFormatter instance cannot be null." };

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CsvExportFormatter"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="CsvExportFormatter"/> to check.</param>
    /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this CsvExportFormatter? value) => value is not null;

    /// <summary>
    /// Ensures that the specified <see cref="CsvExportFormatter"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="CsvExportFormatter"/> to check.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> instance is invalid.</exception>
    public static void EnsureValid(this CsvExportFormatter? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
        }
    }
}
