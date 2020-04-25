# ScheduleRepositoryExtensions

Extension methods for `IRepository<UploadSchedule>` that provide common scheduling operations for YouTube Shorts upload automation.

## API

### `GetNextDueScheduleForUserAsync`

Retrieves the next due upload schedule for a specific user.

- **Parameters**
  - `repository`: The repository instance implementing `IRepository<UploadSchedule>`.
  - `userId`: The unique identifier of the user whose schedules are being queried.
  - `now`: The reference time used to determine due schedules (typically `DateTime.UtcNow`).

- **Return Value**
  Returns a `Task` resolving to the next due `UploadSchedule` for the user, or `null` if none is due.

- **Exceptions**
  Throws if the underlying repository operation fails.

---

### `GetByUserIdAsync`

Retrieves all upload schedules for a user, with pagination support.

- **Parameters**
  - `repository`: The repository instance implementing `IRepository<UploadSchedule>`.
  - `userId`: The unique identifier of the user whose schedules are being queried.
  - `pageNumber`: The 1-based page number for pagination.
  - `pageSize`: The maximum number of schedules to return per page.

- **Return Value**
  Returns a `Task` resolving to a tuple containing:
  - `Schedules`: The list of `UploadSchedule` items for the requested page.
  - `Total`: The total number of schedules across all pages for the user.

- **Exceptions**
  Throws if the underlying repository operation fails.

---

### `GetActiveSchedulesByFrequencyAsync`

Retrieves all active upload schedules grouped by their frequency setting.

- **Parameters**
  - `repository`: The repository instance implementing `IRepository<UploadSchedule>`.
  - `userId`: The unique identifier of the user whose schedules are being queried.

- **Return Value**
  Returns a `Task` resolving to a list of `UploadSchedule` items that are active and grouped by frequency.

- **Exceptions**
  Throws if the underlying repository operation fails.

---

### `GetDueSchedulesInWindowAsync`

Retrieves all upload schedules that are due within a specified time window.

- **Parameters**
  - `repository`: The repository instance implementing `IRepository<UploadSchedule>`.
  - `userId`: The unique identifier of the user whose schedules are being queried.
  - `windowStart`: The start of the time window (inclusive).
  - `windowEnd`: The end of the time window (exclusive).

- **Return Value**
  Returns a `Task` resolving to a list of `UploadSchedule` items that fall within the specified window.

- **Exceptions**
  Throws if the underlying repository operation fails.

## Usage
