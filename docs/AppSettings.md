# AppSettings
The `AppSettings` type is a configuration class designed to hold various settings and parameters used throughout the `youtube-shorts-automator` project. It provides a centralized location for accessing and modifying application-wide settings, including database connections, file directories, upload and processing limits, analytics synchronization, and YouTube API credentials.

## API
The `AppSettings` type exposes the following public members:
* `ConnectionString`: A string representing the connection string to the database.
* `DatabasePath`: A string representing the path to the database file.
* `LogDirectory`: A string representing the directory where log files are stored.
* `ProcessingDirectory`: A string representing the directory where processing files are stored.
* `OutputDirectory`: A string representing the directory where output files are stored.
* `MaxConcurrentUploads`: An integer representing the maximum number of concurrent uploads allowed.
* `MaxConcurrentProcessing`: An integer representing the maximum number of concurrent processing tasks allowed.
* `DefaultRetryCount`: An integer representing the default number of retries for failed operations.
* `UploadTimeoutSeconds`: An integer representing the timeout in seconds for upload operations.
* `ProcessingQueueLimit`: An integer representing the maximum number of items allowed in the processing queue.
* `EnableAnalyticsSyncing`: A boolean indicating whether analytics synchronization is enabled.
* `AnalyticsSyncIntervalHours`: An integer representing the interval in hours between analytics synchronization operations.
* `YouTubeApiKey`: A string representing the YouTube API key.
* `YouTubeClientId`: A string representing the YouTube client ID.
* `YouTubeClientSecret`: A string representing the YouTube client secret.
* `ScheduleCheckIntervalSeconds`: An integer representing the interval in seconds between schedule checks.
* `EnableWatermark`: A boolean indicating whether watermarking is enabled.
* `WatermarkImagePath`: A nullable string representing the path to the watermark image file.

## Usage
Here are two examples of using the `AppSettings` type in C#:
```csharp
// Example 1: Accessing settings
AppSettings settings = new AppSettings();
Console.WriteLine(settings.ConnectionString);
Console.WriteLine(settings.MaxConcurrentUploads);

// Example 2: Modifying settings
AppSettings modifiedSettings = new AppSettings();
modifiedSettings.EnableAnalyticsSyncing = true;
modifiedSettings.AnalyticsSyncIntervalHours = 2;
Console.WriteLine(modifiedSettings.EnableAnalyticsSyncing);
Console.WriteLine(modifiedSettings.AnalyticsSyncIntervalHours);
```

## Notes
When using the `AppSettings` type, note that some settings may have default values or validation rules applied. For example, `MaxConcurrentUploads` and `MaxConcurrentProcessing` should be positive integers, while `EnableAnalyticsSyncing` and `EnableWatermark` are boolean flags. Additionally, `WatermarkImagePath` is a nullable string, which means it can be null if no watermark image is specified. The `AppSettings` type is designed to be thread-safe, but it is still important to ensure that modifications to settings are properly synchronized in multi-threaded environments.
