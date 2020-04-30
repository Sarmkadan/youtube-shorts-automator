// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for DomainException and derived exception types
// =====================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="DomainException"/> and derived exception types.
/// </summary>
public static class DomainExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="DomainException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DomainException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ErrorCode
        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode is null or whitespace.");
        }
        else if (value.ErrorCode.Length > 50)
        {
            problems.Add("ErrorCode exceeds maximum length of 50 characters.");
        }

        // Validate Context
        if (value.Context is not null)
        {
            if (value.Context.Count > 50)
            {
                problems.Add("Context dictionary exceeds maximum size of 50 entries.");
            }

            foreach (var kvp in value.Context)
            {
                if (kvp.Key is null)
                {
                    problems.Add("Context contains a null key.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Context contains an empty or whitespace key.");
                    break;
                }

                if (kvp.Key.Length > 100)
                {
                    problems.Add("Context key exceeds maximum length of 100 characters.");
                    break;
                }
            }
        }

        // Validate Message (from base Exception class)
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message is null or whitespace.");
        }
        else if (value.Message.Length > 1000)
        {
            problems.Add("Message exceeds maximum length of 1000 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DomainException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DomainException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="DomainException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this DomainException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DomainException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="VideoValidationException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this VideoValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate ValidationErrors
        if (value.ValidationErrors is null)
        {
            problems.Add("ValidationErrors collection is null.");
        }
        else
        {
            if (value.ValidationErrors.Count > 1000)
            {
                problems.Add("ValidationErrors collection exceeds maximum size of 1000 entries.");
            }

            for (var i = 0; i < value.ValidationErrors.Count; i++)
            {
                var error = value.ValidationErrors[i];
                if (string.IsNullOrWhiteSpace(error))
                {
                    problems.Add($"ValidationErrors[{i}] is null or whitespace.");
                }
                else if (error.Length > 500)
                {
                    problems.Add($"ValidationErrors[{i}] exceeds maximum length of 500 characters.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoValidationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this VideoValidationException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="VideoValidationException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this VideoValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"VideoValidationException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="ProcessingJobException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingJobException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate JobId
        if (value.JobId == Guid.Empty)
        {
            problems.Add("JobId is empty (Guid.Empty).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ProcessingJobException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ProcessingJobException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ProcessingJobException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ProcessingJobException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProcessingJobException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="UploadException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this UploadException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate YouTubeErrorCode
        if (value.YouTubeErrorCode is not null)
        {
            if (string.IsNullOrWhiteSpace(value.YouTubeErrorCode))
            {
                problems.Add("YouTubeErrorCode is empty or whitespace.");
            }
            else if (value.YouTubeErrorCode.Length > 50)
            {
                problems.Add("YouTubeErrorCode exceeds maximum length of 50 characters.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UploadException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this UploadException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UploadException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this UploadException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"UploadException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="CredentialException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CredentialException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Validate((DomainException)value);
    }

    /// <summary>
    /// Determines whether the specified <see cref="CredentialException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CredentialException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CredentialException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this CredentialException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CredentialException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates an <see cref="OAuthTokenExpiredException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this OAuthTokenExpiredException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((CredentialException)value));

        // Validate CredentialId
        if (value.CredentialId == Guid.Empty)
        {
            problems.Add("CredentialId is empty (Guid.Empty).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="OAuthTokenExpiredException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this OAuthTokenExpiredException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="OAuthTokenExpiredException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this OAuthTokenExpiredException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"OAuthTokenExpiredException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates an <see cref="ApiException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate HttpStatusCode
        if (value.HttpStatusCode.HasValue)
        {
            if (value.HttpStatusCode < 100 || value.HttpStatusCode > 999)
            {
                problems.Add("HttpStatusCode must be a valid HTTP status code (100-999).");
            }
        }

        // Validate ApiResponse
        if (value.ApiResponse is not null)
        {
            if (string.IsNullOrWhiteSpace(value.ApiResponse))
            {
                problems.Add("ApiResponse is empty or whitespace.");
            }
            else if (value.ApiResponse.Length > 10000)
            {
                problems.Add("ApiResponse exceeds maximum length of 10000 characters.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApiException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ApiException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApiException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="QuotaExceededException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QuotaExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate CurrentUsageBytes
        if (value.CurrentUsageBytes < 0)
        {
            problems.Add("CurrentUsageBytes must be non-negative.");
        }

        // Validate QuotaBytes
        if (value.QuotaBytes <= 0)
        {
            problems.Add("QuotaBytes must be positive.");
        }
        else if (value.QuotaBytes < value.CurrentUsageBytes)
        {
            problems.Add("QuotaBytes must be greater than or equal to CurrentUsageBytes.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QuotaExceededException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QuotaExceededException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QuotaExceededException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this QuotaExceededException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QuotaExceededException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates a <see cref="ResourceNotFoundException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ResourceNotFoundException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate ResourceId
        if (value.ResourceId == Guid.Empty)
        {
            problems.Add("ResourceId is empty (Guid.Empty).");
        }

        // Validate ResourceType
        if (string.IsNullOrWhiteSpace(value.ResourceType))
        {
            problems.Add("ResourceType is null or whitespace.");
        }
        else if (value.ResourceType.Length > 100)
        {
            problems.Add("ResourceType exceeds maximum length of 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ResourceNotFoundException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ResourceNotFoundException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ResourceNotFoundException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ResourceNotFoundException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ResourceNotFoundException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }

    /// <summary>
    /// Validates an <see cref="InvalidStateException"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InvalidStateException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>(Validate((DomainException)value));

        // Validate CurrentState
        if (string.IsNullOrWhiteSpace(value.CurrentState))
        {
            problems.Add("CurrentState is null or whitespace.");
        }
        else if (value.CurrentState.Length > 100)
        {
            problems.Add("CurrentState exceeds maximum length of 100 characters.");
        }

        // Validate RequestedOperation
        if (string.IsNullOrWhiteSpace(value.RequestedOperation))
        {
            problems.Add("RequestedOperation is null or whitespace.");
        }
        else if (value.RequestedOperation.Length > 100)
        {
            problems.Add("RequestedOperation exceeds maximum length of 100 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="InvalidStateException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this InvalidStateException value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="InvalidStateException"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this InvalidStateException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"InvalidStateException validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}
