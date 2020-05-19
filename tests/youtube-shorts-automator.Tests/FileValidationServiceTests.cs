// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Moq;
using YouTubeShortAutomator.Constants;
using YouTubeShortAutomator.Services;
using Microsoft.Extensions.Logging;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Contains unit tests for the <see cref="FileValidationService"/> class.
/// Tests file validation, hash calculation, duration extraction, and temporary file cleanup functionality.
/// </summary>
public class FileValidationServiceTests
{
    private readonly Mock<ILogger<FileValidationService>> _mockLogger;
    private readonly FileValidationService _service;
    private readonly string _testDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileValidationServiceTests"/> class.
    /// Sets up mock logger, creates test service instance, and initializes temporary test directory.
    /// </summary>
    public FileValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileValidationService>>();
        _service = new FileValidationService(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Creates a test file with the specified filename and size in the temporary test directory.
    /// </summary>
    /// <param name="filename">The name of the file to create.</param>
    /// <param name="sizeBytes">The size of the file in bytes.</param>
    /// <returns>The full path to the created test file.</returns>
    private string CreateTestFile(string filename, long sizeBytes)
    {
        var filePath = Path.Combine(_testDirectory, filename);
        var fileBytes = new byte[sizeBytes];
        new Random().NextBytes(fileBytes);
        File.WriteAllBytes(filePath, fileBytes);
        return filePath;
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns true for a valid MP4 file within size limits.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithValidMp4File_ReturnsTrue()
    {
        var filePath = CreateTestFile("test.mp4", Constants.Constants.MAX_FILE_SIZE_BYTES / 2);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> throws <see cref="ArgumentException"/> when provided with null file path.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithNullPath_ThrowsArgumentException()
    {
        Action act = () => _service.ValidateVideoFile(null!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or whitespace*");
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> throws <see cref="ArgumentException"/> when provided with empty or whitespace file path.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithEmptyPath_ThrowsArgumentException()
    {
        Action act = () => _service.ValidateVideoFile("   ");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns false when the file does not exist.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithNonExistentFile_ReturnsFalse()
    {
        var result = _service.ValidateVideoFile("/nonexistent/path/video.mp4");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns false when the file exceeds maximum allowed size.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithFileTooLarge_ReturnsFalse()
    {
        var filePath = CreateTestFile("large.mp4", Constants.Constants.MAX_FILE_SIZE_BYTES + 1024);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns false when the file is smaller than minimum allowed size.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithFileTooSmall_ReturnsFalse()
    {
        var filePath = CreateTestFile("tiny.mp4", Constants.Constants.MIN_FILE_SIZE_BYTES - 1);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns false when the file has an unsupported format.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithUnsupportedFormat_ReturnsFalse()
    {
        var filePath = CreateTestFile("file.txt", 5 * 1024 * 1024);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns false when the file is empty.
    /// </summary>
    [Fact]
    public void ValidateVideoFile_WithEmptyFile_ReturnsFalse()
    {
        var filePath = CreateTestFile("empty.mp4", 0);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.ValidateVideoFile"/> returns true for various supported video file formats.
    /// </summary>
    /// <param name="filename">The video filename with supported extension (mp4, avi, mov, mkv).</param>
    [Theory]
    [InlineData("video.mp4")]
    [InlineData("video.avi")]
    [InlineData("video.mov")]
    [InlineData("video.mkv")]
    public void ValidateVideoFile_WithSupportedFormats_ReturnsTrue(string filename)
    {
        var filePath = CreateTestFile(filename, 10 * 1024 * 1024);

        var result = _service.ValidateVideoFile(filePath);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetFileHash"/> returns consistent SHA256 hash for the same file.
    /// </summary>
    [Fact]
    public void GetFileHash_WithValidFile_ReturnsConsistentHash()
    {
        var filePath = CreateTestFile("test.mp4", 1024 * 1024);

        var hash1 = _service.GetFileHash(filePath);
        var hash2 = _service.GetFileHash(filePath);

        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash1.Length.Should().Be(64); // SHA256 hex length
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetFileHash"/> throws <see cref="ArgumentException"/> when provided with null file path.
    /// </summary>
    [Fact]
    public void GetFileHash_WithNullPath_ThrowsArgumentException()
    {
        Action act = () => _service.GetFileHash(null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetFileHash"/> throws <see cref="ArgumentException"/> when provided with empty or whitespace file path.
    /// </summary>
    [Fact]
    public void GetFileHash_WithEmptyPath_ThrowsArgumentException()
    {
        Action act = () => _service.GetFileHash("");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetFileHash"/> throws <see cref="InvalidOperationException"/> when the file does not exist.
    /// </summary>
    [Fact]
    public void GetFileHash_WithNonExistentFile_ThrowsInvalidOperationException()
    {
        Action act = () => _service.GetFileHash("/nonexistent/file.mp4");

        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetFileHash"/> returns different hashes for different files.
    /// </summary>
    [Fact]
    public void GetFileHash_WithDifferentFiles_ReturnsDifferentHashes()
    {
        var file1 = CreateTestFile("file1.mp4", 1024);
        var file2 = CreateTestFile("file2.mp4", 2048);

        var hash1 = _service.GetFileHash(file1);
        var hash2 = _service.GetFileHash(file2);

        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetVideoDuration"/> returns a valid TimeSpan for an existing video file.
    /// </summary>
    [Fact]
    public void GetVideoDuration_WithExistentFile_ReturnsTimeSpan()
    {
        var filePath = CreateTestFile("video.mp4", 1024 * 1024);

        var duration = _service.GetVideoDuration(filePath);

        duration.Should().NotBeNull();
        duration.Should().Be(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetVideoDuration"/> throws <see cref="ArgumentException"/> when provided with null file path.
    /// </summary>
    [Fact]
    public void GetVideoDuration_WithNullPath_ThrowsArgumentException()
    {
        Action act = () => _service.GetVideoDuration(null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetVideoDuration"/> throws <see cref="ArgumentException"/> when provided with empty or whitespace file path.
    /// </summary>
    [Fact]
    public void GetVideoDuration_WithEmptyPath_ThrowsArgumentException()
    {
        Action act = () => _service.GetVideoDuration("  ");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetVideoDuration"/> returns null when the file does not exist.
    /// </summary>
    [Fact]
    public void GetVideoDuration_WithNonExistentFile_ReturnsNull()
    {
        var duration = _service.GetVideoDuration("/nonexistent/video.mp4");

        duration.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.DeleteTemporaryFile"/> successfully deletes an existing temporary file.
    /// </summary>
    [Fact]
    public void DeleteTemporaryFile_WithExistingFile_DeletesFile()
    {
        var filePath = CreateTestFile("temp.mp4", 1024);
        File.Exists(filePath).Should().BeTrue();

        _service.DeleteTemporaryFile(filePath);

        File.Exists(filePath).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.DeleteTemporaryFile"/> throws <see cref="ArgumentException"/> when provided with null file path.
    /// </summary>
    [Fact]
    public void DeleteTemporaryFile_WithNullPath_ThrowsArgumentException()
    {
        Action act = () => _service.DeleteTemporaryFile(null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.DeleteTemporaryFile"/> does not throw when attempting to delete a non-existent file.
    /// </summary>
    [Fact]
    public void DeleteTemporaryFile_WithNonExistentFile_DoesNotThrow()
    {
        Action act = () => _service.DeleteTemporaryFile("/nonexistent/file.mp4");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.CleanupTemporaryDirectory"/> deletes all files in the specified temporary directory.
    /// </summary>
    [Fact]
    public void CleanupTemporaryDirectory_WithMultipleFiles_DeletesAll()
    {
        CreateTestFile("file1.mp4", 1024);
        CreateTestFile("file2.mp4", 2048);
        CreateTestFile("file3.mp4", 4096);

        _service.CleanupTemporaryDirectory(_testDirectory);

        Directory.GetFiles(_testDirectory).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.CleanupTemporaryDirectory"/> does not throw when cleaning up an empty directory.
    /// </summary>
    [Fact]
    public void CleanupTemporaryDirectory_WithEmptyDirectory_DoesNotThrow()
    {
        var emptyDir = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(emptyDir);

        Action act = () => _service.CleanupTemporaryDirectory(emptyDir);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.CleanupTemporaryDirectory"/> throws <see cref="ArgumentException"/> when provided with null directory path.
    /// </summary>
    [Fact]
    public void CleanupTemporaryDirectory_WithNullPath_ThrowsArgumentException()
    {
        Action act = () => _service.CleanupTemporaryDirectory(null!);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.CleanupTemporaryDirectory"/> does not throw when attempting to clean up a non-existent directory.
    /// </summary>
    [Fact]
    public void CleanupTemporaryDirectory_WithNonExistentDirectory_DoesNotThrow()
    {
        Action act = () => _service.CleanupTemporaryDirectory("/nonexistent/dir");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="FileValidationService.GetSupportedFormats"/> returns a non-empty string containing supported video file formats.
    /// </summary>
    [Fact]
    public void GetSupportedFormats_ReturnsFormatsString()
    {
        var formats = _service.GetSupportedFormats();

        formats.Should().NotBeNullOrEmpty();
        formats.Should().Contain("mp4");
        formats.Should().Contain("avi");
    }

    /// <summary>
    /// Finalizer that cleans up the temporary test directory when the test class is disposed.
    /// </summary>
    ~FileValidationServiceTests()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
