# TextOverlayOptions

`TextOverlayOptions` is a configuration class used to define visual and behavioral properties for rendering text overlays on video thumbnails or frames within the `youtube-shorts-automator` project. It controls typography, positioning, bounding boxes, output formatting, and metadata related to the generated output.

## API

### `public int FontSize`
Specifies the font size in pixels for the overlay text. Default values are context-dependent but typically range between 12 and 48. Values below 6 or above 256 may result in rendering artifacts or silent failures during image generation.

### `public string FontColor`
Defines the color of the overlay text in hexadecimal format (e.g., `"#FFFFFF"`). Supports 3-, 6-, and 8-digit hex codes (with optional alpha channel). Invalid formats may throw `ArgumentException` during rendering.

### `public bool ShowBox`
Indicates whether a background box should be drawn behind the overlay text. When `true`, `BoxColor` and `BoxPadding` properties are respected. If `false`, these properties are ignored.

### `public string BoxColor`
Specifies the color of the background box in hexadecimal format (e.g., `"#00000080"`). Only relevant when `ShowBox` is `true`. Invalid formats may throw `ArgumentException`.

### `public int BoxPadding`
Sets the padding in pixels around the overlay text within the background box. Only applicable when `ShowBox` is `true`. Values below 0 are clamped to 0 during rendering.

### `public TextPosition Position`
Determines the anchor point for the overlay text on the image. Valid values include `TopLeft`, `TopCenter`, `TopRight`, `CenterLeft`, `Center`, `CenterRight`, `BottomLeft`, `BottomCenter`, and `BottomRight`. Misconfigured positions may cause text to render outside the image bounds.

### `public int WrapWidth`
Defines the maximum width in pixels before the overlay text wraps to a new line. If set to 0 or a value exceeding the image width, text will not wrap. Negative values are treated as 0.

### `public TimeSpan CaptureTimestamp`
Specifies the timestamp within the video from which the frame or thumbnail is captured. Used for synchronization in automated workflows. Negative values or values exceeding the video duration may result in errors.

### `public string OutputDirectory`
The filesystem path where the generated output file will be saved. Must be an absolute path. If the directory does not exist, it may be created during execution. Invalid paths may throw `DirectoryNotFoundException` or `UnauthorizedAccessException`.

### `public string? OverlayText`
The text content to render as an overlay. If `null` or empty, no text is rendered, though other properties (e.g., `ShowBox`) may still affect the output. Long text without proper `WrapWidth` may overflow image bounds.

### `public TextOverlayOptions TextOverlay`
A nested `TextOverlayOptions` instance for advanced use cases, allowing layered or recursive text overlays. If `null`, this property is ignored. Circular references may cause stack overflows during rendering.

### `public ThumbnailOutputFormat Format`
Specifies the output file format for the generated thumbnail. Valid values include `Jpeg`, `Png`, `Webp`, and `Bmp`. Unsupported formats may throw `NotSupportedException`.

### `public ThumbnailAspectRatio AspectRatio`
Defines the aspect ratio of the output image. Valid values include `Original`, `Square`, `Widescreen`, and `Portrait`. Mismatched aspect ratios may result in cropped or distorted images.

### `public int Quality`
Sets the compression quality for lossy formats (e.g., `Jpeg`, `Webp`). Valid range is 1–100, where 100 represents the highest quality. Values outside this range may throw `ArgumentOutOfRangeException`.

### `public string OutputPath`
The full filesystem path for the generated output file, including filename and extension. Takes precedence over `OutputDirectory` if both are specified. Invalid paths may throw `IOException` or `UnauthorizedAccessException`.

### `public bool Success`
Indicates whether the last operation using this configuration succeeded. Set automatically by the rendering pipeline. Manual modification may lead to inconsistent state.

### `public string? ErrorMessage`
Contains a descriptive error message if `Success` is `false`. `null` if no error occurred or if the operation has not been executed. Read-only after execution.

### `public long FileSizeBytes`
The size of the generated output file in bytes. Populated after successful execution. Zero if the operation failed or has not been executed.

### `public int Width`
The width of the generated output image in pixels. Set during rendering based on `AspectRatio` or other constraints. Zero if the operation failed or has not been executed.

### `public int Height`
The height of the generated output image in pixels. Set during rendering based on `AspectRatio` or other constraints. Zero if the operation failed or has not been executed.

## Usage

### Example 1: Basic Text Overlay on Thumbnail
