# FileValidationService

Provides utilities for validating video files, computing hashes, retrieving metadata such as duration, and managing temporary files used during processing.

## API

### `FileValidationService`

Initializes a new instance of the file validation service. The service manages a dedicated temporary directory for intermediate file operations.

### `bool ValidateVideoFile(string filePath)`

Validates that the specified video file is supported and accessible.

- **Parameters**
  - `filePath` (string): The full path to the video file to validate.
- **Return value**
  - `true` if the file exists, is readable, and has a supported format; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.
  - Throws `ArgumentException` if `filePath` is empty or whitespace.

### `string GetFileHash(string filePath)`

Computes a SHA-256 hash of the specified video file.

- **Parameters**
  - `filePath` (string): The full path to the video file to hash.
- **Return value**
  - A hexadecimal string representing the SHA-256 hash of the file content.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.
  - Throws `ArgumentException` if `filePath` is empty or whitespace.
  - Throws `FileNotFoundException` if the file does not exist.
  - Throws `UnauthorizedAccessException` if the caller lacks sufficient permissions to read the file.

### `TimeSpan? GetVideoDuration(string filePath)`

Retrieves the duration of the specified video file.

- **Parameters**
  - `filePath` (string): The full path to the video file.
- **Return value**
  - A `TimeSpan` representing the video duration, or `null` if the duration cannot be determined.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.
  - Throws `ArgumentException` if `filePath` is empty or whitespace.

### `void DeleteTemporaryFile(string fileName)`

Deletes a file from the service’s temporary directory.

- **Parameters**
  - `fileName` (string): The name of the file to delete within the temporary directory.
- **Exceptions**
  - Throws `ArgumentNullException` if `fileName` is `null`.
  - Throws `ArgumentException` if `fileName` is empty or whitespace.
  - Throws `FileNotFoundException` if the file does not exist in the temporary directory.

### `void CleanupTemporaryDirectory()`

Deletes all files and subdirectories within the service’s temporary directory.

- **Exceptions**
  - Throws `UnauthorizedAccessException` if the caller lacks sufficient permissions to delete files or directories.

### `string GetSupportedFormats()`

Returns a semicolon-separated list of supported video file extensions.

- **Return value**
  - A string containing supported formats (e.g., ".mp4;.mov;.avi").

## Usage
