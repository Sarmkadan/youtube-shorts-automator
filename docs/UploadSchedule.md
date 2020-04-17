# UploadSchedule

The `UploadSchedule` class defines the configuration and lifecycle management for automated video uploads within the `youtube-shorts-automator` system. It encapsulates the temporal logic, user ownership, and execution state required to determine when and how often content should be processed for publication.

## API

### Properties

*   **`Id`** (`Guid`): Unique identifier for the schedule.
*   **`UserId`** (`Guid`): The identifier of the owner `User`.
*   **`User`** (`User?`): Navigation property for the owner `User`.
*   **`ScheduleName`** (`string`): A user-defined label for the schedule.
*   **`Frequency`** (`ScheduleFrequency`): Defines the cadence of the schedule (e.g., Daily, Weekly, Monthly).
*   **`DayOfWeek`** (`DayOfWeek?`): The target day of the week for weekly schedules. Nullable if not applicable.
*   **`DayOfMonth`** (`int?`): The target day of the month for monthly schedules. Nullable if not applicable.
*   **`ScheduledTime`** (`TimeOnly`): The time of day at which the schedule is intended to run.
*   **`TimeZoneId`** (`string`): IANA time zone identifier used for temporal calculations.
*   **`IsActive`** (`bool`): Indicates whether the schedule is currently enabled for execution.
*   **`CreatedAt`** (`DateTime`): UTC timestamp of schedule creation.
*   **`LastExecutedAt`** (`DateTime?`): UTC timestamp of the most recent execution.
*   **`NextScheduledTime`** (`DateTime?`): The calculated UTC timestamp for the next planned execution.
*   **`TotalExecutions`** (`int`): Counter tracking the number of successful executions.
*   **`ScheduledUploads`** (`List<ScheduledUpload>`): Collection of associated upload tasks.

### Methods

*   **`CalculateNextScheduledTime()`** (`DateTime`): Calculates and returns the next execution time based on the frequency, configured time zone, and the last execution state.
*   **`RecordExecution()`** (`void`): Updates `LastExecutedAt`, increments `TotalExecutions`, and triggers a recalculation of `NextScheduledTime`.
*   **`Validate()`** (`(bool IsValid, List<string> Errors)`): Performs configuration validation. Returns a tuple containing a validity boolean and a list of validation errors, if any.
*   **`IsDueForExecution()`** (`bool`): Returns `true` if the current system time warrants execution based on `NextScheduledTime` and the `IsActive` flag.
*   **`Deactivate()`** (`void`): Sets `IsActive` to `false` and resets `NextScheduledTime`.

## Usage

### Creating a Weekly Schedule
```csharp
var schedule = new UploadSchedule
{
    Id = Guid.NewGuid(),
    UserId = currentUserId,
    ScheduleName = "Weekly Vlog",
    Frequency = ScheduleFrequency.Weekly,
    DayOfWeek = DayOfWeek.Friday,
    ScheduledTime = new TimeOnly(18, 0), // 6:00 PM
    TimeZoneId = "America/New_York",
    IsActive = true
};

schedule.NextScheduledTime = schedule.CalculateNextScheduledTime();
```

### Checking and Executing a Schedule
```csharp
if (schedule.IsDueForExecution())
{
    var (isValid, errors) = schedule.Validate();
    
    if (isValid)
    {
        // Execute upload logic...
        schedule.RecordExecution();
    }
}
```

## Notes

*   **Time Zone Handling:** All calculations rely on the provided `TimeZoneId`. Ensure that valid IANA time zone identifiers are used. Incorrect identifiers may cause unexpected behavior during `CalculateNextScheduledTime`.
*   **Validation:** It is recommended to invoke `Validate()` before persisting a new schedule or after modifying configuration properties to ensure `Frequency`-specific requirements (e.g., `DayOfWeek` being set for Weekly schedules) are met.
*   **Thread Safety:** Instances of `UploadSchedule` are not thread-safe. Consumers must manage concurrent access to state-modifying methods (`RecordExecution`, `Deactivate`) if instances are shared across threads.
*   **Nullable Fields:** While `DayOfWeek` and `DayOfMonth` are nullable, logic dependent on `Frequency` must enforce non-null requirements for these fields based on the chosen schedule type.
