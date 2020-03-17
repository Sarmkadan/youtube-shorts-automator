# SystemController

The `SystemController` class provides HTTP endpoints that expose runtime health, version, and diagnostic information for the YouTube Shorts Automator service. It is intended to be used by monitoring tools, orchestration platforms, or clients that need to verify service availability and configuration.

## API

### GetHealthAsync
- **Purpose**: Returns the overall health status of the application, including individual check results.
- **Parameters**: None.
- **Return Value**: `Task<IActionResult>` that resolves to a JSON payload containing a `Status` string, a `Timestamp`, and a `Checks` dictionary mapping check names to `HealthCheckDetail` objects.
- **Throws**: May throw an `InvalidOperationException` if internal health check components fail to initialize; otherwise, returns a 500 status code wrapped in the `IActionResult`.

### GetVersionAsync
- **Purpose**: Retrieves the current version information of the service.
- **Parameters**: None.
- **Return Value**: `IActionResult` containing a JSON object with `Version`, `ApiVersion`, `AppVersion`, `BuildDate`, `GitCommit`, and `SupportedApiVersions`.
- **Throws**: No exceptions are expected under normal operation; unexpected errors result in a 500 response.

### GetSystemInfoAsync
- **Purpose**: Provides details about the runtime environment.
- **Parameters**: None.
- **Return Value**: `IActionResult` with JSON fields `OperatingSystem`, `DotNetVersion`, `Environment`, and `Timestamp`.
- **Throws**: Does not throw; failures are captured and returned as a 500 response.

### GetFeaturesAsync
- **Purpose**: Lists feature flags or capabilities that are currently enabled.
- **Parameters**: None.
- **Return Value**: `IActionResult` containing a JSON representation of feature status (implementation‑specific).
- **Throws**: No specific exceptions; unexpected issues yield a 500 response.

### Status (string)
- **Purpose**: Indicates the overall health status (e.g., "Healthy", "Degraded", "Unhealthy").
- **Parameters**: None.
- **Return Value**: The current status string.
- **Throws**: N/A (property accessor).

### Version (string)
- **Purpose**: The semantic version of the application (e.g., "1.2.3").
- **Parameters**: None.
- **Return Value**: Version string.
- **Throws**: N/A.

### Environment (string)
- **Purpose**: Name of the deployment environment (e.g., "Production", "Staging", "Development").
- **Parameters**: None.
- **Return Value**: Environment string.
- **Throws**: N/A.

### Timestamp (DateTime)
- **Purpose**: UTC timestamp when the controller was instantiated or when the health check was last performed.
- **Parameters**: None.
- **Return Value**: DateTime value.
- **Throws**: N/A.

### Checks (Dictionary<string, HealthCheckDetail>)
- **Purpose**: Mapping of individual health check names to their detailed results.
- **Parameters**: None.
- **Return Value**: Dictionary where each key is a check identifier and each value contains status, description, and optional metrics.
- **Throws**: N/A.

### ResponseTime (string)
- **Purpose**: Human‑readable representation of the average response time for recent requests.
- **Parameters**: None.
- **Return Value**: Formatted time span (e.g., "45 ms").
- **Throws**: N/A.

### ApiVersion (string)
- **Purpose**: The version of the API contract exposed by the controller.
- **Parameters**: None.
- **Return Value**: API version string.
- **Throws**: N/A.

### AppVersion (string)
- **Purpose**: The internal application version (may differ from `Version` if using separate versioning schemes).
- **Parameters**: None.
- **Return Value**: Application version string.
- **Throws**: N/A.

### BuildDate (string)
- **Purpose**: Date and time when the binary was built.
- **Parameters**: None.
- **Return Value**: Build date string (ISO 8601 format preferred).
- **Throws**: N/A.

### GitCommit (string)
- **Purpose**: Short SHA of the Git commit that produced the build.
- **Parameters**: None.
- **Return Value**: Git commit hash.
- **Throws**: N/A.

### SupportedApiVersions (string[])
- **Purpose**: List of API versions that the service currently supports.
- **Parameters**: None.
- **Return Value**: Array of version strings.
- **Throws**: N/A.

### Deprecated (bool)
- **Purpose**: Flag indicating whether this controller or its API is deprecated.
- **Parameters**: None.
- **Return Value**: `true` if deprecated; otherwise `false`.
- **Throws**: N/A.

### OperatingSystem (string)
- **Purpose**: Description of the host operating system (e.g., "Microsoft Windows 10.0.19044").
- **Parameters**: None.
- **Return Value**: OS string.
- **Throws**: N/A.

### DotNetVersion (string)
- **Purpose**: Version of the .NET runtime on which the service is running.
- **Parameters**: None.
- **Return Value**: .NET version string.
- **Throws**: N/A.

## Usage

```csharp
// Example 1: Checking health via dependency injection in an ASP.NET Core controller
public class MonitoringController : ControllerBase
{
    private readonly SystemController _systemController;

    public MonitoringController(SystemController systemController)
    {
        _systemController = systemController;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        // GetHealthAsync returns a Task<IActionResult>; await it directly.
        return await _systemController.GetHealthAsync();
    }
}
```

```csharp
// Example 2: Retrieving version info from a console application
using (var scope = serviceProvider.CreateScope())
{
    var systemCtrl = scope.ServiceProvider.GetRequiredService<SystemController>();
    var versionResult = systemCtrl.GetVersionAsync().Result; // synchronous for demo

    if (versionResult is OkObjectResult ok && ok.Value is var versionInfo)
    {
        Console.WriteLine($"Service version: {versionInfo.Version}");
        Console.WriteLine($"Built on: {versionInfo.BuildDate} from commit {versionInfo.GitCommit}");
    }
}
```

## Notes

- The controller does not maintain mutable state; all property values are derived from read‑only fields or configuration, making it safe for concurrent access by multiple request threads.
- `GetHealthAsync` may perform lightweight diagnostics (e.g., checking database connectivity, external service reachability). If any of these checks block for an extended period, the method’s completion time will increase, but it will not throw unless a check throws an unexpected exception.
- Property getters are expected to be O(1) and side‑effect free; they rely on values set during controller construction or from static environment reads, thus they are thread‑safe without additional synchronization.
- The `Checks` dictionary is populated each time `GetHealthAsync` is invoked; callers should not cache the reference across requests if they require up‑to‑date check results.
- If the service is started in an environment where certain diagnostic components are unavailable (e.g., no database), the corresponding check will report a failure status rather than causing the endpoint to fail entirely.
