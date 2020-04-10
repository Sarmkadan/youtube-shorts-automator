# IntegrationTests

IntegrationTests is a class that provides end-to-end test coverage for the YouTube Shorts Automator system. It exercises the full workflow from video processing through scheduling, analytics, and file validation, ensuring that the application behaves correctly under realistic conditions. The tests validate both happy-path scenarios and edge cases such as concurrent operations and configuration variations.

## API

### `public IntegrationTests()`

Initializes a new instance of the IntegrationTests class. Sets up the test environment, including any required test doubles, mock services, and temporary directories. The constructor ensures that each test starts with a clean state.

### `public async Task EndToEnd_ScheduleUpload_CompletesSuccessfully()`

Validates that a complete upload and scheduling workflow executes without errors. The test simulates uploading a video, scheduling it for publication, and verifying that the process completes successfully with no exceptions thrown. This test covers the main success path for the scheduling subsystem.

### `public async Task EndToEnd_CreateVideo_CreateAnalytics_SyncMetrics()`

Tests the full pipeline from video creation to analytics generation and metric synchronization. The test creates a video asset, processes it, generates analytics, and confirms that metrics are correctly synchronized with the analytics backend. This validates the integration between the video processing, analytics, and metrics subsystems.

### `public async Task ConcurrencyTest_MultipleSchedulesSimultaneously()`

Ensures that the scheduling system can handle multiple simultaneous scheduling requests without race conditions or data corruption. The test spawns several concurrent scheduling operations and verifies that all schedules are created correctly and that no conflicts arise during execution.

### `public async Task ConcurrencyTest_MultipleVideoCreationsSimultaneously()`

Validates that the video creation subsystem can process multiple videos concurrently without interference. The test initiates several video creation operations in parallel and confirms that all videos are created successfully and that their metadata remains consistent.

### `public async Task ConfigurationTest_DifferentProcessingProfiles()`

Tests the system's behavior under different processing profiles. The test runs the same workflow using various configuration profiles (e.g., different resolutions, formats, or processing pipelines) and verifies that the system adapts correctly to each profile without errors.

### `public async Task SchedulingWorkflow_CreateScheduleAndRetrieveUpcoming()`

Validates the ability to create a schedule and retrieve the list of upcoming uploads. The test creates a schedule for a future time, then queries the system to confirm that the schedule appears in the upcoming uploads list with the correct metadata.

### `public async Task SchedulingWorkflow_RescheduleAndVerify()`

Tests the rescheduling functionality by creating a schedule, modifying its time, and verifying that the change is reflected in the system. The test ensures that rescheduling does not introduce inconsistencies and that the updated schedule appears correctly in subsequent queries.

### `public async Task SchedulingWorkflow_CancelUpload()`

Validates the ability to cancel a scheduled upload. The test creates a schedule, cancels it, and confirms that the schedule is removed from the system and no longer appears in the upcoming uploads list.

### `public async Task AnalyticsWorkflow_CreateAndGenerateReport()`

Tests the analytics generation workflow by creating analytics data and generating a report. The test confirms that the report is generated successfully and contains the expected metrics and insights.

### `public async Task FileValidationWorkflow_ValidateAndHash()`

Validates the file validation subsystem by checking file integrity and computing hashes. The test confirms that the system correctly identifies valid files, rejects invalid ones, and computes accurate hashes for verified files.

### `public async Task MainUseCase_ProcessVideoAndScheduleUpload()`

Tests the primary use case of processing a video and scheduling it for upload. The test simulates the full workflow from video ingestion through processing, scheduling, and confirmation, ensuring that the main user scenario completes successfully.

### `public void Dispose()`

Cleans up resources used during testing. This includes deleting temporary files, releasing any file handles, disposing of test doubles, and resetting the system state to ensure that subsequent tests start with a clean environment.

## Usage

### Example 1: Running a single end-to-end test
