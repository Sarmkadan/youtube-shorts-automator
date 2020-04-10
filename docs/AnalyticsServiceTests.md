# AnalyticsServiceTests

Unit test class for the `AnalyticsService` component in the youtube-shorts-automator project. It verifies the behavior of analytics-related operations such as record creation, synchronization, retrieval, reporting, and growth calculations under various input conditions and error scenarios.

## API

### CreateAnalyticsRecordAsync_WithValidVideoId_CreatesRecord
- **Purpose:** Confirms that calling `CreateAnalyticsRecordAsync` with a valid video identifier results in a new analytics record being persisted.
- **Parameters:** `videoId` (string) – a non‑null, non‑empty YouTube video ID.
- **Return Value:** `Task` – completes when the operation finishes; no value is returned.
- **Throws:** Does not expect any exception; the test fails if an exception is thrown.

### CreateAnalyticsRecordAsync_WithRepositoryException_ThrowsInvalidOperationException
- **Purpose:** Ensures that an underlying repository failure propagates as an `InvalidOperationException` from the service method.
- **Parameters:** `videoId` (string) – any valid video ID used to trigger the repository call.
- **Return Value:** `Task` – completes when the exception is thrown.
- **Throws:** `InvalidOperationException` when the mocked repository throws an exception.

### SyncAnalyticsFromYouTubeAsync_WithValidInputs_UpdatesAnalytics
- **Purpose:** Verifies that synchronizing analytics from YouTube updates an existing record with the latest data.
- **Parameters:** `youtubeId` (string) – the YouTube video identifier; `channel` (Channel) – the channel object associated with the video.
- **Return Value:** `Task` – completes when the sync operation finishes.
- **Throws:** Does not expect any exception under normal conditions.

### SyncAnalyticsFromYouTubeAsync_WithNullYoutubeId_ThrowsArgumentNullException
- **Purpose:** Checks that a null YouTube ID results in an `ArgumentNullException`.
- **Parameters:** `youtubeId` (string) – explicitly null; `channel` (Channel) – a valid channel instance.
- **Return Value:** `Task` – completes when the exception is thrown.
- **Throws:** `ArgumentNullException` if `youtubeId` is null.

### SyncAnalyticsFromYouTubeAsync_WithNullChannel_ThrowsArgumentNullException
- **Purpose:** Checks that a null channel argument results in an `ArgumentNullException`.
- **Parameters:** `youtubeId` (string) – a valid video ID; `channel` (Channel) – explicitly null.
- **Return Value:** `Task` – completes when the exception is thrown.
- **Throws:** `ArgumentNullException` if `channel` is null.

### SyncAnalyticsFromYouTubeAsync_WhenNoExistingRecord_CreatesNew
- **Purpose:** Confirms that when no analytics record exists for the given video, a new record is created during synchronization.
- **Parameters:** `youtubeId` (string) – valid video ID; `channel` (Channel) – valid channel.
- **Return Value:** `Task` – completes when the sync operation finishes.
- **Throws:** Does not expect any exception.

### GetVideoAnalyticsAsync_WithValidVideoId_ReturnsAnalytics
- **Purpose:** Ensures that retrieving analytics for an existing video ID returns the corresponding analytics object.
- **Parameters:** `videoId` (string) – a valid, existing YouTube video ID.
- **Return Value:** `Task<Analytics>` – the analytics record associated with the video.
- **Throws:** Does not expect any exception.

### GetVideoAnalyticsAsync_WhenNotFound_ReturnsNull
- **Purpose:** Verifies that requesting analytics for a non‑existent video ID yields a null result.
- **Parameters:** `videoId` (string) – a video ID that does not exist in the data store.
- **Return Value:** `Task<Analytics>` – null when no record is found.
- **Throws:** Does not expect any exception.

