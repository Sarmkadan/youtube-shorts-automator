## FileSystemUtility

`FileSystemUtility` provides static methods for common file system operations, such as ensuring directories exist, validating video files, reading and writing files, and managing directories.

### Usage Example

```csharp
// Ensure the output directory exists
await FileSystemUtility.EnsureDirectoryExistsAsync("./output");

// Check if a file is a valid video file
bool isValid = FileSystemUtility.IsValidVideoFile("./video.mp4");

// Read a text file
string content = await FileSystemUtility.ReadFileAsStringAsync("./notes.txt");

// Write the content to a new file in the output directory
await FileSystemUtility.WriteFileAsync("./output/notes.txt", content);

// Get all text files in the output directory
string[] textFiles = FileSystemUtility.GetFilesWithExtension("./output", "txt");

// Get the total size of the output directory
long directorySize = FileSystemUtility.GetDirectorySizeBytes("./output");
```