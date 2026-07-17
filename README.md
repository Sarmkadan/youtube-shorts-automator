// ... (rest of README.md content remains unchanged)

## DomainException

`DomainException` is a base exception class for domain-specific exceptions in the application. It provides structured error handling with support for error codes, contextual data, and validation errors. This exception type serves as the foundation for domain validation and business rule violations, enabling consistent error reporting across the application.

### Members

- `ErrorCode` - Optional string error code for programmatic error handling
- `Context` - Dictionary for additional contextual data about the error
- `ValidationErrors` - List of validation error messages
- `AddContext` - Method to add contextual data to the exception

### Usage Examples

#### Basic DomainException

```csharp
try
{
    var videoValidator = new VideoValidator();
    var validationResult = videoValidator.Validate(video);
    
    if (!validationResult.IsValid)
    {
        var domainException = new DomainException("Video validation failed");
        foreach (var error in validationResult.Errors)
        {
            domainException.AddContext(error.PropertyName, error.ErrorMessage);
            domainException.ValidationErrors.Add(error.ErrorMessage);
        }
        
        throw domainException;
    }
}
catch (DomainException ex) when (ex.ValidationErrors.Any())
{
    Console.WriteLine($"Validation failed with {ex.ValidationErrors.Count} errors:");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

#### ProcessingJobException with Context

```csharp
try
{
    var job = await _jobRepository.GetByIdAsync(jobId);
    
    if (job == null)
    {
        throw new ProcessingJobException("Job not found", jobId);
    }
    
    if (job.Status == JobStatus.Failed)
    {
        var exception = new ProcessingJobException("Job processing failed", jobId);
        exception.AddContext("RetryCount", job.RetryCount);
        exception.AddContext("LastError", job.LastErrorMessage);
        exception.AddContext("FailedAt", job.FailedAtUtc?.ToString("o"));
        
        throw exception;
    }
}
catch (ProcessingJobException ex)
{
    _logger.LogError(ex, "Job processing failed: {JobId}", ex.JobId);
    await _notificationService.NotifyJobFailure(ex.JobId, ex.Message);
}
```

#### ApiException with HttpStatusCode and ApiResponse

```csharp
try
{
    var response = await _httpClient.GetAsync($"/api/videos/{videoId}");
    
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        var apiException = new ApiException(
            "YouTube API request failed",
            response.StatusCode,
            errorContent
        );
        
        apiException.AddContext("VideoId", videoId);
        apiException.AddContext("Endpoint", $"/api/videos/{videoId}");
        
        throw apiException;
    }
}
catch (ApiException ex) when (ex.HttpStatusCode == HttpStatusCode.TooManyRequests)
{
    _logger.LogWarning(ex, "API quota exceeded for video {VideoId}", ex.Context?["VideoId"]);
    await _quotaService.RecordQuotaUsage(ex.QuotaBytes, ex.CurrentUsageBytes);
}
```

#### QuotaExceededException

```csharp
try
{
    var uploadResult = await _storageService.UploadVideoAsync(videoFilePath);
    
    if (uploadResult.IsQuotaExceeded)
    {
        throw new QuotaExceededException(
            "Storage quota exceeded",
            uploadResult.CurrentUsageBytes,
            uploadResult.QuotaBytes
        );
    }
}
catch (QuotaExceededException ex)
{
    _logger.LogError(ex, "Storage quota exceeded: {CurrentUsage} of {Quota} bytes", 
        ex.CurrentUsageBytes, ex.QuotaBytes);
    
    var quotaPercentage = (double)ex.CurrentUsageBytes / ex.QuotaBytes * 100;
    await _notificationService.SendQuotaAlert(quotaPercentage);
}
```

#### CredentialException with ErrorCode

```csharp
try
{
    var credential = await _credentialRepository.GetByIdAsync(credentialId);
    
    if (credential == null || credential.IsExpired)
    {
        throw new CredentialException("Invalid or expired OAuth credentials", credentialId);
    }
}
catch (CredentialException ex) when (ex.ErrorCode == "CREDENTIAL_INVALID")
{
    _logger.LogError(ex, "Invalid credentials detected for credential ID {CredentialId}", ex.CredentialId);
    await _authService.RefreshCredentialsAsync(ex.CredentialId);
}
```

#### OAuthTokenExpiredException

```csharp
try
{
    var video = await _videoRepository.GetByIdAsync(videoId);
    var uploadResponse = await _youtubeApi.UploadVideoAsync(video);
}
catch (OAuthTokenExpiredException ex)
{
    _logger.LogWarning(ex, "OAuth token expired while uploading video {VideoId}", ex.VideoId);
    await _authService.RefreshAccessTokenAsync();
    // Retry the operation
    var retryResponse = await _youtubeApi.UploadVideoAsync(video);
}
```

## IHttpClientFactory

`IHttpClientFactory` is a factory interface for creating and managing instances of `HttpClient` with predefined configurations. It provides a centralized way to create HTTP clients for different services such as YouTube API, storage, webhooks, and analytics. This abstraction helps manage the lifecycle of HTTP clients and ensures consistent configurations across the application.

### Usage Example

```csharp
var httpClientFactory = new DefaultHttpClientFactory(configuration, logger);

