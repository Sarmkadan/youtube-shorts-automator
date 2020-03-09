// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for TextOverlayOptions to provide common text overlay operations
// =============================================================================

namespace YouTubeShortAutomator.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="TextOverlayOptions"/> to simplify common text overlay operations.
/// </summary>
public static class TextOverlayOptionsExtensions
{
    /// <summary>
    /// Sets the font size for the text overlay.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="fontSize">The font size in pixels.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithFontSize(this TextOverlayOptions options, int fontSize)
    {
        options.FontSize = fontSize;
        return options;
    }

    /// <summary>
    /// Sets the font color for the text overlay.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="color">The color string (e.g., "white", "#FFFFFF", "yellow").</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithFontColor(this TextOverlayOptions options, string color)
    {
        options.FontColor = color;
        return options;
    }

    /// <summary>
    /// Sets whether to show the background box for the text overlay.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="showBox"><c>true</c> to show the box, <c>false</c> to hide it.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithBox(this TextOverlayOptions options, bool showBox)
    {
        options.ShowBox = showBox;
        return options;
    }

    /// <summary>
    /// Sets the background box color for the text overlay.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="boxColor">The box color string with optional alpha (e.g., "black@0.5", "#00000080").</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithBoxColor(this TextOverlayOptions options, string boxColor)
    {
        options.BoxColor = boxColor;
        return options;
    }

    /// <summary>
    /// Sets the padding for the text box.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="padding">The padding in pixels.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithBoxPadding(this TextOverlayOptions options, int padding)
    {
        options.BoxPadding = padding;
        return options;
    }

    /// <summary>
    /// Sets the position for the text overlay.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="position">The text position.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithPosition(this TextOverlayOptions options, TextPosition position)
    {
        options.Position = position;
        return options;
    }

    /// <summary>
    /// Sets the maximum character width before wrapping the text.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="wrapWidth">The wrap width in characters, or 0 to disable wrapping.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithWrapWidth(this TextOverlayOptions options, int wrapWidth)
    {
        options.WrapWidth = wrapWidth;
        return options;
    }

    /// <summary>
    /// Sets all text overlay appearance properties at once.
    /// </summary>
    /// <param name="options">The text overlay options to modify.</param>
    /// <param name="fontSize">The font size in pixels.</param>
    /// <param name="fontColor">The font color string.</param>
    /// <param name="showBox"><c>true</c> to show the box, <c>false</c> to hide it.</param>
    /// <param name="boxColor">The box color string with optional alpha.</param>
    /// <param name="boxPadding">The padding in pixels.</param>
    /// <param name="position">The text position.</param>
    /// <param name="wrapWidth">The wrap width in characters, or 0 to disable wrapping.</param>
    /// <returns>The modified <see cref="TextOverlayOptions"/> for method chaining.</returns>
    public static TextOverlayOptions WithAppearance(
        this TextOverlayOptions options,
        int fontSize = 48,
        string fontColor = "white",
        bool showBox = true,
        string boxColor = "black@0.5",
        int boxPadding = 10,
        TextPosition position = TextPosition.BottomCenter,
        int wrapWidth = 40)
    {
        options.FontSize = fontSize;
        options.FontColor = fontColor;
        options.ShowBox = showBox;
        options.BoxColor = boxColor;
        options.BoxPadding = boxPadding;
        options.Position = position;
        options.WrapWidth = wrapWidth;
        return options;
    }

    /// <summary>
    /// Creates a deep copy of the text overlay options.
    /// </summary>
    /// <param name="options">The text overlay options to copy.</param>
    /// <returns>A new instance with the same property values.</returns>
    public static TextOverlayOptions Clone(this TextOverlayOptions options)
    {
        return new TextOverlayOptions
        {
            FontSize = options.FontSize,
            FontColor = options.FontColor,
            ShowBox = options.ShowBox,
            BoxColor = options.BoxColor,
            BoxPadding = options.BoxPadding,
            Position = options.Position,
            WrapWidth = options.WrapWidth
        };
    }

    /// <summary>
    /// Creates a new TextOverlayOptions with default values.
    /// </summary>
    /// <returns>A new instance of TextOverlayOptions with default settings.</returns>
    public static TextOverlayOptions CreateDefault()
    {
        return new TextOverlayOptions();
    }

    /// <summary>
    /// Creates a new TextOverlayOptions configured for a title overlay.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <returns>A new TextOverlayOptions instance configured for title display.</returns>
    public static TextOverlayOptions ForTitle(string title)
    {
        return new TextOverlayOptions
        {
            FontSize = 64,
            FontColor = "white",
            ShowBox = true,
            BoxColor = "black@0.7",
            BoxPadding = 15,
            Position = TextPosition.BottomCenter,
            WrapWidth = 30
        };
    }

    /// <summary>
    /// Creates a new TextOverlayOptions configured for a subtitle overlay.
    /// </summary>
    /// <param name="subtitle">The subtitle text.</param>
    /// <returns>A new TextOverlayOptions instance configured for subtitle display.</returns>
    public static TextOverlayOptions ForSubtitle(string subtitle)
    {
        return new TextOverlayOptions
        {
            FontSize = 42,
            FontColor = "#FFFF00",
            ShowBox = true,
            BoxColor = "black@0.5",
            BoxPadding = 12,
            Position = TextPosition.BottomCenter,
            WrapWidth = 45
        };
    }

    /// <summary>
    /// Creates a new TextOverlayOptions configured for a channel watermark.
    /// </summary>
    /// <param name="watermark">The watermark text.</param>
    /// <returns>A new TextOverlayOptions instance configured for watermark display.</returns>
    public static TextOverlayOptions ForWatermark(string watermark)
    {
        return new TextOverlayOptions
        {
            FontSize = 36,
            FontColor = "white",
            ShowBox = false,
            Position = TextPosition.TopRight,
            WrapWidth = 0
        };
    }

    private static string EscapeFFmpegText(string text)
    {
        // Escape single quotes and backslashes for FFmpeg
        return text.Replace("'", "'\\''").Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    private static string GetFFmpegPosition(TextPosition position)
    {
        return position switch
        {
            TextPosition.TopLeft => "x=w-tw:y=th",
            TextPosition.TopCenter => "x=(w-tw)/2:y=th",
            TextPosition.TopRight => "x=w-tw:y=th",
            TextPosition.MiddleLeft => "x=w-tw:y=(h-th)/2",
            TextPosition.Center => "x=(w-tw)/2:y=(h-th)/2",
            TextPosition.MiddleRight => "x=w-tw:y=(h-th)/2",
            TextPosition.BottomLeft => "x=w-tw:y=h-(2*th)",
            TextPosition.BottomCenter => "x=(w-tw)/2:y=h-(2*th)",
            TextPosition.BottomRight => "x=w-tw:y=h-(2*th)",
            _ => "x=(w-tw)/2:y=h-(2*th)" // Default to BottomCenter
        };
    }
}