// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for FileSystemUtility operations
// Provides validation, safety checks, and argument validation for file system operations
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for FileSystemUtility operations
/// </summary>
public static class FileSystemUtilityValidation
{
    /// <summary>
    /// Validates directory path for EnsureDirectoryExistsAsync
    /// </summary>
    /// <param name="directoryPath">Directory path to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateDirectoryPath(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath, nameof(directoryPath));
        if (directoryPath.Length == 0) throw new ArgumentException("Value cannot be empty", nameof(directoryPath));

        var problems = new List<string>();

        // Check for invalid path characters
        var invalidChars = Path.GetInvalidPathChars();
        if (directoryPath.IndexOfAny(invalidChars) >= 0)
        {
            problems.Add("Directory path contains invalid characters");
        }

        // Check for relative paths that might be problematic
        if (directoryPath.StartsWith(".", StringComparison.Ordinal) &&
            (directoryPath.Length == 1 || directoryPath[1] == Path.DirectorySeparatorChar || directoryPath[1] == Path.AltDirectorySeparatorChar))
        {
            problems.Add("Relative paths starting with '.' are not recommended");
        }

        // Check path length
        if (directoryPath.Length > 260)
        {
            problems.Add("Directory path exceeds maximum length (260 characters)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if directory path is valid for EnsureDirectoryExistsAsync
    /// </summary>
    /// <param name="directoryPath">Directory path to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidDirectoryPath(string directoryPath)
    {
        try
        {
            return ValidateDirectoryPath(directoryPath).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures directory path is valid, throwing if not
    /// </summary>
    /// <param name="directoryPath">Directory path to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidDirectoryPath(string directoryPath)
    {
        var problems = ValidateDirectoryPath(directoryPath);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Directory path is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates file path for file operations
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateFilePath(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
        if (filePath.Length == 0) throw new ArgumentException("Value cannot be empty", nameof(filePath));

        var problems = new List<string>();

        // Check for invalid path characters
        var invalidChars = Path.GetInvalidPathChars();
        if (filePath.IndexOfAny(invalidChars) >= 0)
        {
            problems.Add("File path contains invalid characters");
        }

        // Check for invalid file name characters
        var fileName = Path.GetFileName(filePath);
        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
        {
            problems.Add("File name contains invalid characters");
        }

        // Check path length
        if (filePath.Length > 260)
        {
            problems.Add("File path exceeds maximum length (260 characters)");
        }

        // Check file extension
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension))
        {
            problems.Add("File path must include a file extension");
        }
        else if (extension.StartsWith(".", StringComparison.Ordinal) && extension.Length == 1)
        {
            problems.Add("File path has invalid extension");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if file path is valid for file operations
    /// </summary>
    /// <param name="filePath">File path to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidFilePath(string filePath)
    {
        try
        {
            return ValidateFilePath(filePath).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures file path is valid, throwing if not
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidFilePath(string filePath)
    {
        var problems = ValidateFilePath(filePath);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"File path is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates video file parameters for IsValidVideoFile
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <param name="validExtensions">Array of valid extensions</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateVideoFileParameters(string filePath, string[] validExtensions)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
        if (filePath.Length == 0) throw new ArgumentException("Value cannot be empty", nameof(filePath));
        ArgumentNullException.ThrowIfNull(validExtensions, nameof(validExtensions));

        if (validExtensions.Length == 0)
        {
            return new[] { "Valid extensions array cannot be empty" }.AsReadOnly();
        }

        var problems = new List<string>();

        // Validate file path
        problems.AddRange(ValidateFilePath(filePath));

        // Validate extensions
        foreach (var ext in validExtensions)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                problems.Add("Valid extension cannot be null or whitespace");
            }
            else if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                problems.Add($"Extension '{ext}' must start with '.'");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if video file parameters are valid for IsValidVideoFile
    /// </summary>
    /// <param name="filePath">File path to check</param>
    /// <param name="validExtensions">Array of valid extensions</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidVideoFileParameters(string filePath, string[] validExtensions)
    {
        try
        {
            return ValidateVideoFileParameters(filePath, validExtensions).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures video file parameters are valid, throwing if not
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <param name="validExtensions">Array of valid extensions</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidVideoFileParameters(string filePath, string[] validExtensions)
    {
        var problems = ValidateVideoFileParameters(filePath, validExtensions);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Video file parameters are invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates content for WriteFileAsync
    /// </summary>
    /// <param name="content">Content to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateFileContent(string content)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        var problems = new List<string>();

        // Check for empty content
        if (string.IsNullOrWhiteSpace(content))
        {
            problems.Add("Content cannot be null, empty, or whitespace");
        }

        // Check content length
        if (content.Length > 1_000_000) // 1MB limit for safety
        {
            problems.Add("Content exceeds maximum length (1MB)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if content is valid for WriteFileAsync
    /// </summary>
    /// <param name="content">Content to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidFileContent(string content)
    {
        try
        {
            return ValidateFileContent(content).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures content is valid, throwing if not
    /// </summary>
    /// <param name="content">Content to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidFileContent(string content)
    {
        var problems = ValidateFileContent(content);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Content is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates directory path and extension for GetFilesWithExtension
    /// </summary>
    /// <param name="directoryPath">Directory path to validate</param>
    /// <param name="extension">File extension to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateDirectoryFilesParameters(string directoryPath, string extension)
    {
        ArgumentNullException.ThrowIfNull(directoryPath, nameof(directoryPath));
        if (directoryPath.Length == 0) throw new ArgumentException("Value cannot be empty", nameof(directoryPath));
        ArgumentNullException.ThrowIfNull(extension, nameof(extension));
        if (extension.Length == 0) throw new ArgumentException("Value cannot be empty", nameof(extension));

        var problems = new List<string>();

        // Validate directory path
        problems.AddRange(ValidateDirectoryPath(directoryPath));

        // Validate extension
        if (!extension.StartsWith(".", StringComparison.Ordinal))
        {
            problems.Add("Extension must start with '.'");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if directory and extension parameters are valid for GetFilesWithExtension
    /// </summary>
    /// <param name="directoryPath">Directory path to check</param>
    /// <param name="extension">File extension to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValidDirectoryFilesParameters(string directoryPath, string extension)
    {
        try
        {
            return ValidateDirectoryFilesParameters(directoryPath, extension).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures directory and extension parameters are valid, throwing if not
    /// </summary>
    /// <param name="directoryPath">Directory path to validate</param>
    /// <param name="extension">File extension to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidDirectoryFilesParameters(string directoryPath, string extension)
    {
        var problems = ValidateDirectoryFilesParameters(directoryPath, extension);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Directory and extension parameters are invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
