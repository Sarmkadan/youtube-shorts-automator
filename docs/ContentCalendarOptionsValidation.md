# ContentCalendarOptionsValidation

The `ContentCalendarOptionsValidation` static class provides extension methods for validating `ContentCalendarOptions` instances. Callers can inspect all validation problems, perform a Boolean validity check, or enforce validity by throwing an exception.

## API

### `Validate(ContentCalendarOptions? value)`

```csharp
public static IReadOnlyList<string> Validate(this ContentCalendarOptions? value)
```

Validates the supplied options and returns a read-only list containing one message for each failed rule. An empty list indicates that the options are valid.

- **Parameters:** `value` (`ContentCalendarOptions?`) — The options instance to validate.
- **Returns:** An `IReadOnlyList<string>` of validation problem messages.
- **Throws:** `ArgumentNullException` when `value` is `null`.

### `IsValid(ContentCalendarOptions? value)`

```csharp
public static bool IsValid(this ContentCalendarOptions? value)
```

Determines whether the supplied options pass every validation rule.

- **Parameters:** `value` (`ContentCalendarOptions?`) — The options instance to check.
- **Returns:** `true` when `Validate` returns no problems; otherwise, `false`. A `null` value returns `false`.

### `EnsureValid(ContentCalendarOptions? value)`

```csharp
public static void EnsureValid(this ContentCalendarOptions? value)
```

Validates the supplied options and returns normally when they are valid. When validation fails, it throws an exception whose message contains every validation problem on a separate line.

- **Parameters:** `value` (`ContentCalendarOptions?`) — The options instance to validate.
- **Returns:** None.
- **Throws:**
  - `ArgumentNullException` when `value` is `null`.
  - `ArgumentException` when one or more validation rules fail.

## Validation rules

- `DefaultLookAheadDays` must be from 1 through 365.
- `MaxTitleLength` must be from 1 through 100.
- `OptimalTitleMinLength` must be positive and must not exceed `MaxTitleLength`.
- `OptimalTitleMaxLength` must be positive, must not exceed `MaxTitleLength`, and must not be less than `OptimalTitleMinLength`.
- `MaxDescriptionLength` must be from 1 through 50,000.
- `MaxTagCount` must be from 1 through 100.
- `OptimizationSuggestionCount` must be from 1 through 20.
- `OptimalPostingHoursUtc` must not be `null` or empty. Each hour must be from 0 through 23, and duplicate hours are not allowed.
- `KeywordWeightMultiplier` must be non-negative.
- `EngagementScoreWeight` must be from 0.0 through 1.0.
- `MinSlotGapMinutes` must be from 1 through 1,440.
- `HighEngagementKeywords` must not be `null`. Each keyword must contain non-whitespace text and must not exceed 100 characters.
- `TrendingHashtags` must not be `null`. Each hashtag must contain non-whitespace text, must not exceed 50 characters, and must begin with `#`.
- `HistoricalSampleSize` must be from 1 through 1,000.
- `HighEngagementBonus` must be from 0.0 through 1.0.
- `EngagementRateThreshold` must be from 0.0 through 100.0.

## Usage

```csharp
using YouTubeShortAutomator.Configuration;

var options = new ContentCalendarOptions
{
    DefaultLookAheadDays = 14,
    MaxTitleLength = 100,
    OptimalTitleMinLength = 40,
    OptimalTitleMaxLength = 70,
    MaxDescriptionLength = 5_000,
    MaxTagCount = 15,
    OptimizationSuggestionCount = 5,
    OptimalPostingHoursUtc = new[] { 14, 18, 22 },
    KeywordWeightMultiplier = 1.5,
    EngagementScoreWeight = 0.7,
    MinSlotGapMinutes = 240,
    HighEngagementKeywords = new[] { "tutorial", "tips" },
    TrendingHashtags = new[] { "#shorts", "#tutorial" },
    HistoricalSampleSize = 100,
    HighEngagementBonus = 0.2,
    EngagementRateThreshold = 5.0
};

var problems = options.Validate();
if (problems.Count > 0)
{
    foreach (var problem in problems)
    {
        Console.WriteLine(problem);
    }
}

if (options.IsValid())
{
    options.EnsureValid();
}
```

## Notes

- The methods are extension methods in the `YouTubeShortAutomator.Configuration` namespace and can be called directly on a `ContentCalendarOptions` instance.
- Validation reports all detected problems rather than stopping at the first failed rule.
- `Validate` returns a read-only collection and never returns `null`.
- Validation does not mutate the supplied options or their arrays.
- `AutoOptimizeOnCreate` has no validation rule.