var youTubeApiClient = httpClientFactory.CreateYouTubeApiClient();
var storageClient = httpClientFactory.CreateStorageClient();
var webhookClient = httpClientFactory.CreateWebhookClient();
var analyticsClient = httpClientFactory.CreateAnalyticsClient();

// Use the created HTTP clients for making requests
var youtubeResponse = await youTubeApiClient.GetAsync("https://api.youtube.com");
var storageResponse = await storageClient.GetAsync("https://storage.example.com");
var webhookResponse = await webhookClient.PostAsync("https://webhook.example.com", content);
var analyticsResponse = await analyticsClient.GetAsync("https://analytics.example.com");
```

## AnalyticsMetric

`AnalyticsMetric` represents YouTube Shorts performance metrics collected over a specific period. It tracks key engagement indicators such as views, likes, comments, shares, subscriber changes, and demographic breakdowns. The class provides methods for calculating engagement scores, determining performance trends, and analyzing audience demographics to help identify successful content patterns.

### Members

- `Id` - Unique identifier for the analytics record
- `VideoId` - Reference to the associated video
- `Video` - Navigation property to the Video entity
- `ViewCount` - Total number of views
- `LikeCount` - Total number of likes
- `CommentCount` - Total number of comments
- `ShareCount` - Total number of shares
- `SubscriberGainedCount` - Number of new subscribers gained
- `SubscriberLostCount` - Number of subscribers lost
- `AverageViewDurationSeconds` - Average duration viewers watched in seconds
- `EngagementRatePercent` - Engagement rate percentage
- `ClickThroughRatePercent` - Click-through rate percentage
- `Period` - Time period the metrics cover (Hourly, Daily, Weekly, Monthly, Cumulative)
- `CollectedAt` - When the metrics were collected
- `UpdatedAt` - When the metrics were last updated
- `TrafficSource` - Source of the traffic
- `DeviceType` - Type of device used
- `Demographics` - List of demographic metrics
- `CalculateEngagementScore()` - Calculates engagement score (0-100)
- `IsHighPerforming()` - Determines if metrics show strong performance

### Usage Examples

```csharp
// Create analytics metrics for a video
var analyticsMetric = new AnalyticsMetric
{
    Id = Guid.NewGuid(),
    VideoId = video.Id,
    ViewCount = 15000,
    LikeCount = 1200,
    CommentCount = 85,
    ShareCount = 245,
    SubscriberGainedCount = 450,
    SubscriberLostCount = 25,
    AverageViewDurationSeconds = 45.5,
    EngagementRatePercent = 9.2,
    ClickThroughRatePercent = 3.8,
    Period = MetricsPeriod.Daily,
    CollectedAt = DateTime.UtcNow,
    TrafficSource = "YouTube Search",
    DeviceType = "Mobile",
    Demographics = new List<DemographicMetric>
    {
        new DemographicMetric { AgeGroup = "18-24", Gender = "Male", ViewCount = 6500 },
        new DemographicMetric { AgeGroup = "25-34", Gender = "Female", ViewCount = 5200 },
        new DemographicMetric { AgeGroup = "18-24", Gender = "Female", ViewCount = 3300 }
    }
};

