// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Startup validation for <see cref="ScheduleOptions"/>. Collects every problem in the section before failing.
/// </summary>
public sealed class ScheduleOptionsValidation : IValidateOptions<ScheduleOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ScheduleOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.CheckIntervalSeconds < ApplicationConstants.OptionLimits.MinScheduleCheckIntervalSeconds
            || options.CheckIntervalSeconds > ApplicationConstants.OptionLimits.MaxScheduleCheckIntervalSeconds)
        {
            failures.Add(
                $"{ScheduleOptions.SectionName}:CheckIntervalSeconds must be between " +
                $"{ApplicationConstants.OptionLimits.MinScheduleCheckIntervalSeconds} and " +
                $"{ApplicationConstants.OptionLimits.MaxScheduleCheckIntervalSeconds}, but was {options.CheckIntervalSeconds}.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
