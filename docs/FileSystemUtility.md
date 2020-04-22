# FileSystemUtility

The `FileSystemUtility` class provides a centralized set of static methods for common file system operations within the `youtube-shorts-automator` project. It encapsulates standard I/O tasks to ensure consistent error handling, path sanitization, and asynchronous execution across the application, simplifying directory management, file manipulation, and metadata retrieval.

## API

### EnsureDirectoryExistsAsync(string path)
Ensures that the directory at the specified path exists, creating it if necessary.
*   **Parameters:** `string path` (The target directory path).
*   **Returns:** `Task<bool>` (True if the directory exists or was created successfully, false otherwise).
*   **Throws:** `IOException`, `UnauthorizedAccessException` if file system permissions prevent creation.

### IsValidVideoFile(string filePath)
Determines if a given file path corresponds to a supported, valid video file.
*   **Parameters:** `string filePath` (The path to the file to check).
*   **Returns:** `bool` (True if the file is a recognized video format, false otherwise).

### DeleteFileAsync(string filePath)
Asynchronously deletes the file at the specified path.
*   **Parameters:** `string filePath` (The path of the file to delete).
*   **Returns:** `Task<bool>` (True if the file was deleted or did not exist, false if deletion failed).
*   **Throws:** `IOException`, `UnauthorizedAccessException`.

### DeleteDirectoryAsync(string directoryPath)
Asynchronously deletes the specified directory and all of its contents.
*   **Parameters:** `string directoryPath` (The path of the directory to delete).
*   **Returns:** `Task<bool>` (True if the directory was deleted successfully, false otherwise).
*   **Throws:** `IOException`, `UnauthorizedAccessException`.

### GetDirectorySizeBytes(string directoryPath)
Calculates the total size of all files within the specified directory, in bytes.
*   **Parameters:** `string directoryPath` (The directory to measure).
*   **Returns:** `long` (Total size in bytes).
*   **Throws:** `DirectoryNotFoundException` if the directory does not exist.

### GetSafeFileName(string fileName)
Sanitizes a string to be used as a valid filename by removing or replacing invalid characters.
*   **Parameters:** `string fileName` (The raw filename string).
*   **Returns:** `string` (The sanitized filename).

### ReadFileAsStringAsync(string filePath)
Asynchronously reads the content of a file into a string.
*   **Parameters:** `string filePath` (The path to the file).
*   **Returns:** `Task<string?>` (The file content, or null if the file does not exist).
*   **Throws:** `IOException`, `UnauthorizedAccessException`.

### WriteFileAsync(string filePath, string content)
Asynchronously writes the provided string content to the specified file path.
*   **Parameters:** `string filePath` (The target file path), `string content` (The content to write).
*   **Returns:** `Task<bool>` (True if the write operation was successful).
*   **Throws:** `IOException`, `UnauthorizedAccessException`.

### GetFilesWithExtension(string directoryPath, string extension)
Retrieves a list of files within a directory matching the specified extension (e.g., ".mp4").
*   **Parameters:** `string directoryPath` (The directory to search), `string extension` (The extension filter).
*   **Returns:** `string[]` (An array of absolute file paths).

### GetFileModificationTime(string filePath)
Retrieves the date and time the specified file was last modified.
*   **Parameters:** `string filePath` (The path to the file).
*   **Returns:** `DateTime` (The last write time).
*   **Throws:** `FileNotFoundException`.

## Usage

### Example 1: Reading and Writing Configuration Files
```csharp
string configPath = "settings/config.json";

// Ensure the directory exists before writing
await FileSystemUtility.EnsureDirectoryExistsAsync("settings");

string content = await FileSystemUtility.ReadFileAsStringAsync(configPath) ?? "{}";
// Modify content...
bool success = await FileSystemUtility.WriteFileAsync(configPath, content);
```

### Example 2: Cleaning up Video Directories
```csharp
string videoDir = "temp/processed_videos";

// Get all MP4 files
string[] files = FileSystemUtility.GetFilesWithExtension(videoDir, ".mp4");

foreach (string file in files)
{
    if (!FileSystemUtility.IsValidVideoFile(file))
    {
        await FileSystemUtility.DeleteFileAsync(file);
    }
}
```

## Notes

*   **Asynchronous Operations:** All methods suffixed with `Async` perform non-blocking I/O operations and should be awaited.
*   **Thread Safety:** The methods in this class are thread-safe regarding the `FileSystemUtility` internal state; however, they are not atomic with respect to the underlying file system. Race conditions may occur if multiple processes or threads attempt to modify the same file or directory simultaneously.
*   **Exception Handling:** While `FileSystemUtility` handles basic path validation, consumers should still implement appropriate `try-catch` blocks for `IOException`, `UnauthorizedAccessException`, and other potential file system-related exceptions when calling these methods, especially in environments with shared storage or strict permission settings.
*   **Pathing:** All paths should be passed as valid absolute or relative file system paths. The class does not automatically resolve environment variables or special shell expansions.
