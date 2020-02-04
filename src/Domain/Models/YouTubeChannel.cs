// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

public class YouTubeChannel
{
    public int Id { get; set; }
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public long SubscriberCount { get; set; }
    public long ViewCount { get; set; }
    public long VideoCount { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string DefaultLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }

    // Navigation property
    public ICollection<VideoShort> VideoShorts { get; set; } = new List<VideoShort>();

    public bool IsTokenExpired()
    {
        // Checks if the access token has expired
        return DateTime.UtcNow > TokenExpiresAt;
    }

    public void UpdateToken(string accessToken, string refreshToken, int expiresInSeconds)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds - 300);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncChannelStats(long subscribers, long views, long videos)
    {
        // Updates channel statistics from YouTube API
        SubscriberCount = subscribers;
        ViewCount = views;
        VideoCount = videos;
        LastSyncAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

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

    public bool NeedsTokenRefresh()
    {
        // Check if token refresh is needed (within 1 hour of expiration)
        return DateTime.UtcNow.AddHours(1) > TokenExpiresAt;
    }
}
