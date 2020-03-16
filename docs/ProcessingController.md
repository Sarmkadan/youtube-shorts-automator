# ProcessingController

A controller that handles video processing requests for YouTube Shorts automation. It manages the submission of video files, tracks processing status, allows cancellation, and provides available encoding profiles.

## API

### `ProcessingController`
The controller class exposing endpoints for video processing operations. It provides methods to submit videos, check status, retrieve profiles, and cancel processing.

### `public async Task<IActionResult> SubmitVideoAsync`
Submits a video file for processing with the specified title, description, and encoding profile.

- **Parameters**:
  - `IFormFile? file`: The video file to process. May be null if a file was not provided.
  - `string title`: The title for the YouTube Short.
  - `string description`: The description for the YouTube Short.
  - `string processingProfile`: The name of the encoding profile to use.
- **Return value**: An `IActionResult` indicating success or failure. On success, returns HTTP 202 Accepted with the processing ID and status. On failure, returns HTTP 400 Bad Request with an error message.
- **Throws**: May throw `ArgumentException` if required fields are missing or invalid.

### `public async Task<IActionResult> GetProcessingStatusAsync`
Retrieves the current status of a processing operation.

- **Parameters**: None.
- **Return value**: An `IActionResult` containing the processing status. On success, returns HTTP 200 OK with the current status, progress, message, and metadata. On failure, returns HTTP 404 Not Found if the processing ID is invalid.
- **Throws**: No exceptions expected under normal operation.

### `public async Task<IActionResult> GetAvailableProfilesAsync`
Retrieves the list of available encoding profiles supported by the system.

- **Parameters**: None.
- **Return value**: An `IActionResult` containing a list of profiles. On success, returns HTTP 200 OK with an array of profile objects. On failure, returns HTTP 500 Internal Server Error if profiles cannot be retrieved.
- **Throws**: No exceptions expected under normal operation.

### `public async Task<IActionResult> CancelProcessingAsync`
Cancels an ongoing processing operation.

- **Parameters**: None.
- **Return value**: An `IActionResult` indicating success or failure. On success, returns HTTP 200 OK with a confirmation message. On failure, returns HTTP 400 Bad Request if the operation cannot be canceled or HTTP 404 Not Found if the processing ID is invalid.
- **Throws**: No exceptions expected under normal operation.

### `public IFormFile? File`
Gets or sets the video file to be processed. May be null if no file was provided.

### `public string Title`
Gets or sets the title for the YouTube Short.

### `public string Description`
Gets or sets the description for the YouTube Short.

### `public string ProcessingProfile`
Gets or sets the name of the encoding profile to use for processing.

### `public Guid ProcessingId`
Gets the unique identifier for the current processing operation.

### `public string Status`
Gets the current status of the processing operation (e.g., "Pending", "Processing", "Completed", "Failed").

### `public string Message`
Gets the last status message associated with the processing operation.

### `public int Progress`
Gets the current progress percentage of the processing operation (0–100).

### `public DateTime CreatedAtUtc`
Gets the UTC timestamp when the processing operation was initiated.

### `public string Name`
Gets the display name of an encoding profile (when retrieved via `GetAvailableProfilesAsync`).

### `public string Code`
Gets the internal code identifier of an encoding profile (when retrieved via `GetAvailableProfilesAsync`).

### `public string Resolution`
Gets the resolution specification of an encoding profile (when retrieved via `GetAvailableProfilesAsync`).

### `public string Bitrate`
Gets the bitrate specification of an encoding profile (when retrieved via `GetAvailableProfilesAsync`).

## Usage

### Example 1: Submit a video for processing
