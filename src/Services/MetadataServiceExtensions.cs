// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YouTubeShortAutomator.Services;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Extension methods for <see cref="MetadataService"/>.
/// </summary>
public static class MetadataServiceExtensions
{
    /// <summary>
    /// Attempts to sanitize and validate a <see cref="VideoShort"/>'s metadata.
    /// </summary>
    /// <param name="metadataService">The <see cref="MetadataService"/> instance.</param>
    /// <param name="video">The <see cref="VideoShort"/> to validate.</param>
    /// <returns>A tuple containing a boolean indicating success and a dictionary of validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadataService"/> or <paramref name="video"/> is null.</exception>
    public static (bool IsValid, IReadOnlyDictionary<string, string> Errors) ValidateAndSanitizeMetadata(this MetadataService metadataService, VideoShort video)
    {
        ArgumentNullException.ThrowIfNull(metadataService);
        ArgumentNullException.ThrowIfNull(video);

        var errors = metadataService.ValidateMetadata(video);

        if (errors.Count > 0)
        {
            return (false, errors);
        }

        video.Title = metadataService.SanitizeTitle(video.Title);
        video.Description = metadataService.SanitizeDescription(video.Description);
        if (video.Tags != null)
        {
            video.Tags = metadataService.SanitizeTags(video.Tags);
        }

        return (true, new Dictionary<string, string>());
    }

    /// <summary>
    /// Attempts to validate a title and sanitize it if valid.
    /// </summary>
    /// <param name="metadataService">The <see cref="MetadataService"/> instance.</param>
    /// <param name="title">The title text to validate and sanitize.</param>
    /// <returns>A tuple containing a boolean indicating success and the sanitized title.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadataService"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="title"/> is empty or null.</exception>
    public static (bool IsValid, string SanitizedTitle) ValidateAndSanitizeTitle(this MetadataService metadataService, string title)
    {
        ArgumentNullException.ThrowIfNull(metadataService);
        ArgumentException.ThrowIfNullOrEmpty(title);

        if (metadataService.ValidateTitle(title))
        {
            return (true, metadataService.SanitizeTitle(title));
        }

        return (false, string.Empty);
    }
}
