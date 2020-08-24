using System;
using System.Globalization;
using System.Linq;
using YouTubeShortAutomator.Domain.Models;

public static class ProcessingProfileExtensions
{
    /// <summary>
    /// Throws an exception if the processing profile is null.
    /// </summary>
    /// <param name="processingProfile">The processing profile to check.</param>
    /// <exception cref="ArgumentNullException">If the processing profile is null.</exception>
    public static void ThrowIfNull(this ProcessingProfile processingProfile)
    {
        ArgumentNullException.ThrowIfNull(processingProfile);
    }

    /// <summary>
    /// Throws an exception if the processing profile name is null or empty.
    /// </summary>
    /// <param name="processingProfile">The processing profile to check.</param>
    /// <exception cref="ArgumentException">If the processing profile name is null or empty.</exception>
    public static void ThrowIfNameNullOrEmpty(this ProcessingProfile processingProfile)
    {
        ArgumentNullException.ThrowIfNull(processingProfile);
        ArgumentException.ThrowIfNullOrEmpty(processingProfile.Name);
    }

    /// <summary>
    /// Returns a string representation of the processing profile.
    /// </summary>
    /// <param name="processingProfile">The processing profile to convert.</param>
    /// <returns>A string representation of the processing profile.</returns>
    public static string ToString(this ProcessingProfile processingProfile)
    {
        ArgumentNullException.ThrowIfNull(processingProfile);
        return $"ProcessingProfile {{ Id = {processingProfile.Id}, Name = {processingProfile.Name}, VideoWidth = {processingProfile.VideoWidth}, VideoHeight = {processingProfile.VideoHeight}, VideoBitrate = {processingProfile.VideoBitrate}, AudioBitrate = {processingProfile.AudioBitrate}, FrameRate = {processingProfile.FrameRate}, VideoCodec = {processingProfile.VideoCodec}, AudioCodec = {processingProfile.AudioCodec}, Container = {processingProfile.Container}, ApplyWatermark = {processingProfile.ApplyWatermark}, WatermarkPath = {processingProfile.WatermarkPath}, ApplyColorGrading = {processingProfile.ApplyColorGrading}, ColorGradingProfile = {processingProfile.ColorGradingProfile}, CompressionLevel = {processingProfile.CompressionLevel}, IsDefault = {processingProfile.IsDefault}, IsActive = {processingProfile.IsActive}, CreatedAt = {processingProfile.CreatedAt}, UpdatedAt = {processingProfile.UpdatedAt} }}";
    }

    /// <summary>
    /// Returns a string representation of the processing profile in a format suitable for display.
    /// </summary>
    /// <param name="processingProfile">The processing profile to convert.</param>
    /// <returns>A string representation of the processing profile in a format suitable for display.</returns>
    public static string ToDisplayString(this ProcessingProfile processingProfile)
    {
        ArgumentNullException.ThrowIfNull(processingProfile);
        return $"{processingProfile.Name} ({processingProfile.VideoWidth}x{processingProfile.VideoHeight}, {processingProfile.VideoBitrate} kbps, {processingProfile.FrameRate} fps)";
    }
}
