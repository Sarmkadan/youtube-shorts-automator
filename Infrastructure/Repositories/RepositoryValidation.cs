// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Validation helpers for Repository&lt;TEntity&gt; to ensure repository instances are valid
/// </summary>
public static class RepositoryValidation
{
    /// <summary>
    /// Validates a Repository&lt;TEntity&gt; instance
    /// </summary>
    /// <typeparam name="TEntity">The entity type the repository manages</typeparam>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate<TEntity>([NotNull] this Repository<TEntity>? value) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(value);

        // Repository&lt;TEntity&gt; has no public properties to validate beyond being non-null
        // The base class requires ApplicationDbContext and ILogger&lt;T&gt; in its constructor,
        // but those are injected dependencies and not part of the public API to validate
        // Validation at this level primarily ensures the repository instance itself is not null

        return [];
    }

    /// <summary>
    /// Checks if a Repository&lt;TEntity&gt; instance is valid
    /// </summary>
    /// <typeparam name="TEntity">The entity type the repository manages</typeparam>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid<TEntity>([NotNullWhen(true)] this Repository<TEntity>? value) where TEntity : class
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures a Repository&lt;TEntity&gt; instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <typeparam name="TEntity">The entity type the repository manages</typeparam>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with detailed error messages</exception>
    public static void EnsureValid<TEntity>([NotNull] this Repository<TEntity>? value) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Repository<{typeof(TEntity).Name}> is invalid:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)
                }");
        }
    }
}