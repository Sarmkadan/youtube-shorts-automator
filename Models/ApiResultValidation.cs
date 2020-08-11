using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Provides validation helpers for <see cref="ApiResult"/> and <see cref="ApiResult{T}"/> instances
/// </summary>
public static class ApiResultValidation
{
    /// <summary>
    /// Validates an <see cref="ApiResult"/> instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="value">The ApiResult instance to validate</param>
    /// <returns>A read-only list of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(ApiResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate IsSuccess consistency
        if (value.IsSuccess && !string.IsNullOrEmpty(value.ErrorCode))
        {
            errors.Add("Success result should not have an ErrorCode set.");
        }

        if (!value.IsSuccess && string.IsNullOrEmpty(value.Message))
        {
            errors.Add("Failure result must have a non-empty Message.");
        }

        // Validate Message is never null or empty
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            errors.Add("Message must be a non-empty, non-whitespace string.");
        }

        // Validate ErrorCode format when present
        if (!string.IsNullOrEmpty(value.ErrorCode))
        {
            if (value.ErrorCode.Length > 50)
            {
                errors.Add("ErrorCode must be 50 characters or less.");
            }

            if (!IsValidErrorCode(value.ErrorCode))
            {
                errors.Add("ErrorCode should use UPPER_SNAKE_CASE format with alphanumeric characters and underscores only.");
            }
        }

        // Validate Errors dictionary when present
        if (value.Errors is not null)
        {
            if (value.IsSuccess)
            {
                errors.Add("Success result should not have Errors dictionary populated.");
            }

            foreach (var error in value.Errors)
            {
                if (string.IsNullOrWhiteSpace(error.Key))
                {
                    errors.Add("Error dictionary contains an entry with null or empty key.");
                }

                if (string.IsNullOrWhiteSpace(error.Value))
                {
                    errors.Add($"Error dictionary entry '{error.Key}' has null or empty error message.");
                }
            }
        }
        else if (!value.IsSuccess && value.ErrorCode != "VALIDATION_ERROR")
        {
            errors.Add("Failure result should have either ErrorCode or Errors dictionary populated for proper error reporting.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ApiResult"/> instance is valid
    /// </summary>
    /// <param name="value">The ApiResult instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(ApiResult value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures an <see cref="ApiResult"/> instance is valid, throwing an <see cref="ArgumentException"/> with detailed validation messages if not
    /// </summary>
    /// <param name="value">The ApiResult instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when the ApiResult is invalid, containing all validation problems</exception>
    public static void EnsureValid(ApiResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ApiResult is invalid. Validation problems:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates an <see cref="ApiResult{T}"/> instance and returns a list of human-readable validation problems
    /// </summary>
    /// <typeparam name="T">The type of data in the result</typeparam>
    /// <param name="value">The ApiResult{T} instance to validate</param>
    /// <returns>A read-only list of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate<T>(ApiResult<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate IsSuccess consistency
        if (value.IsSuccess && !string.IsNullOrEmpty(value.ErrorCode))
        {
            errors.Add("Success result should not have an ErrorCode set.");
        }

        if (!value.IsSuccess && string.IsNullOrEmpty(value.Message))
        {
            errors.Add("Failure result must have a non-empty Message.");
        }

        // Validate Message is never null or empty
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            errors.Add("Message must be a non-empty, non-whitespace string.");
        }

        // Validate ErrorCode format when present
        if (!string.IsNullOrEmpty(value.ErrorCode))
        {
            if (value.ErrorCode.Length > 50)
            {
                errors.Add("ErrorCode must be 50 characters or less.");
            }

            if (!IsValidErrorCode(value.ErrorCode))
            {
                errors.Add("ErrorCode should use UPPER_SNAKE_CASE format with alphanumeric characters and underscores only.");
            }
        }

        // Validate Errors dictionary when present
        if (value.Errors is not null)
        {
            if (value.IsSuccess)
            {
                errors.Add("Success result should not have Errors dictionary populated.");
            }

            foreach (var error in value.Errors)
            {
                if (string.IsNullOrWhiteSpace(error.Key))
                {
                    errors.Add("Error dictionary contains an entry with null or empty key.");
                }

                if (string.IsNullOrWhiteSpace(error.Value))
                {
                    errors.Add($"Error dictionary entry '{error.Key}' has null or empty error message.");
                }
            }
        }
        else if (!value.IsSuccess && value.ErrorCode != "VALIDATION_ERROR")
        {
            errors.Add("Failure result should have either ErrorCode or Errors dictionary populated for proper error reporting.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ApiResult{T}"/> instance is valid
    /// </summary>
    /// <typeparam name="T">The type of data in the result</typeparam>
    /// <param name="value">The ApiResult{T} instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(ApiResult<T> value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures an <see cref="ApiResult{T}"/> instance is valid, throwing an <see cref="ArgumentException"/> with detailed validation messages if not
    /// </summary>
    /// <typeparam name="T">The type of data in the result</typeparam>
    /// <param name="value">The ApiResult{T} instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when the ApiResult{T} is invalid, containing all validation problems</exception>
    public static void EnsureValid<T>(ApiResult<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ApiResult<T> is invalid. Validation problems:\n- {string.Join("\n- ", errors)}");
        }
    }

    private static bool IsValidErrorCode(string errorCode)
    {
        if (string.IsNullOrEmpty(errorCode))
        {
            return false;
        }

        // Must start with a letter
        if (!char.IsLetter(errorCode[0]))
        {
            return false;
        }

        // All characters must be alphanumeric or underscore
        foreach (var c in errorCode)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return false;
            }
        }

        // Must be in UPPER_SNAKE_CASE format
        var parts = errorCode.Split('_');
        if (parts.Length == 0)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part) || !part.All(char.IsUpper))
            {
                return false;
            }
        }

        return true;
    }
}