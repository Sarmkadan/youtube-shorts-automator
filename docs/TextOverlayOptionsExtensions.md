# TextOverlayOptionsExtensions

Provides a fluent interface for configuring and transforming `TextOverlayOptions` instances, enabling concise and readable composition of text overlay properties such as font, box, positioning, and appearance settings.

## API

### `TextOverlayOptions WithFontSize(this TextOverlayOptions options, float size)`
Sets the font size of the text overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `size`: The desired font size in points.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated font size.
- **Throws**
  Throws `ArgumentOutOfRangeException` if `size` is less than or equal to zero.

---

### `TextOverlayOptions WithFontColor(this TextOverlayOptions options, Color color)`
Sets the font color of the text overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `color`: The desired font color.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated font color.

---

### `TextOverlayOptions WithBox(this TextOverlayOptions options, bool enabled)`
Enables or disables the background box behind the text.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `enabled`: `true` to enable the box; `false` to disable it.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the box setting updated.

---

### `TextOverlayOptions WithBoxColor(this TextOverlayOptions options, Color color)`
Sets the background box color of the text overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `color`: The desired box color.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated box color.

---

### `TextOverlayOptions WithBoxPadding(this TextOverlayOptions options, float padding)`
Sets the padding around the text within the background box.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `padding`: The desired padding in pixels.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated box padding.
- **Throws**
  Throws `ArgumentOutOfRangeException` if `padding` is negative.

---
### `TextOverlayOptions WithPosition(this TextOverlayOptions options, TextPosition position)`
Sets the position of the text overlay on the frame.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `position`: The desired `TextPosition` (e.g., top-left, center).
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated position.

---
### `TextOverlayOptions WithWrapWidth(this TextOverlayOptions options, float width)`
Sets the maximum line width for text wrapping.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `width`: The desired maximum line width in pixels.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated wrap width.
- **Throws**
  Throws `ArgumentOutOfRangeException` if `width` is less than or equal to zero.

---
### `TextOverlayOptions WithAppearance(this TextOverlayOptions options, TextAppearance appearance)`
Sets the overall appearance style of the text overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to modify.
  - `appearance`: The desired `TextAppearance` (e.g., bold, italic).
- **Return Value**
  Returns a new `TextOverlayOptions` instance with the updated appearance.

---
### `TextOverlayOptions Clone(this TextOverlayOptions options)`
Creates a deep copy of the current `TextOverlayOptions` instance.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to clone.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with identical property values.

---
### `TextOverlayOptions CreateDefault()`
Creates a new `TextOverlayOptions` instance with default values.

- **Return Value**
  Returns a new `TextOverlayOptions` instance initialized with sensible defaults (e.g., font size 24, white font, no box).

---
### `TextOverlayOptions ForTitle(this TextOverlayOptions options)`
Configures the options for rendering a video title overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to configure.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with properties optimized for titles (e.g., large font, centered position).

---
### `TextOverlayOptions ForSubtitle(this TextOverlayOptions options)`
Configures the options for rendering a subtitle overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to configure.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with properties optimized for subtitles (e.g., medium font, bottom-center position, box enabled).

---
### `TextOverlayOptions ForWatermark(this TextOverlayOptions options)`
Configures the options for rendering a watermark overlay.

- **Parameters**
  - `options`: The source `TextOverlayOptions` instance to configure.
- **Return Value**
  Returns a new `TextOverlayOptions` instance with properties optimized for watermarks (e.g., small font, semi-transparent, top-right position, no box).

## Usage
