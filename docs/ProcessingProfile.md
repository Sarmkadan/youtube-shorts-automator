# ProcessingProfile

`ProcessingProfile` encapsulates the encoding and post‑processing settings used by the YouTube Shorts Automator when generating short‑form video assets. Each instance defines a complete set of parameters—resolution, bitrates, codecs, container format, and optional processing steps such as watermarking or color grading—that can be persisted, selected as the default profile, or activated for a particular job.

## API

| Member | Type | Purpose | Parameters | Return Value | Exceptions |
|--------|------|---------|------------|--------------|------------|
| `Id` | `int` | Unique identifier for the profile within the data store. | None | The identifier value. | None |
| `Name` | `string` | Human‑readable name of the profile (e.g., “High Quality 1080p”). | None | The name string. | None |
| `Description` | `string` | Optional detailed description of the profile’s intended use. | None | The description string. | None |
| `VideoWidth` | `int` | Width of the output video in pixels. | None | Width value. | None |
| `VideoHeight` | `int` | Height of the output video in pixels. | None | Height value. | None |
| `VideoBitrate` | `int` | Target video bitrate in kilobits per second (kbps). | None | Bitrate value. | None |
| `AudioBitrate` | `int` | Target audio bitrate in kilobits per second (kbps). | None | Bitrate value. | None |
| `FrameRate` | `int` | Frames per second of the output video. | None | Frame rate value. | None |
| `VideoCodec` | `string` | Video codec identifier (e.g., “libx264”, “hevc”). | None | Codec name string. | None |
| `AudioCodec` | `string` | Audio codec identifier (e.g., “aac”, “mp3”). | None | Codec name string. | None |
| `Container` | `string` | Container format for the encoded file (e.g., “mp4”, “mkv”). | None | Container string. | None |
| `ApplyWatermark` | `bool` | Indicates whether a watermark image should be overlaid on the video. | None | `true` if watermarking is enabled; otherwise `false`. | None |
| `WatermarkPath` | `string?` | File system path to the watermark image when `ApplyWatermark` is `true`. Can be `null` if no watermark is configured. | None | Path string or `null`. | None |
| `ApplyColorGrading` | `bool` | Indicates whether a color grading lookup table (LUT) or profile should be applied. | None | `true` if color grading is enabled; otherwise `false`. | None |
| `ColorGradingProfile` | `string?` | Path or identifier of the color grading profile to use when `ApplyColorGrading` is `true`. Can be `null`. | None | Profile string or `null`. | None |
| `CompressionLevel` | `int` | Encoder‑specific compression level (meaning depends on the selected codec). Higher values generally mean slower encoding with better compression. | None | Integer level. | None |
| `IsDefault` | `bool` | Flags the profile as the default choice when no explicit profile is selected for a job. | None | `true` if this profile is the default; otherwise `false`. | None |
| `IsActive` | `bool` | Indicates whether the profile is currently active and available for use. Inactive profiles are typically hidden from selection lists. | None | `true` if active; otherwise `false`. | None |
| `CreatedAt` | `DateTime` | Timestamp when the profile record was first created. | None | UTC creation date/time. | None |
| `UpdatedAt` | `DateTime` | Timestamp when the profile record was last modified. | None | UTC last‑update date/time. | None |

All members are simple auto‑implemented properties with public getters and setters; they perform no validation or side effects, thus they do not throw exceptions under normal usage.

## Usage

### Example 1: Creating and configuring a new profile

```csharp
var profile = new ProcessingProfile
{
    Name          = "Shorts 1080p High Quality",
    Description   = "Default profile for YouTube Shorts with H.264 video and AAC audio.",
    VideoWidth    = 1080,
    VideoHeight   = 1920,
    VideoBitrate  = 8000,   // 8 Mbps
    AudioBitrate  = 192,    // 192 kbps
    FrameRate     = 30,
    VideoCodec    = "libx264",
    AudioCodec    = "aac",
    Container     = "mp4",
    ApplyWatermark= true,
    WatermarkPath = @"C:\Assets\watermark.png",
    ApplyColorGrading = false,
    ColorGradingProfile = null,
    CompressionLevel = 4,
    IsDefault = true,
    IsActive  = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Persist the profile using your data‑access layer (pseudo‑code)
// repository.Add(profile);
```

### Example 2: Retrieving the active default profile and applying it to a job

```csharp
// Assume a service that returns the profile marked as default and active
ProcessingProfile? defaultProfile = profileRepository
    .GetAll()
    .FirstOrDefault(p => p.IsDefault && p.IsActive);

if (defaultProfile == null)
{
    throw new InvalidOperationException("No active default processing profile found.");
}

// Use the profile's settings when invoking the encoder
var encoderSettings = new EncoderSettings
{
    Width         = defaultProfile.VideoWidth,
    Height        = defaultProfile.VideoHeight,
    VideoBitrate  = defaultProfile.VideoBitrate * 1000, // convert kbps to bps
    AudioBitrate  = defaultProfile.AudioBitrate * 1000,
    FrameRate     = defaultProfile.FrameRate,
    VideoCodec    = defaultProfile.VideoCodec,
    AudioCodec    = defaultProfile.AudioCodec,
    Container     = defaultProfile.Container,
    WatermarkPath = defaultProfile.ApplyWatermark ? defaultProfile.WatermarkPath : null,
    ColorGradingPath = defaultProfile.ApplyColorGrading ? defaultProfile.ColorGradingProfile : null,
    CompressionLevel = defaultProfile.CompressionLevel
};

encoder.Encode(sourceVideo, outputPath, encoderSettings);
```

## Notes

- **Validation**: The type does not enforce constraints such as non‑negative dimensions or bitrates. Consumers should validate values before passing them to external tools (e.g., FFmpeg) to avoid runtime errors.
- **Nullability**: `WatermarkPath` and `ColorGradingProfile` are nullable strings. When the corresponding `Apply*` flag is `false`, these properties should be set to `null`; when the flag is `true`, a non‑null path is expected, though the type itself does not enforce this.
- **Thread safety**: The properties are simple mutable fields. Concurrent reads are safe, but concurrent writes from multiple threads without external synchronization can lead to race conditions and inconsistent state. If the instance is shared across threads, protect writes with a lock or use immutable copies.
- **DateTime values**: `CreatedAt` and `UpdatedAt` are expected to be stored in UTC. When displaying to users, convert to the appropriate local time zone.
- **Default profile**: Only one profile should have `IsDefault` set to `true` at any given time; enforcing this uniqueness is the responsibility of the data‑access layer or business logic.
- **Active flag**: Setting `IsActive` to `false` does not delete the profile; it merely excludes it from selection queries. Reactivating a profile requires setting `IsActive` back to `true`.
