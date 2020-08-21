using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Validation helpers for <see cref="UploadJobRepositoryExtensions"/>.
/// </summary>
public static class UploadJobRepositoryExtensionsValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadJobRepositoryExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No specific validation rules for UploadJobRepositoryExtensions,
        // as it's a static class with extension methods.

        return problems;
    }

    /// <summary>
    /// Checks if the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadJobRepositoryExtensions value)
    {
        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The value to ensure.</param>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static void EnsureValid(this UploadJobRepositoryExtensions value)
    {
        var problems = Validate(value);
        if (problems.Any())
        {
            throw new ArgumentException($"Invalid value: {string.Join(Environment.NewLine, problems)}", nameof(value));
        }
    }
}
