// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Startup validation for <see cref="AnalyticsOptions"/>. Collects every problem in the section before failing.
/// </summary>
public sealed class AnalyticsOptionsValidation : IValidateOptions<AnalyticsOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AnalyticsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.PollingIntervalMinutes < ApplicationConstants.OptionLimits.MinAnalyticsPollingIntervalMinutes)
        {
            failures.Add(
                $"{AnalyticsOptions.SectionName}:PollingIntervalMinutes must be at least " +
                $"{ApplicationConstants.OptionLimits.MinAnalyticsPollingIntervalMinutes}, but was {options.PollingIntervalMinutes}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
