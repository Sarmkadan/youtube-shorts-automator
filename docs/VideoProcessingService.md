# VideoProcessingService

The `VideoProcessingService` class provides the core video processing pipeline for the youtube-shorts-automator project. It handles file validation, asynchronous task creation, video processing (including watermarking and color grading), and thumbnail generation. All operations are asynchronous and designed to work with a queue-based processing model.

## API

### `public VideoProcessingService()`

Initializes a new instance of the `VideoProcessingService`.  
No parameters.  
No return value.  
Does not throw.

### `public async Task<bool> ValidateVideoFileAsync(string filePath)`

Validates that the specified video file exists, has a supported format, and is not corrupted.  
**Parameters:**  
- `filePath` – The full path to the video file.  
**Returns:** `true` if the file is valid; otherwise `false`.  
**Throws:**  
- `ArgumentNullException` – if `filePath` is `null` or empty.  
- `FileNotFoundException` – if the file does not exist.  
- `InvalidOperationException` – if the video codec or container is unsupported.

### `public async Task<VideoShort> CreateProcessingTaskAsync(string filePath, string outputDirectory)`

Creates a new `VideoShort` entity representing a processing job and persists it to the underlying store.  
**Parameters:**  
- `filePath` – The source video file path.  
- `outputDirectory` – The directory where processed outputs will be saved.  
**Returns:** A `VideoShort` instance with a unique identifier and initial status.  
**Throws:**  
- `ArgumentNullException` – if any parameter is `null` or empty.  
- `InvalidOperationException` – if a task for the same file already exists and is not completed.

### `public async Task<ProcessingTask> ProcessVideoAsync(VideoShort videoShort)`

Executes the full processing pipeline on the given `VideoShort`: trimming, resizing, and applying any queued effects.  
**Parameters:**  
- `videoShort` – The `VideoShort` to process (must have been created via `CreateProcessingTaskAsync`).  
**Returns:** A `ProcessingTask` containing the result status, output file paths, and any error details.  
**Throws:**  
- `ArgumentNullException` – if `videoShort` is `null`.  
- `InvalidOperationException` – if the video is already being processed or its source file is missing.

### `public async Task<bool> ApplyWatermarkAsync(string inputPath, string watermarkPath, string outputPath)`

Overlays a watermark image onto the video.  
**Parameters:**  
- `inputPath` – Path to the source video.  
- `watermarkPath` – Path to the watermark image (PNG with transparency recommended).  
- `outputPath` – Destination path for the watermarked video.  
**Returns:** `true` if the watermark was applied successfully; otherwise `false`.  
**Throws:**  
- `ArgumentNullException` – if any path is `null` or empty.  
- `FileNotFoundException` – if `inputPath` or `watermarkPath` does not exist.  
- `InvalidOperationException` – if the watermark dimensions exceed the video frame size.

### `public async Task<bool> ApplyColorGradingAsync(string inputPath, string lutPath, string outputPath)`

Applies a color grading LUT (Look-Up Table) to the video.  
**Parameters:**  
- `inputPath` – Path to the source video.  
- `lutPath` – Path to the `.cube` LUT file.  
- `outputPath` – Destination path for the color‑graded video.  
**Returns:** `true` if color grading succeeded; otherwise `false`.  
**Throws:**  
- `ArgumentNullException` – if any path is `null` or empty.  
- `FileNotFoundException` – if `inputPath` or `lutPath` does not exist.  
- `InvalidOperationException` – if the LUT format is unsupported or the video color space is incompatible.

### `public async Task<string> GenerateThumbnailAsync(string videoPath, string outputPath, TimeSpan? atTime = null)`

Extracts a single frame from the video and saves it as a JPEG thumbnail.  
**Parameters:**  
- `videoPath` – Path to the source video.  
- `outputPath` – Destination path for the thumbnail (`.jpg` extension recommended).  
- `atTime` – Optional timestamp to capture the frame; defaults to the midpoint of the video if `null`.  
**Returns:** The full path to the generated thumbnail file.  
**Throws:**  
- `ArgumentNullException` – if `videoPath` or `outputPath` is `null` or empty.  
- `FileNotFoundException` – if `videoPath` does not exist.  
- `InvalidOperationException` – if the video duration is zero or the frame cannot be extracted.

## Usage

### Example 1: Basic processing pipeline

```csharp
var service = new VideoProcessingService();
string sourceFile = @"C:\videos\input.mp4";
string outputDir = @"C:\videos\output";

// Validate the source file
bool isValid = await service.ValidateVideoFileAsync(sourceFile);
if (!isValid)
{
    Console.WriteLine("Invalid video file.");
    return;
}

// Create a processing task
VideoShort videoShort = await service.CreateProcessingTaskAsync(sourceFile, outputDir);

// Process the video (trim, resize, etc.)
ProcessingTask result = await service.ProcessVideoAsync(videoShort);
if (result.Status == ProcessingStatus.Completed)
{
    Console.WriteLine($"Processed video saved to {result.OutputPath}");
}
```

### Example 2: Applying watermark and color grading with thumbnail

```csharp
var service = new VideoProcessingService();
string input = @"C:\videos\raw.mp4";
string watermark = @"C:\assets\logo.png";
string lut = @"C:\assets\warm.cube";
string gradedOutput = @"C:\videos\graded.mp4";
string watermarkedOutput = @"C:\videos\final.mp4";
string thumbnail = @"C:\videos\thumb.jpg";

// Apply color grading first
bool gradingOk = await service.ApplyColorGradingAsync(input, lut, gradedOutput);
if (!gradingOk)
{
    Console.WriteLine("Color grading failed.");
    return;
}

// Then watermark the graded video
bool watermarkOk = await service.ApplyWatermarkAsync(gradedOutput, watermark, watermarkedOutput);
if (!watermarkOk)
{
    Console.WriteLine("Watermark application failed.");
    return;
}

// Generate a thumbnail from the final video
string thumbPath = await service.GenerateThumbnailAsync(watermarkedOutput, thumbnail, TimeSpan.FromSeconds(10));
Console.WriteLine($"Thumbnail saved to {thumbPath}");
```

## Notes

- **Thread safety:** This class is **not thread‑safe**. Concurrent calls to `ProcessVideoAsync`, `ApplyWatermarkAsync`, or `ApplyColorGradingAsync` on the same instance may lead to resource contention or corrupted output. Use separate instances or external synchronization when processing multiple videos simultaneously.
- **Dependencies:** All video operations rely on an external FFmpeg installation. Ensure FFmpeg is available in the system PATH or configured via application settings. Missing FFmpeg will cause `InvalidOperationException` at runtime.
- **Large files:** Methods that read or write large video files may consume significant memory and disk I/O. For files exceeding 4 GB, ensure the output directory has sufficient free space and that the underlying file system supports large files.
- **Unsupported formats:** `ValidateVideoFileAsync` checks for common container/codec combinations (e.g., MP4/H.264, MOV/ProRes). Unusual formats (e.g., WebM with VP9) may return `false` even if the file is technically valid.
- **Error recovery:** If `ApplyWatermarkAsync` or `ApplyColorGradingAsync` returns `false`, the output file may be incomplete or missing. Always check the return value before proceeding to the next step.
- **Thumbnail timing:** `GenerateThumbnailAsync` with a `null` `atTime` uses the video’s midpoint. For very short videos (< 1 second), the midpoint may be at the same frame as the start; consider specifying an explicit timestamp.
