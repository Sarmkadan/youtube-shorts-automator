using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="YouTubeChannel"/> instances.
/// </summary>
public static class YouTubeChannelValidation
{
    /// <summary>
    /// Validates a <see cref="YouTubeChannel"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The channel to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the channel is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this YouTubeChannel value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add("Id must be a positive integer.");
        }

        // Validate ChannelId
        if (string.IsNullOrWhiteSpace(value.ChannelId))
        {
            errors.Add("ChannelId cannot be null or whitespace.");
        }
        else if (!value.ChannelId.StartsWith("UC", StringComparison.Ordinal) || value.ChannelId.Length != 24)
        {
            errors.Add("ChannelId must be a valid YouTube channel ID starting with 'UC' and be 24 characters long.");
        }

        // Validate ChannelName
        if (string.IsNullOrWhiteSpace(value.ChannelName))
        {
            errors.Add("ChannelName cannot be null or whitespace.");
        }
        else if (value.ChannelName.Length > 100)
        {
            errors.Add("ChannelName cannot exceed 100 characters.");
        }

        // Validate Description
        if (!string.IsNullOrEmpty(value.Description) && value.Description.Length > 5000)
        {
            errors.Add("Description cannot exceed 5000 characters.");
        }

        // Validate AccessToken
        if (string.IsNullOrWhiteSpace(value.AccessToken))
        {
            errors.Add("AccessToken cannot be null or whitespace.");
        }

        // Validate RefreshToken
        if (string.IsNullOrWhiteSpace(value.RefreshToken))
        {
            errors.Add("RefreshToken cannot be null or whitespace.");
        }

        // Validate TokenExpiresAt
        if (value.TokenExpiresAt == default)
        {
            errors.Add("TokenExpiresAt must be a valid DateTime.");
        }
        else if (value.TokenExpiresAt < DateTime.UtcNow)
        {
            errors.Add("TokenExpiresAt must be in the future.");
        }

        // Validate SubscriberCount
        if (value.SubscriberCount < 0)
        {
            errors.Add("SubscriberCount cannot be negative.");
        }

        // Validate ViewCount
        if (value.ViewCount < 0)
        {
            errors.Add("ViewCount cannot be negative.");
        }

        // Validate VideoCount
        if (value.VideoCount < 0)
        {
            errors.Add("VideoCount cannot be negative.");
        }

        // Validate ProfileImageUrl
        if (string.IsNullOrWhiteSpace(value.ProfileImageUrl))
        {
            errors.Add("ProfileImageUrl cannot be null or whitespace.");
        }
        else if (!Uri.IsWellFormedUriString(value.ProfileImageUrl, UriKind.Absolute))
        {
            errors.Add("ProfileImageUrl must be a valid absolute URI.");
        }

        // Validate DefaultLanguage
        if (!string.IsNullOrEmpty(value.DefaultLanguage) && value.DefaultLanguage.Length > 10)
        {
            errors.Add("DefaultLanguage cannot exceed 10 characters.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow)
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt must be a valid DateTime.");
        }
        else if (value.UpdatedAt > DateTime.UtcNow)
        {
            errors.Add("UpdatedAt cannot be in the future.");
        }

        // Validate LastSyncAt
        if (value.LastSyncAt.HasValue && value.LastSyncAt > DateTime.UtcNow)
        {
            errors.Add("LastSyncAt cannot be in the future.");
        }

        // Validate CreatedAt <= UpdatedAt
        if (value.CreatedAt != default && value.UpdatedAt != default && value.CreatedAt > value.UpdatedAt)
        {
            errors.Add("CreatedAt cannot be after UpdatedAt.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="YouTubeChannel"/> instance is valid.
    /// </summary>
    /// <param name="value">The channel to check.</param>
    /// <returns>True if the channel is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this YouTubeChannel value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="YouTubeChannel"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The channel to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the channel is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this YouTubeChannel value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"YouTubeChannel is invalid. Problems: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}