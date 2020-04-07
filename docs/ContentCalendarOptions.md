# ContentCalendarOptions

The `ContentCalendarOptions` class serves as a configuration container for defining the behavioral parameters and constraints of the content scheduling engine within the `youtube-shorts-automator` project. It encapsulates rules for title and description length limits, keyword weighting strategies, posting time optimizations, and engagement thresholds to ensure generated content aligns with platform best practices and specific channel strategies.

## API

### DefaultLookAheadDays
**Type:** `int`  
Specifies the number of days into the future the scheduler should attempt to populate with content slots by default. This value determines the horizon for the initial calendar generation.

### MaxTitleLength
**Type:** `int`  
Defines the absolute maximum character count allowed for a video title. Titles exceeding this limit during generation or optimization will be truncated or flagged.

### OptimalTitleMinLength
**Type:** `int`  
Sets the lower bound for the ideal title length range. Titles shorter than this value may be considered suboptimal for search visibility or click-through rates.

### OptimalTitleMaxLength
**Type:** `int`  
Sets the upper bound for the ideal title length range. This value should be less than or equal to `MaxTitleLength` and represents the target maximum for optimal engagement.

### MaxDescriptionLength
**Type:** `int`  
Defines the absolute maximum character count allowed for the video description field.

### MaxTagCount
**Type:** `int`  
Specifies the maximum number of tags allowed per video entry. Any tag list exceeding this count will be trimmed based on relevance scoring.

### OptimizationSuggestionCount
**Type:** `int`  
Determines the number of alternative title or description variations the engine should generate when requesting optimization suggestions.

### OptimalPostingHoursUtc
**Type:** `int[]`  
An array of integers representing hours in UTC (0-23) identified as high-traffic periods. The scheduler prioritizes these slots when `AutoOptimizeOnCreate` is enabled.

### KeywordWeightMultiplier
**Type:** `double`  
A scalar value applied to the base score of keywords found in the `HighEngagementKeywords` list. Higher values increase the priority of content containing these terms.

### EngagementScoreWeight
**Type:** `double`  
Defines the relative importance of historical engagement metrics when calculating the overall score for a content slot or suggestion.

### MinSlotGapMinutes
**Type:** `int`  
Enforces the minimum time duration required between two consecutive scheduled posts. This prevents content cannibalization by ensuring adequate spacing.

### AutoOptimizeOnCreate
**Type:** `bool`  
When set to `true`, the content creation pipeline automatically applies optimization logic (titles, tags, scheduling) upon instantiation of a new calendar entry. If `false`, entries are created with raw input data.

### HighEngagementKeywords
**Type:** `string[]`  
A list of specific keywords associated with historically high performance. These terms trigger the `KeywordWeightMultiplier` bonus during scoring.

### TrendingHashtags
**Type:** `string[]`  
A curated list of hashtags currently identified as trending. These are prioritized for injection into video descriptions when space permits.

### HistoricalSampleSize
**Type:** `int`  
Specifies the number of past content items to analyze when calculating dynamic metrics such as average engagement rates or optimal posting times.

### HighEngagementBonus
**Type:** `double`  
A fixed score additive applied to content slots that match criteria for high potential engagement, influencing the final sorting order of the calendar.

### EngagementRateThreshold
**Type:** `double`  
The minimum engagement rate (e.g., views/likes ratio) required for a past video to be considered a "successful" sample in historical analysis.

## Usage

### Example 1: Basic Configuration for High-Frequency Posting
This example demonstrates initializing the options for a channel that posts frequently with strict character limits and specific UTC posting windows.

```csharp
var options = new ContentCalendarOptions
{
    DefaultLookAheadDays = 14,
    MaxTitleLength = 60,
    OptimalTitleMinLength = 40,
    OptimalTitleMaxLength = 55,
    MaxDescriptionLength = 500,
    MaxTagCount = 15,
    MinSlotGapMinutes = 240, // 4 hours between posts
    OptimalPostingHoursUtc = new[] { 14, 18, 22 },
    AutoOptimizeOnCreate = true
};

// Initialize the scheduler with these constraints
var scheduler = new ContentScheduler(options);
```

### Example 2: Advanced Weighting and Keyword Strategy
This example configures the engine to heavily favor specific keywords and uses a larger historical dataset for decision-making.

```csharp
var options = new ContentCalendarOptions
{
    HistoricalSampleSize = 100,
    EngagementRateThreshold = 0.05, // 5% minimum engagement
    KeywordWeightMultiplier = 2.5,
    HighEngagementBonus = 10.0,
    EngagementScoreWeight = 0.8,
    HighEngagementKeywords = new[] { "tutorial", "shorts", "viral", "tips" },
    TrendingHashtags = new[] { "#fyp", "#trending", "#youtube shorts" },
    OptimizationSuggestionCount = 5,
    AutoOptimizeOnCreate = false // Manual review before optimization
};

var generator = new ContentGenerator(options);
var draft = generator.CreateDraft("How to edit fast");
// Optimization will be triggered manually later
```

## Notes

*   **Range Validation**: Implementers must ensure that `OptimalTitleMinLength` does not exceed `OptimalTitleMaxLength`, and that `OptimalTitleMaxLength` does not exceed `MaxTitleLength`. The system does not automatically correct inverted ranges and may throw runtime exceptions during validation phases if these constraints are violated.
*   **Array Mutability**: The properties `OptimalPostingHoursUtc`, `HighEngagementKeywords`, and `TrendingHashtags` expose reference types. Modifying the contents of these arrays after passing the options object to a running service may result in race conditions or inconsistent scheduling behavior. It is recommended to treat these arrays as immutable once the configuration is loaded.
*   **Thread Safety**: The `ContentCalendarOptions` class itself is a plain data container (POCO) and does not implement internal locking mechanisms. While reading properties is generally safe, swapping out array references or modifying primitive values while a background worker is actively consuming the configuration is not thread-safe. Configuration updates should occur on a single thread or be protected by external synchronization primitives.
*   **UTC Assumption**: All time-based logic relies strictly on `OptimalPostingHoursUtc` being in Coordinated Universal Time. No automatic conversion from local system time is performed; callers must convert local preferred times to UTC before assigning them to the array.
