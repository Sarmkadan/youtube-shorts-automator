// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Represents a processing profile for video processing tasks.
/// </summary>
public class ProcessingProfile
{
    /// <summary>
    /// Gets or sets the unique identifier of the processing profile.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the processing profile.
    /// </summary>
    /// <value>
    /// The name of the processing profile. It must not be null or empty, and its length must not exceed 100 characters.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the processing profile.
    /// </summary>
    /// <value>
    /// The description of the processing profile. It can be null or empty.
    /// </value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the video in pixels.
    /// </summary>
    /// <value>
    /// The width of the video in pixels. It must be between 360 and 1920 inclusive.
    /// </value>
    public int VideoWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the video in pixels.
    /// </summary>
    /// <value>
    /// The height of the video in pixels. It must be between 640 and 1920 inclusive.
    /// </value>
    public int VideoHeight { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the video in kbps.
    /// </summary>
    /// <value>
    /// The bitrate of the video in kbps. It must be between 500 and 20000 inclusive.
    /// </value>
    public int VideoBitrate { get; set; }

    /// <summary>
    /// Gets or sets the bitrate of the audio in kbps.
    /// </summary>
    /// <value>
    /// The bitrate of the audio in kbps. It must be between 64 and 320 inclusive.
    /// </value>
    public int AudioBitrate { get; set; }

    /// <summary>
    /// Gets or sets the frame rate of the video in frames per second.
    /// </summary>
    /// <value>
    /// The frame rate of the video in frames per second. It must be between 24 and 60 inclusive.
    /// </value>
    public int FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the codec of the video.
    /// </summary>
    /// <value>
    /// The codec of the video. It defaults to "h264".
    /// </value>
    public string VideoCodec { get; set; } = "h264";

    /// <summary>
    /// Gets or sets the codec of the audio.
    /// </summary>
    /// <value>
    /// The codec of the audio. It defaults to "aac".
    /// </value>
    public string AudioCodec { get; set; } = "aac";

    /// <summary>
    /// Gets or sets the container format of the video.
    /// </summary>
    /// <value>
    /// The container format of the video. It defaults to "mp4".
    /// </value>
    public string Container { get; set; } = "mp4";

    /// <summary>
    /// Gets or sets a value indicating whether to apply a watermark to the video.
    /// </summary>
    /// <value>
    /// true if a watermark should be applied; otherwise, false.
    /// </value>
    public bool ApplyWatermark { get; set; }

    /// <summary>
    /// Gets or sets the path to the watermark image.
    /// </summary>
    /// <value>
    /// The path to the watermark image. It can be null.
    /// </value>
    public string? WatermarkPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to apply color grading to the video.
    /// </summary>
    /// <value>
    /// true if color grading should be applied; otherwise, false.
    /// </value>
    public bool ApplyColorGrading { get; set; }

    /// <summary>
    /// Gets or sets the color grading profile to apply.
    /// </summary>
    /// <value>
    /// The color grading profile to apply. It can be null.
    /// </value>
    public string? ColorGradingProfile { get; set; }

    /// <summary>
    /// Gets or sets the compression level of the video.
    /// </summary>
    /// <value>
    /// The compression level of the video. It must be between 0 and 10 inclusive.
    /// </value>
    public int CompressionLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the default processing profile.
    /// </summary>
    /// <value>
    /// true if this is the default processing profile; otherwise, false.
    /// </value>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this processing profile is active.
    /// </summary>
    /// <value>
    /// true if this processing profile is active; otherwise, false.
    /// </value>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the processing profile was created.
    /// </summary>
    /// <value>
    /// The date and time when the processing profile was created.
    /// </value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the processing profile was last updated.
    /// </summary>
    /// <value>
    /// The date and time when the processing profile was last updated.
    /// </value>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets the collection of video shorts associated with this processing profile.
    /// </summary>
    /// <value>
    /// The collection of video shorts associated with this processing profile.
    /// </value>
    public ICollection<VideoShort> VideoShorts { get; set; } = new List<VideoShort>();

    /// <summary>
    /// Validates the processing profile.
    /// </summary>
    /// <returns>
    /// true if the processing profile is valid; otherwise, false.
    /// </returns>
    public bool IsValid()
    {
        // Validates the processing profile
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100)
            return false;
        if (VideoWidth < 360 || VideoWidth > 1920)
            return false;
        if (VideoHeight < 640 || VideoHeight > 1920)
            return false;
        if (VideoBitrate < 500 || VideoBitrate > 20000)
            return false;
        if (AudioBitrate < 64 || AudioBitrate > 320)
            return false;
        if (FrameRate < 24 || FrameRate > 60)
            return false;
        if (CompressionLevel < 0 || CompressionLevel > 10)
            return false;
        return true;
    }

    /// <summary>
    /// Sets this processing profile as the default.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates this processing profile.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this processing profile.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a copy of this processing profile for template usage.
    /// </summary>
    /// <returns>
    /// A new processing profile that is a copy of this one.
    /// </returns>
    public ProcessingProfile Clone()
    {
        // Creates a copy of the profile for template usage
        return new ProcessingProfile
        {
            Name = $"{Name}_Copy",
            Description = Description,
            VideoWidth = VideoWidth,
            VideoHeight = VideoHeight,
            VideoBitrate = VideoBitrate,
            AudioBitrate = AudioBitrate,
            FrameRate = FrameRate,
            VideoCodec = VideoCodec,
            AudioCodec = AudioCodec,
            Container = Container,
            ApplyWatermark = ApplyWatermark,
            WatermarkPath = WatermarkPath,
            ApplyColorGrading = ApplyColorGrading,
            ColorGradingProfile = ColorGradingProfile,
            CompressionLevel = CompressionLevel,
            IsDefault = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
