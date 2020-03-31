# ProcessingTask
The `ProcessingTask` type represents a single task in the processing pipeline of the youtube-shorts-automator project. It encapsulates the metadata and status of a task, including its type, priority, start and completion times, and any error messages that may have occurred during processing. This type is used to manage and track the progress of tasks as they are executed.

## API
The `ProcessingTask` type has the following public members:
* `Id`: A unique identifier for the task.
* `VideoShortId`: The identifier of the video short associated with this task.
* `TaskType`: A string indicating the type of task being performed.
* `Status`: The current status of the task, represented by a `ProcessingStatus` enum value.
* `Priority`: An integer indicating the priority of the task.
* `StartedAt`: The date and time at which the task was started.
* `CompletedAt`: The date and time at which the task was completed, or null if the task has not been completed.
* `ElapsedTime`: The time elapsed between the start and completion of the task, or null if the task has not been completed.
* `ErrorMessage`: An error message associated with the task, or null if no error occurred.
* `TaskLog`: A log of events that occurred during the task.
* `OutputWidth`, `OutputHeight`, `OutputBitrate`, `OutputFormat`: Properties describing the output of the task.
* `CreatedAt` and `UpdatedAt`: The dates and times at which the task was created and last updated, respectively.
* `VideoShort`: The video short associated with this task, or null if no video short is associated.
* `MarkAsStarted()`: Marks the task as started.
* `MarkAsCompleted()`: Marks the task as completed.
* `MarkAsFailed()`: Marks the task as failed.

## Usage
Here are two examples of using the `ProcessingTask` type:
```csharp
// Create a new task and mark it as started
var task = new ProcessingTask { TaskType = "VideoEncoding" };
task.MarkAsStarted();

// Log some events and mark the task as completed
task.TaskLog += "Encoding started\n";
task.TaskLog += "Encoding completed\n";
task.MarkAsCompleted();
```

```csharp
// Retrieve a task and check its status
var task = GetTaskFromDatabase(1);
if (task.Status == ProcessingStatus.Completed)
{
    Console.WriteLine("Task completed successfully");
}
else if (task.Status == ProcessingStatus.Failed)
{
    Console.WriteLine("Task failed with error: " + task.ErrorMessage);
}
```

## Notes
When using the `ProcessingTask` type, note that the `MarkAsStarted`, `MarkAsCompleted`, and `MarkAsFailed` methods do not throw exceptions, but instead update the task's status and log accordingly. Also, the `VideoShort` property may be null if no video short is associated with the task. The `ProcessingTask` type is not thread-safe, so care should be taken when accessing and modifying task instances from multiple threads. Additionally, the `ElapsedTime` property will be null if the task has not been completed, and the `CompletedAt` property will be null if the task has not been completed. The `OutputWidth`, `OutputHeight`, `OutputBitrate`, and `OutputFormat` properties describe the output of the task, but their values may not be set until the task is completed.