// Calculate engagement score
var engagementScore = analyticsMetric.CalculateEngagementScore();
Console.WriteLine($"Engagement score: {engagementScore:F2}");

// Check if high performing
var isHighPerforming = analyticsMetric.IsHighPerforming();
Console.WriteLine($"High performing: {isHighPerforming}");

// Calculate retention rate
var retentionRate = analyticsMetric.CalculateRetentionRate();
Console.WriteLine($"Retention rate: {retentionRate:F1}%");

// Get primary demographic
var primaryDemographic = analyticsMetric.GetPrimaryDemographic();
Console.WriteLine($"Primary demographic: {primaryDemographic?.AgeGroup} {primaryDemographic?.Gender}");
```

## IGoogleApiClient

`IGoogleApiClient` is an interface for interacting with the YouTube API, providing methods for uploading videos, retrieving video analytics, updating video metadata, and listing available YouTube channels. It abstracts YouTube-specific operations and provides strongly-typed models for video analytics data.

### Usage Example

```csharp
// Initialize the Google API client
var googleApiClient = new GoogleApiClient(httpClientFactory, logger, configuration);

// Upload a new video to YouTube
var uploadId = await googleApiClient.UploadVideoAsync(
    "./videos/my-short.mp4",
    new VideoMetadata
    {
        Title = "My Awesome Short Video",
        Description = "Check out this amazing short video!",
        Tags = new List<string> { "shorts", "funny", "entertainment" },
        CategoryId = "22" // Entertainment category
    }
);

if (uploadId != null)
{
    Console.WriteLine($"Video uploaded successfully with ID: {uploadId}");
    
    // Retrieve analytics for the uploaded video
    var analytics = await googleApiClient.GetVideoAnalyticsAsync(uploadId);
    
    if (analytics != null)
    {
        Console.WriteLine($"Video Analytics for {analytics.VideoId}:");
        Console.WriteLine($"  Views: {analytics.ViewCount}");
        Console.WriteLine($"  Likes: {analytics.LikeCount}");
        Console.WriteLine($"  Comments: {analytics.CommentCount}");
        Console.WriteLine($"  Shares: {analytics.ShareCount}");
        Console.WriteLine($"  Engagement Rate: {analytics.EngagementRate:P}");
        Console.WriteLine($"  Fetched at: {analytics.FetchedAtUtc:u}");
        
        // Update video metadata
        var updateSuccess = await googleApiClient.UpdateVideoMetadataAsync(
            uploadId,
            new VideoMetadata
            {
                Title = "My Awesome Short Video - Updated",
                Description = "Check out this amazing short video! Updated description.",
                Tags = new List<string> { "shorts", "funny", "entertainment", "updated" }
            }
        );
        
        Console.WriteLine($"Metadata update successful: {updateSuccess}");
    }
}

// List all available YouTube channels
var channels = await googleApiClient.ListChannelsAsync();
Console.WriteLine($"Available channels: {string.Join(", ", channels)}");
```

## ApiResult

`ApiResult` is a generic wrapper class for API operations that encapsulates success/failure state with optional data and error details. It provides a consistent way to handle API responses throughout the application, supporting both simple operations and operations that return data.

### Members

- `IsSuccess` - Indicates whether the operation was successful
- `Message` - Human-readable status message
- `ErrorCode` - Optional error code for programmatic error handling
- `Errors` - Dictionary of validation errors (when applicable)

- Static factory methods: `Success()`, `Failure()`, `ValidationFailure()`

### Usage Examples

#### Basic ApiResult (non-generic)

```csharp
// Successful operation
var successResult = ApiResult.Success("Video uploaded successfully");
Console.WriteLine($"Success: {successResult.IsSuccess}, Message: {successResult.Message}");

