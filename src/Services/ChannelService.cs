// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

public class ChannelService
{
    private readonly ILogger<ChannelService> _logger;

    public ChannelService(ILogger<ChannelService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsChannelTokenValid(YouTubeChannel channel)
    {
        // Checks if the channel's access token is still valid
        if (channel == null)
        {
            return false;
        }

        return !channel.IsTokenExpired() && channel.IsActive;
    }

    public bool NeedsTokenRefresh(YouTubeChannel channel)
    {
        // Checks if the token should be refreshed proactively
        return channel.NeedsTokenRefresh();
    }

    public void UpdateChannelStatus(YouTubeChannel channel, bool isActive)
    {
        // Updates the active status of a channel
        if (isActive)
        {
            channel.Activate();
        }
        else
        {
            channel.Deactivate();
        }

        _logger.LogInformation($"Updated channel {channel.ChannelName} status to {(isActive ? "active" : "inactive")}");
    }

    public string GetChannelStatusSummary(YouTubeChannel channel)
    {
        // Returns a human-readable summary of channel status
        var summary = $"Channel: {channel.ChannelName}\n";
        summary += $"Status: {(channel.IsActive ? "Active" : "Inactive")}\n";
        summary += $"Verified: {(channel.IsVerified ? "Yes" : "No")}\n";
        summary += $"Subscribers: {channel.SubscriberCount:N0}\n";
        summary += $"Views: {channel.ViewCount:N0}\n";
        summary += $"Videos: {channel.VideoCount}\n";
        summary += $"Token Valid: {(channel.IsTokenExpired() ? "Expired" : "Valid")}\n";
        summary += $"Last Synced: {channel.LastSyncAt:F}\n";

        return summary;
    }

    public bool ValidateChannelCredentials(YouTubeChannel channel)
    {
        // Validates that all required credentials are present
        if (!channel.IsValid())
        {
            _logger.LogWarning($"Channel {channel.ChannelId} failed validation");
            return false;
        }

        if (string.IsNullOrWhiteSpace(channel.AccessToken))
        {
            _logger.LogWarning("Channel missing access token");
            return false;
        }

        if (string.IsNullOrWhiteSpace(channel.RefreshToken))
        {
            _logger.LogWarning("Channel missing refresh token");
            return false;
        }

        return true;
    }
}
