# ThumbnailAbTestService

Provides functionality to run A/B tests on video thumbnails to determine the more effective variant based on view metrics. Tracks variant performance, concludes tests, and exposes results for integration into content optimization workflows.

## API

### `ThumbnailAbTestService`

Initializes a new instance of the thumbnail A/B test service. Requires a video short ID and a collection of thumbnail variants to test.

### `async Task<(ThumbnailVariant VariantA, ThumbnailVariant VariantB)> CreateTestAsync()`

Creates a new A/B test by randomly selecting two distinct thumbnail variants from the provided list. Returns the selected variants as a tuple.

- **Returns**: A tuple containing `VariantA` and `VariantB`.
- **Throws**: `InvalidOperationException` if fewer than two variants are available.

### `async Task RecordViewEventAsync()`

Records a view event for the currently active A/B test. This event is used to track engagement and determine the winning variant.

- **Throws**: `InvalidOperationException` if no active test exists or if the test has already concluded.

### `async Task SyncAnalyticsAsync()`

Synchronously pulls the latest analytics data for the active A/B test from the analytics provider. Updates internal state with the most recent view and engagement metrics.

- **Throws**: `InvalidOperationException` if no active test exists.

### `async Task<ThumbnailVariant?> EvaluateAndConcludeAsync()`

Evaluates the performance of the active A/B test using accumulated analytics data. Determines the winning variant if sufficient data is available and concludes the test.

- **Returns**: The winning `ThumbnailVariant` if a conclusion is reached; otherwise, `null`.
- **Throws**: `InvalidOperationException` if no active test exists.

### `async Task<ThumbnailAbTestResult> GetTestResultAsync()`

Retrieves the current result of the A/B test, including the winning variant (if concluded), summary statistics, and conclusion status.

- **Returns**: A `ThumbnailAbTestResult` containing test outcome and metadata.
- **Throws**: `InvalidOperationException` if no test has been created or results are unavailable.

### `int VideoShortId`

Gets the unique identifier of the video short associated with this A/B test.

### `IReadOnlyList<ThumbnailVariant> Variants`

Gets the collection of thumbnail variants included in this A/B test. This list is read-only and reflects the variants provided at initialization.

### `string? WinnerLabel`

Gets the label of the winning thumbnail variant, if the test has concluded. `null` if the test is still active or inconclusive.

### `bool IsComplete`

Gets a value indicating whether the A/B test has concluded and a winner has been determined.

### `DateTime GeneratedAt`

Gets the timestamp when this A/B test was created.

### `string GetSummary()`

Generates a human-readable summary of the A/B test, including test ID, variants, conclusion status, and winner (if applicable).

- **Returns**: A formatted string containing test summary information.

## Usage

### Example 1: Creating and Running a Thumbnail A/B Test
