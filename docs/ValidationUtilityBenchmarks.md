# ValidationUtilityBenchmarks
The `ValidationUtilityBenchmarks` type provides a set of methods for validating various types of input data, such as email addresses, URLs, and YouTube channel and video IDs. These methods can be used to test the performance and accuracy of validation logic in the `youtube-shorts-automator` project.

## API
The following methods are available:
* `ValidateEmail_Valid`: Validates a valid email address. Returns a tuple containing a boolean indicating whether the email is valid and an optional error message. Does not throw any exceptions.
* `ValidateEmail_Invalid`: Validates an invalid email address. Returns a tuple containing a boolean indicating whether the email is valid and an optional error message. Does not throw any exceptions.
* `ValidateUrl_Valid`: Validates a valid URL. Returns a tuple containing a boolean indicating whether the URL is valid and an optional error message. Does not throw any exceptions.
* `ValidateYouTubeChannelId_Valid`: Validates a valid YouTube channel ID. Returns a tuple containing a boolean indicating whether the channel ID is valid and an optional error message. Does not throw any exceptions.
* `ValidateYouTubeVideoId_Valid`: Validates a valid YouTube video ID. Returns a tuple containing a boolean indicating whether the video ID is valid and an optional error message. Does not throw any exceptions.

## Usage
Here are some examples of using the `ValidationUtilityBenchmarks` type:
```csharp
var (isValid, error) = ValidationUtilityBenchmarks.ValidateEmail_Valid();
if (isValid)
{
    Console.WriteLine("Email is valid");
}
else
{
    Console.WriteLine($"Email is invalid: {error}");
}

var (isUrlValid, urlError) = ValidationUtilityBenchmarks.ValidateUrl_Valid();
if (isUrlValid)
{
    Console.WriteLine("URL is valid");
}
else
{
    Console.WriteLine($"URL is invalid: {urlError}");
}
```

## Notes
When using the `ValidationUtilityBenchmarks` type, note that the `ValidateEmail_Invalid`, `ValidateUrl_Valid`, `ValidateYouTubeChannelId_Valid`, and `ValidateYouTubeVideoId_Valid` methods do not throw any exceptions, even if the input data is invalid. Instead, they return a tuple containing a boolean indicating whether the data is valid and an optional error message. Additionally, these methods are designed to be thread-safe, as they do not rely on any shared state or external resources. However, it is still important to ensure that any code using these methods is properly synchronized if necessary, to avoid any potential concurrency issues.
