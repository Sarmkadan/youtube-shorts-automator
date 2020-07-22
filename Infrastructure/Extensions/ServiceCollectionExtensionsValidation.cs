// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace YouTubeShortsAutomator.Infrastructure.Extensions;

/// <summary>
/// Validation helpers for <see cref="ServiceCollectionExtensions"/> extension methods
/// </summary>
public static class ServiceCollectionExtensionsValidation
{
    /// <summary>
    /// Validates configuration parameters for ServiceCollectionExtensions methods
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <param name="configuration">The configuration to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var problems = new List<string>();

        // Validate configuration is not null (already done by ArgumentNullException)
        // Validate connection strings exist where required
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            problems.Add("Configuration is missing required connection string 'DefaultConnection'");
        }

        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            // Redis is optional, but if provided it should be valid
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates configuration parameters for ServiceCollectionExtensions methods
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <param name="configuration">The configuration to validate</param>
    /// <returns>A list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);

        var problems = new List<string>();

        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            problems.Add($"Configuration is missing required connection string '{connectionStringName}'");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the ServiceCollectionExtensions configuration is valid
    /// </summary>
    /// <param name="services">The service collection to check</param>
    /// <param name="configuration">The configuration to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static bool IsValid(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var problems = services.Validate(configuration);
        return problems.Count == 0;
    }

    /// <summary>
    /// Determines whether the ServiceCollectionExtensions configuration is valid
    /// </summary>
    /// <param name="services">The service collection to check</param>
    /// <param name="configuration">The configuration to check</param>
    /// <param name="connectionStringName">The name of the connection string to validate</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="configuration"/>, or <paramref name="connectionStringName"/> is <see langword="null"/></exception>
    public static bool IsValid(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);

        var problems = services.Validate(configuration, connectionStringName);
        return problems.Count == 0;
    }

    /// <summary>
    /// Ensures that the ServiceCollectionExtensions configuration is valid
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <param name="configuration">The configuration to validate</param>
    /// <exception cref="ArgumentException">Configuration is invalid with detailed error messages</exception>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/></exception>
    public static void EnsureValid(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var problems = services.Validate(configuration);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ServiceCollectionExtensions configuration is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems),
                nameof(configuration));
        }
    }

    /// <summary>
    /// Ensures that the ServiceCollectionExtensions configuration is valid
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <param name="configuration">The configuration to validate</param>
    /// <param name="connectionStringName">The name of the connection string to validate</param>
    /// <exception cref="ArgumentException">Configuration is invalid with detailed error messages</exception>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="configuration"/>, or <paramref name="connectionStringName"/> is <see langword="null"/></exception>
    public static void EnsureValid(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(connectionStringName);

        var problems = services.Validate(configuration, connectionStringName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ServiceCollectionExtensions configuration is invalid:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems),
                nameof(configuration));
        }
    }
}