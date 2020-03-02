// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Base exception for all custom exceptions in the YouTube Shorts Automator project.
/// </summary>
public class YoutubeShortsAutomatorException : Exception
{
    public YoutubeShortsAutomatorException() { }

    public YoutubeShortsAutomatorException(string message) : base(message) { }

    public YoutubeShortsAutomatorException(string message, Exception innerException)
        : base(message, innerException) { }
}
