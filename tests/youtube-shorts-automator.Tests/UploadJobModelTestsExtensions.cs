// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Domain.Models;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Provides extension methods for the <see cref="UploadJobModelTests"/> class to assist with test setup and verification.
/// </summary>
public static class UploadJobModelTestsExtensions
{
    /// <summary>
    /// Creates a base <see cref="UploadJob"/> configured with default values for testing purposes.
    /// </summary>
    /// <returns>A new <see cref="UploadJob"/> instance.</returns>
    public static UploadJob CreateDefaultUploadJob()
    {
        return new UploadJob
        {
            AttemptCount = 0,
            MaxRetries = 3,
            Status = UploadStatus.Pending
        };
    }

    /// <summary>
    /// Configures an <see cref="UploadJob"/> to be in a failed state for testing retry logic.
    /// </summary>
    /// <param name="job">The <see cref="UploadJob"/> instance to configure.</param>
    /// <param name="attemptCount">The current attempt count.</param>
    /// <param name="maxRetries">The maximum allowed retries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public static void ConfigureFailedJob(this UploadJob job, int attemptCount, int maxRetries)
    {
        ArgumentNullException.ThrowIfNull(job);
        
        job.Status = UploadStatus.Failed;
        job.AttemptCount = attemptCount;
        job.MaxRetries = maxRetries;
    }
}
