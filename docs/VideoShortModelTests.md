# VideoShortModelTests

Unit tests for the `VideoShortModel` class, verifying validation rules and state transitions for YouTube Shorts processing. These tests ensure that metadata validation, duration limits, and completion/failure status handling behave as expected.

## API

### `IsValid_WithValidMetadata_ReturnsTrue`

Ensures that a `VideoShortModel` with valid metadata (non-empty title, duration ≤ 60 seconds) is considered valid.

- **Parameters**: None
- **Return value**: `void` (asserts via test framework)
- **Throws**: No exceptions expected under normal test conditions

### `IsValid_WithEmptyTitle_ReturnsFalse`

Validates that a `VideoShortModel` with an empty or whitespace-only title fails validation.

- **Parameters**: None
- **Return value**: `void` (asserts via test framework)
- **Throws**: No exceptions expected under normal test conditions

### `IsValid_WithDurationBeyond60Seconds_ReturnsFalse`

Confirms that a `VideoShortModel` with a duration exceeding 60 seconds is rejected during validation.

- **Parameters**: None
- **Return value**: `void` (asserts via test framework)
- **Throws**: No exceptions expected under normal test conditions

### `MarkAsProcessed_WithoutError_SetsCompletedStatusAndTimestamp`

Verifies that calling `MarkAsProcessed` without an error message updates the model’s status to `Completed` and sets the completion timestamp.

- **Parameters**: None
- **Return value**: `void` (asserts via test framework)
- **Throws**: No exceptions expected under normal test conditions

### `MarkAsProcessed_WithErrorMessage_SetsFailedStatusAndPreservesError`

Ensures that calling `MarkAsProcessed` with an error message updates the model’s status to `Failed` and retains the error message while preserving the timestamp.

- **Parameters**: None
- **Return value**: `void` (asserts via test framework)
- **Throws**: No exceptions expected under normal test conditions

## Usage
