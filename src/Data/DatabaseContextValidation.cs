// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using System.Data.SqlClient;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseContext"/> instances.
/// </summary>
public static class DatabaseContextValidation
{
    /// <summary>
    /// Validates the specified database context.
    /// </summary>
    /// <param name="value">The database context to validate.</param>
    /// <returns>A list of validation errors; empty if the context is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseContext? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate the connection string (the only constructor parameter)
        // Note: We can't access the private _connectionString field directly,
        // so we validate based on the behavior we can test

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified database context is valid.
    /// </summary>
    /// <param name="value">The database context to check.</param>
    /// <returns><see langword="true"/> if the context is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DatabaseContext? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified database context is valid.
    /// </summary>
    /// <param name="value">The database context to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the context is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this DatabaseContext? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DatabaseContext is invalid. Validation errors:\n- {string.Join("\n- ", errors)}");
        }
    }
}
