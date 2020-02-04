// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class ApiCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ApiCredentialType CredentialType { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsValid { get; set; } = true;
    public string? Scope { get; set; }
    public CredentialStatus Status { get; set; } = CredentialStatus.Active;
    public int RefreshAttempts { get; set; }

    /// <summary>
    /// Checks if the access token has expired
    /// </summary>
    public bool IsAccessTokenExpired()
    {
        return DateTime.UtcNow >= AccessTokenExpiresAt;
    }

    /// <summary>
    /// Checks if credential needs refresh (30 minutes before expiration)
    /// </summary>
    public bool NeedsRefresh()
    {
        var refreshThreshold = AccessTokenExpiresAt.AddMinutes(-30);
        return DateTime.UtcNow >= refreshThreshold;
    }

    /// <summary>
    /// Updates access token with new values
    /// </summary>
    public void UpdateAccessToken(string newToken, DateTime expiresAt)
    {
        AccessToken = newToken;
        AccessTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
        RefreshAttempts = 0;
    }

    /// <summary>
    /// Marks credential refresh as attempted
    /// </summary>
    public void RecordRefreshAttempt()
    {
        RefreshAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (RefreshAttempts > 5)
        {
            Status = CredentialStatus.RefreshFailed;
            IsValid = false;
        }
    }

    /// <summary>
    /// Invalidates the credential
    /// </summary>
    public void Invalidate(string reason)
    {
        IsValid = false;
        Status = CredentialStatus.Invalid;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates credential completeness
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ClientId))
            errors.Add("Client ID is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            errors.Add("Client secret is required");

        if (string.IsNullOrWhiteSpace(AccessToken))
            errors.Add("Access token is required");

        if (AccessTokenExpiresAt < DateTime.UtcNow)
            errors.Add("Access token has already expired");

        if (string.IsNullOrWhiteSpace(RefreshToken) && CredentialType == ApiCredentialType.GoogleOAuth)
            errors.Add("Refresh token is required for OAuth credentials");

        return (errors.Count == 0, errors);
    }
}

public enum ApiCredentialType
{
    GoogleOAuth = 0,
    GoogleServiceAccount = 1,
    YouTubeApiKey = 2,
    CustomApiKey = 3
}

public enum CredentialStatus
{
    Active = 0,
    Inactive = 1,
    Expired = 2,
    RefreshFailed = 3,
    Invalid = 4,
    Revoked = 5
}