### GetTopPerformingVideosAsync_WithValidLimit_ReturnsTopVideos
- **Purpose:** Checks that the method returns the top‑performing videos up to the specified limit.
- **Parameters:** `limit` (int) – a positive integer indicating the maximum number of videos to return.
- **Return Value:** `Task<IEnumerable<Video>>` – a sequence containing up to `limit` video objects ordered by performance.
- **Throws:** Does not expect any exception.

### GetTopPerformingVideosAsync_WithInvalidLimit_ThrowsArgumentOutOfRangeException
- **Purpose:** Ensures that an invalid (e.g., zero) limit triggers an `ArgumentOutOfRangeException`.
- **Parameters:** `limit` (int) – zero or another value considered invalid by the service.
- **Return Value:** `Task<IEnumerable<Video>>` – completes when the exception is thrown.
- **Throws:** `ArgumentOutOfRangeException` if `limit` is invalid.

### GetTopPerformingVideosAsync_WithNegativeLimit_ThrowsArgumentOutOfRangeException
- **Purpose:** Confirms that a negative limit results in an `ArgumentOutOfRangeException`.
- **Parameters:** `limit` (int) – a negative integer.
- **Return Value:** `Task<IEnumerable<Video>>` – completes when the exception is thrown.
- **Throws:** `ArgumentOutOfRangeException` if `limit` is negative.

### GeneratePeriodReportAsync_WithValidDateRange_ReturnsReport
- **Purpose:** Validates that a properly ordered date range produces a populated analytics report.
- **Parameters:** `startDate` (DateTime) – the beginning of the period; `endDate` (DateTime) – the end of the period, must be later than `startDate`.
- **Return Value:** `Task<AnalyticsReport>` – a report containing aggregated metrics for the period.
- **Throws:** Does not expect any exception.

### GeneratePeriodReportAsync_WithInvertedDates_ThrowsArgumentException
- **Purpose:** Ensures that supplying a start date after the end date throws an `ArgumentException`.
- **Parameters:** `startDate` (DateTime) – a date later than `endDate`; `endDate` (DateTime) – the earlier date.
- **Return Value:** `Task<AnalyticsReport>` – completes when the exception is thrown.
- **Throws:** `ArgumentException` if `startDate` > `endDate`.

### GeneratePeriodReportAsync_WithEmptyAnalytics_ReturnsEmptyReport
- **Purpose:** Confirms that when no analytics data exist for the requested period, an empty report is returned.
- **Parameters:** `startDate` (DateTime) – start of period; `endDate` (DateTime) – end of period.
- **Return Value:** `Task<AnalyticsReport>` – a report with zero or default values.
- **Throws:** Does not expect any exception.

### AnalyzePerformanceMetrics_WithValidData_ReturnsInsights
- **Purpose:** Checks that valid performance data yields a meaningful insights string.
- **Parameters:** `metrics` (IEnumerable<Analytics>) – a non‑empty collection of analytics records.
- **Return Value:** `string` – descriptive insights derived from the data.
- **Throws:** Does not expect any exception.

### AnalyzePerformanceMetrics_WithLowEngagement_ReturnsWarning
- **Purpose:** Verifies that low engagement metrics produce a warning‑style message.
- **Parameters:** `metrics` (IEnumerable<Analytics>) – data indicating low view counts, likes, or comments.
- **Return Value:** `string` – a warning message advising improvement.
- **Throws:** Does not expect any exception.

### AnalyzePerformanceMetrics_WithoutValidData_ReturnsDefaultMessage
- **Purpose:** Ensures that null or empty input results in a fallback default message.
- **Parameters:** `metrics` (IEnumerable<Analytics>) – null or empty collection.
- **Return Value:** `string` – a predefined default message indicating insufficient data.
- **Throws:** Does not expect any exception.

### CalculateChannelGrowthAsync_WithMultipleAnalytics_ReturnsTotalGrowth
- **Purpose:** Confirms that aggregating growth across several analytics records yields the correct total.
- **Parameters:** `analytics` (IEnumerable<Analytics>) – a set of analytics records for the channel.
- **Return Value:** `Task<long>` – the summed growth (e.g., subscriber increase) over the period.
- **Throws:** Does not expect any exception.

