// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.IntegrationTests;

/// <summary>
/// A uniquely named temporary directory that is deleted recursively on disposal.
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string DirectoryPath { get; } =
        Path.Combine(Path.GetTempPath(), "shorts-integration-" + Guid.NewGuid().ToString("N"));

    public TempDirectory()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch (IOException)
        {
            // Leftover temp files are not a test failure.
        }
        catch (UnauthorizedAccessException)
        {
            // Leftover temp files are not a test failure.
        }
    }
}
