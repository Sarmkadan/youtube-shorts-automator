# OptimizationSuggestion

The `OptimizationSuggestion` record represents a scheduled or completed optimization recommendation for a YouTube Shorts video. It captures the proposed title, description, tags, category, optimal publish slot, and the outcome of any automated title optimization. Each suggestion is linked to a specific YouTube channel and optionally to a `VideoShort` entity and an upload job. The record is used by the scheduling and optimization pipeline to track when a video should be published and whether its metadata has been refined.

## API

### `public sealed record OptimizationSuggestion`

The type is a sealed record. It cannot be inherited and provides value-based equality semantics.

### `public DateTime NextOptimalSlot`

The calculated best time to publish the video. This value is set by the optimization algorithm and may be updated after re‑evaluation.

### `public int Id`

The primary key, assigned by the persistence layer.

### `public required string Title`

The video title as it appears on YouTube. This property is required and must be provided when constructing the record.

### `public string Description`

The video description. Can be `null` if not set.

### `public string[] Tags`

An array of tags associated with the video. Can be empty but not `null`.

### `public ContentCategory Category`

The YouTube content category assigned to the video. The `ContentCategory` type is an enum defined elsewhere in the project.

### `public CalendarEntryStatus Status`

The current status of the suggestion in the scheduling calendar. The `CalendarEntryStatus` enum indicates states such as `Scheduled`, `Published`, or `Failed`.

### `public DateTime ScheduledPublishAt`

The date and time at which the video is scheduled to be published. This may differ from `NextOptimalSlot` if the user overrides the suggestion.

### `public int? VideoShortId`

The foreign key to the associated `VideoShort` entity. `null` if the suggestion is not yet linked to a specific short.

### `public int? UploadJobId`

The foreign key to the upload job that processed this suggestion. `null` if no upload job has been created.

### `public int YouTubeChannelId`

The identifier of the YouTube channel to which this suggestion belongs.

### `public TitleOptimizationResult? LastOptimization`

The result of the most recent title optimization run. `null` if no optimization has been performed.

### `public bool OptimizationApplied`

Indicates whether the optimized title (from `LastOptimization`) has been applied to the video on YouTube.

### `public string? Notes`

Free‑text notes attached to the suggestion. Can be `null`.

### `public string[] Keywords`

An array of keywords used for search optimization. Can be empty but not `null`.

### `public DateTime CreatedAt`

The timestamp when the suggestion was first created.

### `public DateTime UpdatedAt`

The timestamp of the last modification to the suggestion.

### `public VideoShort? VideoShort`

Navigation property to the associated `VideoShort` entity. `null` if `VideoShortId` is `null`.

## Usage

### Example 1: Creating a new suggestion

```csharp
var suggestion = new OptimizationSuggestion
{
    Title = "How to Bake Sourdough in 60 Seconds",
    Description = "Quick sourdough recipe for busy bakers.",
    Tags = new[] { "sourdough", "baking", "shorts" },
    Category = ContentCategory.Howto,
    Status = CalendarEntryStatus.Scheduled,
    ScheduledPublishAt = DateTime.UtcNow.AddHours(2),
    NextOptimalSlot = DateTime.UtcNow.AddHours(2),
    YouTubeChannelId = 42,
    Keywords = new[] { "sourdough", "bread", "quick" },
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Persist via repository
await repository.AddAsync(suggestion);
```

### Example 2: Applying an optimization result

```csharp
// Retrieve an existing suggestion
var suggestion = await repository.GetByIdAsync(101);

if (suggestion.LastOptimization is not null && !suggestion.OptimizationApplied)
{
    // Apply the optimized title to the video
    suggestion.Title = suggestion.LastOptimization.OptimizedTitle;
    suggestion.OptimizationApplied = true;
    suggestion.UpdatedAt = DateTime.UtcNow;

    await repository.UpdateAsync(suggestion);
}
```

## Notes

- **Required properties**: `Title` must be provided at construction time; the compiler enforces this for record types with `required` members.
- **Nullability**: `Description`, `Notes`, `LastOptimization`, `VideoShort`, `VideoShortId`, and `UploadJobId` are nullable. Always check for `null` before accessing their members.
- **Array properties**: `Tags` and `Keywords` are never `null` but may be empty arrays. Avoid mutating the array after assignment; replace the entire array if changes are needed.
- **Thread safety**: Instances of `OptimizationSuggestion` are not inherently thread‑safe. Concurrent reads and writes to the same instance should be synchronized (e.g., via a lock or by using immutable patterns). The record’s value equality does not protect against race conditions during mutation.
- **Equality**: As a record, two instances are considered equal if all their properties (including array contents) are equal. Array comparison uses structural equality, not reference equality.
- **Persistence**: The `Id` property is typically auto‑generated by the database. Do not assign a value when creating a new suggestion unless you are explicitly setting it for testing or migration purposes.
