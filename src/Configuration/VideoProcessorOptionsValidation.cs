// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using YouTubeShortAutomator.Constants;

namespace YouTubeShortAutomator.Configuration;

/// <summary>
/// Startup validation for <see cref="VideoProcessorOptions"/>. Reports every problem in one pass so that
/// a misconfigured host lists all of its errors at once.
/// </summary>
public sealed class VideoProcessorOptionsValidation : IValidateOptions<VideoProcessorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, VideoProcessorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!MediaProcessingValidationHelpers.IsToolAvailable(options.FfmpegPath))
        {
            failures.Add($"Processing:FFmpegPath '{options.FfmpegPath}' is not an existing file and was not found on PATH.");
        }

        if (!MediaProcessingValidationHelpers.IsToolAvailable(options.FfprobePath))
        {
            failures.Add($"Processing:FFprobePath '{options.FfprobePath}' is not an existing file and was not found on PATH.");
        }

        if (options.MaxDurationSeconds < Constants.Constants.MIN_DURATION_SECONDS
            || options.MaxDurationSeconds > Constants.Constants.MAX_DURATION_SECONDS)
        {
            failures.Add(
                $"VideoProcessor:MaxDurationSeconds must be between {Constants.Constants.MIN_DURATION_SECONDS} " +
                $"and {Constants.Constants.MAX_DURATION_SECONDS}, but was {options.MaxDurationSeconds}.");
        }

        ValidateResolution(options, failures);

        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            failures.Add("VideoProcessor:OutputDirectory must not be empty.");
        }
        else if (!MediaProcessingValidationHelpers.IsDirectoryWritable(options.OutputDirectory, out var problem))
        {
            failures.Add($"VideoProcessor:OutputDirectory '{options.OutputDirectory}' is not writable: {problem}");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateResolution(VideoProcessorOptions options, List<string> failures)
    {
        if (options.TargetWidth <= 0 || options.TargetHeight <= 0)
        {
            failures.Add(
                $"VideoProcessor:TargetWidth and VideoProcessor:TargetHeight must be positive, " +
                $"but were {options.TargetWidth}x{options.TargetHeight}.");
            return;
        }

        // yuv420p (used by the encoder) requires even dimensions.
        if (options.TargetWidth % 2 != 0 || options.TargetHeight % 2 != 0)
        {
            failures.Add(
                $"VideoProcessor:TargetWidth and VideoProcessor:TargetHeight must both be even, " +
                $"but were {options.TargetWidth}x{options.TargetHeight}.");
        }

        // Use long arithmetic so that large configured values cannot overflow the 9:16 comparison.
        if ((long)options.TargetWidth * 16 != (long)options.TargetHeight * 9)
        {
            failures.Add(
                $"VideoProcessor:TargetWidth x TargetHeight must be a 9:16 size (for example 1080x1920), " +
                $"but was {options.TargetWidth}x{options.TargetHeight}.");
        }
    }
}
