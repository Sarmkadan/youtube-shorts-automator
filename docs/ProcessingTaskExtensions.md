# ProcessingTaskExtensions
The `ProcessingTaskExtensions` class provides a set of extension methods for working with processing tasks in the youtube-shorts-automator project. These methods enable developers to easily determine the completion percentage, check the processing status, and adjust the priority of tasks, among other operations.

## API
* `public static int GetCompletionPercentage`: Returns the completion percentage of a processing task. This method does not take any parameters and returns an integer value between 0 and 100. It does not throw any exceptions.
* `public static bool IsProcessing`: Checks if a processing task is currently being processed. This method does not take any parameters and returns a boolean value indicating whether the task is being processed. It does not throw any exceptions.
* `public static bool IsTerminal`: Checks if a processing task has reached a terminal state. This method does not take any parameters and returns a boolean value indicating whether the task is terminal. It does not throw any exceptions.
* `public static string GetDurationDisplay`: Returns a string representation of the duration of a processing task. This method does not take any parameters and returns a string value. It does not throw any exceptions.
* `public static void AdjustPriority`: Adjusts the priority of a processing task. This method does not take any parameters and does not return any value. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ProcessingTaskExtensions` class:
```csharp
// Example 1: Checking the completion percentage of a task
var task = new ProcessingTask();
int completionPercentage = task.GetCompletionPercentage();
Console.WriteLine($"Task completion percentage: {completionPercentage}%");

// Example 2: Adjusting the priority of a task
var task2 = new ProcessingTask();
task2.AdjustPriority();
Console.WriteLine("Task priority adjusted");
```

## Notes
When using the `ProcessingTaskExtensions` class, note that the `GetCompletionPercentage` and `GetDurationDisplay` methods may return inaccurate results if the task is not properly initialized or if its state is not up-to-date. Additionally, the `IsProcessing` and `IsTerminal` methods may return incorrect results if the task is being processed concurrently by multiple threads. The `AdjustPriority` method may have unintended consequences if called concurrently by multiple threads. Therefore, it is recommended to use these methods in a thread-safe manner and to ensure that the task is properly initialized and updated before calling these methods.
