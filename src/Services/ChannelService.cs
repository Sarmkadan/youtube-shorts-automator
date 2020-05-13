// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Data;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides methods for managing YouTube channel status and credentials.
/// </summary>
public class ChannelService
{
    private readonly ILogger<ChannelService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ChannelService(ILogger<ChannelService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks if the channel's access token is still valid.
    /// </summary>
    /// <param name="channel">The YouTube channel instance.</param>
    /// <returns>true if the token is valid; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsChannelTokenValid(YouTubeChannel channel)
    {
        // Checks if the channel's access token is still valid
        if (channel == null)
        {
            return false;
        }

        return !channel.IsTokenExpired() && channel.IsActive;
    }

    /// <summary>
    /// Checks if the token should be refreshed proactively.
    /// </summary>
    /// <param name="channel">The YouTube channel instance.</param>
    /// <returns>true if the token should be refreshed; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool NeedsTokenRefresh(YouTubeChannel channel)
    {
        // Fix: Add null check for channel parameter.
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel), "YouTube channel cannot be null.");
        }
        // Checks if the token should be refreshed proactively
        return channel.NeedsTokenRefresh();
    }

    /// <summary>
    /// Updates the active status of a channel.
    /// </summary>
    /// <param name="channel">The YouTube channel instance.</param>
    /// <param name="isActive">true to activate the channel; otherwise, false.</param>
    public void UpdateChannelStatus(YouTubeChannel channel, bool isActive)
    {
        // Fix: Add null check for channel parameter.
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel), "YouTube channel cannot be null.");
        }
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

    /// <summary>
    /// Returns a human-readable summary of channel status.
    /// </summary>
    /// <param name="channel">The YouTube channel instance.</param>
    /// <returns>A string containing the channel status summary.</returns>
    public string GetChannelStatusSummary(YouTubeChannel channel)
    {
        // Fix: Add null check for channel parameter.
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel), "YouTube channel cannot be null.");
        }
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

    /// <summary>
    /// Validates that all required credentials are present.
    /// </summary>
    /// <param name="channel">The YouTube channel instance.</param>
    /// <returns>true if the credentials are valid; otherwise, false.</returns>
    public bool ValidateChannelCredentials(YouTubeChannel channel)
    {
        // Fix: Add null check for channel parameter.
        if (channel == null)
        {
            throw new ArgumentNullException(nameof(channel), "YouTube channel cannot be null.");
        }
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
