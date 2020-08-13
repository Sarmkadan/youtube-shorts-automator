using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Provides validation helpers for <see cref="ApiCredentialService"/>.
/// </summary>
public static class ApiCredentialServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="ApiCredentialService"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiCredentialService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Since ApiCredentialService is a service class and does not have any properties,
        // there is nothing to validate.

        return problems.ToImmutableList();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApiCredentialService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiCredentialService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the specified <see cref="ApiCredentialService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ApiCredentialService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
        }
    }
}
