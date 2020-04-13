# AnalyticsDataTests

`AnalyticsDataTests` contains unit tests that verify the correctness of analytics-related operations for YouTube Shorts. It exercises the computation of engagement metrics, the classification of performance levels, the clamping of retention data, and the retrieval of video shorts through a mocked repository. The class ensures that the underlying analytics logic behaves predictably under known inputs and edge conditions.

## API

### `public void RecalculateEngagementMetrics_WithKnownInteractionCounts_ComputesExpectedRate`

Verifies that the engagement recalculation logic produces the expected rate when supplied with a predetermined set of interaction counts (e.g., likes, comments, shares, views). The test asserts that the computed rate matches a pre-calculated value derived from the same inputs.

- **Parameters:** None (parameterless test method).
- **Return value:** `void`.
- **Throws:** Assertion failures if the computed engagement rate deviates from the expected value.

### `public void GetPerformanceLevel_WhenEngagementRateIsAboveTen_ReturnsExcellent`

Confirms that the performance-level classifier returns the `Excellent` designation when the engagement rate exceeds the 10% threshold. The test feeds a rate strictly greater than 10 and checks the resulting level.

- **Parameters:** None.
- **Return value:** `void`.
- **Throws:** Assertion failures if the returned performance level is not `Excellent`.

### `public void UpdateRetentionData_WhenRetentionExceedsHundred_ClampsAtMaximum`

Ensures that retention data is clamped to the maximum allowable value (100) when an input retention percentage greater than 100 is supplied. The test passes an out-of-range value and asserts that the stored or returned retention is exactly 100.

- **Parameters:** None.
- **Return value:** `void`.
- **Throws:** Assertion failures if the clamped value is not 100.

### `public async Task GetAllAsync_WhenCalledOnMockedRepository_ReturnsMockedVideoShorts`

Validates that the asynchronous `GetAllAsync` method, when invoked against a mocked repository, returns the collection of video shorts that the mock was configured to provide. The test awaits the call and asserts that the result set matches the expected mocked data in both count and content.

- **Parameters:** None.
- **Return value:** `Task` (asynchronous test method).
- **Throws:** Assertion failures if the returned collection is null, empty, or differs from the mocked setup.

## Usage

```csharp
// Example 1: Running the engagement metrics test inside a test suite
[Test]
public void Engagement_WithKnownInputs_ShouldPass()
{
    var sut = new AnalyticsDataTests();
    
    // This test method asserts internally; no explicit Act/Assert needed here
    sut.RecalculateEngagementMetrics_WithKnownInteractionCounts_ComputesExpectedRate();
}
```

```csharp
// Example 2: Running the async repository test with a test runner that supports async
[Test]
public async Task Repository_WithMockedData_ShouldReturnExpectedShorts()
{
    var sut = new AnalyticsDataTests();
    
    await sut.GetAllAsync_WhenCalledOnMockedRepository_ReturnsMockedVideoShorts();
    
    // If no exception is thrown, the internal assertions passed
    Assert.Pass();
}
```

## Notes

- **Edge cases:** `UpdateRetentionData_WhenRetentionExceedsHundred_ClampsAtMaximum` explicitly covers the overflow scenario where retention exceeds 100; it does not test negative values or exactly 100. `GetPerformanceLevel_WhenEngagementRateIsAboveTen_ReturnsExcellent` only validates the `> 10` path — boundary values such as exactly 10 or 0 are not covered by this member.
- **Thread safety:** All public members are synchronous test methods except `GetAllAsync`, which is asynchronous. The tests themselves are single-threaded by design (typical unit-test execution). No shared mutable state is exposed, so concurrent invocation across multiple test runners would not introduce race conditions within the type itself. The mocked repository used in `GetAllAsync` is assumed to be configured per-test and not shared across parallel executions unless the mocking framework guarantees isolation.
