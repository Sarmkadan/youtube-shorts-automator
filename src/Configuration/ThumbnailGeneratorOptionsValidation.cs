// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Startup validation for <see cref="ThumbnailGeneratorOptions"/>. Reports every problem in one pass.
/// </summary>
public sealed class ThumbnailGeneratorOptionsValidation : IValidateOptions<ThumbnailGeneratorOptions>
{
    /// <summary>Inclusive lower bound for <see cref="ThumbnailGeneratorOptions.DefaultQuality"/>.</summary>
    public const int MinQuality = 1;

    /// <summary>Inclusive upper bound for <see cref="ThumbnailGeneratorOptions.DefaultQuality"/>.</summary>
    public const int MaxQuality = 100;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ThumbnailGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!MediaProcessingValidationHelpers.IsToolAvailable(options.FfmpegPath))
        {
            failures.Add($"Processing:FFmpegPath '{options.FfmpegPath}' is not an existing file and was not found on PATH.");
        }

        if (options.FfmpegTimeoutSeconds <= 0)
        {
            failures.Add($"Processing:FFmpegTimeoutSeconds must be positive, but was {options.FfmpegTimeoutSeconds}.");
        }

        if (options.DefaultQuality < MinQuality || options.DefaultQuality > MaxQuality)
        {
            failures.Add(
                $"ThumbnailGenerator:DefaultQuality must be between {MinQuality} and {MaxQuality}, " +
                $"but was {options.DefaultQuality}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
