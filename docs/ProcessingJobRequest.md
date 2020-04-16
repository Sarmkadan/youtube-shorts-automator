# ProcessingJobRequest

The `ProcessingJobRequest` class encapsulates the necessary parameters and metadata required to initiate a video processing task within the `youtube-shorts-automator` system. It functions as a data transfer object, providing the processing pipeline with the source file location, desired output specifications, and administrative context required to execute automated video conversion, optimization, and metadata assignment workflows.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `RequestId` | `Guid` | Unique identifier assigned to the specific job request. |
| `VideoFilePath` | `string` | Absolute file system path to the source video file. |
| `Title` | `string` | The target title for the processed video. |
| `Description` | `string` | The target description for the processed video. |
| `Tags` | `string[]` | An array of metadata tags to be associated with the video. |
| `ProcessingProfile` | `string` | Identifier for the predefined processing configuration profile to be applied. |
| `Options` | `ProcessingOptions` | An object containing specific technical transformation settings. |
| `CreatedAtUtc` | `DateTime` | The UTC timestamp marking when the request was created. |
| `RequestedBy` | `string` | Identifier of the user or system that initiated the request. |
| `EnableWatermark` | `bool` | Specifies whether a watermark should be applied to the output video. |
| `WatermarkImagePath` | `string?` | Optional file path to the image asset used for watermarking. |
| `AutoGenerateThumbnail` | `bool` | Indicates whether the system should automatically generate a thumbnail. |
| `OptimizeForMobile` | `bool` | If true, applies specific optimizations for mobile device playback. |
| `MaxWidth` | `int` | The maximum allowed pixel width for the output video. |
| `MaxHeight` | `int` | The maximum allowed pixel height for the output video. |
| `BitrateKbps` | `int` | The target video bitrate in kilobits per second. |
| `EnableAudioNormalization` | `bool` | If true, enables automated audio level normalization. |
| `VideoId` | `Guid` | The unique identifier associated with the video asset. |

## Usage

### Instantiation
```csharp
var request = new ProcessingJobRequest
{
    RequestId = Guid.NewGuid(),
    VideoId = Guid.NewGuid(),
    VideoFilePath = "/mnt/data/raw/input_video.mp4",
    Title = "Project Highlights",
    Description = "A brief overview of our latest project milestones.",
    Tags = new[] { "milestones", "demo", "tech" },
    ProcessingProfile = "high-quality-short",
    CreatedAtUtc = DateTime.UtcNow,
    RequestedBy = "system-user",
    EnableWatermark = true,
    WatermarkImagePath = "/assets/branding/logo.png",
    AutoGenerateThumbnail = true,
    OptimizeForMobile = true,
    MaxWidth = 1080,
    MaxHeight = 1920,
    BitrateKbps = 5000,
    EnableAudioNormalization = true
};
```

### Passing to a Processor Service
```csharp
public async Task SubmitJob(ProcessingJobRequest request, IProcessingService service)
{
    // Validate request before submission
    if (string.IsNullOrEmpty(request.VideoFilePath))
    {
        throw new ArgumentException("VideoFilePath cannot be null or empty.");
    }

    await service.QueueJobAsync(request);
}
```

## Notes

*   **Thread Safety**: This class is a Plain Old CLR Object (POCO) designed primarily for data transfer. It is not inherently thread-safe. Concurrent modifications to the properties of a `ProcessingJobRequest` instance from multiple threads are not supported and may lead to inconsistent state.
*   **Validation**: While the class structures the data, it does not perform automated validation of paths or configurations upon instantiation. It is expected that consumer services validate the existence of `VideoFilePath` and, if `EnableWatermark` is set to true, the existence and accessibility of `WatermarkImagePath`.
*   **Nullability**: `WatermarkImagePath` is nullable and should only be accessed if `EnableWatermark` is `true`.
