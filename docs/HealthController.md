# HealthController

The `HealthController` provides essential endpoints for monitoring the operational status, readiness, and liveness of the `youtube-shorts-automator` service. It facilitates infrastructure management and diagnostic monitoring by providing insights into database connectivity, configuration validity, and system versioning, enabling service orchestrators to perform health probes and ensure reliable service availability.

## API

### Members

*   **`HealthController()`**: Initializes a new instance of the `HealthController` class.
*   **`async Task<IActionResult> GetStatus()`**: Performs a comprehensive health check, evaluating the overall system status, including database and configuration health. Returns an `OkObjectResult` containing detailed status information or an appropriate error response if the system is unhealthy.
*   **`IActionResult GetSystemInfo()`**: Synchronously retrieves static system information, including the current service version and the timestamp when the request was processed.
*   **`async Task<IActionResult> GetReadiness()`**: Asynchronously verifies if the service and its critical dependencies (e.g., database) are initialized and ready to handle incoming traffic.
*   **`IActionResult GetLiveness()`**: Synchronously verifies if the service process is responsive and running.
*   **`string Status`**: Gets or sets the overall health status string of the service.
*   **`string Database`**: Gets or sets the connectivity status string of the database dependency.
*   **`string Configuration`**: Gets or sets the health status string of the current service configuration.
*   **`List<string>? ConfigurationErrors`**: Gets or sets an optional list containing strings that detail any detected configuration errors.
*   **`DateTime Timestamp`**: Gets or sets the `DateTime` representing when the health information was captured.
*   **`string Version`**: Gets or sets the version string identifying the current release of the service.

## Usage

### Example 1: Invoking a health check from an integration test
```csharp
var controller = new HealthController();
var result = await controller.GetStatus() as OkObjectResult;

if (result?.Value != null)
{
    // Accessing the health data from the result
    var healthInfo = result.Value;
    Console.WriteLine($"System status: {healthInfo}");
}
```

### Example 2: Checking service readiness within a utility wrapper
```csharp
var controller = new HealthController();
var readinessResponse = await controller.GetReadiness();

if (readinessResponse is OkResult || readinessResponse is OkObjectResult)
{
    // Proceed with operations requiring a ready service
    Logger.Info("Service is ready to handle traffic.");
}
else
{
    Logger.Error("Service is not ready.");
}
```

## Notes

*   **Thread Safety**: ASP.NET Core controllers are typically instantiated with a scoped lifetime (created per request). While the controller instance is not shared across concurrent requests, developers should treat class properties as ephemeral to the context of the current request.
*   **Database Connectivity**: The `GetStatus` and `GetReadiness` methods perform I/O operations to verify database connectivity. If the database is unreachable, these methods may throw exceptions or return error-based `IActionResult` responses. Callers must implement appropriate error handling.
*   **Configuration Errors**: The `ConfigurationErrors` property is nullable. Always verify if the property is `null` before attempting to enumerate its contents to prevent a `NullReferenceException`.
