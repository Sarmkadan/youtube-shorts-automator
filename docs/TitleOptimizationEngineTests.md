# TitleOptimizationEngineTests

Unit test suite for the `TitleOptimizationEngine` class, validating title scoring, keyword extraction, and optimization logic for YouTube Shorts content. Tests cover edge cases, scoring heuristics, and asynchronous optimization workflows to ensure robust performance under various input conditions.

## API

### `TitleOptimizationEngineTests`
Public test class containing unit tests for title optimization functionality. Serves as a regression and validation suite for scoring algorithms, keyword extraction, and suggestion generation.

### `void ScoreTitle_EmptyString_ReturnsZero()`
Verifies that an empty title string receives a score of zero, ensuring no false positives in scoring logic.

### `void ScoreTitle_NullInput_ReturnsZero()`
Confirms that a null title input is handled gracefully by returning a score of zero without throwing exceptions.

### `void ScoreTitle_ShortTitle_ReturnsLowerScoreThanOptimal()`
Ensures that short titles receive lower scores than longer, descriptive titles, validating length-based scoring heuristics.

### `void ScoreTitle_TitleWithPowerWord_ReceivesBoost()`
Checks that titles containing power words (e.g., "secret", "proven") receive a scoring boost, confirming keyword-based scoring rules.

### `void ScoreTitle_TitleWithQuestion_ReceivesBoost()`
Validates that titles phrased as questions receive a scoring boost, testing question-mark detection in scoring logic.

### `void ScoreTitle_TitleWithNumber_ReceivesBoost()`
Confirms that titles containing numbers receive a scoring boost, verifying numeric pattern detection in scoring rules.

### `void ScoreTitle_ReturnValueIsClamped()`
Ensures that the score returned by `ScoreTitle` is clamped to a valid range (e.g., 0–100), preventing out-of-bounds values.

### `void ExtractKeywords_ExtractsNonTrivialWords()`
Verifies that non-trivial words (excluding common stop words) are correctly extracted from titles, testing keyword extraction logic.

### `void ExtractKeywords_FiltersStopWords()`
Confirms that common stop words (e.g., "the", "and") are filtered out during keyword extraction, ensuring high-quality keyword lists.

### `void ExtractKeywords_ReturnsAtMostTenKeywords()`
Ensures that `ExtractKeywords` returns no more than ten keywords per title, validating truncation logic.

### `void ExtractKeywords_EmptyInputs_ReturnsEmptyArray()`
Checks that empty or whitespace-only inputs return an empty array of keywords, testing edge-case handling.

### `void ExtractKeywords_ReturnsLowercaseWords()`
Validates that all extracted keywords are returned in lowercase, ensuring consistency in keyword storage and comparison.

### `async Task OptimizeAsync_WithValidInput_ReturnsSuggestions()`
Verifies that `OptimizeAsync` returns a non-null collection of suggestions for valid input titles, testing the core optimization workflow.

### `async Task OptimizeAsync_SuggestionsHavePositiveConfidenceScore()`
Ensures that all suggestions returned by `OptimizeAsync` have a positive confidence score, validating suggestion quality thresholds.

### `async Task OptimizeAsync_BestSuggestionIsHighestConfidence()`
Confirms that the best suggestion (first in the list) has the highest confidence score, testing sorting and prioritization logic.

### `async Task OptimizeAsync_RecommendedHashtagsIncludeShortsTag()`
Validates that all suggestions include the `#Shorts` hashtag in their recommended hashtags, ensuring compliance with YouTube Shorts requirements.

### `async Task OptimizeAsync_NullOrWhitespaceTitle_ThrowsArgumentException()`
Checks that `OptimizeAsync` throws an `ArgumentException` when provided with a null or whitespace title, enforcing input validation.

### `async Task OptimizeAsync_OptimalPostingHourIsWithinValidRange()`
Ensures that the optimal posting hour returned by `OptimizeAsync` falls within the valid range (0–23), validating time-based recommendation logic.

### `async Task RecommendPostingTimesAsync_ReturnsRequestedCount()`
Verifies that `RecommendPostingTimesAsync` returns the requested number of posting time suggestions, testing pagination or count-based logic.

## Usage

### Example 1: Validating Title Scoring
