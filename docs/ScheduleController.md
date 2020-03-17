# ScheduleController

The `ScheduleController` is an ASP.NET Core MVC controller that handles CRUD operations and bulk import for video upload schedules in the YouTube Shorts Automator application. It exposes endpoints for creating, retrieving, updating, deleting, and importing schedules, and its public properties are used for model binding from request data.

## API

### ScheduleController()
- **Purpose:** Initializes a new instance of the controller. Dependencies (e.g., services, logging) are supplied via ASP.NET Core dependency injection when the framework creates the controller.
- **Parameters:** None.
- **Return value:** A ready‑to‑use `ScheduleController` instance.
- **Throws:** None under normal construction; any exception thrown by the dependency injection container will propagate upward.

### CreateScheduleAsync
- **Purpose:** Creates a new schedule for a video upload.
- **Parameters:** Receives a schedule creation payload (typically containing `VideoId`, `ScheduledUploadTimeUtc`, `RecurrencePattern`, and optional `TimeZone`) from the request body.
- **Return value:** 
  - `201 Created` with the newly created schedule details when successful.
  - `400 BadRequest` if the payload fails validation.
  - `500 InternalServerError` for unexpected server‑side errors.
- **Throws:** May throw `OperationCanceledException` if the client aborts the request; any exception from underlying services is propagated as a 500 response.

### GetScheduleAsync
- **Purpose:** Retrieves a single schedule by its identifier.
- **Parameters:** `Guid scheduleId` supplied via route or query string.
- **Return value:** 
  - `200 OK` with the schedule data when found.
  - `404 NotFound` if no schedule matches the supplied ID.
  - `400 BadRequest` if the ID is malformed.
  - `500 InternalServerError` on unexpected errors.
- **Throws:** Propagates `OperationCanceledException` on client cancellation; other exceptions become 500 responses.

### ListSchedulesAsync
- **Purpose:** Returns:** schedules, optionally filtered by query parameters (Description inferred from name) Returns a collection of schedules, possibly with paging or filtering.
- **Parameters:** Accepts optional filter/query string parameters (e.g., page size, continuation token) – exact parameters are not part of the public surface listed.
- **Return value:** 
  - `200 OK` with an array or paginated result of schedules.
  - `400 BadRequest` for invalid query parameters.
  - `500 InternalServerError` for server errors.
- **Throws:** Same as other async actions.

### UpdateScheduleAsync
- **Purpose:** Updates an existing schedule.
- **Parameters:** `Guid scheduleId` (route) and a schedule update payload (containing fields such as `ScheduledUploadTimeUtc`, `RecurrencePattern`, `TimeZone`, `Status`) from the request body.
- **Return value:** 
  - `200 OK` with the updated schedule when successful.
  - `400 BadRequest` if validation fails.
  - `404 NotFound` if the schedule does not exist.
  - `500 InternalServerError` on unexpected errors.
- **Throws:** Propagates cancellation and service exceptions as described above.

### DeleteScheduleAsync
- **Purpose:** Deletes a schedule.
- **Parameters:** `Guid scheduleId` from the route.
- **Return value:** 
  - `204 NoContent` on successful deletion.
  - `404 NotFound` if the schedule is not found.
  - `400 BadRequest` for an invalid ID.
  - `500 InternalServerError` for server errors.
- **Throws:** Same cancellation and error propagation semantics.

### ImportSchedulesFromCsvAsync
- **Purpose:** Imports multiple schedules from a CSV file uploaded by the client.
- **Parameters:** Accepts an `IFormFile` (or similar) containing CSV data; the exact parameter type is not listed but is implied by the method name.
- **Return value:** 
  - `200 OK` with a summary of imported/failed records.
  - `400 BadRequest` if the file is missing, malformed, or cannot be parsed.
  - `500 InternalServerError` for unexpected processing errors.
- **Throws:** Propagates `OperationCanceledException` and any exceptions from the CSV parsing/service layer.

### Public Properties (used for model binding)

| Property | Type | Description |
|----------|------|-------------|
| `VideoId` | `Guid` | Identifier of the video associated with the schedule. |
| `ScheduledUploadTimeUtc` | `DateTime` | Desired upload time in UTC (non‑nullable variant). |
| `ScheduledUploadTimeUtc` | `DateTime?` | Nullable variant allowing the time to be omitted. |
| `RecurrencePattern` | `string?` | Optional cron‑like or interval pattern for recurring uploads. |
| `TimeZone` | `string?` | Optional IANA time zone identifier for interpreting local times. |
| `ScheduleId` | `Guid` | Unique identifier of the schedule entity. |
| `Status` | `string` | Current state of the schedule (e.g., `Pending`, `Ready`, `Failed`). |

*Notes on properties:* These fields are populated by ASP.NET Core model binding from request data (body, route, or query). The presence of both nullable and non‑nullable variants for `ScheduledUploadTimeUtc` reflects separate binding scenarios (e.g., optional vs. required fields).

## Usage

### Example 1: Creating a schedule via the controller (unit‑test style)

```csharp
// Arrange
var controller = new ScheduleController(); // DI supplies dependencies in real app
var newSchedule = new
{
    VideoId = Guid.NewGuid(),
    ScheduledUploadTimeUtc = DateTime.UtcNow.AddHours(2),
    RecurrencePattern = "0 0 * * *", // daily at midnight
    TimeZone = "America/New_York"
};

// Act
var result = await controller.CreateScheduleAsync(newSchedule);

// Assert
var createdResult = result as CreatedResult;
Assert.NotNull(createdResult);
Assert.IsType<ScheduleDto>(createdResult.Value);
```

### Example 2: Listing schedules with optional filtering

```csharp
var controller = new ScheduleController();
// Simulate query parameters via controller properties or a custom request context
var result = await controller.ListSchedulesAsync(pageSize: 50, continuationToken: null);

var okResult = result as OkObjectResult;
Assert.NotNull(okResult);
var schedules = okResult.Value as IEnumerable<ScheduleDto>;
Assert.NotNull(schedules);
Assert.True(schedules.Count() <= 50);
```

## Notes

- **Model binding ambiguity:** The controller declares both `DateTime ScheduledUploadTimeUtc` and `DateTime? ScheduledUploadTimeUtc`. In practice, only one of these will be bound depending on whether the request supplies a value; developers should ensure that routing/action signatures do not create conflicting binding rules.
- **Thread safety:** Controllers are instantiated per request (or scoped) and do not store mutable state after construction. Therefore, they are safe for concurrent use across multiple requests.
- **Error handling:** All asynchronous actions propagate `OperationCanceledException` when the client aborts the request; other unexpected exceptions are translated into generic 500 responses by the ASP.NET Core middleware pipeline.
- **CSV import:** The `ImportSchedulesFromCsvAsync` method expects a well‑formed CSV with columns matching the schedule properties; malformed rows are reported in the response summary rather than causing a hard failure.
- **Dependency lifetime:** Services injected into the controller (e.g., a scheduling service) should be scoped or singleton as appropriate; the controller itself holds no state that would require disposal.
