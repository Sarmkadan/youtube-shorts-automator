# UploadJobModelTests

The `UploadJobModelTests` class contains unit tests that validate the behavior of the `UploadJobModel` used in the youtube-shorts-automator project. These tests verify retry eligibility, progress calculation, completion handling, and resume‑state preservation.

## API

### CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue
**Purpose:** Confirms that `UploadJobModel.CanRetry` returns `true` when the upload has failed and the number of attempts is below the configured maximum retry limit.  
**Parameters:** None.  
**Return value:** `void` – the test passes silently; on failure the test framework throws an assertion exception.  
**When it throws:** Throws an exception from the unit‑test framework (e.g., `AssertFailedException`) if `CanRetry` returns `false` under the described conditions.

### CanRetry_WhenAttemptCountMatchesMaxRetries_ReturnsFalse
**Purpose:** Confirms that `CanRetry` returns `false` when the attempt count has reached the maximum allowed retries.  
**Parameters:** None.  
**Return value:** `void`.  
**When it throws:** Throws an assertion exception if `CanRetry` returns `true` when attempts equal the max retries.

### UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage
**Purpose:** Verifies that `UpdateProgress` correctly computes the progress percentage when exactly half of the total bytes have been transferred.  
**Parameters:** None.  
**Return value:** `void`.  
**When it throws:** Throws an assertion exception if the calculated progress does not equal the expected 50 % (or the value derived from the model’s total size).

### MarkAsCompleted_AssignsVideoIdAndSetsProgressToFull
**Purpose:** Ensures that calling `MarkAsCompleted` with a video identifier stores the `Id` and sets the progress to 100 %.  
**Parameters:** None.  
**Return value:** `void`.  
**When it throws:** Throws an assertion exception if the `VideoId` is not set or `Progress` is not 100 % after the method call.

### UploadedBytes_WhenSet_PreservesValueForResume
**Purpose:** Confirms that setting the `UploadedBytes` property retains its value across subsequent accesses, enabling correct resume behavior.  
**Parameters:** None.  
**Return value:** `void`.  
**When it throws:** Throws an assertion exception if the retrieved `UploadedBytes` differs from the value that was previously set.

## Usage

**Example 1 – Running the tests with NUnit**
```csharp
using NUnit.Framework;
using YoutubeShortsAutomator.Models;

namespace YoutubeShortsAutomator.Tests
{
    [TestFixture]
    public class UploadJobModelTestsRunner
    {
        [Test]
        public void VerifyCanRetryLogic()
        {
            var test = new UploadJobModelTests();
            test.CanRetry_WhenFailedAndUnderRetryLimit_ReturnsTrue();
        }

        [Test]
        public void VerifyUpdateProgressLogic()
        {
            var test = new UploadJobModelTests();
            test.UpdateProgress_WithHalfTransferredBytes_CalculatesCorrectPercentage();
        }
    }
}
```

**Example 2 – Using UploadJobModel in application code (behavior validated by the tests)**
```csharp
using YoutubeShortsAutomator.Models;

var upload = new UploadJobModel(totalBytes: 10_000_000, maxRetries: 3);

// Simulate a failure and check retry eligibility.
upload.MarkAsFailed();
bool canRetry = upload.CanRetry(); // Expected true while attempts < maxRetries

// Update progress after transferring half the data.
upload.UpdateProgress(5_000_000);
int progress = upload.Progress; // Expected 50

// Upon successful completion.
upload.MarkAsCompleted(videoId: "abc123");
string videoId = upload.VideoId; // Expected "abc123"
int finalProgress = upload.Progress; // Expected 100
```

## Notes
- The tests assume a known initial state (total size and retry limit) set by the `UploadJobModel` constructor or factory. Altering those defaults may cause the tests to fail.
- These test methods are not thread‑safe; they should be executed on a single thread or with proper isolation because they mutate instance fields of `UploadJobModel`.
- Directly setting `UploadedBytes` bypasses any validation that might exist in the property setter; the test only confirms that the backing field preserves the assigned value.
- If `UploadJobModel.CanRetry` throws under exceptional conditions (e.g., negative retry limit), the corresponding test will propagate that exception, resulting in a test failure.
- `UpdateProgress` is expected to clamp values to the range 0‑100; the half‑transferred test validates the midpoint calculation.
- `MarkAsCompleted` is assumed to be idempotent; subsequent calls should keep the `VideoId` unchanged and progress at 100 %.
