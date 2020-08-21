using System;
using System.Collections.Generic;
using System.Linq;
using YouTubeShortAutomator.Data;

namespace YouTubeShortAutomator.Data;

/// <summary>
/// Validation helpers for <see cref="UploadJobRepositoryExtensions"/>.
/// </summary>
public static class UploadJobRepositoryExtensionsValidation
{
    /// <summary>
    /// Validates the specified <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">The repository to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadJobRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var problems = new List<string>();

        // Add validation rules here

        return problems;
    }

    /// <summary>
    /// Checks if the specified <paramref name="repository"/> is valid.
    /// </summary>
    /// <param name="repository">The repository to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static bool IsValid(this UploadJobRepository repository)
    {
        return !Validate(repository).Any();
    }

    /// <summary>
    /// Ensures the specified <paramref name="repository"/> is valid.
    /// </summary>
    /// <param name="repository">The repository to ensure.</param>
    /// <exception cref="ArgumentException">The <paramref name="repository"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static void EnsureValid(this UploadJobRepository repository)
    {
        var problems = Validate(repository);
        if (problems.Any())
        {
            throw new ArgumentException($"Invalid repository: {string.Join(Environment.NewLine, problems)}", nameof(repository));
        }
    }
}
