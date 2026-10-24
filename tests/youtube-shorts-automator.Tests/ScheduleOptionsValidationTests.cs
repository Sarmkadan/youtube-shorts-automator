// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortsAutomator.Configuration;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Tests for <see cref="ScheduleOptionsValidation"/>, the startup check for <see cref="ScheduleOptions"/>.
/// </summary>
public class ScheduleOptionsValidationTests
{
    private readonly ScheduleOptionsValidation _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var result = _validator.Validate(null, new ScheduleOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(3600)]
    public void Validate_CheckIntervalAtBounds_Succeeds(int seconds)
    {
        var result = _validator.Validate(null, new ScheduleOptions { CheckIntervalSeconds = seconds });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(3601)]
    public void Validate_CheckIntervalOutOfBounds_FailsWithSettingName(int seconds)
    {
        var result = _validator.Validate(null, new ScheduleOptions { CheckIntervalSeconds = seconds });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Schedule:CheckIntervalSeconds");
    }
}
