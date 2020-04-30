// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ProcessingError"/> instances
/// </summary>
public static class ProcessingErrorValidation
{
    /// <summary>
    /// Validates a <see cref="ProcessingError"/> instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The error to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ProcessingError value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            problems.Add("Id must be a non-empty GUID");
        }

        // Validate JobId
        if (value.JobId == Guid.Empty)
        {
            problems.Add("JobId must be a non-empty GUID");
        }

        // Validate ErrorType
        if (!Enum.IsDefined(typeof(ProcessingErrorType), value.ErrorType))
        {
            problems.Add($"ErrorType must be a valid ProcessingErrorType value, but was {value.ErrorType}");
        }

        // Validate ErrorMessage
        if (string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            problems.Add("ErrorMessage must not be null or whitespace");
        }
        else if (value.ErrorMessage.Length > 1000)
        {
            problems.Add($"ErrorMessage must be 1000 characters or less, but was {value.ErrorMessage.Length}");
        }

        // Validate StackTrace (if provided)
        if (value.StackTrace is { Length: > 0 } && value.StackTrace.Length > 5000)
        {
            problems.Add($"StackTrace must be 5000 characters or less, but was {value.StackTrace.Length}");
        }

        // Validate ErrorCode (if provided)
        if (value.ErrorCode is { Length: > 0 })
        {
            if (value.ErrorCode.Length > 100)
            {
                problems.Add($"ErrorCode must be 100 characters or less, but was {value.ErrorCode.Length}");
            }

            if (value.ErrorCode.Any(char.IsWhiteSpace))
            {
                problems.Add("ErrorCode must not contain whitespace");
            }
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a valid DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddHours(1))
        {
            problems.Add($"OccurredAt cannot be in the future, but was {value.OccurredAt:O}");
        }
        else if (value.OccurredAt > DateTime.UtcNow)
        {
            problems.Add($"OccurredAt is in the future ({value.OccurredAt:O}), which may indicate clock skew");
        }

        // Validate Severity
        if (!Enum.IsDefined(typeof(ErrorSeverity), value.Severity))
        {
            problems.Add($"Severity must be a valid ErrorSeverity value, but was {value.Severity}");
        }

        // Validate ResolvedAt (if set)
        if (value.ResolvedAt.HasValue)
        {
            if (value.ResolvedAt.Value == default)
            {
                problems.Add("ResolvedAt must be a valid DateTime if set");
            }
            else if (value.ResolvedAt.Value > DateTime.UtcNow.AddHours(1))
            {
                problems.Add($"ResolvedAt cannot be in the future, but was {value.ResolvedAt.Value:O}");
            }
            else if (!value.IsResolved)
            {
                problems.Add("ResolvedAt is set but IsResolved is false");
            }
            else if (value.ResolvedAt.Value < value.OccurredAt)
            {
                problems.Add($"ResolvedAt ({value.ResolvedAt.Value:O}) cannot be before OccurredAt ({value.OccurredAt:O})");
            }
        }
        else if (value.IsResolved)
        {
            problems.Add("IsResolved is true but ResolvedAt is not set");
        }

        // Validate ResolutionNotes (if provided and error is resolved)
        if (value.IsResolved && value.ResolutionNotes is { Length: > 0 })
        {
            if (value.ResolutionNotes.Length > 2000)
            {
                problems.Add($"ResolutionNotes must be 2000 characters or less, but was {value.ResolutionNotes.Length}");
            }
        }

        // Validate RetryAttemptCount
        if (value.RetryAttemptCount < 0)
        {
            problems.Add($"RetryAttemptCount must be non-negative, but was {value.RetryAttemptCount}");
        }
        else if (value.RetryAttemptCount > 100)
        {
            problems.Add($"RetryAttemptCount must be reasonable (≤ 100), but was {value.RetryAttemptCount}");
        }

        // Validate consistency between IsResolved and ResolvedAt
        if (value.IsResolved && !value.ResolvedAt.HasValue)
        {
            problems.Add("IsResolved is true but ResolvedAt is null");
        }

        // Validate that ResolvedAt is set when IsResolved is true
        if (value.ResolvedAt.HasValue && !value.IsResolved)
        {
            problems.Add("ResolvedAt is set but IsResolved is false");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ProcessingError"/> instance is valid
    /// </summary>
    /// <param name="value">The error to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this ProcessingError value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ProcessingError"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The error to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, containing the list of problems</exception>
    public static void EnsureValid(this ProcessingError value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ProcessingError is invalid:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)
            }");
    }
}