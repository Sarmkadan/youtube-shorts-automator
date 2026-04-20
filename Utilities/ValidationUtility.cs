// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation utilities for common data types and formats
/// Email, URLs, video metadata, and custom validation rules
/// </summary>
public static class ValidationUtility
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UrlRegex = new(
        @"^https?://",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex YouTubeChannelIdRegex = new(
        @"^UC[a-zA-Z0-9_-]{21}[AQgw]$",
        RegexOptions.Compiled);

    private static readonly Regex YouTubeVideoIdRegex = new(
        @"^[a-zA-Z0-9_-]{11}$",
        RegexOptions.Compiled);

    public static (bool IsValid, string? Error) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");

        if (email.Length > 254)
            return (false, "Email is too long");

        if (!EmailRegex.IsMatch(email))
            return (false, "Email format is invalid");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (false, "URL is required");

        if (!UrlRegex.IsMatch(url))
            return (false, "URL must start with http:// or https://");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return (false, "URL format is invalid");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateYouTubeChannelId(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            return (false, "Channel ID is required");

        if (!YouTubeChannelIdRegex.IsMatch(channelId))
            return (false, "Invalid YouTube channel ID format");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateYouTubeVideoId(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
            return (false, "Video ID is required");

        if (!YouTubeVideoIdRegex.IsMatch(videoId))
            return (false, "Invalid YouTube video ID format (must be 11 characters)");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateVideoTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return (false, "Title is required");

        if (title.Length < 5)
            return (false, "Title must be at least 5 characters");

        if (title.Length > 100)
            return (false, "Title must not exceed 100 characters");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateVideoDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (false, "Description is required");

        if (description.Length < 10)
            return (false, "Description must be at least 10 characters");

        if (description.Length > 5000)
            return (false, "Description must not exceed 5000 characters");

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateVideoTags(string[] tags)
    {
        if (tags == null || tags.Length == 0)
            return (false, "At least one tag is required");

        if (tags.Length > 30)
            return (false, "Maximum 30 tags allowed");

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return (false, "Tags cannot be empty");

            if (tag.Length > 30)
                return (false, "Each tag must not exceed 30 characters");
        }

        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateVideoFile(string filePath, long maxSizeBytes = 10_737_418_240)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return (false, "File path is required");

        if (!File.Exists(filePath))
            return (false, "File does not exist");

        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length == 0)
            return (false, "File is empty");

        if (fileInfo.Length > maxSizeBytes)
            return (false, $"File exceeds maximum size of {maxSizeBytes / 1_073_741_824}GB");

        return (true, null);
    }

    public static bool IsValidTimeSpan(string timeSpan)
    {
        return TimeSpan.TryParse(timeSpan, out _);
    }

    public static bool IsValidJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (bool IsValid, string? Error) ValidateScheduleTime(string scheduledTime)
    {
        if (string.IsNullOrWhiteSpace(scheduledTime))
            return (false, "Schedule time is required");

        if (!DateTime.TryParse(scheduledTime, out var parsedTime))
            return (false, "Invalid schedule time format");

        if (parsedTime <= DateTime.UtcNow.AddMinutes(1))
            return (false, "Schedule time must be at least 1 minute in the future");

        return (true, null);
    }
}
