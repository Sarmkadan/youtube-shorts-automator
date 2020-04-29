// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Services;

public static class FileValidationServiceValidation
{
    /// <summary>
    /// Validates the FileValidationService instance for common issues.
    /// </summary>
    /// <param name="value">The FileValidationService instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this FileValidationService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // FileValidationService has no state to validate beyond being non-null
        // All validation is done through its methods which validate their parameters

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the FileValidationService instance is valid.
    /// </summary>
    /// <param name="value">The FileValidationService instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this FileValidationService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true; // FileValidationService has no state to validate
    }

    /// <summary>
    /// Ensures that the FileValidationService instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The FileValidationService instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this FileValidationService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // FileValidationService has no state to validate beyond being non-null
        // All validation is done through its methods which validate their parameters
    }
}