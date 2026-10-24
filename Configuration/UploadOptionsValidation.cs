// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Startup validation for <see cref="UploadOptions"/>. Collects every problem in the section before failing.
/// </summary>
public sealed class UploadOptionsValidation : IValidateOptions<UploadOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, UploadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.MaxRetries < ApplicationConstants.OptionLimits.MinUploadMaxRetries
            || options.MaxRetries > ApplicationConstants.OptionLimits.MaxUploadMaxRetries)
        {
            failures.Add(
                $"{UploadOptions.SectionName}:MaxRetries must be between " +
                $"{ApplicationConstants.OptionLimits.MinUploadMaxRetries} and " +
                $"{ApplicationConstants.OptionLimits.MaxUploadMaxRetries}, but was {options.MaxRetries}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
