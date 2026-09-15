# DependencyInjection

The `DependencyInjection` class provides extension methods for registering the application services, content calendar components, and logging used by the YouTube Shorts Automator application. All methods extend `IServiceCollection` and return the same collection so registrations can be chained.

## API

### `AddApplicationServices`

Registers the application's database context, repositories, and core services.

**Parameters:**
- `services` (`IServiceCollection`): The service collection to extend.
- `appSettings` (`AppSettings`): Application settings containing the database connection string.

**Returns:**
- `IServiceCollection`: The same service collection, allowing for method chaining.

**Throws:**
- `ArgumentNullException` if `services` or `appSettings` is `null`.

The method registers `DatabaseContext` as a singleton. It registers the video short, upload job, analytics, upload history, and thumbnail A/B test repositories as scoped services. It also registers `VideoProcessingService`, `YouTubeUploadService`, `SchedulingService`, `AnalyticsService`, `ThumbnailAbTestService`, `IThumbnailGeneratorService`, and `JobOrchestrationService` as scoped services.

---

### `AddContentCalendar`

Registers the content calendar repository, title optimization engine, and content calendar service. The optional configuration delegate can customize the `ContentCalendarOptions` instance before it is registered.

**Parameters:**
- `services` (`IServiceCollection`): The service collection to extend.
- `configure` (`Action<ContentCalendarOptions>?`, optional): A delegate that overrides the default content calendar options.

**Returns:**
- `IServiceCollection`: The same service collection, allowing for method chaining.

**Throws:**
- `ArgumentNullException` if `services` is `null`.

The configured `ContentCalendarOptions` instance is registered as a singleton. `ContentCalendarRepository`, `ITitleOptimizationEngine`, and `IContentCalendarService` are registered as scoped services. These registrations depend on services supplied by `AddApplicationServices`, including `DatabaseContext`, `AnalyticsService`, `SchedulingService`, and `VideoShortRepository`.

---

### `AddApplicationLogging`

Adds console logging and sets the minimum log level to `Information`.

**Parameters:**
- `services` (`IServiceCollection`): The service collection to extend.
- `appSettings` (`AppSettings`): The application settings instance. The method validates this argument but does not read any settings from it.

**Returns:**
- `IServiceCollection`: The same service collection, allowing for method chaining.

**Throws:**
- `ArgumentNullException` if `services` or `appSettings` is `null`.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using YouTubeShortAutomator.Configuration;

var services = new ServiceCollection();
var appSettings = new AppSettings
{
    ConnectionString = "Data Source=youtube-shorts.db"
};

services
    .AddApplicationServices(appSettings)
    .AddContentCalendar(options =>
    {
        // Customize content calendar options here.
    })
    .AddApplicationLogging(appSettings);

using var serviceProvider = services.BuildServiceProvider();
```

## Notes

- Call `AddApplicationServices` before resolving any of the registered application or content calendar services.
- `AddContentCalendar` may be omitted when content calendar functionality is not required.
- Services registered with scoped lifetimes should be resolved from an `IServiceScope` in long-running or non-web applications.
- Logging dependencies used by the registered services are provided after `AddApplicationLogging` configures the logging pipeline.
