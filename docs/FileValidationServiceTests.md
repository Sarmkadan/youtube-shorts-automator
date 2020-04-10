# FileValidationServiceTests

The `FileValidationServiceTests` class serves as the comprehensive test suite for the `FileValidationService` component within the `youtube-shorts-automator` project. It validates the correctness of file existence checks, format verification, size constraints, hash generation, duration extraction, and temporary file cleanup operations. Each method within this class corresponds to a specific test case designed to ensure the service behaves correctly under valid conditions, handles edge cases gracefully, and throws appropriate exceptions when provided with invalid input.

## API

### Constructors

#### `public FileValidationServiceTests()`
Initializes a new instance of the `FileValidationServiceTests` class. This constructor sets up the necessary test context, including any required mocks or temporary directory structures needed for executing file system operations safely during testing.

### Test Methods

#### `public void ValidateVideoFile_WithValidMp4File_ReturnsTrue()`
Verifies that the validation service returns `true` when provided with a valid MP4 file path that meets all size and format criteria.
*   **Parameters**: None (uses internally generated test fixtures).
*   **Return Value**: None (asserts result internally).
*   **Exceptions**: Throws an assertion failure if the result is not `true`.

#### `public void ValidateVideoFile_WithNullPath_ThrowsArgumentException()`
Ensures that passing a `null` file path to the validation logic results in an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void ValidateVideoFile_WithEmptyPath_ThrowsArgumentException()`
Ensures that passing an empty string as a file path to the validation logic results in an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void ValidateVideoFile_WithNonExistentFile_ReturnsFalse()`
Confirms that the validation service returns `false` when the specified file path does not exist on the disk.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void ValidateVideoFile_WithFileTooLarge_ReturnsFalse()`
Validates that files exceeding the maximum allowed size limit for YouTube Shorts are rejected with a `false` return value.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void ValidateVideoFile_WithFileTooSmall_ReturnsFalse()`
Validates that files below the minimum required size threshold are rejected with a `false` return value.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse()`
Checks that files with extensions or MIME types not supported by the automator (e.g., `.txt`, `.avi`) result in a `false` return value.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void ValidateVideoFile_WithEmptyFile_ReturnsFalse()`
Ensures that a file with zero bytes is correctly identified as invalid and returns `false`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void ValidateVideoFile_WithSupportedFormats_ReturnsTrue()`
Iterates through a list of supported video formats (beyond just MP4) to ensure they all pass validation successfully.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Throws an assertion failure if any supported format is rejected.

#### `public void GetFileHash_WithValidFile_ReturnsConsistentHash()`
Verifies that calling the hash generation method multiple times on the same valid file yields the identical hash string.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Throws an assertion failure if hashes differ.

#### `public void GetFileHash_WithNullPath_ThrowsArgumentException()`
Ensures that requesting a hash for a `null` path throws an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void GetFileHash_WithEmptyPath_ThrowsArgumentException()`
Ensures that requesting a hash for an empty path string throws an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void GetFileHash_WithNonExistentFile_ThrowsInvalidOperationException()`
Confirms that attempting to generate a hash for a file that does not exist results in an `InvalidOperationException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `InvalidOperationException`.

#### `public void GetFileHash_WithDifferentFiles_ReturnsDifferentHashes()`
Validates that two distinct files, even if similar in content or size, produce unique hash values.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Throws an assertion failure if hashes match.

#### `public void GetVideoDuration_WithExistentFile_ReturnsTimeSpan()`
Checks that a valid video file returns a non-negative `TimeSpan` object representing the duration.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Throws an assertion failure if the result is null or negative.

#### `public void GetVideoDuration_WithNullPath_ThrowsArgumentException()`
Ensures that requesting duration for a `null` path throws an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void GetVideoDuration_WithEmptyPath_ThrowsArgumentException()`
Ensures that requesting duration for an empty path string throws an `ArgumentException`.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Expects `ArgumentException`.

#### `public void GetVideoDuration_WithNonExistentFile_ReturnsNull()`
Confirms that attempting to get the duration of a non-existent file returns `null` rather than throwing an exception.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: None.

#### `public void DeleteTemporaryFile_WithExistingFile_DeletesFile()`
Verifies that the cleanup method successfully removes a temporary file from the disk and that the file no longer exists post-execution.
*   **Parameters**: None.
*   **Return Value**: None.
*   **Exceptions**: Throws an assertion failure if the file persists.

## Usage

The following examples demonstrate how the test class might be instantiated and executed within a .NET test runner context, highlighting typical test flows for validation and hashing.

### Example 1: Running Validation Scenarios
This example illustrates the logical flow covered by the validation tests, ensuring various file states are handled correctly.

```csharp
using Xunit;

namespace YouTubeShortsAutomator.Tests
{
    public class ValidationFlowExample
    {
        [Fact]
        public void RunValidationSuite()
        {
            // Instantiate the test class to access its test methods
            var tests = new FileValidationServiceTests();
            
            // Execute specific test scenarios manually if needed for debugging
            // Note: In standard practice, the test runner invokes these directly.
            tests.ValidateVideoFile_WithValidMp4File_ReturnsTrue();
            tests.ValidateVideoFile_WithNullPath_ThrowsArgumentException();
            tests.ValidateVideoFile_WithNonExistentFile_ReturnsFalse();
            
            // Verify that unsupported formats are rejected
            tests.ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse();
        }
    }
}
```

### Example 2: Verifying Hash Consistency and Cleanup
This example focuses on the integrity checks and temporary file management capabilities verified by the suite.

```csharp
using Xunit;

namespace YouTubeShortsAutomator.Tests
{
    public class IntegrityAndCleanupExample
    {
        [Fact]
        public void RunHashAndCleanupTests()
        {
            var tests = new FileValidationServiceTests();
            
            // Ensure hashing is deterministic for the same file
            tests.GetFileHash_WithValidFile_ReturnsConsistentHash();
            
            // Ensure different files yield different hashes
            tests.GetFileHash_WithDifferentFiles_ReturnsDifferentHashes();
            
            // Verify that invalid inputs throw expected exceptions
            tests.GetFileHash_WithNullPath_ThrowsArgumentException();
            
            // Confirm temporary files are removed correctly after processing
            tests.DeleteTemporaryFile_WithExistingFile_DeletesFile();
        }
    }
}
```

## Notes

*   **Exception Handling Consistency**: The API distinguishes between argument validation and operational failures. Methods consistently throw `ArgumentException` for `null` or empty paths across all three main operations (Validation, Hashing, Duration). However, behavior diverges for non-existent files: `ValidateVideoFile` returns `false`, `GetVideoDuration` returns `null`, while `GetFileHash` throws an `InvalidOperationException`. Consumers of the underlying service must handle these distinct patterns appropriately.
*   **File System Dependencies**: These tests rely heavily on the actual file system. They create temporary files to simulate conditions like "file too large," "empty file," or "unsupported format." Execution environment permissions and disk space availability may impact test reliability if not properly isolated.
*   **Thread Safety**: As a test class, `FileValidationServiceTests` is designed to be instantiated per test case by the test runner. There is no shared mutable static state within the class itself, implying that tests can theoretically run in parallel provided the underlying file system operations do not conflict on shared temporary paths. However, the methods are instance members and are not inherently thread-safe if called concurrently on the same instance without external synchronization.
*   **Edge Cases**: The suite explicitly covers zero-byte files (empty files) and boundary conditions for file size (too large/too small). It also verifies that the duration extractor handles missing metadata or corrupted headers gracefully by returning `null` instead of crashing, whereas the hasher treats missing files as a fatal operational error.
