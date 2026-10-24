// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Application.Services;

/// <summary>
/// Uploads a processed video to YouTube with retry handling
/// </summary>
public interface IVideoUploader
{
    Task<UploadResult> UploadWithRetryAsync(Guid videoId, Guid userId);
}
