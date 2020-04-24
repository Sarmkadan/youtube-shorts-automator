# ContentCalendarController

The `ContentCalendarController` is an ASP.NET Core controller responsible for managing the scheduling, retrieval, modification, and optimization of content entries within the `youtube-shorts-automator` system. It provides the interface for interacting with the content calendar, enabling the automated management of YouTube Shorts publishing workflows.

## API

### Constructor
*   `public ContentCalendarController()`: Initializes a new instance of the controller.

### Methods
*   `public async Task<IActionResult> CreateEntry(...)`: Creates a new content entry in the calendar.
*   `public async Task<IActionResult> GetEntry(...)`: Retrieves a specific content entry by its unique identifier.
*   `public async Task<IActionResult> GetUpcoming(...)`: Retrieves a list of content entries scheduled for upcoming publication.
*   `public async Task<IActionResult> GetInRange(...)`: Retrieves content entries scheduled within a specified date and time range.
*   `public async Task<IActionResult> UpdateEntry(...)`: Updates the details of an existing content entry.
*   `public async Task<IActionResult> DeleteEntry(...)`: Deletes an existing content entry from the calendar.
*   `public async Task<IActionResult> OptimizeEntry(...)`: Triggers the optimization process for a specific content entry.
*   `public async Task<IActionResult> ApplyOptimization(...)`: Applies recommended optimizations to an existing content entry.
*   `public async Task<IActionResult> ScheduleEntry(...)`: Sets or updates the scheduled publication time for a specific entry.
*   `public async Task<IActionResult> GetRecommendedSlots(...)`: Returns recommended publication time slots for a specific YouTube channel over a defined number of days.

### Properties
*   `public required string Title`: The title of the content entry.
*   `public string? Description`: The description associated with the content entry.
*   `public string[]? Tags`: An array of tags for categorizing the content.
*   `public ContentCategory Category`: The category classification of the content.
*   `public DateTime ScheduledPublishAt`: The date and time when the content is scheduled to be published.
*   `public int YouTubeChannelId`: The identifier of the YouTube channel the content belongs to.
*   `public string? Notes`: Additional notes regarding the content entry.
*   `public string[]? Keywords`: An array of keywords used for content optimization.

## Usage

### Creating and Scheduling an Entry
```csharp
[HttpPost]
public async Task<IActionResult> CreateAndSchedule([FromBody] ContentEntryDto dto)
{
    var result = await _controller.CreateEntry(dto);
    if (result is CreatedAtActionResult createdResult)
    {
        var entryId = (int)createdResult.Value;
        await _controller.ScheduleEntry(entryId, DateTime.UtcNow.AddDays(2));
        return Ok(entryId);
    }
    return result;
}
```

### Optimizing an Upcoming Entry
```csharp
[HttpPost("{id}/optimize-now")]
public async Task<IActionResult> OptimizeUpcoming(int id)
{
    var optimizationResult = await _controller.OptimizeEntry(id);
    if (optimizationResult is OkObjectResult)
    {
        return await _controller.ApplyOptimization(id);
    }
    return optimizationResult;
}
```

## Notes

*   **Thread Safety:** This controller follows standard ASP.NET Core controller behavior and is intended to be used in a thread-safe manner within the scope of a single HTTP request. Avoid storing non-read-only state within the controller instance.
*   **Asynchronous Execution:** All data-accessing methods are `async` and return `Task<IActionResult>`. Callers should ensure they correctly `await` these calls to prevent blocking the request thread.
*   **Validation:** Input properties marked as `required` must be populated before invoking `CreateEntry` or `UpdateEntry` to avoid validation errors.
*   **Error Handling:** Methods returning `IActionResult` will typically return `NotFound` if an entry with the provided ID does not exist, or `BadRequest` if provided parameters fail validation. Ensure appropriate handling of these status codes in the calling code.
