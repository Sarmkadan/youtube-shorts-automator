# IFFmpegWrapper

A lightweight wrapper around FFmpeg operations used by the youtube-shorts-automator project to perform common media processing tasks such as format conversion, metadata extraction, and thumbnail generation.

## API

### `FFmpegWrapper`

The concrete implementation of `IFFmpegWrapper`. This class is instantiated with a file path and exposes properties and methods for inspecting and transforming the associated media file.

### `public async Task<bool> ConvertVideoAsync()`

Converts the loaded media file to a target format using FFmpeg.
- **Parameters**: None.
- **Return value**: `true` if conversion succeeded; otherwise `false`.
- **Exceptions**: Throws `InvalidOperationException` if the file is not loaded or FFmpeg fails to start. Throws `IOException` if the output file cannot be written.

### `public async Task<VideoMetadata?> GetVideoMetadataAsync()`

Extracts technical metadata from the loaded media file.
- **Parameters**: None.
- **Return value**: A `VideoMetadata` object containing width, height, duration, frame rate, codec, and file size; or `null` if extraction fails.
- **Exceptions**: Throws `InvalidOperationException` if the file is not loaded. Throws `IOException` if FFmpeg fails to run or the output cannot be parsed.

### `public async Task<bool> ExtractThumbnailAsync()`

Generates a thumbnail image from the first frame of the loaded media file.
- **Parameters**: None.
- **Return value**: `true` if the thumbnail was created; otherwise `false`.
- **Exceptions**: Throws `InvalidOperationException` if the file is not loaded. Throws `IOException` if FFmpeg fails to run or the output file cannot be written.

### `public string FilePath`

Gets the absolute path to the media file being processed.
- **Type**: `string`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `null` if the file has not been set.

### `public long FileSize`

Gets the size of the media file in bytes.
- **Type**: `long`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `0` if the file has not been set or the size cannot be determined.

### `public int Width`

Gets the pixel width of the video stream.
- **Type**: `int`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `0` if the metadata has not been loaded.

### `public int Height`

Gets the pixel height of the video stream.
- **Type**: `int`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `0` if the metadata has not been loaded.

### `public double DurationSeconds`

Gets the total duration of the media in seconds.
- **Type**: `double`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `0.0` if the metadata has not been loaded.

### `public double FrameRate`

Gets the average frame rate of the video stream.
- **Type**: `double`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `0.0` if the metadata has not been loaded.

### `public string Codec`

Gets the name of the primary video codec.
- **Type**: `string`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `null` if the metadata has not been loaded.

### `public DateTime ExtractedAtUtc`

Gets the UTC timestamp when the metadata was last extracted.
- **Type**: `DateTime`
- **Access**: Read-only.
- **Exceptions**: Never throws; returns `DateTime.MinValue` if the metadata has not been loaded.

## Usage
