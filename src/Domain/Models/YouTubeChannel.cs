// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a YouTube channel.
/// </summary>
public class YouTubeChannel
{
    /// <summary>
    /// Gets the unique identifier of the YouTube channel.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the ID of the YouTube channel.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the name of the YouTube channel.
    /// </summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the description of the YouTube channel.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the access token for the YouTube channel.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets the refresh token for the YouTube channel.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the access token expires.
    /// </summary>
    public DateTime TokenExpiresAt { get; set; }

    /// <summary>
    /// Gets the number of subscribers to the YouTube channel.
    /// </summary>
    public long SubscriberCount { get; set; }

    /// <summary>
    /// Gets the number of views on the YouTube channel.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Gets the number of videos on the YouTube channel.
    /// </summary>
    public long VideoCount { get; set; }

    /// <summary>
    /// Gets the URL of the profile image of the YouTube channel.
    /// </summary>
    public string ProfileImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the YouTube channel is verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets a value indicating whether the YouTube channel is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets the default language of the YouTube channel.
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// Gets the date and time when the YouTube channel was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time when the YouTube channel was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets the date and time when the YouTube channel was last synchronized.
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Gets a collection of video shorts associated with the YouTube channel.
    /// </summary>
    public ICollection<VideoShort> VideoShorts { get; set; } = new List<VideoShort>();

    /// <summary>
    /// Checks if the access token has expired.
    /// </summary>
    /// <returns>true if the access token has expired; otherwise, false.</returns>
    public bool IsTokenExpired()
    {
        // Checks if the access token has expired
        return DateTime.UtcNow > TokenExpiresAt;
    }

    /// <summary>
    /// Updates the access token, refresh token, and expiration date for the YouTube channel.
    /// </summary>
    /// <param name="accessToken">The new access token.</param>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <param name="expiresInSeconds">The number of seconds until the access token expires.</param>
    public void UpdateToken(string accessToken, string refreshToken, int expiresInSeconds)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds - 300);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the channel statistics for the YouTube channel.
    /// </summary>
    /// <param name="subscribers">The new number of subscribers.</param>
    /// <param name="views">The new number of views.</param>
    /// <param name="videos">The new number of videos.</param>
    public void SyncChannelStats(long subscribers, long views, long videos)
    {
        // Updates channel statistics from YouTube API
        SubscriberCount = subscribers;
        ViewCount = views;
        VideoCount = videos;
        LastSyncAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifies the YouTube channel.
    /// </summary>
    public void Verify()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the YouTube channel.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the YouTube channel.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the YouTube channel.
    /// </summary>
    /// <returns>true if the YouTube channel is valid; otherwise, false.</returns>
    public bool IsValid()
    {
        // Validates the YouTube channel
        if (string.IsNullOrWhiteSpace(ChannelId) || !ChannelId.StartsWith("UC"))
            return false;
        if (string.IsNullOrWhiteSpace(ChannelName) || ChannelName.Length > 200)
            return false;
        if (string.IsNullOrWhiteSpace(AccessToken))
            return false;
        if (string.IsNullOrWhiteSpace(RefreshToken))
            return false;
        return true;
    }

    /// <summary>
    /// Checks if the access token needs to be refreshed.
    /// </summary>
    /// <returns>true if the access token needs to be refreshed; otherwise, false.</returns>
    public bool NeedsTokenRefresh()
    {
        // Check if token refresh is needed (within 1 hour of expiration)
        return DateTime.UtcNow.AddHours(1) > TokenExpiresAt;
    }
}
