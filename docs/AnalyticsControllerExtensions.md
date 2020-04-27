# AnalyticsControllerExtensions

Provides extension methods for generating analytics responses from YouTube Shorts video data, including engagement calculations, watch time metrics, and summary reporting.

## API

### `CalculateEngagementPerView`
Computes the engagement per view metric by dividing total engagement actions by total views.

- **Parameters**
  - `engagementActions` (int): The total number of engagement actions (likes, comments, shares).
  - `totalViews` (int): The total number of video views.
- **Return value**
  - `double`: The engagement per view ratio. Returns `0.0` if `totalViews` is zero to avoid division by zero.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if either `engagementActions` or `totalViews` is negative.

---

### `GetEngagementRatePercentage`
Formats the engagement rate as a percentage string with two decimal places.

- **Parameters**
  - `engagementRate` (double): The engagement rate value (e.g., `0.1234`).
- **Return value**
  - `string`: The formatted percentage (e.g., `"12.34%"`). Returns `"0.00%"` if the input is negative or invalid.
- **Exceptions**
  - None.

---

### `GetWatchTimeHours`
Converts total watch time in seconds to hours, rounded to two decimal places.

- **Parameters**
  - `totalWatchTimeSeconds` (double): The total watch time in seconds.
- **Return value**
  - `double`: The watch time in hours. Returns `0.0` if the input is negative.
- **Exceptions**
  - None.

---
### `GetAverageWatchDurationMinutes`
Calculates the average watch duration in minutes from total watch time and total views.

- **Parameters**
  - `totalWatchTimeSeconds` (double): The total watch time in seconds.
  - `totalViews` (int): The total number of video views.
- **Return value**
  - `double`: The average watch duration in minutes. Returns `0.0` if `totalViews` is zero or if `totalWatchTimeSeconds` is negative.
- **Exceptions**
  - None.

---
### `CreateVideoAnalyticsResponse`
Constructs a `VideoAnalyticsResponse` object populated with calculated analytics metrics.

- **Parameters**
  - `engagementActions` (int): Total engagement actions.
  - `totalViews` (int): Total video views.
  - `totalWatchTimeSeconds` (double): Total watch time in seconds.
  - `title` (string): The video title.
  - `videoId` (string): The YouTube video ID.
- **Return value**
  - `VideoAnalyticsResponse`: A populated analytics response object.
- **Exceptions**
  - Throws `ArgumentNullException` if `title` or `videoId` is `null` or empty.
  - Throws `ArgumentOutOfRangeException` if any numeric parameter is negative.

---
### `CreateAnalyticsSummaryResponse`
Generates an `AnalyticsSummaryResponse` containing aggregated analytics metrics for reporting.

- **Parameters**
  - `engagementActions` (int): Total engagement actions.
  - `totalViews` (int): Total video views.
  - `totalWatchTimeSeconds` (double): Total watch time in seconds.
- **Return value**
  - `AnalyticsSummaryResponse`: A summary response object with computed metrics.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if any numeric parameter is negative.

## Usage
