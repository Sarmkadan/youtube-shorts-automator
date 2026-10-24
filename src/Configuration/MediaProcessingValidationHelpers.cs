// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Filesystem checks shared by the media processing options validators.
/// </summary>
internal static class MediaProcessingValidationHelpers
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="command"/> names an existing file. A rooted or relative path
    /// with a directory part is checked as-is; a bare command name is searched for in every PATH directory.
    /// </summary>
    public static bool IsToolAvailable(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        if (command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
        {
            return File.Exists(command);
        }

        var pathDirectories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        var extensions = OperatingSystem.IsWindows()
            ? new[] { string.Empty }.Concat((Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
                .Split(';', StringSplitOptions.RemoveEmptyEntries))
            : [string.Empty];

        return pathDirectories.Any(directory =>
            extensions.Any(extension => File.Exists(Path.Combine(directory, command + extension))));
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="directory"/> exists (or can be created) and a file can be written
    /// into it. When the directory does not exist yet, the nearest existing ancestor is probed instead.
    /// The probe file is deleted immediately.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <param name="problem">A human-readable reason when the check fails; <c>null</c> otherwise.</param>
    public static bool IsDirectoryWritable(string directory, out string? problem)
    {
        problem = null;

        try
        {
            var existing = Path.GetFullPath(directory);
            while (!Directory.Exists(existing))
            {
                if (File.Exists(existing))
                {
                    problem = $"'{existing}' is a file, not a directory";
                    return false;
                }

                var parent = Path.GetDirectoryName(existing);
                if (parent is null)
                {
                    problem = $"no existing ancestor found for '{directory}'";
                    return false;
                }

                existing = parent;
            }

            var probe = Path.Combine(existing, $".write-probe-{Guid.NewGuid():N}");
            File.Create(probe).Dispose();
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            problem = ex.Message;
            return false;
        }
    }
}
