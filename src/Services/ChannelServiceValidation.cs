// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ChannelService to ensure method parameters are valid
// before operations are performed.
// =============================================================================

using YouTubeShortAutomator.Domain.Models;
using System;
using System.Collections.Generic;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides validation helpers for <see cref="ChannelService"/> to validate method
/// parameters before operations are performed.
/// </summary>
public static class ChannelServiceValidation
{
    /// <summary>
    /// Validates a YouTubeChannel instance for use with ChannelService methods.
    /// </summary>
    /// <param name="value">The ChannelService instance.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this ChannelService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates a YouTubeChannel instance for the IsChannelTokenValid method.
    /// </summary>
    /// <param name="value">The ChannelService instance.</param>
    /// <param name="channel">The YouTubeChannel to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value or channel is null.</exception>
    public static IReadOnlyList<string> Validate(
        this ChannelService value,
        YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(channel);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(channel.ChannelId))
        {
            errors.Add("Channel.ChannelId cannot be null or whitespace.");
        }
        else if (!channel.ChannelId.StartsWith("UC", StringComparison.Ordinal))
        {
            errors.Add("Channel.ChannelId must start with 'UC'.");
        }

        if (string.IsNullOrWhiteSpace(channel.ChannelName))
        {
            errors.Add("Channel.ChannelName cannot be null or whitespace.");
        }
        else if (channel.ChannelName.Length > 200)
        {
            errors.Add("Channel.ChannelName cannot exceed 200 characters.");
        }

        if (channel.TokenExpiresAt == default)
        {
            errors.Add("Channel.TokenExpiresAt cannot be default(DateTime).");
        }
        else if (channel.TokenExpiresAt < DateTime.UtcNow)
        {
            errors.Add("Channel.TokenExpiresAt cannot be in the past.");
        }

        if (string.IsNullOrWhiteSpace(channel.AccessToken))
        {
            errors.Add("Channel.AccessToken cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(channel.RefreshToken))
        {
            errors.Add("Channel.RefreshToken cannot be null or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified ChannelService instance is valid.
    /// </summary>
    /// <param name="value">The ChannelService instance to check.</param>
    /// <returns>true if the ChannelService instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this ChannelService value)
    {
        return value != null;
    }

    /// <summary>
    /// Determines whether the specified ChannelService and YouTubeChannel are valid
    /// for the IsChannelTokenValid method.
    /// </summary>
    /// <param name="value">The ChannelService instance.</param>
    /// <param name="channel">The YouTubeChannel to check.</param>
    /// <returns>
    /// true if the ChannelService and YouTubeChannel are valid for IsChannelTokenValid;
    /// otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if value or channel is null.</exception>
    public static bool IsValid(
        this ChannelService value,
        YouTubeChannel channel)
    {
        return value.IsValid() && channel != null && !channel.IsTokenExpired() && channel.IsActive;
    }

    /// <summary>
    /// Ensures that the ChannelService instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The ChannelService instance to check.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static void EnsureValid(this ChannelService value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    /// <summary>
    /// Ensures that the ChannelService and YouTubeChannel are valid for the
    /// IsChannelTokenValid method, throwing an exception if not.
    /// </summary>
    /// <param name="value">The ChannelService instance.</param>
    /// <param name="channel">The YouTubeChannel to check.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if value or channel is null, or if validation fails.
    /// </exception>
    public static void EnsureValid(
        this ChannelService value,
        YouTubeChannel channel)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(channel);

        var errors = value.Validate(channel);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(" ", errors),
                nameof(channel));
        }
    }
}