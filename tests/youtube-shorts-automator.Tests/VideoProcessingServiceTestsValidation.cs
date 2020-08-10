using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortAutomator.Tests;

/// <summary>
/// Validation helpers for <see cref="VideoProcessingServiceTests"/> test class.
/// Provides methods to validate test class instances for common issues like null values,
/// empty strings, out-of-range numbers, and default dates.
/// </summary>
public static class VideoProcessingServiceTestsValidation
{
    /// <summary>
    /// Validates the given <see cref="VideoProcessingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this VideoProcessingServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // VideoProcessingServiceTests is a test fixture class with no stateful members to validate
        // All validation is structural (null checks on method parameters within the methods themselves)
        // This validation method exists for API consistency with other validation helpers in this project

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoProcessingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this VideoProcessingServiceTests? value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="VideoProcessingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this VideoProcessingServiceTests? value)
    {
        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"VideoProcessingServiceTests instance is not valid:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems),
            nameof(value));
    }
}