// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YouTubeShortsAutomator.Configuration;

/// <summary>
/// Registers the upload, schedule and analytics options. Each section is validated with <c>ValidateOnStart</c>,
/// so a bad value stops the host during startup instead of failing on first use.
/// </summary>
public static class OptionsRegistrationExtensions
{
    /// <summary>
    /// Binds <see cref="UploadOptions"/>, <see cref="ScheduleOptions"/> and <see cref="AnalyticsOptions"/>
    /// from configuration and registers their validators.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Application configuration that holds the <c>Upload</c>, <c>Schedule</c>
    /// and <c>Analytics</c> sections.</param>
    public static IServiceCollection AddUploadScheduleAnalyticsOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<UploadOptions>()
            .Bind(configuration.GetSection(UploadOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<UploadOptions>, UploadOptionsValidation>();

        services.AddOptions<ScheduleOptions>()
            .Bind(configuration.GetSection(ScheduleOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ScheduleOptions>, ScheduleOptionsValidation>();

        services.AddOptions<AnalyticsOptions>()
            .Bind(configuration.GetSection(AnalyticsOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AnalyticsOptions>, AnalyticsOptionsValidation>();

        return services;
    }
}
