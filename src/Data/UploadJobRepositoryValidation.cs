// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for UploadJobRepository to ensure repository contracts
// are properly initialized before use.
// =============================================================================

using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Provides validation helpers for <see cref="UploadJobRepository"/> instances.
/// </summary>
public static class UploadJobRepositoryValidation
{
    /// <summary>
    /// Validates an <see cref="UploadJobRepository"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadJobRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate public members
        if (value.GetByIdAsync == null)
        {
            problems.Add("GetByIdAsync delegate cannot be null.");
        }

        if (value.GetAllAsync == null)
        {
            problems.Add("GetAllAsync delegate cannot be null.");
        }

        if (value.GetByStatusAsync == null)
        {
            problems.Add("GetByStatusAsync delegate cannot be null.");
        }

        if (value.GetScheduledForUploadAsync == null)
        {
            problems.Add("GetScheduledForUploadAsync delegate cannot be null.");
        }

        if (value.GetRetryableFailedJobsAsync == null)
        {
            problems.Add("GetRetryableFailedJobsAsync delegate cannot be null.");
        }

        if (value.AddAsync == null)
        {
            problems.Add("AddAsync delegate cannot be null.");
        }

        if (value.UpdateAsync == null)
        {
            problems.Add("UpdateAsync delegate cannot be null.");
        }

        if (value.DeleteAsync == null)
        {
            problems.Add("DeleteAsync delegate cannot be null.");
        }

        if (value.ExistsAsync == null)
        {
            problems.Add("ExistsAsync delegate cannot be null.");
        }

        if (value.CountAsync == null)
        {
            problems.Add("CountAsync delegate cannot be null.");
        }

        if (value.SaveChangesAsync == null)
        {
            problems.Add("SaveChangesAsync delegate cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="UploadJobRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadJobRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="UploadJobRepository"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// if not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this UploadJobRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"UploadJobRepository is invalid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}