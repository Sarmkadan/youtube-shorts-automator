// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ApiCredential"/> providing common credential operations
/// </summary>
public static class ApiCredentialExtensions
{
    /// <summary>
    /// Creates a basic authentication header value for HTTP requests using the access token
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Authorization header value in format "Bearer {accessToken}"</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static string CreateBearerTokenHeader(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        if (string.IsNullOrWhiteSpace(credential.AccessToken))
        {
            throw new InvalidOperationException("Cannot create bearer token header: Access token is empty");
        }

        return $"Bearer {credential.AccessToken}";
    }

    /// <summary>
    /// Checks if the credential has a valid OAuth configuration (has both client ID and client secret)
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>True if OAuth configuration is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static bool HasValidOAuthConfig(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return !string.IsNullOrWhiteSpace(credential.ClientId)
            && !string.IsNullOrWhiteSpace(credential.ClientSecret);
    }

    /// <summary>
    /// Gets the remaining lifetime of the access token in seconds
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Remaining lifetime in seconds, or 0 if already expired</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static int GetAccessTokenLifetimeSeconds(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        var remaining = credential.AccessTokenExpiresAt - DateTime.UtcNow;
        return remaining.TotalSeconds > 0 ? (int)remaining.TotalSeconds : 0;
    }

    /// <summary>
    /// Gets the remaining lifetime of the refresh token in seconds
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Remaining lifetime in seconds, or 0 if no expiry or already expired</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static int GetRefreshTokenLifetimeSeconds(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        if (!credential.RefreshTokenExpiresAt.HasValue)
        {
            return int.MaxValue; // No expiry tracked
        }

        var remaining = credential.RefreshTokenExpiresAt.Value - DateTime.UtcNow;
        return remaining.TotalSeconds > 0 ? (int)remaining.TotalSeconds : 0;
    }

    /// <summary>
    /// Gets the credential type as a formatted string for display purposes
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Formatted credential type description</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static string GetCredentialTypeDisplay(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return credential.CredentialType switch
        {
            ApiCredentialType.GoogleOAuth => "Google OAuth 2.0",
            ApiCredentialType.GoogleServiceAccount => "Google Service Account",
            ApiCredentialType.YouTubeApiKey => "YouTube API Key",
            ApiCredentialType.CustomApiKey => "Custom API Key",
            _ => credential.CredentialType.ToString()
        };
    }

    /// <summary>
    /// Gets the credential status as a formatted string with color coding for UI display
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Formatted status string with ANSI color codes</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static string GetStatusDisplay(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return credential.Status switch
        {
            CredentialStatus.Active => "[32mActive[0m", // Green
            CredentialStatus.Inactive => "[33mInactive[0m", // Yellow
            CredentialStatus.Expired => "[31mExpired[0m", // Red
            CredentialStatus.RefreshFailed => "[31mRefresh Failed[0m", // Red
            CredentialStatus.Invalid => "[35mInvalid[0m", // Magenta
            CredentialStatus.Revoked => "[31mRevoked[0m", // Red
            _ => credential.Status.ToString()
        };
    }

    /// <summary>
    /// Gets all credential information as a formatted string for logging/debugging
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <param name="maskSecrets">Whether to mask sensitive values like tokens</param>
    /// <returns>Formatted credential information</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static string ToDebugString(this ApiCredential credential, bool maskSecrets = true)
    {
        ArgumentNullException.ThrowIfNull(credential);

        var maskedToken = maskSecrets ? "***REDACTED***" : credential.AccessToken;
        var maskedClientSecret = maskSecrets ? "***REDACTED***" : credential.ClientSecret;
        var maskedRefreshToken = maskSecrets ? "***REDACTED***" : credential.RefreshToken;

        return $"""
ApiCredential: {credential.Id}
Type: {credential.GetCredentialTypeDisplay()}
Status: {credential.GetStatusDisplay()}
User: {credential.User?.DisplayName ?? "None"} ({credential.UserId})
Valid: {credential.IsValid}
Access Token Expires: {credential.AccessTokenExpiresAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}
Remaining Lifetime: {credential.GetAccessTokenLifetimeSeconds()}s
Refresh Token Expires: {credential.RefreshTokenExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "Never"}
Refresh Attempts: {credential.RefreshAttempts}
Created: {credential.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}
Updated: {credential.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "Never"}
""".Trim();
    }

    /// <summary>
    /// Gets all credential information as a dictionary for serialization
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <param name="maskSecrets">Whether to mask sensitive values like tokens</param>
    /// <returns>Dictionary containing credential information</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static IReadOnlyDictionary<string, object> ToDictionary(this ApiCredential credential, bool maskSecrets = true)
    {
        ArgumentNullException.ThrowIfNull(credential);

        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = credential.Id,
            ["UserId"] = credential.UserId,
            ["CredentialType"] = credential.CredentialType,
            ["IsValid"] = credential.IsValid,
            ["Status"] = credential.Status,
            ["AccessTokenExpiresAt"] = credential.AccessTokenExpiresAt,
            ["RefreshAttempts"] = credential.RefreshAttempts,
            ["CreatedAt"] = credential.CreatedAt,
            ["UpdatedAt"] = credential.UpdatedAt ?? DateTime.MinValue
        };

        if (credential.User != null)
        {
            result["UserEmail"] = credential.User.Email;
            result["UserDisplayName"] = credential.User.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(credential.Scope))
        {
            result["Scope"] = credential.Scope;
        }

        if (maskSecrets)
        {
            result["AccessToken"] = "***REDACTED***";
            result["ClientSecret"] = "***REDACTED***";
            result["RefreshToken"] = "***REDACTED***";
        }
        else
        {
            result["AccessToken"] = credential.AccessToken;
            result["ClientSecret"] = credential.ClientSecret;
            result["RefreshToken"] = credential.RefreshToken;
        }

        result["ClientId"] = credential.ClientId;
        result["CredentialTypeDisplay"] = credential.GetCredentialTypeDisplay();
        result["StatusDisplay"] = credential.GetStatusDisplay();
        result["AccessTokenLifetimeSeconds"] = credential.GetAccessTokenLifetimeSeconds();
        result["RefreshTokenLifetimeSeconds"] = credential.GetRefreshTokenLifetimeSeconds();

        return result;
    }

    /// <summary>
    /// Determines if the credential is suitable for YouTube API operations
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>True if suitable for YouTube API</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static bool IsSuitableForYouTubeApi(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return credential.IsValid
            && credential.Status == CredentialStatus.Active
            && (credential.CredentialType == ApiCredentialType.GoogleOAuth
                || credential.CredentialType == ApiCredentialType.YouTubeApiKey);
    }

    /// <summary>
    /// Gets the credential's age in days
    /// </summary>
    /// <param name="credential">The API credential</param>
    /// <returns>Age in days</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static double GetAgeInDays(this ApiCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);
        return (DateTime.UtcNow - credential.CreatedAt).TotalDays;
    }
}