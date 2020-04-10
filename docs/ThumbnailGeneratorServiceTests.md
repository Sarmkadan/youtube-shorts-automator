# ThumbnailGeneratorServiceTests

Unit‑test class for the `ThumbnailGeneratorService` that validates dimension calculations, argument validation, file‑system interactions, and default value correctness of related DTOs.

## API

### GetDimensions_Vertical_Returns720x1280
- **Purpose**: Verifies that the service returns a 720 × 1280 pixel size when the requested orientation is vertical.
- **Parameters**: None.
- **Return value**: None (the method returns `void`; success is inferred from the absence of test‑framework assertions failures).
- **When it throws**: Throws a test‑framework exception if the actual dimensions differ from 720 × 1280.

### GetDimensions_Horizontal_Returns1280x720
- **Purpose**: Verifies that the service returns a 1280 × 720 pixel size when the requested orientation is horizontal.
- **Parameters**: None.
- **Return value**: None.
- **When it throws**: Throws a test‑framework exception if the actual dimensions differ from 1280 × 720.

### GetDimensions_Square_Returns720x720
- **Purpose**: Verifies that the service returns a 720 × 720 pixel size when the requested orientation is square.
- **Parameters**: None.
- **Return value**: None.
- **When it throws**: Throws a test‑framework exception if the actual dimensions differ from 720 × 720.

### GenerateFromVideoAsync_NullVideoPath_ThrowsArgumentException
- **Purpose**: Ensures that calling `GenerateFromVideoAsync` with a `null` video path throws an `ArgumentException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentException` (or throws a different exception type).

### GenerateFromVideoAsync_EmptyVideoPath_ThrowsArgumentException
- **Purpose**: Ensures that calling `GenerateFromVideoAsync` with an empty string video path throws an `ArgumentException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentException`.

### GenerateFromVideoAsync_NullRequest_ThrowsArgumentNullException
- **Purpose**: Ensures that calling `GenerateFromVideoAsync` with a `null` request object throws an `ArgumentNullException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentNullException`.

### GenerateFromVideoAsync_MissingVideoFile_ThrowsVideoProcessingException
- **Purpose**: Ensures that calling `GenerateFromVideoAsync` with a non‑existent video file throws a `VideoProcessingException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw a `VideoProcessingException`.

### GenerateFromVideoAsync_EmptyOutputDirectory_ThrowsValidationException
- **Purpose**: Ensures that calling `GenerateFromVideoAsync` with an empty output directory path throws a `ValidationException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw a `ValidationException`.

### GenerateWithTextOverlayAsync_NullImagePath_ThrowsArgumentException
- **Purpose**: Ensures that calling `GenerateWithTextOverlayAsync` with a `null` image path throws an `ArgumentException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentException`.

### GenerateWithTextOverlayAsync_EmptyText_ThrowsArgumentException
- **Purpose**: Ensures that calling `GenerateWithTextOverlayAsync` with an empty text string throws an `ArgumentException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentException`.

### GenerateWithTextOverlayAsync_MissingImageFile_ThrowsVideoProcessingException
- **Purpose**: Ensures that calling `GenerateWithTextOverlayAsync` with a non‑existent image file throws a `VideoProcessingException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw a `VideoProcessingException`.

### GenerateBatchAsync_ZeroFrameCount_ThrowsArgumentOutOfRangeException
- **Purpose**: Ensures that calling `GenerateBatchAsync` with a frame count of zero throws an `ArgumentOutOfRangeException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentOutOfRangeException`.

### GenerateBatchAsync_NegativeVideoDuration_ThrowsArgumentOutOfRangeException
- **Purpose**: Ensures that calling `GenerateBatchAsync` with a negative video duration throws an `ArgumentOutOfRangeException`.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the method does not throw an `ArgumentOutOfRangeException`.

### ThumbnailGenerationRequest_DefaultValues_AreCorrect
- **Purpose**: Validates that a newly instantiated `ThumbnailGenerationRequest` has the expected default property values.
- **Parameters**: None.
- **Return value**: None.
- **When it throws**: Throws a test‑framework exception if any default value deviates from the expectation.

### TextOverlayOptions_DefaultValues_AreCorrect
- **Purpose**: Validates that a newly instantiated `TextOverlayOptions` has the expected default property values.
- **Parameters**: None.
- **Return value**: None.
- **When it throws**: Throws a test‑framework exception if any default value deviates from the expectation.

### ThumbnailGenerationResult_SuccessFalseByDefault
- **Purpose**: Validates that the `Success` property of a newly instantiated `ThumbnailGenerationResult` is `false` by default.
- **Parameters**: None.
- **Return value**: None.
- **When it throws**: Throws a test‑framework exception if the `Success` property is not `false`.

### GenerateFromVideoAsync_CreatesOutputDirectoryIfMissing
- **Purpose**: Ensures that when the output directory does not exist, `GenerateFromVideoAsync` creates it before writing the thumbnail.
- **Parameters**: None.
- **Return value**: Returns a `Task` that completes when the assertion finishes.
- **When it throws**: Throws a test‑framework exception if the output directory is not created after the method call.

## Usage

```csharp
using System.Threading.Tasks;
using Xunit; // or NUnit/MSTest attribute as appropriate

public class ExampleTests
{
    [Fact]
    public async Task VerifyNullVideoPathThrows()
    {
        var test = new ThumbnailGeneratorServiceTests();
        await test.GenerateFromVideoAsync_NullVideoPath_ThrowsArgumentException();
    }
}
```

```csharp
using System.Threading.Tasks;
using Xunit;

public class ThumbnailGeneratorServiceTestsDemo
{
    [Fact]
    public async Task VerifyOutputDirectoryCreation()
    {
        var test = new ThumbnailGeneratorServiceTests();
        await test.GenerateFromVideoAsync_CreatesOutputDirectoryIfMissing();
    }
}
```

## Notes

- All test methods are stateless; they do not rely on or modify shared fields, making them safe to execute in parallel test runs.
- The methods that validate argument checks (`*_ThrowsArgumentException`, `*_ThrowsArgumentNullException`, `*_ThrowsArgumentOutOfRangeException`) assume the underlying service throws the exact exception type specified; deriving exception types will cause the test to fail.
- File‑system‑related tests (`*_MissingVideoFile_ThrowsVideoProcessingException`, `*_MissingImageFile_ThrowsVideoProcessingException`, `*_CreatesOutputDirectoryIfMissing`) depend on the test environment’s ability to create temporary paths; ensure any cleanup logic removes created directories after the test completes to avoid side effects.
- Default‑value tests (`*_DefaultValues_AreCorrect`, `*_SuccessFalseByDefault`) are sensitive to changes in the DTO constructors; if defaults are altered, these tests must be updated accordingly.
