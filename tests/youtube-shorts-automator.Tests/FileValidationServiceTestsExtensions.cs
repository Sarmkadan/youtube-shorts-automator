using System;
using System.IO;
using System.Reflection;
using YouTubeShortAutomator.Services;

namespace YouTubeShortAutomator.Tests
{
    /// <summary>
    /// Extension methods that provide additional helpers for <see cref="FileValidationServiceTests"/>.
    /// </summary>
    public static class FileValidationServiceTestsExtensions
    {
        /// <summary>
        /// Gets the temporary test directory used by the test instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The full path of the temporary directory.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">When the private field cannot be read.</exception>
        public static string GetTestDirectory(this FileValidationServiceTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            var field = typeof(FileValidationServiceTests).GetField("_testDirectory", BindingFlags.Instance | BindingFlags.NonPublic);
            return (string?)field?.GetValue(test) ?? throw new InvalidOperationException("Unable to retrieve test directory.");
        }

        /// <summary>
        /// Gets the <see cref="FileValidationService"/> instance used by the test.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The service instance.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">When the private field cannot be read.</exception>
        public static FileValidationService GetService(this FileValidationServiceTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            var field = typeof(FileValidationServiceTests).GetField("_service", BindingFlags.Instance | BindingFlags.NonPublic);
            return (FileValidationService?)field?.GetValue(test) ?? throw new InvalidOperationException("Unable to retrieve FileValidationService.");
        }

        /// <summary>
        /// Creates a file of the specified size inside the test's temporary directory.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="filename">The name of the file to create.</param>
        /// <param name="sizeBytes">The desired size in bytes.</param>
        /// <returns>The full path to the created file.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> or <paramref name="filename"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">When <paramref name="filename"/> is empty or whitespace.</exception>
        public static string CreateTestFile(this FileValidationServiceTests test, string filename, long sizeBytes)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentException.ThrowIfNullOrEmpty(filename);
            var dir = test.GetTestDirectory();
            var path = Path.Combine(dir, filename);
            var bytes = new byte[sizeBytes];
            new Random().NextBytes(bytes);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        /// <summary>
        /// Deletes all files in the test's temporary directory using the underlying <see cref="FileValidationService"/>.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        public static void CleanupTestDirectory(this FileValidationServiceTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            var dir = test.GetTestDirectory();
            var service = test.GetService();
            service.CleanupTemporaryDirectory(dir);
        }
    }
}
