// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

public class MetadataService
{
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(ILogger<MetadataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool ValidateTitle(string title)
    {
        // Validates YouTube video title according to platform rules
        if (string.IsNullOrWhiteSpace(title))
        {
            _logger.LogWarning("Title is empty");
            return false;
        }

        if (title.Length > Constants.Constants.MAX_TITLE_LENGTH)
        {
            _logger.LogWarning($"Title exceeds max length: {title.Length} > {Constants.Constants.MAX_TITLE_LENGTH}");
            return false;
        }

        if (title.Length < 5)
        {
            _logger.LogWarning("Title is too short (minimum 5 characters)");
            return false;
        }

        return true;
    }

    public bool ValidateDescription(string description)
    {
        // Validates YouTube video description
        if (description.Length > Constants.Constants.MAX_DESCRIPTION_LENGTH)
        {
            _logger.LogWarning($"Description exceeds max length: {description.Length}");
            return false;
        }

        return true;
    }

    public bool ValidateTags(string[] tags)
    {
        // Validates YouTube video tags
        if (tags.Length > Constants.Constants.MAX_TAGS_COUNT)
        {
            _logger.LogWarning($"Too many tags: {tags.Length} > {Constants.Constants.MAX_TAGS_COUNT}");
            return false;
        }

        foreach (var tag in tags)
        {
            if (tag.Length > Constants.Constants.MAX_TAG_LENGTH)
            {
                _logger.LogWarning($"Tag too long: {tag}");
                return false;
            }
        }

        return true;
    }

    public string SanitizeTitle(string title)
    {
        // Removes special characters and sanitizes title
        var sanitized = System.Text.RegularExpressions.Regex.Replace(title, @"[<>:""/\\|?*]", "");
        return sanitized.Substring(0, Math.Min(sanitized.Length, Constants.Constants.MAX_TITLE_LENGTH));
    }

    public string SanitizeDescription(string description)
    {
        // Truncates description to maximum allowed length
        if (description.Length > Constants.Constants.MAX_DESCRIPTION_LENGTH)
        {
            return description.Substring(0, Constants.Constants.MAX_DESCRIPTION_LENGTH);
        }

        return description;
    }

    public string[] SanitizeTags(string[] tags)
    {
        // Filters and sanitizes tags
        var sanitized = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Substring(0, Math.Min(t.Length, Constants.Constants.MAX_TAG_LENGTH)))
            .Distinct()
            .Take(Constants.Constants.MAX_TAGS_COUNT)
            .ToArray();

        return sanitized;
    }

    public Dictionary<string, string> ValidateMetadata(VideoShort video)
    {
        // Validates all video metadata and returns validation errors
        var errors = new Dictionary<string, string>();

        if (!ValidateTitle(video.Title))
        {
            errors["title"] = "Invalid title";
        }

        if (!ValidateDescription(video.Description))
        {
            errors["description"] = "Description exceeds maximum length";
        }

        if (video.Tags != null && !ValidateTags(video.Tags))
        {
            errors["tags"] = "Invalid tags";
        }

        return errors;
    }
}
