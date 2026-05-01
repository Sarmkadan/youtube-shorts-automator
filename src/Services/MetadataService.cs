// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Validates and sanitizes YouTube Shorts video metadata (titles, descriptions, tags)
/// according to YouTube platform rules and length constraints.
/// </summary>
public class MetadataService
{
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(ILogger<MetadataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates a YouTube video title against platform rules: must be non-empty,
    /// at least 5 characters, and within <see cref="Constants.Constants.MAX_TITLE_LENGTH"/>.
    /// </summary>
    /// <param name="title">The title text to validate.</param>
    /// <returns><c>true</c> if the title meets all YouTube requirements; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Validates a YouTube video description by checking it does not exceed
    /// <see cref="Constants.Constants.MAX_DESCRIPTION_LENGTH"/>.
    /// </summary>
    /// <param name="description">The description text to validate.</param>
    /// <returns><c>true</c> if within the allowed length; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Validates an array of YouTube video tags. Checks total tag count does not exceed
    /// <see cref="Constants.Constants.MAX_TAGS_COUNT"/> and each individual tag is within
    /// <see cref="Constants.Constants.MAX_TAG_LENGTH"/>.
    /// </summary>
    /// <param name="tags">The tag array to validate.</param>
    /// <returns><c>true</c> if all tags pass validation; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Removes characters disallowed in YouTube titles (<c>&lt; &gt; : " / \ | ? *</c>)
    /// and truncates the result to <see cref="Constants.Constants.MAX_TITLE_LENGTH"/>.
    /// </summary>
    /// <param name="title">The raw title to sanitize.</param>
    /// <returns>A sanitized title safe for YouTube upload.</returns>
    public string SanitizeTitle(string title)
    {
        // Removes special characters and sanitizes title
        var sanitized = System.Text.RegularExpressions.Regex.Replace(title, @"[<>:""/\\|?*]", "");
        return sanitized.Substring(0, Math.Min(sanitized.Length, Constants.Constants.MAX_TITLE_LENGTH));
    }

    /// <summary>
    /// Truncates a description to <see cref="Constants.Constants.MAX_DESCRIPTION_LENGTH"/>
    /// if it exceeds the limit.
    /// </summary>
    /// <param name="description">The raw description text.</param>
    /// <returns>The description, truncated if necessary.</returns>
    public string SanitizeDescription(string description)
    {
        // Truncates description to maximum allowed length
        if (description.Length > Constants.Constants.MAX_DESCRIPTION_LENGTH)
        {
            return description.Substring(0, Constants.Constants.MAX_DESCRIPTION_LENGTH);
        }

        return description;
    }

    /// <summary>
    /// Filters, deduplicates, and truncates tags to conform to YouTube limits.
    /// Removes empty/whitespace tags, enforces per-tag length, and caps total count.
    /// </summary>
    /// <param name="tags">The raw tag array to sanitize.</param>
    /// <returns>A sanitized array of unique tags within platform limits.</returns>
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

    /// <summary>
    /// Validates all metadata fields on a <see cref="VideoShort"/> and returns a dictionary
    /// of field-level validation errors. An empty dictionary indicates all metadata is valid.
    /// </summary>
    /// <param name="video">The video entity whose metadata to validate.</param>
    /// <returns>
    /// A dictionary keyed by field name ("title", "description", "tags") with error messages
    /// as values. Empty when validation passes.
    /// </returns>
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
