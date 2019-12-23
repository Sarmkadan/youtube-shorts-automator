// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

public class ProcessingProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int VideoWidth { get; set; }
    public int VideoHeight { get; set; }
    public int VideoBitrate { get; set; }
    public int AudioBitrate { get; set; }
    public int FrameRate { get; set; }
    public string VideoCodec { get; set; } = "h264";
    public string AudioCodec { get; set; } = "aac";
    public string Container { get; set; } = "mp4";
    public bool ApplyWatermark { get; set; }
    public string? WatermarkPath { get; set; }
    public bool ApplyColorGrading { get; set; }
    public string? ColorGradingProfile { get; set; }
    public int CompressionLevel { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<VideoShort> VideoShorts { get; set; } = new List<VideoShort>();

    public bool IsValid()
    {
        // Validates the processing profile
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100)
            return false;
        if (VideoWidth < 360 || VideoWidth > 1920)
            return false;
        if (VideoHeight < 640 || VideoHeight > 1080)
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

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

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
