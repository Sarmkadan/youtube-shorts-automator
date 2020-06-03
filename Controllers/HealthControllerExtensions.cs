// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Controllers;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Extension methods for HealthController providing common health check operations and convenience methods
/// </summary>
public static class HealthControllerExtensions
{
    /// <summary>
    /// Executes multiple health checks in parallel and returns consolidated status
    /// </summary>
    /// <param name="controller">The HealthController instance</param>
    /// <param name="checks">Collection of health check endpoints to execute</param>
    /// <returns>IActionResult containing consolidated health status for all checks</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller or checks is null</exception>
    public static async Task<IActionResult> CheckHealthEndpointsAsync(
        this HealthController controller,
        IEnumerable<string> checks)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(checks);

        if (!checks.Any())
        {
            return controller.Ok(new { success = true, data = new { results = Array.Empty<object>() } });
        }

        var results = new Dictionary<string, object>();
        var errors = new List<string>();

        foreach (var check in checks)
        {
            try
            {
                IActionResult result = check.ToLowerInvariant() switch
                {
                    "status" or "getstatus" => await controller.GetStatus(),
                    "info" or "getinfo" => controller.GetSystemInfo(),
                    "ready" or "getreadiness" => await controller.GetReadiness(),
                    "live" or "getliveness" => controller.GetLiveness(),
                    _ => controller.BadRequest(new { error = $"Unknown health check endpoint: {check}" })
                };

                if (result is OkObjectResult okResult)
                {
                    results[check] = okResult.Value ?? new { success = true };
                }
                else if (result is StatusCodeResult statusCodeResult)
                {
                    results[check] = new { statusCode = statusCodeResult.StatusCode, success = false };
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{check}: {ex.Message}");
            }
        }

        return controller.Ok(new
        {
            success = errors.Count == 0,
            data = new
            {
                results,
                errors,
                totalChecks = checks.Count(),
                successfulChecks = results.Count,
                failedChecks = errors.Count
            }
        });
    }

    /// <summary>
    /// Executes all standard health check endpoints and returns comprehensive health report
    /// </summary>
    /// <param name="controller">The HealthController instance</param>
    /// <returns>IActionResult containing comprehensive health report</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller is null</exception>
    public static async Task<IActionResult> GetComprehensiveHealthReportAsync(
        this HealthController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var standardChecks = new[] { "status", "info", "ready", "live" };
        var results = new Dictionary<string, object>();

        foreach (var check in standardChecks)
        {
            try
            {
                IActionResult result = check.ToLowerInvariant() switch
                {
                    "status" => await controller.GetStatus(),
                    "info" => controller.GetSystemInfo(),
                    "ready" => await controller.GetReadiness(),
                    "live" => controller.GetLiveness(),
                    _ => controller.BadRequest(new { error = $"Unknown health check endpoint: {check}" })
                };

                if (result is OkObjectResult okResult)
                {
                    results[check] = okResult.Value ?? new { success = true };
                }
            }
            catch (Exception ex)
            {
                results[check] = new { error = ex.Message, success = false };
            }
        }

        var overallSuccess = results.Values
            .OfType<OkObjectResult>()
            .All(r => r.Value?.GetType().GetProperty("success")?.GetValue(r.Value) as bool? == true);

        return controller.Ok(new
        {
            success = overallSuccess,
            data = new
            {
                timestamp = DateTime.UtcNow,
                endpoints = results,
                totalEndpoints = results.Count,
                successfulEndpoints = results.Count(r => r.Value is OkObjectResult ok && ok.Value?.GetType().GetProperty("success")?.GetValue(ok.Value) as bool? == true),
                failedEndpoints = results.Count(r => r.Value is not OkObjectResult || (r.Value as OkObjectResult)?.Value?.GetType().GetProperty("success")?.GetValue((r.Value as OkObjectResult)?.Value) as bool? == false)
            }
        });
    }

    /// <summary>
    /// Checks health status with custom timeout for each endpoint
    /// </summary>
    /// <param name="controller">The HealthController instance</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds for each health check</param>
    /// <returns>IActionResult containing health status with timeout information</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeoutMilliseconds is less than 1</exception>
    public static async Task<IActionResult> GetStatusWithTimeoutAsync(
        this HealthController controller,
        int timeoutMilliseconds = 5000)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutMilliseconds, 1);

        using var cts = new CancellationTokenSource(timeoutMilliseconds);

        try
        {
            var statusTask = controller.GetStatus();
            var result = await statusTask.WaitAsync(cts.Token);

            return result;
        }
        catch (OperationCanceledException)
        {
            return controller.StatusCode(408, new
            {
                success = false,
                error = "Timeout waiting for health status",
                timeoutMilliseconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets system information with additional environment details
    /// </summary>
    /// <param name="controller">The HealthController instance</param>
    /// <param name="includeEnvironment">Whether to include detailed environment information</param>
    /// <param name="includeSystem">Whether to include system/runtime information</param>
    /// <returns>IActionResult containing enhanced system information</returns>
    /// <exception cref="ArgumentNullException">Thrown when controller is null</exception>
    public static IActionResult GetEnhancedSystemInfo(
        this HealthController controller,
        bool includeEnvironment = true,
        bool includeSystem = true)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var baseResult = controller.GetSystemInfo();

        if (baseResult is not OkObjectResult okResult)
        {
            return baseResult;
        }

        var systemInfo = okResult.Value as dynamic;
        var enhancedInfo = new Dictionary<string, object>();

        if (includeEnvironment)
        {
            enhancedInfo["environmentDetails"] = new
            {
                aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                osArchitecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture,
                processArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
                machineName = Environment.MachineName,
                userName = Environment.UserName,
                currentDirectory = Environment.CurrentDirectory,
                commandLine = Environment.CommandLine
            };
        }

        if (includeSystem)
        {
            enhancedInfo["systemDetails"] = new
            {
                processorCount = Environment.ProcessorCount,
                totalMemoryBytes = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64,
                workingSet = Environment.WorkingSet,
                systemPageSize = Environment.SystemPageSize,
                is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                is64BitProcess = Environment.Is64BitProcess,
                currentManagedThreadId = Environment.CurrentManagedThreadId,
                systemUptime = TimeSpan.FromMilliseconds(Environment.TickCount64)
            };
        }

        return controller.Ok(new
        {
            success = true,
            data = new
            {
                baseInfo = systemInfo,
                enhancedInfo,
                timestamp = DateTime.UtcNow
            }
        });
    }
}