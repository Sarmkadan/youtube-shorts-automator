// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;
using YouTubeShortsAutomator.Application.Repositories;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Infrastructure.Repositories;

/// <summary>
/// Validation helpers for ApiCredentialRepository operations
/// </summary>
public static class ApiCredentialRepositoryValidation
{
    /// <summary>
    /// Validates all parameters for repository operations
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if repository is null</exception>
    public static IReadOnlyList<string> Validate(this ApiCredentialRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository-level validations would go here if there were any
        // Currently the repository itself has no state to validate beyond being non-null

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the repository instance is valid
    /// </summary>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ApiCredentialRepository value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the repository instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if repository is null</exception>
    /// <exception cref="ArgumentException">Thrown if repository is invalid with detailed error messages</exception>
    public static void EnsureValid(this ApiCredentialRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Repository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetByUserIdAsync
    /// </summary>
    /// <param name="userId">User ID to query by</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this Guid userId)
    {
        var problems = new List<string>();

        // Guid.Empty is a valid sentinel value for "all users" scenarios
        // No validation needed beyond null check (Guid is a struct)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the userId parameter is valid for GetByUserIdAsync
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this Guid userId)
    {
        return userId.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the userId parameter is valid, throwing an exception if not
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <exception cref="ArgumentException">Thrown if userId is invalid</exception>
    public static void EnsureValid(this Guid userId)
    {
        var problems = userId.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"User ID validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetActiveCredentialAsync
    /// </summary>
    /// <param name="userId">User ID to query by</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this Guid userId, [NotNull] string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (userId == Guid.Empty)
        {
            problems.Add($"{paramName}: User ID cannot be empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the userId parameter is valid for GetActiveCredentialAsync
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="paramName">Name of the parameter being validated</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this Guid userId, [NotNull] string paramName)
    {
        return userId.Validate(paramName).Count == 0;
    }

    /// <summary>
    /// Ensures the userId parameter is valid, throwing an exception if not
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <param name="paramName">Name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown if userId is invalid</exception>
    public static void EnsureValid(this Guid userId, [NotNull] string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = userId.Validate(paramName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Parameter validation failed for '{paramName}':{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetByStatusAsync
    /// </summary>
    /// <param name="status">Credential status to filter by</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this CredentialStatus status)
    {
        // All CredentialStatus enum values are valid
        // No validation needed
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the status parameter is valid for GetByStatusAsync
    /// </summary>
    /// <param name="status">Status to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this CredentialStatus status)
    {
        return status.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the status parameter is valid, throwing an exception if not
    /// </summary>
    /// <param name="status">Status to validate</param>
    /// <exception cref="ArgumentException">Thrown if status is invalid</exception>
    public static void EnsureValid(this CredentialStatus status)
    {
        var problems = status.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Status validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetExpiredCredentialsAsync
    /// </summary>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate()
    {
        // No parameters to validate
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the parameters are valid for GetExpiredCredentialsAsync
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the parameters are valid, throwing an exception if not
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Parameter validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates parameters for GetByTypeAsync
    /// </summary>
    /// <param name="type">Credential type to filter by</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this ApiCredentialType type)
    {
        // All ApiCredentialType enum values are valid
        // No validation needed
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the type parameter is valid for GetByTypeAsync
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ApiCredentialType type)
    {
        return type.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the type parameter is valid, throwing an exception if not
    /// </summary>
    /// <param name="type">Type to validate</param>
    /// <exception cref="ArgumentException">Thrown if type is invalid</exception>
    public static void EnsureValid(this ApiCredentialType type)
    {
        var problems = type.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Type validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}