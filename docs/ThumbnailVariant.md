# ThumbnailVariant

`ThumbnailVariant` represents a single candidate thumbnail for a YouTube Short within the `youtube-shorts-automator` project. Each variant tracks its own performance metrics (impressions, clicks, view rate) and maintains a reference to its parent `VideoShort`. The class provides methods to update analytics data, declare a winning variant, and determine if sufficient data has been collected for meaningful comparison.

## API

### Properties

#### `public int Id`
Unique identifier for the thumbnail variant in the database.

#### `public int VideoShortId`
Foreign key referencing the associated `VideoShort`.

#### `public string Label`
Human-readable label for the variant (e.g., "Variant A", "Red Background"). Used for identification in reports and analytics.

#### `public string ThumbnailPath`
Filesystem path to the thumbnail image file.

#### `public long ImpressionCount`
Total number of times this thumbnail has been displayed to viewers.

#### `public long ClickCount`
Total number of times this thumbnail has been clicked.

#### `public double ViewRate`
Calculated click-through rate (`ClickCount / ImpressionCount`). Returns `0` if `ImpressionCount` is `0`.

#### `public bool IsActive`
Indicates whether this variant is currently eligible for impression tracking. Inactive variants are excluded from analytics updates.

#### `public bool IsWinner`
Indicates whether this variant has been declared the winner for its parent `VideoShort`. Only one variant per `VideoShort` may be a winner.

#### `public DateTime CreatedAt`
Timestamp of when the variant was created.

#### `public DateTime UpdatedAt`
Timestamp of the last update to the variant's analytics or metadata.

#### `public VideoShort? VideoShort`
Navigation property to the parent `VideoShort`. May be `null` if not loaded.

### Methods

#### `public void RecordImpression()`
Increments the `ImpressionCount` by `1` and updates the `UpdatedAt` timestamp. Does not throw exceptions.

#### `public void RecordClick()`
Increments the `ClickCount` by `1` and updates the `UpdatedAt` timestamp. Does not throw exceptions.

#### `public void UpdateFromAnalytics(long impressions, long clicks)`
Updates the variant's `ImpressionCount` and `ClickCount` with the provided values, replacing existing counts. Typically called after fetching aggregated analytics data from an external source.

**Parameters:**
- `impressions` (long): Total impressions to assign. Must be ≥ `0`.
- `clicks` (long): Total clicks to assign. Must be ≥ `0` and ≤ `impressions`.

**Throws:**
- `ArgumentOutOfRangeException`: If `impressions` or `clicks` are negative, or if `clicks` exceeds `impressions`.

#### `public void DeclareWinner()`
Marks this variant as the winner (`IsWinner = true`) and deactivates all other variants for the parent `VideoShort`. If this variant is already the winner, the method has no effect. Updates the `UpdatedAt` timestamp.

**Throws:**
- `InvalidOperationException`: If the parent `VideoShort` is `null`.

#### `public bool HasSufficientData(int minImpressions = 100, double minViewRateDelta = 0.02)`
Determines whether the variant has collected enough data to be statistically meaningful for comparison.

**Parameters:**
- `minImpressions` (int): Minimum required impressions (default: `100`).
- `minViewRateDelta` (double): Minimum required difference in view rate between this variant and others (default: `0.02` or 2%).

**Returns:**
- `true` if:
  - `ImpressionCount` ≥ `minImpressions`, **and**
  - The absolute difference between this variant's `ViewRate` and the highest `ViewRate` of any other active variant for the parent `VideoShort` is ≥ `minViewRateDelta`.
- `false` otherwise.

**Throws:**
- `InvalidOperationException`: If the parent `VideoShort` is `null` or has no other active variants.

## Usage

### Example 1: Recording Impressions and Clicks