// Failed operation
var failureResult = ApiResult.Failure("Failed to process video", "VIDEO_PROCESSING_ERROR");
Console.WriteLine($"Success: {failureResult.IsSuccess}, Error: {failureResult.Message}, Code: {failureResult.ErrorCode}");

// Validation failure
var validationErrors = new Dictionary<string, string>
{
    { "title", "Title is required" },
    { "description", "Description must be at least 50 characters" }
};
var validationResult = ApiResult.ValidationFailure(validationErrors);
Console.WriteLine($"Validation failed with {validationResult.Errors?.Count} errors");
```

#### Generic ApiResult<T>

```csharp
// Successful operation with data
var videoData = new { Id = "abc123", Title = "My Short Video" };
var successWithData = ApiResult<VideoData>.Success(videoData, "Video retrieved successfully");

if (successWithData.IsSuccess)
{
    Console.WriteLine($"Video ID: {successWithData.Data?.Id}, Title: {successWithData.Data?.Title}");
}

// Failed operation with data type
var notFoundResult = ApiResult<VideoData>.NotFound("Video with ID xyz789 not found");
Console.WriteLine($"Not found: {notFoundResult.IsSuccess}, Message: {notFoundResult.Message}");

// Using Map to transform data
var mappedResult = successWithData.Map(data => data?.Title?.ToUpper());
Console.WriteLine($"Mapped title: {mappedResult}");

// Async mapping
var asyncMappedResult = await successWithData.MapAsync(async data => 
    (await Task.FromResult(data?.Id)) ?? "unknown");
Console.WriteLine($"Async mapped ID: {asyncMappedResult}");

