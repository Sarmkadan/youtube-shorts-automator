// =============================================================================
// Validation helpers for CacheServiceBenchmarks.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Provides validation extension methods for <see cref="CacheServiceBenchmarks"/>.
/// </summary>
public static class CacheServiceBenchmarksValidation
{
    /// <summary>
    /// Validates the state of the supplied <see cref="CacheServiceBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>
    /// A read‑only list of human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this CacheServiceBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // The benchmark class does not expose any public mutable state.
        // Validation therefore consists of confirming that the instance itself is non‑null.
        // If future public members are added, corresponding checks can be introduced here.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="CacheServiceBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns><c>true</c> if no validation problems are reported; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CacheServiceBenchmarks value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the supplied <see cref="CacheServiceBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this CacheServiceBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CacheServiceBenchmarks validation failed: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}
