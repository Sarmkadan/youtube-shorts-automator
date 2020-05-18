using System;

namespace YouTubeShortAutomator.Domain.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="UploadHistoryEntry"/> to enhance its functionality.
    /// </summary>
    public static class UploadHistoryEntryExtensions
    {
        /// <summary>
        /// Determines whether the upload history entry represents a successful upload.
        /// </summary>
        /// <param name="entry">The upload history entry to evaluate.</param>
        /// <returns><c>true</c> if the entry has a successful status and no error message; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
        public static bool IsSuccessful(this UploadHistoryEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            return entry.Status == UploadHistoryStatus.Success && string.IsNullOrEmpty(entry.ErrorMessage);
        }

        /// <summary>
        /// Calculates the duration since the upload was completed.
        /// </summary>
        /// <param name="entry">The upload history entry to evaluate.</param>
        /// <returns>The time span representing the duration since the upload was completed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
        public static TimeSpan GetUploadDuration(this UploadHistoryEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            return DateTime.UtcNow - entry.UploadedAt;
        }

        /// <summary>
        /// Gets a summary of the upload status, including any error message if present.
        /// </summary>
        /// <param name="entry">The upload history entry to evaluate.</param>
        /// <returns>A formatted string containing the status and error message (if any).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.</exception>
        public static string GetStatusSummary(this UploadHistoryEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            return entry.IsSuccessful()
                ? $"Upload successful at {entry.UploadedAt:yyyy-MM-dd HH:mm:ss}"
                : $"Upload failed: {entry.ErrorMessage} (Status: {entry.Status})";
        }
    }
}
