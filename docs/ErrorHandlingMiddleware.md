# ErrorHandlingMiddleware

`ErrorHandlingMiddleware` is a specialized component in the `youtube-shorts-automator` request pipeline designed to intercept unhandled exceptions, ensure consistent error logging, and provide standardized, machine-readable responses to clients. By encapsulating exception handling logic within this middleware, the application maintains a uniform error response contract, improving client-side error handling while ensuring internal server errors are not inadvertently exposed to end users.

## API

### Constructors

- `public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)`
  Initializes a new instance of the `ErrorHandlingMiddleware` class.

### Methods

- `public async Task InvokeAsync(HttpContext context)`
  Processes the incoming HTTP request. This method wraps the execution of the next delegate in the pipeline within a try-catch block. If an unhandled exception occurs, it logs the error and writes a standardized error response to the `HttpContext.Response`.

### Properties

- `public string Message`
  Gets or sets the human-readable description of the error encountered.

- `public string ErrorCode`
  Gets or sets a unique, machine-readable identifier representing the specific type of error (e.g., "INVALID_INPUT", "SERVICE_UNAVAILABLE").

- `public string? Details`
  Gets or sets additional, optional technical context or diagnostic information associated with the error. May be null if no further details are available or if masking internal details is required.

- `public DateTime Timestamp`
  Gets or sets the UTC date and time when the error occurred.

## Usage

### Registering the Middleware

The middleware should be added to the ASP.NET Core request pipeline during application startup, typically in `Program.cs`.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register the error handling middleware early in the pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
app.Run();
```

### Implementing a Controller Action that Triggers an Exception

When an exception occurs during the execution of a controller, `ErrorHandlingMiddleware` catches it and formats the response using the defined properties.

```csharp
[ApiController]
[Route("[controller]")]
public class VideoController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetVideo(string id)
    {
        // This will be caught by ErrorHandlingMiddleware if it fails
        var video = _videoService.GetById(id) 
            ?? throw new KeyNotFoundException($"Video with id {id} not found.");
        
        return Ok(video);
    }
}
```

## Notes

- **Thread Safety**: The `ErrorHandlingMiddleware` class is designed to be thread-safe. As middleware is typically registered as a singleton in the ASP.NET Core dependency injection container, the `InvokeAsync` method must ensure that request-specific data is accessed only via the provided `HttpContext`, and no state is stored in instance fields.
- **Error Masking**: While the `Details` property can hold technical information, implementation should ensure that sensitive information (such as database connection strings or internal file paths) is not included in the `Details` property when running in production environments.
- **Performance**: Placing this middleware early in the pipeline is critical to ensure that exceptions thrown by subsequent middleware or framework components are captured effectively.
