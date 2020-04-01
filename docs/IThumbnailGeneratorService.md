# IThumbnailGeneratorService

Service responsible for generating thumbnails from video content, either by extracting frames or overlaying custom text. Handles batch processing and exposes configurable output dimensions.

## API

### `ThumbnailGeneratorService`

Constructor for the thumbnail generator service. Initializes the service with default or injected dependencies required for thumbnail generation.

### `async Task<ThumbnailGenerationResult> GenerateFromVideoAsync`

Generates a thumbnail by extracting a representative frame from the specified video file.

- **Parameters**
  - `videoPath` (string): Absolute or relative path to the source video file.
- **Return value**
  - `Task<ThumbnailGenerationResult>`: A task that resolves to a `ThumbnailGenerationResult` containing the generated thumbnail image data and metadata.
- **Exceptions**
  - Throws `FileNotFoundException` if the video file does not exist.
  - Throws `InvalidOperationException` if the video file is corrupted or unsupported.
  - Throws `IOException` on file access or I/O errors.

### `async Task<ThumbnailGenerationResult> GenerateWithTextOverlayAsync`

Generates a thumbnail by overlaying the specified text onto a frame extracted from the video.

- **Parameters**
  - `videoPath` (string): Absolute or relative path to the source video file.
  - `text` (string): Text to overlay on the thumbnail.
  - `fontSize` (int, optional): Font size for the overlaid text. Defaults to 48.
  - `fontColor` (string, optional): Color of the text in hex format (e.g., `"#FFFFFF"`). Defaults to white.
- **Return value**
  - `Task<ThumbnailGenerationResult>`: A task that resolves to a `ThumbnailGenerationResult` containing the generated thumbnail with text overlay.
- **Exceptions**
  - Throws `ArgumentException` if `text` is null or whitespace.
  - Throws `FileNotFoundException` if the video file does not exist.
  - Throws `InvalidOperationException` if the video file is corrupted or unsupported.
  - Throws `IOException` on file access or I/O errors.

### `async Task<IEnumerable<ThumbnailGenerationResult>> GenerateBatchAsync`

Generates thumbnails in batch for multiple video files.

- **Parameters**
  - `videoPaths` (IEnumerable<string>): Collection of absolute or relative paths to source video files.
- **Return value**
  - `Task<IEnumerable<ThumbnailGenerationResult>>`: A task that resolves to a collection of `ThumbnailGenerationResult` objects corresponding to each input video.
- **Exceptions**
  - Throws `ArgumentNullException` if `videoPaths` is null.
  - Throws `AggregateException` containing individual exceptions for each failed video file.
  - Throws `InvalidOperationException` if any video file is corrupted or unsupported.
  - Throws `IOException` on file access or I/O errors.

### `(int Width, int Height) GetDimensions`

Returns the configured output dimensions for generated thumbnails.

- **Return value**
  - `(int Width, int Height)`: A tuple representing the width and height in pixels of generated thumbnails.
- **Remarks**
  - Dimensions are fixed and set during service initialization.

## Usage

### Generate a single thumbnail from a video
