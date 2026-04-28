# YouTube Shorts Automator - Upload Retry Fix

## Issue
The upload retry functionality was not resuming from the last chunk position when a network timeout occurred. Instead, it was always starting the upload from the beginning, causing unnecessary re-uploads of data that was already successfully transferred.

## Root Cause
In `YouTubeUploadService.cs`, the `UploadVideoAsync` method was always initializing `uploadedBytes = 0` regardless of any previous upload progress that had been saved to the database.

## Fix Applied
Changed line 84 in `YouTubeUploadService.cs`:
```csharp
// Before (always start from 0)
long uploadedBytes = 0;

// After (resume from last saved position)
long uploadedBytes = uploadJob.UploadedBytes;
```

Added comment: `// Hotfix: Resume upload from last saved position instead of starting from 0`

## Test Coverage
Added a new test case `UploadedBytes_WhenSet_PreservesValueForResume` to verify that:
1. The UploadJob correctly preserves the UploadedBytes value
2. Progress percentage is calculated correctly when resuming
3. The resume functionality works as expected

## Files Changed
1. `src/Services/YouTubeUploadService.cs` - Main fix
2. `tests/youtube-shorts-automator.Tests/UploadJobModelTests.cs` - Added test coverage

## Impact
This fix ensures that when an upload fails due to network timeout and is subsequently retried, it will resume from the last successfully uploaded chunk rather than restarting from the beginning. This significantly reduces bandwidth usage and upload time during retries.