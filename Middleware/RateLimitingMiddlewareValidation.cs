// =============================================================================
// Validation helpers for RateLimitingMiddleware
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Provides validation extension methods for <see cref="RateLimitingMiddleware"/>.
/// </summary>
public static class RateLimitingMiddlewareValidation
{
    /// <summary>
    /// Returns a read‑only list of validation problems for the supplied middleware instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> containing human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate the RequestDelegate (private field "_next")
        var nextField = typeof(RateLimitingMiddleware).GetField("_next", BindingFlags.Instance | BindingFlags.NonPublic);
        if (nextField?.GetValue(value) is null)
        {
            problems.Add("RequestDelegate (next) is not configured.");
        }

        // Validate the options (private field "_options")
        var optionsField = typeof(RateLimitingMiddleware).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
        if (optionsField?.GetValue(value) is not RateLimitingOptions options)
        {
            problems.Add("RateLimitingOptions are missing.");
            return problems; // cannot continue without options
        }

        if (options.RequestsPerWindow <= 0)
        {
            problems.Add($"RequestsPerWindow must be greater than 0 (actual: {options.RequestsPerWindow}).");
        }

        if (options.WindowSizeSeconds <= 0)
        {
            problems.Add($"WindowSizeSeconds must be greater than 0 (actual: {options.WindowSizeSeconds}).");
        }

        // Validate that the bucket dictionary (private field "_buckets") is instantiated
        var bucketsField = typeof(RateLimitingMiddleware).GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bucketsField?.GetValue(value) is null)
        {
            problems.Add("Internal bucket collection is not initialized.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the supplied middleware instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this RateLimitingMiddleware value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the supplied middleware instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this RateLimitingMiddleware value)
    {
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", problems));
        }
    }
}
