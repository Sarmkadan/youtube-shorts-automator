# VideoProcessingServiceTests

Unit test suite for the `VideoProcessingService` class, verifying video file validation, task creation, and processing behavior. Tests cover file size and duration constraints, repository interactions, status transitions, and FFmpeg configuration.

## API

### `VideoProcessingServiceTests`

Public test class containing methods to validate and exercise video processing functionality.

### `async Task ValidateVideoFileAsync_WithValidFile_ReturnsTrue`

Validates that a video file meeting all constraints (size, format, duration) is accepted. No parameters. Returns `true` when validation passes. Does not throw exceptions.

### `async Task ValidateVideoFileAsync_WithNonExistentFile_ReturnsFalse`

Validates that a non-existent file path is rejected. No parameters. Returns `false` when the file does not exist. Does not throw exceptions.

### `async Task ValidateVideoFileAsync_WithFileTooLarge_ReturnsFalse`

Validates that a file exceeding the maximum allowed size is rejected. No parameters. Returns `false` when the file size exceeds the configured limit. Does not throw exceptions.

### `async Task ValidateVideoFileAsync_WithFileTooSmall_ReturnsFalse`

Validates that a file below the minimum allowed size is rejected. No parameters. Returns `false` when the file size is below the configured limit. Does not throw exceptions.

### `async Task CreateProcessingTaskAsync_WithValidVideo_CreatesTask`

Verifies that a valid video file results in a new processing task being created in the repository. No parameters. Returns `void`. Throws no exceptions on success.

### `async Task CreateProcessingTaskAsync_WithInvalidVideo_ThrowsValidationException`

Verifies that an invalid video file causes a `ValidationException` to be thrown. No parameters. Throws `ValidationException` when the video fails validation. Does not return a value.

### `async Task CreateProcessingTaskAsync_WithDurationTooLong_ThrowsValidationException`

Verifies that a video exceeding the maximum allowed duration causes a `ValidationException` to be thrown. No parameters. Throws `ValidationException` when the video duration exceeds the configured limit. Does not return a value.

### `async Task CreateProcessingTaskAsync_WithRepositoryException_ThrowsVideoProcessingException`

Verifies that a repository failure during task creation results in a `VideoProcessingException` being thrown. No parameters. Throws `VideoProcessingException` when the repository throws during persistence. Does not return a value.

### `async Task CreateProcessingTaskAsync_SetsStatusToPending`

Verifies that a newly created processing task is assigned the `Pending` status. No parameters. Returns `void`. Throws no exceptions on success.

### `async Task CreateProcessingTaskAsync_SetsCreatedAtToCurrentTime`

Verifies that the `CreatedAt` timestamp of a new processing task reflects the current system time. No parameters. Returns `void`. Throws no exceptions on success.

### `async Task ProcessVideoAsync_WithValidInputs_ReturnsProcessingTask`

Verifies that processing a valid video returns the created `ProcessingTask`. No parameters. Returns the `ProcessingTask` instance representing the ongoing processing job. Does not throw exceptions.

### `async Task ProcessVideoAsync_WithInvalidProfile_ThrowsValidationException`

Verifies that an invalid processing profile causes a `ValidationException` to be thrown. No parameters. Throws `ValidationException` when the profile is invalid or incompatible. Does not return a value.

### `async Task ProcessVideoAsync_SetsCorrectPriority`

Verifies that the processing task is assigned the correct priority based on the profile. No parameters. Returns `void`. Throws no exceptions on success.

### `async Task ProcessVideoAsync_WithDifferentProfiles_AppliesCorrectSettings`

Verifies that different processing profiles result in the correct FFmpeg settings being applied. No parameters. Returns `void`. Throws no exceptions on success.

### `async Task ProcessVideoAsync_Sets_FFmpegTranscodeTaskType`

Verifies that the processing task is configured with the correct FFmpeg task type for transcoding. No parameters. Returns `void`. Throws no exceptions on success.

## Usage
