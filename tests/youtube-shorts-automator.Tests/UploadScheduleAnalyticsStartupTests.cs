// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YouTubeShortsAutomator.Configuration;

namespace YouTubeShortsAutomator.Tests;

/// <summary>
/// Runs the <c>ValidateOnStart</c> path for the upload, schedule and analytics options through a real generic host,
/// so a bad value is reported by <c>StartAsync</c> rather than on first use.
/// </summary>
public class UploadScheduleAnalyticsStartupTests
{
    [Fact]
    public async Task Startup_ValidConfiguration_Starts()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            ["Upload:MaxRetries"] = "5",
            ["Schedule:CheckIntervalSeconds"] = "60",
            ["Analytics:PollingIntervalMinutes"] = "360"
        });

        var act = () => host.StartAsync();

        await act.Should().NotThrowAsync();
        await host.StopAsync();
    }

    [Fact]
    public async Task Startup_MissingSections_UsesValidDefaults()
    {
        using var host = BuildHost(new Dictionary<string, string?>());

        var act = () => host.StartAsync();

        await act.Should().NotThrowAsync();
        await host.StopAsync();
    }

    [Fact]
    public async Task Startup_UploadMaxRetriesOutOfRange_FailsWithSettingName()
    {
        using var host = BuildHost(new Dictionary<string, string?> { ["Upload:MaxRetries"] = "25" });

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*Upload:MaxRetries*");
    }

    [Fact]
    public async Task Startup_ScheduleIntervalOutOfRange_FailsWithSettingName()
    {
        using var host = BuildHost(new Dictionary<string, string?> { ["Schedule:CheckIntervalSeconds"] = "1" });

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*Schedule:CheckIntervalSeconds*");
    }

    [Fact]
    public async Task Startup_AnalyticsPollingBelowOneMinute_FailsWithSettingName()
    {
        using var host = BuildHost(new Dictionary<string, string?> { ["Analytics:PollingIntervalMinutes"] = "0" });

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*Analytics:PollingIntervalMinutes*");
    }

    private static IHost BuildHost(Dictionary<string, string?> settings) =>
        new HostBuilder()
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(settings))
            .ConfigureServices((context, services) => services.AddUploadScheduleAnalyticsOptions(context.Configuration))
            .Build();
}
