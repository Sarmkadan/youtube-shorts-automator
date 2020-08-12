// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace YouTubeShortsAutomator.Middleware;

/// <summary>
/// Validation helpers for <see cref="ErrorResponse"/>.
/// </summary>
public static class ErrorResponseValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> instance to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of problems, or an empty list if the <paramref name="value"/> is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this ErrorResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.Message))
        {
            problems.Add("Message cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(value.ErrorCode))
        {
            problems.Add("ErrorCode cannot be null or empty.");
        }

        if (value.Timestamp == DateTime.MinValue || value.Timestamp == DateTime.MaxValue)
        {
            problems.Add("Timestamp must be a valid date.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> instance to validate.</param>
    /// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this ErrorResponse value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the specified <paramref name="value"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The <see cref="ErrorResponse"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this ErrorResponse value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            var message = new StringBuilder();
            message.AppendLine("Invalid ErrorResponse:");
            foreach (var problem in problems)
            {
                message.AppendLine(problem);
            }
            throw new ArgumentException(message.ToString(), nameof(value));
        }
    }
}
