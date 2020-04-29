# AnalyticsServiceExtensions

The `AnalyticsServiceExtensions` class provides a set of static asynchronous extension methods designed to augment the analytics capabilities within the `youtube-shorts-automator` project. These utilities facilitate high-level statistical computations and comparative analyses on video performance data, including engagement ratios, top-performing content identification, average view duration calculations, and direct performance comparisons between video assets. By operating asynchronously, these methods ensure non-blocking execution when interacting with underlying data repositories or external analytics APIs.

## API

### CalculateEngagementRatioAsync
Computes the engagement ratio for a specific set of video metrics, returning a decimal value representing the proportion of interactions relative to total impressions or views.
*   **Parameters**: Accepts the necessary metric entities (e.g., `VideoMetrics` or specific count parameters) required to derive the ratio.
*   **Return Value**: Returns a `Task<decimal>` containing the calculated ratio.
*   **Exceptions**: Throws an exception if the input data contains invalid states, such as negative interaction counts or a division-by-zero scenario where the denominator (views/impressions) is zero.

### GetTopPerformingVideosAsync
Retrieves a collection of video analytics data sorted by performance criteria, limited to the top performers within a specified dataset or time range.
*   **Parameters**: Requires filtering criteria such as a date range, minimum view threshold, or the count of items to retrieve.
*   **Return Value**: Returns a `Task<IEnumerable<AnalyticsData>>` containing the sequence of high-performing video records.
*   **Exceptions**: Throws if the data source is unreachable, the query parameters are malformed, or the underlying repository returns a null reference unexpectedly.

### CalculateAverageViewDurationHoursAsync
Aggregates view duration data across a provided collection of analytics records and computes the mean duration, converted into hours.
*   **Parameters**: Takes an enumerable collection of analytics records or a specific video identifier with an associated time window.
*   **Return Value**: Returns a `Task<double>` representing the average view duration in hours.
*   **Exceptions**: Throws if the provided collection is empty, preventing a valid average calculation, or if individual duration values are outside acceptable logical bounds.

### CompareVideoPerformanceAsync
Performs a comparative analysis between two distinct video entities, generating a textual summary or report detailing the performance delta.
*   **Parameters**: Requires identifiers or data objects for the two videos being compared, along with optional comparison metrics (e.g., CTR, retention).
*   **Return Value**: Returns a `Task<string>` containing the formatted comparison result.
*   **Exceptions**: Throws if either of the video entities cannot be resolved, if the videos are identical, or if insufficient data exists to perform a meaningful comparison.

## Usage

The following example demonstrates how to retrieve the top performing videos from the last 30 days and subsequently calculate their average engagement ratio.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using YouTubeShortsAutomator.Services;
using YouTubeShortsAutomator.Models;

public class PerformanceReportGenerator
{
    private readonly IAnalyticsRepository _repository;

    public PerformanceReportGenerator(IAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public async Task GenerateReportAsync()
    {
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-30);

        // Retrieve top 10 performing videos
        var topVideos = await AnalyticsServiceExtensions.GetTopPerformingVideosAsync(
            _repository, 
            startDate, 
            endDate, 
            count: 10
        );

        if (topVideos.Any())
        {
            // Calculate aggregate engagement for the top set
            var totalEngagement = await AnalyticsServiceExtensions.CalculateEngagementRatioAsync(
                topVideos.SelectMany(v => v.Metrics)
            );

            Console.WriteLine($"Top 10 Engagement Ratio: {totalEngagement:P2}");
        }
    }
}
```

This example illustrates comparing two specific video variants to determine which achieved better retention and outputting the analysis result.

```csharp
using System;
using System.Threading.Tasks;
using YouTubeShortsAutomator.Services;

public class VariantAnalyzer
{
    public async Task AnalyzeVariantsAsync(string videoIdA, string videoIdB)
    {
        try
        {
            // Compare performance between Variant A and Variant B
            string comparisonResult = await AnalyticsServiceExtensions.CompareVideoPerformanceAsync(
                videoIdA, 
                videoIdB, 
                includeRetention: true
            );

            Console.WriteLine("Performance Comparison:");
            Console.WriteLine(comparisonResult);

            // Calculate average view duration for the winner (assumed to be videoIdA for this snippet)
            double avgDurationHours = await AnalyticsServiceExtensions.CalculateAverageViewDurationHoursAsync(
                videoIdA
            );

            Console.WriteLine($"Winner Average View Duration: {avgDurationHours:F2} hours");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Analysis failed: {ex.Message}");
        }
    }
}
```

## Notes

*   **Thread Safety**: As this class consists entirely of static methods that do not maintain internal mutable state, it is inherently thread-safe. Multiple concurrent calls to these methods from different threads will not result in race conditions within the extension class itself.
*   **Asynchronous Execution**: All methods return `Task` objects and should be awaited to prevent blocking the calling thread. Failure to await these methods may result in unobserved exceptions or deadlocks, particularly in UI or ASP.NET contexts.
*   **Edge Cases**:
    *   `CalculateEngagementRatioAsync` and `CalculateAverageViewDurationHoursAsync` must handle empty datasets gracefully; callers should ensure collections are populated or be prepared to catch exceptions related to invalid mathematical operations (e.g., division by zero).
    *   `CompareVideoPerformanceAsync` requires two distinct, valid video identifiers. Passing null, empty strings, or identical identifiers for both parameters will result in an exception.
    *   `GetTopPerformingVideosAsync` may return an empty enumerable if no videos match the specified criteria within the given timeframe; this is not an exceptional state but a valid return value.
