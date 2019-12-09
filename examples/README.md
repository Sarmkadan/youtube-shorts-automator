// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Examples

Complete working examples demonstrating YouTube Shorts Automator features.

## Running Examples

All examples require the application to be running:

```bash
# Terminal 1: Start application
dotnet run

# Terminal 2: Compile and run example
cd examples
dotnet run --project 01-basic-upload.cs
```

Or compile to executable:

```bash
dotnet build -o ./bin
./bin/YouTubeShortsAutomator.Examples01 --api-key your-api-key
```

## Example 1: Basic Upload

**File**: `01-basic-upload.cs`

Simple workflow demonstrating:
- Creating a processing profile
- Uploading a video file
- Scheduling YouTube upload
- Monitoring upload progress

**Usage**:
```bash
dotnet run --project examples/01-basic-upload.cs
```

**Output**:
```
=== Basic YouTube Upload Example ===

Step 1: Creating processing profile...
✓ Profile created with ID: 1

Step 2: Uploading video file...
✓ Video uploaded with ID: 5

Step 3: Scheduling YouTube upload...
✓ Upload scheduled with Job ID: 10

Step 4: Monitoring upload progress...
  Status: Processing
  Status: Uploading
  Status: Uploaded

✓ Example completed successfully!
```

## Example 2: Batch Processing

**File**: `02-batch-processing.cs`

Parallel processing of multiple videos:
- Upload multiple videos concurrently
- Batch schedule uploads with time spacing
- Track all jobs simultaneously
- Monitor completion status

**Usage**:
```bash
# Requires video files: video1.mp4, video2.mp4, video3.mp4, video4.mp4
dotnet run --project examples/02-batch-processing.cs
```

**Key Features**:
- Processes 4 videos in parallel
- Schedules uploads 1 hour apart
- Real-time progress display
- Error handling per video

## Example 3: Analytics Report

**File**: `03-analytics-report.cs`

Generate performance reports:
- Retrieve overall metrics
- Analyze trending videos
- Export to CSV and JSON
- Performance comparison

**Usage**:
```bash
dotnet run --project examples/03-analytics-report.cs
```

**Output Files**:
- `analytics_report.csv` - Spreadsheet format
- `analytics_report.json` - Structured data format

**Metrics Included**:
- Total views, likes, comments, shares
- Average engagement rate
- Top performing videos
- Trending videos for last 7 days

## Example 4: Advanced Scheduling

**File**: `04-advanced-scheduling.cs`

Intelligent upload scheduling:
- Optimal time analysis
- Timezone-aware scheduling
- Recurring upload patterns
- Retry configuration

**Usage**:
```bash
dotnet run --project examples/04-advanced-scheduling.cs
```

**Features**:
- Suggests optimal upload times (8am, 12pm, 5pm)
- Schedules uploads in specific timezone
- Configures exponential backoff retry
- Displays schedule summary

## Example 5: Error Handling & Recovery

**File**: `05-error-handling.cs`

Robust error handling patterns:
- Catch and handle specific exceptions
- Implement retry strategies
- Graceful degradation
- Logging and error reporting

## Example 6: Custom Processing Pipeline

**File**: `06-custom-processing.cs`

Advanced video processing:
- Custom FFmpeg profiles
- Multiple processing steps
- Quality validation
- Output verification

## Example 7: Channel Management

**File**: `07-channel-management.cs`

Multi-channel operations:
- Register YouTube channels
- Set default channel
- Manage credentials
- Upload to specific channel

## Example 8: Webhook Integration

**File**: `08-webhook-integration.cs`

Event notifications:
- Setup webhook receiver
- Listen for completion events
- Process webhook payloads
- Webhook signature validation

## Running All Examples

```bash
# Build all examples
dotnet build examples/

# Run each example sequentially
for example in examples/*.cs; do
  echo "Running $example..."
  dotnet run --project "$example"
  echo ""
done
```

## Configuration

All examples use environment variable for API key:

```bash
# Set API key
export YOUTUBE_SHORTS_AUTOMATOR_API_KEY=your-api-key

# Or create .env file
echo "API_KEY=your-api-key" > examples/.env
```

## Example Structure

Each example follows this pattern:

```csharp
class ExampleName
{
    private const string BASE_URL = "http://localhost:5000/api";
    private const string API_KEY = "your-api-key-here";
    private readonly HttpClient _client;

    public ExampleName()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
    }

    public async Task RunAsync()
    {
        try
        {
            // Example implementation
            Console.WriteLine("Running example...");
            await Task.CompletedTask;
            Console.WriteLine("✓ Example completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task Main(string[] args)
    {
        var example = new ExampleName();
        await example.RunAsync();
    }
}
```

## Common Tasks

### Upload Local Video

See: `01-basic-upload.cs`

```csharp
var jobId = await UploadVideoAsync("myvideo.mp4", profileId);
```

### Schedule Multiple Videos

See: `02-batch-processing.cs`

```csharp
var jobIds = await Task.WhenAll(
    videoPaths.Select(v => UploadAndScheduleAsync(v))
);
```

### Get Performance Metrics

See: `03-analytics-report.cs`

```csharp
var metrics = await GetOverallMetricsAsync();
Console.WriteLine($"Total Views: {metrics.TotalViews}");
```

### Schedule with Retry Logic

See: `04-advanced-scheduling.cs`

```csharp
var retryConfig = new
{
    maxRetries = 3,
    initialDelaySeconds = 60,
    backoffMultiplier = 2.0
};
```

## Troubleshooting Examples

### Connection Refused

```
Error: Unable to connect to http://localhost:5000/api
```

**Solution**: Start application first
```bash
dotnet run
```

### Invalid API Key

```
Error: Unauthorized
X-API-Key: invalid
```

**Solution**: Set correct API key
```bash
export YOUTUBE_SHORTS_AUTOMATOR_API_KEY=your-valid-key
```

### File Not Found

```
Error: File not found: video.mp4
```

**Solution**: Ensure video file exists in current directory
```bash
ls -la *.mp4
```

## Best Practices

1. **Always use try-catch**: Handle network errors gracefully
2. **Set timeouts**: Prevent hanging on slow connections
3. **Log results**: Track successful and failed operations
4. **Validate input**: Check file existence before upload
5. **Monitor progress**: Provide user feedback for long operations
6. **Handle retries**: Implement exponential backoff
7. **Clean resources**: Dispose of HttpClient and streams

## Related Documentation

- [API Reference](../docs/api-reference.md)
- [Getting Started](../docs/getting-started.md)
- [Architecture](../docs/architecture.md)
- [FAQ](../docs/faq.md)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**
