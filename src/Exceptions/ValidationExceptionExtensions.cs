// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension helpers for ValidationException
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionExtensions
{
    /// <summary>
    /// Adds an error entry to the <see cref="ValidationException.Errors"/> dictionary.
    /// </summary>
    /// <param name="exception">The exception to augment.</param>
    /// <param name="key">The error key (e.g., field name).</param>
    /// <param name="message">The error message associated with the key.</param>
    /// <returns>The same <see cref="ValidationException"/> instance, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is <c>null</c> or empty.</exception>
    public static ValidationException AddError(this ValidationException exception, string key, string message)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(message);

        // If the key already exists we keep the first message; otherwise we add.
        exception.Errors.TryAdd(key, message);
        return exception;
    }

    /// <summary>
    /// Determines whether the exception contains any error entries.
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><c>true</c> if at least one error is present; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static bool HasErrors(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.Errors.Count > 0;
    }

    /// <summary>
    /// Retrieves all error messages as a read‑only collection.
    /// </summary>
    /// <param name="exception">The exception to query.</param>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> containing each error message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static IReadOnlyCollection<string> GetErrorMessages(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.Errors.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Formats the exception's errors into a single human‑readable summary string.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A multi‑line string where each line is <c>key: message</c>. Returns an empty string if no errors exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public static string ToErrorSummary(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.HasErrors()
            ? string.Join(Environment.NewLine, exception.Errors.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
            : string.Empty;
    }
}
