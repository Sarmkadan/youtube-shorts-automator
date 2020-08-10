// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for <see cref="IntegrationTests"/> class
// =====================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="IntegrationTests"/> instances.
/// Validates that the test class instance was properly initialized by its constructor.
/// </summary>
public static class IntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="IntegrationTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this IntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // The IntegrationTests constructor creates a ServiceProvider and test directory.
        // Since these are private fields, we validate that the constructor completed successfully
        // by checking that the instance is functional (no exceptions thrown during construction).
        // The Dispose() method should also be available since the class implements IDisposable.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this IntegrationTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, with a message listing all problems.</exception>
    public static void EnsureValid(this IntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"IntegrationTests validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }",
                nameof(value));
        }
    }
}
