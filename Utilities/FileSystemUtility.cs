// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides file system operations with safety checks and error handling
/// Handles directory creation, file validation, and cleanup operations
/// </summary>
public static class FileSystemUtility
{
    private const long MaxFileSizeBytes = 10_737_418_240; // 10 GB

    public static async Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidVideoFile(string filePath, string[] validExtensions)
    {
        if (!File.Exists(filePath))
            return false;

        var fileInfo = new FileInfo(filePath);

        // Check file extension
        var extension = fileInfo.Extension.ToLowerInvariant();
        if (!validExtensions.Contains(extension))
            return false;

        // Check file size
        if (fileInfo.Length > MaxFileSizeBytes || fileInfo.Length == 0)
            return false;

        return true;
    }

    public static async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await Task.Delay(100); // Allow filesystem to catch up
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = true)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive);
                await Task.Delay(100);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static long GetDirectorySizeBytes(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return 0;

        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }
        catch
        {
            return 0;
        }
    }

    public static string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(safeName) ? "file" : safeName;
    }

    public static async Task<string?> ReadFileAsStringAsync(string filePath)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> WriteFileAsync(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                await EnsureDirectoryExistsAsync(directory);
            }

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string[] GetFilesWithExtension(string directoryPath, string extension)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                return Array.Empty<string>();

            return Directory.GetFiles(directoryPath, $"*{extension}", SearchOption.TopDirectoryOnly);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static DateTime GetFileModificationTime(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return DateTime.MinValue;

            return File.GetLastWriteTimeUtc(filePath);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}
