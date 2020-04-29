# ContentCalendarServiceExtensions

Extension methods for registering and interacting with the content calendar service in the YouTube Shorts Automator pipeline. These methods provide high-level operations for creating, retrieving, optimizing, and scheduling content calendar entries while abstracting lower-level service interactions.

## API

### `CreateEntryAsync`

Creates a new content calendar entry with the specified title and optional description. The entry is immediately persisted and returned upon successful creation.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `title` (string): The title of the content calendar entry.
- `description` (string?, optional): An optional description for the entry.

**Returns:**
- `Task<ContentCalendarEntry>`: The newly created content calendar entry.

**Throws:**
- `ArgumentNullException` if `service` or `title` is null.
- `InvalidOperationException` if the entry cannot be persisted due to validation or system errors.

---

### `GetRequiredEntryAsync`

Retrieves a content calendar entry by its unique identifier, throwing an exception if the entry does not exist.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `entryId` (Guid): The unique identifier of the content calendar entry.

**Returns:**
- `Task<ContentCalendarEntry>`: The requested content calendar entry.

**Throws:**
- `ArgumentNullException` if `service` is null.
- `KeyNotFoundException` if no entry with the specified `entryId` exists.

---

### `GetEntriesInRangeAsync`

Retrieves all content calendar entries whose scheduled publication dates fall within the specified inclusive range.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `start` (DateTime): The start of the date range (inclusive).
- `end` (DateTime): The end of the date range (inclusive).

**Returns:**
- `Task<IEnumerable<ContentCalendarEntry>>`: An enumerable of content calendar entries within the specified range.

**Throws:**
- `ArgumentNullException` if `service` is null.
- `ArgumentException` if `start` is after `end`.

---

### `GetUpcomingEntriesAsync`

Retrieves all content calendar entries that are scheduled for publication in the future relative to the current UTC time.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.

**Returns:**
- `Task<IEnumerable<ContentCalendarEntry>>`: An enumerable of upcoming content calendar entries, ordered by scheduled publication time ascending.

**Throws:**
- `ArgumentNullException` if `service` is null.

---
### `OptimizeAndApplyAsync`

Optimizes the title and description of a content calendar entry using platform-specific SEO heuristics, then applies the optimized values to the entry if improvements are found.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `entry` (ContentCalendarEntry): The content calendar entry to optimize.
- `platform` (PlatformType): The target platform for optimization (e.g., YouTube Shorts).

**Returns:**
- `Task<(TitleOptimizationResult Result, ContentCalendarEntry? Entry)>`: A tuple where `Result` indicates whether optimization was applied, and `Entry` is the updated entry if optimization occurred; otherwise, `null`.

**Throws:**
- `ArgumentNullException` if `service`, `entry`, or `platform` is null.
- `InvalidOperationException` if the entry cannot be updated due to system constraints.

---
### `ScheduleAtOptimalTimeAsync`

Determines and applies the optimal publication time for a content calendar entry based on historical engagement patterns and platform-specific scheduling heuristics.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `entry` (ContentCalendarEntry): The content calendar entry to schedule.
- `platform` (PlatformType): The target platform for scheduling.

**Returns:**
- `Task<ContentCalendarEntry>`: The updated content calendar entry with the optimal publication time applied.

**Throws:**
- `ArgumentNullException` if `service`, `entry`, or `platform` is null.
- `InvalidOperationException` if scheduling cannot be performed due to system constraints or invalid entry state.

---
### `IsReadyToPublishAsync`

Determines whether a content calendar entry meets all criteria required for publication, including validation of assets, metadata, and scheduling constraints.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.
- `entry` (ContentCalendarEntry): The content calendar entry to evaluate.

**Returns:**
- `Task<bool>`: `true` if the entry is ready for publication; otherwise, `false`.

**Throws:**
- `ArgumentNullException` if `service` or `entry` is null.

---
### `GetEntriesNeedingOptimizationAsync`

Retrieves all content calendar entries that have not been optimized for SEO or scheduling within the last 7 days, indicating they may benefit from re-optimization.

**Parameters:**
- `service` (IContentCalendarService): The content calendar service instance.

**Returns:**
- `Task<IEnumerable<ContentCalendarEntry>>`: An enumerable of content calendar entries needing optimization.

**Throws:**
- `ArgumentNullException` if `service` is null.

## Usage

### Example 1: Creating and scheduling a new entry
