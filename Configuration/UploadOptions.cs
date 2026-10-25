// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Upload settings, bound from the <c>Upload</c> configuration section.
/// </summary>
public sealed class UploadOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Upload";

    /// <summary>Number of retry attempts after a failed upload. Zero disables retries.</summary>
    public int MaxRetries { get; set; } = ApplicationConstants.OptionLimits.DefaultUploadMaxRetries;
}
