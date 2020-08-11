// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ApiCredential model
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="ApiCredential"/> instances
/// </summary>
public static class ApiCredentialValidation
{
    /// <summary>
    /// Validates an ApiCredential instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The credential to validate</param>
    /// <returns>Read-only list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this ApiCredential value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.ClientId))
        {
            errors.Add("Client ID is required and cannot be empty or whitespace");
        }
        else if (value.ClientId.Length > 255)
        {
            errors.Add("Client ID cannot exceed 255 characters");
        }

        if (string.IsNullOrWhiteSpace(value.ClientSecret))
        {
            errors.Add("Client secret is required and cannot be empty or whitespace");
        }
        else if (value.ClientSecret.Length > 255)
        {
            errors.Add("Client secret cannot exceed 255 characters");
        }

        if (string.IsNullOrWhiteSpace(value.AccessToken))
        {
            errors.Add("Access token is required and cannot be empty or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.RefreshToken) && value.CredentialType == ApiCredentialType.GoogleOAuth)
        {
            errors.Add("Refresh token is required for Google OAuth credentials");
        }

        // Validate token expiration dates
        if (value.AccessTokenExpiresAt == default)
        {
            errors.Add("Access token expiration date must be set");
        }
        else if (value.AccessTokenExpiresAt < DateTime.UtcNow)
        {
            errors.Add("Access token has already expired");
        }
        else if (value.AccessTokenExpiresAt > DateTime.UtcNow.AddYears(1))
        {
            errors.Add("Access token expiration date is unreasonably far in the future");
        }

        if (value.RefreshTokenExpiresAt.HasValue)
        {
            if (value.RefreshTokenExpiresAt.Value == default)
            {
                errors.Add("Refresh token expiration date must be a valid date if specified");
            }
            else if (value.RefreshTokenExpiresAt.Value < DateTime.UtcNow)
            {
                errors.Add("Refresh token has already expired");
            }
            else if (value.RefreshTokenExpiresAt.Value > DateTime.UtcNow.AddYears(1))
            {
                errors.Add("Refresh token expiration date is unreasonably far in the future");
            }
        }

        // Validate creation and update timestamps
        if (value.CreatedAt == default)
        {
            errors.Add("Creation date must be set");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Creation date cannot be in the future");
        }

        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                errors.Add("Update timestamp must be a valid date if specified");
            }
            else if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("Update timestamp cannot be in the future");
            }
        }

        // Validate refresh attempts
        if (value.RefreshAttempts < 0)
        {
            errors.Add("Refresh attempts cannot be negative");
        }

        // Validate status
        if (!Enum.IsDefined(typeof(CredentialStatus), value.Status))
        {
            errors.Add("Credential status is invalid");
        }

        // Validate credential type
        if (!Enum.IsDefined(typeof(ApiCredentialType), value.CredentialType))
        {
            errors.Add("Credential type is invalid");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an ApiCredential instance is valid
    /// </summary>
    /// <param name="value">The credential to check</param>
    /// <returns>True if the credential is valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this ApiCredential value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an ApiCredential instance is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The credential to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the credential is invalid, containing a list of problems</exception>
    public static void EnsureValid(this ApiCredential value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ApiCredential is invalid. Problems: {string.Join(", ", errors)}");
        }
    }
}