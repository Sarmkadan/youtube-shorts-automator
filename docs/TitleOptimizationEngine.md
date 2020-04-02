# TitleOptimizationEngine

A utility class that analyzes and optimizes YouTube Shorts titles for engagement potential by scoring keyword relevance, extracting key terms, and recommending optimal posting times based on historical performance patterns.

## API

### `public TitleOptimizationEngine`

Initializes a new instance of the title optimization engine with default configuration.

### `public async Task<TitleOptimizationResult> OptimizeAsync(string title)`

Asynchronously evaluates a YouTube Shorts title and returns an optimization result containing a score and recommended improvements.

- **Parameters**
  - `title` (string): The YouTube Shorts title to analyze.
- **Return Value**
  - `Task<TitleOptimizationResult>`: A task that resolves to a `TitleOptimizationResult` object containing the title score and suggested improvements.
- **Exceptions**
  - Throws `ArgumentNullException` if `title` is null.
  - Throws `ArgumentException` if `title` is empty or exceeds maximum length.

### `public async Task<IEnumerable<DateTime>> RecommendPostingTimesAsync()`

Asynchronously recommends optimal posting times for a YouTube Shorts video based on historical engagement patterns and audience availability.

- **Return Value**
  - `Task<IEnumerable<DateTime>>`: A task that resolves to a collection of recommended posting times (UTC).
- **Exceptions**
  - Throws `InvalidOperationException` if the engine lacks sufficient historical data to make recommendations.

### `public double ScoreTitle(string title)`

Calculates a normalized score (0.0 to 1.0) representing the engagement potential of a given title.

- **Parameters**
  - `title` (string): The YouTube Shorts title to score.
- **Return Value**
  - `double`: A score between 0.0 (poor engagement potential) and 1.0 (high engagement potential).
- **Exceptions**
  - Throws `ArgumentNullException` if `title` is null.
  - Throws `ArgumentException` if `title` is empty.

### `public string[] ExtractKeywords(string title)`

Extracts relevant keywords from a YouTube Shorts title for SEO and audience targeting purposes.

- **Parameters**
  - `title` (string): The YouTube Shorts title to analyze.
- **Return Value**
  - `string[]`: An array of extracted keywords, ordered by relevance.
- **Exceptions**
  - Throws `ArgumentNullException` if `title` is null.
  - Throws `ArgumentException` if `title` is empty.

## Usage
