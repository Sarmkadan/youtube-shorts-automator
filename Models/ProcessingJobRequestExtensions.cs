using System;
using System.Globalization;
using System.Linq;

namespace YouTubeShortsAutomator.Models;

/// <summary>
/// Provides extension methods for <see cref="ProcessingJobRequest"/>.
/// </summary>
public static class ProcessingJobRequestExtensions
{
    /// <summary>
    /// Checks if the request has any tags.
    /// </summary>
    /// <param name="request">The processing job request.</param>
    /// <returns>True if tags are present; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool HasTags(this ProcessingJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Tags.Length > 0;
    }

    /// <summary>
    /// Checks if watermark is enabled in the request options.
    /// </summary>
    /// <param name="request">The processing job request.</param>
    /// <returns>True if watermark is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool IsWatermarkEnabled(this ProcessingJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Options.EnableWatermark;
    }

    /// <summary>
    /// Returns a formatted summary of the processing job request.
    /// </summary>
    /// <param name="request">The processing job request.</param>
    /// <returns>A summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static string GetSummary(this ProcessingJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return string.Format(CultureInfo.InvariantCulture,
            "Request {0}: {1} (Created at {2:O})",
            request.RequestId, request.Title, request.CreatedAtUtc);
    }
}
