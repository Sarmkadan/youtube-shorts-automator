// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>Specifies the output image format for a generated thumbnail.</summary>
public enum ThumbnailOutputFormat
{
    /// <summary>JPEG — smaller file, lossy. Best for photographic frames.</summary>
    Jpeg,
    /// <summary>PNG — lossless, larger file. Best for graphics-heavy thumbnails.</summary>
    Png,
    /// <summary>WebP — efficient lossy/lossless format with modern browser support.</summary>
    WebP
}

/// <summary>Target aspect ratio and resolution for the generated thumbnail.</summary>
public enum ThumbnailAspectRatio
{
    /// <summary>16:9 landscape — 1280 × 720, suitable for standard YouTube videos.</summary>
    Horizontal,
    /// <summary>9:16 portrait — 720 × 1280, required for YouTube Shorts.</summary>
    Vertical,
    /// <summary>1:1 square — 720 × 720, ideal for social-media cross-posting.</summary>
    Square
}

/// <summary>Anchor position for the text overlay rendered on the thumbnail.</summary>
public enum TextPosition
{
    /// <summary>Top-left corner.</summary>
    TopLeft,
    /// <summary>Top-centre.</summary>
    TopCenter,
    /// <summary>Top-right corner.</summary>
    TopRight,
    /// <summary>Vertically centred, left-aligned.</summary>
    MiddleLeft,
    /// <summary>Exact centre of the image.</summary>
    Center,
    /// <summary>Vertically centred, right-aligned.</summary>
    MiddleRight,
    /// <summary>Bottom-left corner.</summary>
    BottomLeft,
    /// <summary>Bottom-centre — default; highest visual impact for titles.</summary>
    BottomCenter,
    /// <summary>Bottom-right corner.</summary>
    BottomRight
}

/// <summary>
/// Configures the appearance of a text overlay drawn on top of a thumbnail image.
/// All values are consumed by FFmpeg's <c>drawtext</c> filter.
/// </summary>
public sealed class TextOverlayOptions
{
    /// <summary>Font size in pixels. Defaults to <c>48</c>.</summary>
    public int FontSize { get; set; } = 48;

    /// <summary>
    /// FFmpeg-compatible colour string for the text, e.g. <c>white</c>, <c>#FFFFFF</c>.
    /// Defaults to <c>white</c>.
    /// </summary>
    public string FontColor { get; set; } = "white";

    /// <summary>
    /// Whether to draw a semi-transparent box behind the text for readability.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ShowBox { get; set; } = true;

    /// <summary>
    /// FFmpeg-compatible colour string for the background box, including optional alpha.
    /// Defaults to <c>black@0.5</c>.
    /// </summary>
    public string BoxColor { get; set; } = "black@0.5";

    /// <summary>Padding in pixels added around the text inside the box. Defaults to <c>10</c>.</summary>
    public int BoxPadding { get; set; } = 10;

    /// <summary>Anchor position of the text on the image. Defaults to <see cref="TextPosition.BottomCenter"/>.</summary>
    public TextPosition Position { get; set; } = TextPosition.BottomCenter;

    /// <summary>
    /// Maximum character width before the overlay text is word-wrapped.
    /// Set to <c>0</c> to disable wrapping. Defaults to <c>40</c>.
    /// </summary>
    public int WrapWidth { get; set; } = 40;
}

/// <summary>
/// Encapsulates all parameters required to generate a thumbnail from a video or image source.
/// </summary>
public sealed class ThumbnailGenerationRequest
{
    /// <summary>
    /// Video timestamp from which the frame will be captured. Defaults to 1 second
    /// to avoid black frames at the very start of the clip.
    /// </summary>
    public TimeSpan CaptureTimestamp { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Directory where the generated thumbnail file will be written.</summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Optional text to overlay on the thumbnail (e.g. the video title or channel name).
    /// When <c>null</c> or empty, no text overlay is applied.
    /// </summary>
    public string? OverlayText { get; set; }

    /// <summary>Appearance options for the text overlay.</summary>
    public TextOverlayOptions TextOverlay { get; set; } = new();

    /// <summary>Output image format. Defaults to <see cref="ThumbnailOutputFormat.Jpeg"/>.</summary>
    public ThumbnailOutputFormat Format { get; set; } = ThumbnailOutputFormat.Jpeg;

    /// <summary>
    /// Target aspect ratio and resolution. Defaults to <see cref="ThumbnailAspectRatio.Vertical"/>
    /// (9:16, 720 × 1280) — the optimal format for YouTube Shorts.
    /// </summary>
    public ThumbnailAspectRatio AspectRatio { get; set; } = ThumbnailAspectRatio.Vertical;

    /// <summary>
    /// JPEG/WebP quality (1–100). Higher values produce larger, sharper images.
    /// Ignored for <see cref="ThumbnailOutputFormat.Png"/>. Defaults to <c>85</c>.
    /// </summary>
    public int Quality { get; set; } = 85;
}

/// <summary>
/// The result returned by <see cref="IThumbnailGeneratorService"/> after a generation attempt.
/// </summary>
public sealed class ThumbnailGenerationResult
{
    /// <summary>Absolute path of the generated thumbnail file, or empty on failure.</summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary><c>true</c> when the thumbnail was created successfully.</summary>
    public bool Success { get; set; }

    /// <summary>Error description when <see cref="Success"/> is <c>false</c>.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Size of the output file in bytes, or <c>0</c> on failure.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Width of the generated image in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Height of the generated image in pixels.</summary>
    public int Height { get; set; }

    /// <summary>The video timestamp from which the frame was captured.</summary>
    public TimeSpan CaptureTimestamp { get; set; }

    /// <summary>UTC timestamp when generation was completed.</summary>
    public DateTime GeneratedAt { get; set; }
}
