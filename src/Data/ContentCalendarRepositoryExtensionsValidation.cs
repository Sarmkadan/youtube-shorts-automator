// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using YouTubeShortAutomator.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="ContentCalendarRepository"/>.
/// </summary>
public static class ContentCalendarRepositoryExtensionsValidation
{
    /// <summary>
    /// Validates the repository instance.
    /// </summary>
    /// <param name="value">The repository instance.</param>
    /// <returns>A collection of validation problems (currently empty as repositories are stateless regarding their content).</returns>
    public static IReadOnlyList<string> Validate(this ContentCalendarRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new List<string>().AsReadOnly();
    }

    /// <summary>
    /// Checks if the repository instance is valid.
    /// </summary>
    /// <param name="value">The repository instance.</param>
    /// <returns><c>true</c> if valid.</returns>
    public static bool IsValid(this ContentCalendarRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Ensures the repository instance is valid, or throws an exception.
    /// </summary>
    /// <param name="value">The repository instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this ContentCalendarRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}
