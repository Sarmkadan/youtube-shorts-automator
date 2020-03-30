# AnalyticsData

`AnalyticsData` is a data model class that represents performance metrics and engagement statistics for a YouTube Short video. It stores view counts, engagement rates, audience retention, and other analytics data retrieved from the YouTube API, enabling tracking and analysis of video performance over time.

## API

### Properties

#### `public int Id`
Unique identifier for the analytics record in the database.

#### `public int VideoShortId`
Foreign key referencing the associated `VideoShort` record.

#### `public long ViewCount`
Total number of views the video has received. Updated via `UpdateFromAPI`.

#### `public long LikeCount`
Total number of likes the video has received. Updated via `UpdateFromAPI`.

#### `public long CommentCount`
Total number of comments on the video. Updated via `UpdateFromAPI`.

#### `public long ShareCount`
Total number of shares the video has received. Updated via `UpdateFromAPI`.

#### `public double AverageViewDuration`
Average duration (in seconds) that viewers watched the video. Updated via `UpdateFromAPI`.

#### `public double EngagementRate`
Calculated metric representing the ratio of engaged interactions (likes, comments, shares) to total views. Updated via `RecalculateEngagementMetrics`.

#### `public double ClickThroughRate`
Calculated metric representing the ratio of impressions that resulted in a view. Updated via `UpdateFromAPI` or derived from `ImpressionCount` and `ViewCount`.

#### `public int SubscribersGained`
Number of new subscribers gained from this video. Updated via `UpdateSubscriberMetrics`.

#### `public int SubscribersLost`
Number of subscribers lost after viewing this video. Updated via `UpdateSubscriberMetrics`.

#### `public double AudienceRetentionPercentage`
Percentage of the video watched on average by viewers. Updated via `UpdateRetentionData`.

#### `public int TrafficSources`
Bitmask or enumerated value representing the sources of traffic (e.g., browse features, external sites). Updated via `UpdateFromAPI`.

#### `public int ImpressionCount`
Total number of times the video thumbnail was shown to viewers. Updated via `UpdateFromAPI`.

#### `public DateTime UpdatedAt`
Timestamp of the last update to this record.

#### `public VideoShort? VideoShort`
Navigation property to the associated `VideoShort` entity. May be `null` if not loaded.

### Methods

#### `public void UpdateFromAPI()`
Fetches the latest analytics data from the YouTube API and updates the corresponding properties (`ViewCount`, `LikeCount`, `CommentCount`, `ShareCount`, `AverageViewDuration`, `ImpressionCount`, `TrafficSources`). Throws `InvalidOperationException` if the associated `VideoShort` is not loaded or lacks a valid YouTube ID. May throw `HttpRequestException` if the API request fails.

#### `public void RecalculateEngagementMetrics()`
Recalculates derived metrics (`EngagementRate`, `ClickThroughRate`) based on the current property values. Throws `DivideByZeroException` if `ViewCount` or `ImpressionCount` is zero.

#### `public void UpdateRetentionData()`
Updates `AudienceRetentionPercentage` based on `AverageViewDuration` and the video's total duration. Throws `InvalidOperationException` if the associated `VideoShort` is not loaded or lacks duration data.

#### `public void UpdateSubscriberMetrics()`
Updates `SubscribersGained` and `SubscribersLost` by querying the YouTube API for subscriber changes attributed to this video. Throws `InvalidOperationException` if the associated `VideoShort` is not loaded or lacks a valid YouTube ID. May throw `HttpRequestException` if the API request fails.

## Usage

### Example 1: Fetching and Updating Analytics
