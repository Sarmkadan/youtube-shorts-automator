using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.Services;

/// <summary>
/// Provides extension methods for the <see cref="FileValidationService"/> class.
/// </summary>
public static class FileValidationServiceExtensions
{
    /// <summary>
    /// Validates a collection of video files for processing.
    /// </summary>
    /// <param name="service">The <see cref="FileValidationService"/> instance.</param>
    /// <param name="filePaths">The collection of file paths to validate.</param>
    /// <returns>A read-only list with one <see cref="bool"/> validation result per file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="filePaths"/> is null.</exception>
    public static IReadOnlyList<bool> ValidateVideoFiles(this FileValidationService service, IEnumerable<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(filePaths);

        return filePaths
            .Select(filePath => service.ValidateVideoFile(filePath))
            .ToList();
    }

    /// <summary>
    /// Calculates the SHA256 hash of a collection of files for integrity verification.
    /// </summary>
    /// <param name="service">The <see cref="FileValidationService"/> instance.</param>
    /// <param name="filePaths">The collection of file paths to hash.</param>
    /// <returns>A read-only list with one <see cref="string"/> hash per file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="filePaths"/> is null.</exception>
    public static IReadOnlyList<string> GetFileHashes(this FileValidationService service, IEnumerable<string> filePaths)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(filePaths);

        return filePaths
            .Select(filePath => service.GetFileHash(filePath))
            .ToList();
    }
}