### CalculateChannelGrowthAsync_WithNegativeGrowth_ReturnsNegativeValue
- **Purpose:** Verifies that a net decrease in metrics is reported as a negative growth value.
- **Parameters:** `analytics` (IEnumerable<Analytics>) – data where later values are lower than earlier ones.
- **Return Value:** `Task<long>` – a negative number representing the decline.
- **Throws:** Does not expect any exception.

## Usage

The following examples demonstrate how to instantiate the test class and invoke its members in a test scenario. In practice, these methods are executed by a unit‑test runner, but they can be called directly for debugging or exploratory testing.

```csharp
using System.Threading.Tasks;
using YoutubeShortsAutomator.Tests; // namespace containing AnalyticsServiceTests
using YoutubeShortsAutomator.Models; // for Channel, Analytics, etc.

public class ExampleUsage
{
    public async Task RunTests()
    {
        var testInstance = new AnalyticsServiceTests();

        // Successful synchronization with valid inputs
        await testInstance.SyncAnalyticsFromYouTubeAsync_WithValidInputs_UpdatesAnalytics(
            youtubeId: "dQw4w9WgXcQ",
            channel: new Channel { Id = "UC_x5XG1OV2P6uZZ5FSM9Ttw", Title = "TestChannel" });

        // Expecting an exception when a null YouTube ID is supplied
        try
        {
            await testInstance.SyncAnalyticsFromYouTubeAsync_WithNullYoutubeId_ThrowsArgumentNullException(
                youtubeId: null,
                channel: new Channel { Id = "UC_test", Title = "Test" });
        }
        catch (System.ArgumentNullException ex)
        {
            // Expected outcome – test passes if exception is of this type
            System.Console.WriteLine($"Caught expected exception: {ex.Message}");
        }
    }
}
```

```csharp
using System.Threading.Tasks;
using YoutubeShortsAutomator.Tests;

public class ReportGenerationExample
{
    public async Task GenerateReport()
    {
        var tests = new AnalyticsServiceTests();

        // Valid date range – should return a populated report
        var report = await tests.GeneratePeriodReportAsync_WithValidDateRange_ReturnsReport(
            startDate: new System.DateTime(2024, 1, 1),
            endDate: new System.DateTime(2024, 1, 31));

        System.Console.WriteLine($"Report views: {report?.TotalViews}");

        // Inverted dates – should throw ArgumentException
        try
        {
            await tests.GeneratePeriodReportAsync_WithInvertedDates_ThrowsArgumentException(
                startDate: new System.DateTime(2024, 2, 1),
                endDate: new System.DateTime(2024, 1, 1));
        }
        catch (System.ArgumentException ex)
        {
            System.Console.WriteLine($"Expected exception caught: {ex.Message}");
        }
    }
}
```

## Notes

- The test class does not maintain any mutable state; each method relies solely on its parameters and any mocked dependencies supplied by the test framework. Consequently, instances are thread‑safe for concurrent execution by test runners.
- All asynchronous methods return `Task` or `Task<T>` and are intended to be awaited. Failure to await may result in unobserved exceptions or incomplete test validation.
- Argument validation tests (`*_ThrowsArgumentNullException`, `*_ThrowsArgumentOutOfRangeException`, `*_ThrowsArgumentException`) assume that the service under test validates inputs before any internal processing. If the service changes its validation order, these tests may need adjustment.
- Tests that expect no exception (`*_CreatesRecord`, `*_UpdatesAnalytics`, etc.) will fail if the underlying service throws; they serve as a guard against regressions that introduce unintended error paths.
- When using real implementations of repositories or external services, ensure that mocks or test doubles are configured to replicate the described behavior (e.g., throwing a repository exception to trigger the `InvalidOperationException` path). Otherwise, the tests may produce false positives or negatives.
