# ThumbnailVariantExtensions

Provides static helper methods for evaluating and comparing the performance of thumbnail variants in the YouTube Shorts automator workflow. These methods operate on `ThumbnailVariant` instances or raw conversion‑rate values to derive metrics, compare variants, and express performance in human‑readable terms.

## API

### GetConversionRate
Calculates the conversion rate (clicks per impression) for a supplied thumbnail variant.

- **Parameters**
  - `variant`: The `ThumbnailVariant` whose impressions and clicks are used for the calculation.
- **Return value**
  - A `double` representing the conversion rate as a ratio (clicks ÷ impressions). If impressions are zero, the method returns `0`.
- **Exceptions**
  - `ArgumentNullException` – thrown when `variant` is `null`.

### IsBetterThan
Determines whether the first thumbnail variant has a higher conversion rate than the second.

- **Parameters**
  - `first`: The `ThumbnailVariant` to evaluate as the potentially better option.
  - `second`: The `ThumbnailVariant` to compare against.
- **Return value**
  - `true` if `first`’s conversion rate exceeds `second`’s; otherwise `false`.
- **Exceptions**
  - `ArgumentNullException` – thrown when either `first` or `second` is `null`.

### GetPerformanceStatus
Maps a numeric conversion rate to a categorical performance label.

- **Parameters**
  - `conversionRate`: The conversion rate to assess. Expected to be non‑negative.
- **Return value**
  - A `string` indicating the performance tier: `"Poor"` (< 0.01), `"Average"` (0.01 ≤ rate < 0.03), `"Good"` (0.03 ≤ rate < 0.07), or `"Excellent"` (≥ 0.07).
- **Exceptions**
  - `ArgumentOutOfRangeException` – thrown when `conversionRate` is negative.

### GetRelativeImprovement
Computes the relative improvement between an old and a new conversion rate, expressed as a percentage.

- **Parameters**
  - `oldRate`: The baseline conversion rate.
  - `newRate`: The updated conversion rate to compare against the baseline.
- **Return value**
  - A `double` representing the percentage change: `((newRate - oldRate) / oldRate) * 100`. If `oldRate` is zero, the method returns `0` to avoid division by zero.
- **Exceptions**
  - None; the method handles a zero baseline gracefully by returning zero.

## Usage

```csharp
using YouTubeShortsAutomator.Models;
using YouTubeShortsAutomator.Extensions;

// Assume thumbnailVariantA and thumbnailVariantB are populated ThumbnailVariant instances
double rateA = ThumbnailVariantExtensions.GetConversionRate(thumbnailVariantA);
double rateB = ThumbnailVariantExtensions.GetConversionRate(thumbnailVariantB);

bool aIsBetter = ThumbnailVariantExtensions.IsBetterThan(thumbnailVariantA, thumbnailVariantB);
// aIsBetter is true when variant A outperforms variant B

string statusA = ThumbnailVariantExtensions.GetPerformanceStatus(rateA);
// statusA might be "Good" if rateA falls within the 0.03–0.07 range

double improvement = ThumbnailVariantExtensions.GetRelativeImprovement(rateA, rateB);
// improvement shows the percent change from A's rate to B's rate
```

```csharp
// Batch evaluation of a list of variants
var variants = GetThumbnailVariantsFromExperiment();

foreach (var v in variants)
{
    double cr = ThumbnailVariantExtensions.GetConversionRate(v);
    string perf = ThumbnailVariantExtensions.GetPerformanceStatus(cr);
    Log($"Variant {v.Id}: CR={cr:P2}, Status={perf}");
}

// Identify the best variant
var best = variants.Aggregate((curr, next) =>
    ThumbnailVariantExtensions.IsBetterThan(next, curr) ? next : curr);
Log($"Best variant: {best.Id}");
```

## Notes

- All methods are stateless and rely solely on their input arguments, making them thread‑safe for concurrent invocation from multiple threads.
- `GetConversionRate` returns zero when the variant records zero impressions to avoid division‑by‑zero errors; callers should treat this as an indeterminate rate rather than a confirmed zero performance.
- `GetRelativeImprovement` returns zero when the baseline (`oldRate`) is zero, which indicates that no meaningful relative change can be computed; in such cases consider examining the absolute difference instead.
- The performance thresholds used by `GetPerformanceStatus` (`Poor`, `Average`, `Good`, `Excellent`) are based on empirical observations from historical Shorts data and may be adjusted in future releases without changing the method signature.
- Passing `null` to any method that expects a `ThumbnailVariant` will result in an `ArgumentNullException`; defensive code should validate arguments prior to invocation when the source of the variant is uncertain.
