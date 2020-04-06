# SchedulingException
The `SchedulingException` class represents an exception that occurs during the scheduling process in the youtube-shorts-automator project. It provides information about the scheduling error, including the scheduled time and job ID, allowing for more informed error handling and debugging.

## API
* `public DateTime? ScheduledTime`: Gets the scheduled time of the job that caused the exception, or `null` if not applicable.
* `public int? JobId`: Gets the ID of the job that caused the exception, or `null` if not applicable.
* `public SchedulingException(string message)`: Initializes a new instance of the `SchedulingException` class with a specified error message.
* `public SchedulingException()`: Initializes a new instance of the `SchedulingException` class with a default error message.
* `public SchedulingException()`: Initializes a new instance of the `SchedulingException` class (overload with no parameters, likely calling the base class constructor).

## Usage
The following examples demonstrate how to use the `SchedulingException` class:
```csharp
try
{
    // Schedule a job
    ScheduleJob(job);
}
catch (SchedulingException ex)
{
    Console.WriteLine($"Scheduling error: {ex.Message}");
    if (ex.ScheduledTime.HasValue)
    {
        Console.WriteLine($"Scheduled time: {ex.ScheduledTime.Value}");
    }
    if (ex.JobId.HasValue)
    {
        Console.WriteLine($"Job ID: {ex.JobId.Value}");
    }
}

try
{
    // Schedule a job with a specific time
    ScheduleJob(job, DateTime.Now.AddHours(1));
}
catch (SchedulingException ex)
{
    Console.WriteLine($"Scheduling error: {ex.Message}");
    if (ex.ScheduledTime.HasValue)
    {
        Console.WriteLine($"Scheduled time: {ex.ScheduledTime.Value}");
    }
    if (ex.JobId.HasValue)
    {
        Console.WriteLine($"Job ID: {ex.JobId.Value}");
    }
}
```

## Notes
When using the `SchedulingException` class, consider the following edge cases:
* If the `ScheduledTime` or `JobId` properties are `null`, it may indicate that the scheduling error occurred before the job was fully initialized.
* The `SchedulingException` class is designed to be thread-safe, as it only provides read-only access to its properties. However, the underlying scheduling process may still be subject to threading issues, depending on the implementation.
* When catching `SchedulingException` instances, be sure to check the `ScheduledTime` and `JobId` properties to retrieve relevant information about the scheduling error.
