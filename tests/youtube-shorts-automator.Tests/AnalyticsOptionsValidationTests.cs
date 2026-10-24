// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortsAutomator.Configuration;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Tests for <see cref="AnalyticsOptionsValidation"/>, the startup check for <see cref="AnalyticsOptions"/>.
/// </summary>
public class AnalyticsOptionsValidationTests
{
    private readonly AnalyticsOptionsValidation _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var result = _validator.Validate(null, new AnalyticsOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1440)]
    public void Validate_PollingIntervalAtLeastOneMinute_Succeeds(int minutes)
    {
        var result = _validator.Validate(null, new AnalyticsOptions { PollingIntervalMinutes = minutes });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_PollingIntervalBelowOneMinute_FailsWithSettingName(int minutes)
    {
        var result = _validator.Validate(null, new AnalyticsOptions { PollingIntervalMinutes = minutes });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Analytics:PollingIntervalMinutes");
    }
}
