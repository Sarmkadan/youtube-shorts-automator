// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using YouTubeShortsAutomator.Configuration;
using YouTubeShortsAutomator.Domain.Constants;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Tests for <see cref="UploadOptionsValidation"/>, the startup check for <see cref="UploadOptions"/>.
/// </summary>
public class UploadOptionsValidationTests
{
    private readonly UploadOptionsValidation _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        var result = _validator.Validate(null, new UploadOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Validate_MaxRetriesAtBounds_Succeeds(int maxRetries)
    {
        var result = _validator.Validate(null, new UploadOptions { MaxRetries = maxRetries });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_MaxRetriesOutOfBounds_FailsWithSettingName(int maxRetries)
    {
        var result = _validator.Validate(null, new UploadOptions { MaxRetries = maxRetries });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("Upload:MaxRetries")
            .And.Contain(maxRetries.ToString());
    }

    [Fact]
    public void Validate_NullOptions_Throws()
    {
        var act = () => _validator.Validate(null, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