// Conflict scenario
var conflictResult = ApiResult<VideoData>.Conflict("Video with this title already exists");
Console.WriteLine($"Conflict detected: {conflictResult.IsSuccess}, Code: {conflictResult.ErrorCode}");
```

## ApiCredential

`ApiCredential` represents OAuth and API credentials used for authenticating with external services like YouTube API. It stores client credentials, access tokens, refresh tokens, and metadata about token expiration and refresh attempts. The class provides methods for checking token validity, managing refresh cycles, and validating credential completeness.

### Members

- `Id` - Unique identifier for the credential
- `UserId` - Reference to the associated user
- `User` - Navigation property to the User entity
- `CredentialType` - Type of credential (Google OAuth, Google Service Account, YouTube API Key, Custom API Key)
- `ClientId` - OAuth client ID or API key identifier
- `ClientSecret` - OAuth client secret or API key secret
- `AccessToken` - Current access token for API authentication
- `RefreshToken` - Refresh token for obtaining new access tokens
- `AccessTokenExpiresAt` - When the access token expires
- `RefreshTokenExpiresAt` - When the refresh token expires (nullable)
- `CreatedAt` - When the credential was created
- `UpdatedAt` - When the credential was last updated
- `IsValid` - Whether the credential is currently valid
- `Scope` - OAuth scope string
- `Status` - Current credential status (Active, Inactive, Expired, RefreshFailed, Invalid, Revoked)
- `RefreshAttempts` - Number of refresh attempts made
- `IsAccessTokenExpired()` - Checks if access token has expired
- `IsRefreshTokenExpired()` - Checks if refresh token has expired
- `NeedsRefresh()` - Checks if credential needs refresh (30 minutes before expiration)
- `UpdateAccessToken()` - Updates access token with new values
- `RecordRefreshAttempt()` - Records a refresh attempt
- `MarkRefreshTokenExpired()` - Marks refresh token as expired
- `Invalidate()` - Invalidates the credential
- `Validate()` - Validates credential completeness

### Usage Examples

#### Creating and Using API Credentials

```csharp
// Create a new Google OAuth credential
var credential = new ApiCredential
{
  Id = Guid.NewGuid(),
  UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
  CredentialType = ApiCredentialType.GoogleOAuth,
  ClientId = "your-client-id.apps.googleusercontent.com",
  ClientSecret = "your-client-secret",
  AccessToken = "ya29.a0Ae4...",
  RefreshToken = "1//09j8...",
  AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
  RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
  Scope = "https://www.googleapis.com/auth/youtube.upload",
  Status = CredentialStatus.Active,
  IsValid = true,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

// Check if token needs refresh
if (credential.NeedsRefresh())
{
  Console.WriteLine("Access token will expire soon, refresh needed");
}

// Check token expiration
if (credential.IsAccessTokenExpired())
{
  Console.WriteLine("Access token has expired");
}

// Update access token after successful refresh
credential.UpdateAccessToken("new-access-token-xyz", DateTime.UtcNow.AddHours(1));

// Record refresh attempt
credential.RecordRefreshAttempt();

// Validate credential
var (isValid, validationErrors) = credential.Validate();
if (!isValid)
{
  Console.WriteLine("Credential validation failed:");
  foreach (var error in validationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}
```

#### Managing Service Account Credentials

```csharp
// Create a Google Service Account credential
var serviceAccountCredential = new ApiCredential
{
  Id = Guid.NewGuid(),
  UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
  CredentialType = ApiCredentialType.GoogleServiceAccount,
  ClientId = "service-account-id@project.iam.gserviceaccount.com",
  ClientSecret = "-----BEGIN PRIVATE KEY-----\n...service account key...\n-----END PRIVATE KEY-----",
  AccessToken = "ya29.c.b0AX...",
  AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
  Scope = "https://www.googleapis.com/auth/youtube.upload",
  Status = CredentialStatus.Active,
  IsValid = true,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

// Service accounts typically don't use refresh tokens
serviceAccountCredential.RefreshToken = null;

// Check if service account token is valid
if (serviceAccountCredential.IsValid && !serviceAccountCredential.IsAccessTokenExpired())
{
  Console.WriteLine("Service account token is valid and ready to use");
}
```

### Usage Example

```csharp
var processingJobRequest = new ProcessingJobRequest
{
    RequestId = Guid.NewGuid(),
    VideoFilePath = "./videos/my-short.mp4",
    Title = "My Awesome Short Video",
    Description = "Check out this amazing short video!",
    Tags = new[] { "shorts", "funny", "entertainment" },
    ProcessingProfile = "standard",
    Options = new ProcessingOptions
    {
        EnableWatermark = true,
        WatermarkImagePath = "./watermark.png",
        AutoGenerateThumbnail = true,
        OptimizeForMobile = true,
        MaxWidth = 1920,
        MaxHeight = 1080,
        BitrateKbps = 4000,
        EnableAudioNormalization = true
    },
    CreatedAtUtc = DateTime.UtcNow,
    RequestedBy = "John Doe"
};

Console.WriteLine($"Request ID: {processingJobRequest.RequestId}");
Console.WriteLine($"Video file path: {processingJobRequest.VideoFilePath}");
Console.WriteLine($"Title: {processingJobRequest.Title}");
Console.WriteLine($"Description: {processingJobRequest.Description}");
Console.WriteLine($"Tags: {string.Join(", ", processingJobRequest.Tags)}");
Console.WriteLine($"Processing profile: {processingJobRequest.ProcessingProfile}");
Console.WriteLine($"Enable watermark: {processingJobRequest.Options.EnableWatermark}");
Console.WriteLine($"Watermark image path: {processingJobRequest.Options.WatermarkImagePath}");
Console.WriteLine($"Auto generate thumbnail: {processingJobRequest.Options.AutoGenerateThumbnail}");
Console.WriteLine($"Optimize for mobile: {processingJobRequest.Options.OptimizeForMobile}");
Console.WriteLine($"Max width: {processingJobRequest.Options.MaxWidth}");
Console.WriteLine($"Max height: {processingJobRequest.Options.MaxHeight}");
Console.WriteLine($"Bitrate (kbps): {processingJobRequest.Options.BitrateKbps}");
Console.WriteLine($"Enable audio normalization: {processingJobRequest.Options.EnableAudioNormalization}");
Console.WriteLine($"Requested by: {processingJobRequest.RequestedBy}");
Console.WriteLine($"Created at UTC: {processingJobRequest.CreatedAtUtc}");
```
