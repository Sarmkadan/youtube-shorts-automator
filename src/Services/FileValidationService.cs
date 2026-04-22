// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Exceptions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace YouTubeShortAutomator.Services;

public class FileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    private readonly string[] _supportedFormats = Constants.Constants.SUPPORTED_INPUT_FORMATS.Split(',');

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool ValidateVideoFile(string filePath)
    {
        // Fix: Add validation for filePath to prevent null or empty paths.
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        // Validates a video file for processing
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                return false;
            }

            var fileInfo = new FileInfo(filePath);

            // Check file size
            if (fileInfo.Length > Constants.Constants.MAX_FILE_SIZE_BYTES)
            {
                _logger.LogWarning($"File too large: {fileInfo.Length} bytes");
                return false;
            }

            if (fileInfo.Length < Constants.Constants.MIN_FILE_SIZE_BYTES)
            {
                _logger.LogWarning($"File too small: {fileInfo.Length} bytes");
                return false;
            }

            // Check file extension
            var extension = fileInfo.Extension.TrimStart('.').ToLower();
            if (!_supportedFormats.Contains(extension))
            {
                _logger.LogWarning($"Unsupported file format: {extension}");
                return false;
            }

            // Check file readability
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    if (stream.Length == 0)
                    {
                        _logger.LogWarning("File is empty");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot read file: {ex.Message}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"File validation error: {ex.Message}");
            return false;
        }
    }

    public string GetFileHash(string filePath)
    {
        // Fix: Add validation for filePath to prevent null or empty paths.
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        // Calculates SHA256 hash of file for integrity verification
        try
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating file hash: {ex.Message}");
            throw new InvalidOperationException($"Failed to calculate file hash: {ex.Message}", ex);
        }
    }

    public TimeSpan? GetVideoDuration(string filePath)
    {
        // Fix: Add validation for filePath to prevent null or empty paths.
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        // Attempts to get video duration (in production, would use FFprobe)
        try
        {
            if (!File.Exists(filePath))
                return null;

            // Simulate getting video metadata
            _logger.LogInformation($"Retrieving duration for {filePath}");
            
            // In production, this would call FFprobe to get actual duration
            return TimeSpan.FromSeconds(30); // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting video duration: {ex.Message}");
            return null;
        }
    }

    public void DeleteTemporaryFile(string filePath)
    {
        // Fix: Add validation for filePath to prevent null or empty paths.
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        // Safely deletes a temporary file
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation($"Deleted temporary file: {filePath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to delete temporary file {filePath}: {ex.Message}");
        }
    }

    public void CleanupTemporaryDirectory(string directoryPath)
    {
        // Fix: Add validation for directoryPath to prevent null or empty paths.
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be null or whitespace.", nameof(directoryPath));
        }
        // Cleans up temporary files in a directory
        try
        {
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    DeleteTemporaryFile(file);
                }

                _logger.LogInformation($"Cleaned up temporary directory: {directoryPath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cleaning up temporary directory: {ex.Message}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetSupportedFormats()
    {
        // Returns a string of supported video formats
        return Constants.Constants.SUPPORTED_INPUT_FORMATS;
    }
}
