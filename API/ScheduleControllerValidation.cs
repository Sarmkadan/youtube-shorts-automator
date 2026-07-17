// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for ScheduleController and related schedule models
// Provides validation, IsValid, and EnsureValid methods for schedule-related data models
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for ScheduleController and related schedule models
/// </summary>
public static class ScheduleControllerValidation
{
    /// <summary>
    /// Validates a <see cref="CreateScheduleRequest"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this CreateScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<string>();

        // Validate VideoId
        if (request.VideoId == Guid.Empty)
        {
            errors.Add("VideoId must be a non-empty GUID.");
        }

        // Validate ScheduledUploadTimeUtc
        if (request.ScheduledUploadTimeUtc == default)
        {
            errors.Add("ScheduledUploadTimeUtc must be a valid DateTime.");
        }
        else if (request.ScheduledUploadTimeUtc.Kind != DateTimeKind.Utc)
        {
            errors.Add("ScheduledUploadTimeUtc must be in UTC format.");
        }
        else if (request.ScheduledUploadTimeUtc <= DateTime.UtcNow)
        {
            errors.Add("ScheduledUploadTimeUtc must be in the future.");
        }

        // Validate RecurrencePattern (if provided)
        if (!string.IsNullOrEmpty(request.RecurrencePattern) && request.RecurrencePattern.Length > 100)
        {
            errors.Add("RecurrencePattern must be 100 characters or less.");
        }

        // Validate TimeZone (if provided)
        if (!string.IsNullOrEmpty(request.TimeZone) && request.TimeZone.Length > 50)
        {
            errors.Add("TimeZone must be 50 characters or less.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CreateScheduleRequest"/> instance is valid.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid([NotNullWhen(true)] this CreateScheduleRequest? request) => request?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CreateScheduleRequest"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages.</exception>
    public static void EnsureValid([NotNull] this CreateScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CreateScheduleRequest validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates an <see cref="UpdateScheduleRequest"/> instance.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this UpdateScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<string>();

        // Validate ScheduledUploadTimeUtc (if provided)
        if (request.ScheduledUploadTimeUtc.HasValue)
        {
            var time = request.ScheduledUploadTimeUtc.Value;
            if (time == default)
            {
                errors.Add("ScheduledUploadTimeUtc must be a valid DateTime.");
            }
            else if (time.Kind != DateTimeKind.Utc)
            {
                errors.Add("ScheduledUploadTimeUtc must be in UTC format.");
            }
            else if (time <= DateTime.UtcNow)
            {
                errors.Add("ScheduledUploadTimeUtc must be in the future.");
            }
        }

        // Validate RecurrencePattern (if provided)
        if (!string.IsNullOrEmpty(request.RecurrencePattern) && request.RecurrencePattern.Length > 100)
        {
            errors.Add("RecurrencePattern must be 100 characters or less.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UpdateScheduleRequest"/> instance is valid.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid([NotNullWhen(true)] this UpdateScheduleRequest? request) => request?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="UpdateScheduleRequest"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages.</exception>
    public static void EnsureValid([NotNull] this UpdateScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UpdateScheduleRequest validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="ScheduledUpload"/> instance.
    /// </summary>
    /// <param name="upload">The scheduled upload to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="upload"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this ScheduledUpload upload)
    {
        ArgumentNullException.ThrowIfNull(upload);

        var errors = new List<string>();

        // Validate ScheduleId
        if (upload.ScheduleId == Guid.Empty)
        {
            errors.Add("ScheduleId must be a non-empty GUID.");
        }

        // Validate VideoId
        if (upload.VideoId == Guid.Empty)
        {
            errors.Add("VideoId must be a non-empty GUID.");
        }

        // Validate ScheduledUploadTimeUtc
        if (upload.ScheduledUploadTimeUtc == default)
        {
            errors.Add("ScheduledUploadTimeUtc must be a valid DateTime.");
        }
        else if (upload.ScheduledUploadTimeUtc.Kind != DateTimeKind.Utc)
        {
            errors.Add("ScheduledUploadTimeUtc must be in UTC format.");
        }
        else if (upload.ScheduledUploadTimeUtc <= DateTime.UtcNow)
        {
            errors.Add("ScheduledUploadTimeUtc must be in the future.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(upload.Status))
        {
            errors.Add("Status must be specified.");
        }
        else if (upload.Status.Length > 50)
        {
            errors.Add("Status must be 50 characters or less.");
        }

        // Validate TimeZone (if provided)
        if (!string.IsNullOrEmpty(upload.TimeZone) && upload.TimeZone.Length > 50)
        {
            errors.Add("TimeZone must be 50 characters or less.");
        }

        // Validate RecurrencePattern (if provided)
        if (!string.IsNullOrEmpty(upload.RecurrencePattern) && upload.RecurrencePattern.Length > 100)
        {
            errors.Add("RecurrencePattern must be 100 characters or less.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ScheduledUpload"/> instance is valid.
    /// </summary>
    /// <param name="upload">The scheduled upload to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid([NotNullWhen(true)] this ScheduledUpload? upload) => upload?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ScheduledUpload"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="upload">The scheduled upload to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="upload"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of error messages.</exception>
    public static void EnsureValid([NotNull] this ScheduledUpload upload)
    {
        ArgumentNullException.ThrowIfNull(upload);

        var errors = Validate(upload);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ScheduledUpload validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}