// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="UploadResult"/> to enhance upload analysis and reporting capabilities.
/// </summary>
public static class UploadResultExtensions
{
    /// <summary>
    /// Calculates the upload completion percentage based on uploaded bytes vs total bytes.
    /// </summary>
    /// <param name="uploadResult">The upload result to calculate completion for.</param>
    /// <returns>The completion percentage (0-100), or 0 if total bytes is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uploadResult"/> is null.</exception>
    public static double GetCompletionPercentage(this UploadResult uploadResult)
    {
        ArgumentNullException.ThrowIfNull(uploadResult);

        if (uploadResult.TotalBytes <= 0)
        {
            return 0;
        }

        return Math.Round((double)uploadResult.UploadedBytes / uploadResult.TotalBytes * 100, 2);
    }

    /// <summary>
    /// Determines whether the upload was completed within the expected time based on file size and average speed.
    /// </summary>
    /// <param name="uploadResult">The upload result to check.</param>
    /// <param name="expectedSpeedMbPerSecond">The expected minimum upload speed in MB/s.</param>
    /// <returns>True if the upload completed at or above the expected speed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uploadResult"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedSpeedMbPerSecond"/> is less than or equal to 0.</exception>
    public static bool IsWithinExpectedSpeed(this UploadResult uploadResult, double expectedSpeedMbPerSecond)
    {
        ArgumentNullException.ThrowIfNull(uploadResult);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expectedSpeedMbPerSecond, 0);

        if (!uploadResult.IsSuccessful || uploadResult.UploadDuration.TotalSeconds <= 0)
        {
            return false;
        }

        double actualSpeed = uploadResult.AverageUploadSpeed;
        return actualSpeed >= expectedSpeedMbPerSecond;
    }

    /// <summary>
    /// Gets a human-readable status message describing the upload result.
    /// </summary>
    /// <param name="uploadResult">The upload result to get status for.</param>
    /// <returns>A formatted status message indicating success/failure and key metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uploadResult"/> is null.</exception>
    public static string GetStatusMessage(this UploadResult uploadResult)
    {
        ArgumentNullException.ThrowIfNull(uploadResult);

        if (uploadResult.IsSuccessful)
        {
            return $"✅ Upload successful | Video: {uploadResult.VideoId} | Speed: {uploadResult.GetUploadSpeedFormatted()} | Duration: {uploadResult.GetDurationFormatted()}";
        }
        else
        {
            return $"❌ Upload failed | Error: {uploadResult.ErrorDetails ?? "Unknown error"} | Duration: {uploadResult.GetDurationFormatted()}";
        }
    }

    /// <summary>
    /// Gets a detailed performance report for the upload operation.
    /// </summary>
    /// <param name="uploadResult">The upload result to analyze.</param>
    /// <returns>A formatted string containing detailed upload performance metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uploadResult"/> is null.</exception>
    public static string GetPerformanceReport(this UploadResult uploadResult)
    {
        ArgumentNullException.ThrowIfNull(uploadResult);

        var report = new System.Text.StringBuilder();
        report.AppendLine("📊 Upload Performance Report");
        report.AppendLine("========================");
        report.AppendLine($"Video ID: {uploadResult.VideoId}");
        report.AppendLine($"Status: {(uploadResult.IsSuccessful ? "✅ Success" : "❌ Failed")}");

        if (uploadResult.IsSuccessful)
        {
            report.AppendLine($"Video URL: {uploadResult.VideoUrl}");
            report.AppendLine($"File Size: {FormatFileSize(uploadResult.TotalBytes)}");
            report.AppendLine($"Uploaded: {FormatFileSize(uploadResult.UploadedBytes)}");
            report.AppendLine($"Completion: {uploadResult.GetCompletionPercentage():F2}%");
            report.AppendLine($"Duration: {uploadResult.GetDurationFormatted()}");
            report.AppendLine($"Average Speed: {uploadResult.GetUploadSpeedFormatted()}");

            if (uploadResult.UploadJob != null)
            {
                report.AppendLine($"Job ID: {uploadResult.UploadJob.Id}");
                report.AppendLine($"Video Short ID: {uploadResult.UploadJob.VideoShortId}");
                report.AppendLine($"Status: {uploadResult.UploadJob.Status}");
            }
        }
        else
        {
            report.AppendLine($"Error Details: {uploadResult.ErrorDetails ?? "No details provided"}");
        }

        report.AppendLine($"Completed At: {uploadResult.CompletedAt:yyyy-MM-dd HH:mm:ss UTC}");
        return report.ToString();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double len = bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}