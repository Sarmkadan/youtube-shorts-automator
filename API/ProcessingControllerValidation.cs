// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for processing controller data validation
/// </summary>
public static class ProcessingControllerValidation
{
    /// <summary>
    /// Validates processing controller data and returns any validation errors
    /// </summary>
    /// <param name="file">The IFormFile instance</param>
    /// <param name="title">The title string</param>
    /// <param name="description">The description string</param>
    /// <param name="processingProfile">The processing profile string</param>
    /// <param name="processingId">The processing identifier</param>
    /// <param name="status">The status string</param>
    /// <param name="message">The message string</param>
    /// <param name="progress">The progress percentage</param>
    /// <param name="createdAtUtc">The creation timestamp</param>
    /// <param name="name">The profile name</param>
    /// <param name="code">The profile code</param>
    /// <param name="resolution">The video resolution</param>
    /// <param name="bitrate">The video bitrate</param>
    /// <returns>List of validation error messages, or empty list if valid</returns>
    public static IReadOnlyList<string> Validate(
        IFormFile? file,
        string title,
        string description,
        string processingProfile,
        Guid processingId,
        string status,
        string message,
        int progress,
        DateTime createdAtUtc,
        string name,
        string code,
        string resolution,
        string bitrate)
    {
        var errors = new List<string>();

        // Validate File
        if (file == null)
        {
            errors.Add("File cannot be null");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title cannot be null or whitespace");
        }
        else if (title.Length > 100)
        {
            errors.Add("Title cannot exceed 100 characters");
        }

        // Validate Description
        if (description != null && description.Length > 5000)
        {
            errors.Add("Description cannot exceed 5000 characters");
        }

        // Validate ProcessingProfile
        if (string.IsNullOrWhiteSpace(processingProfile))
        {
            errors.Add("ProcessingProfile cannot be null or whitespace");
        }
        else if (processingProfile.Length > 50)
        {
            errors.Add("ProcessingProfile cannot exceed 50 characters");
        }

        // Validate ProcessingId
        if (processingId == Guid.Empty)
        {
            errors.Add("ProcessingId cannot be empty");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(status))
        {
            errors.Add("Status cannot be null or whitespace");
        }
        else if (status.Length > 50)
        {
            errors.Add("Status cannot exceed 50 characters");
        }
        else if (!IsValidStatus(status))
        {
            errors.Add("Status must be one of: Queued, Processing, Completed, Failed, Cancelled");
        }

        // Validate Message
        if (message != null && message.Length > 1000)
        {
            errors.Add("Message cannot exceed 1000 characters");
        }

        // Validate Progress
        if (progress < 0 || progress > 100)
        {
            errors.Add("Progress must be between 0 and 100");
        }

        // Validate CreatedAtUtc
        if (createdAtUtc == default)
        {
            errors.Add("CreatedAtUtc cannot be default (Unix epoch)");
        }
        else if (createdAtUtc > DateTime.UtcNow.AddHours(1))
        {
            errors.Add("CreatedAtUtc cannot be in the future");
        }

        // Validate Profile Name
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Name cannot be null or whitespace");
        }
        else if (name.Length > 100)
        {
            errors.Add("Name cannot exceed 100 characters");
        }

        // Validate Profile Code
        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add("Code cannot be null or whitespace");
        }
        else if (code.Length > 20)
        {
            errors.Add("Code cannot exceed 20 characters");
        }

        // Validate Resolution
        if (string.IsNullOrWhiteSpace(resolution))
        {
            errors.Add("Resolution cannot be null or whitespace");
        }
        else if (resolution.Length > 20)
        {
            errors.Add("Resolution cannot exceed 20 characters");
        }

        // Validate Bitrate
        if (string.IsNullOrWhiteSpace(bitrate))
        {
            errors.Add("Bitrate cannot be null or whitespace");
        }
        else if (bitrate.Length > 20)
        {
            errors.Add("Bitrate cannot exceed 20 characters");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the processing controller data is valid
    /// </summary>
    /// <param name="file">The IFormFile instance</param>
    /// <param name="title">The title string</param>
    /// <param name="description">The description string</param>
    /// <param name="processingProfile">The processing profile string</param>
    /// <param name="processingId">The processing identifier</param>
    /// <param name="status">The status string</param>
    /// <param name="message">The message string</param>
    /// <param name="progress">The progress percentage</param>
    /// <param name="createdAtUtc">The creation timestamp</param>
    /// <param name="name">The profile name</param>
    /// <param name="code">The profile code</param>
    /// <param name="resolution">The video resolution</param>
    /// <param name="bitrate">The video bitrate</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(
        IFormFile? file,
        string title,
        string description,
        string processingProfile,
        Guid processingId,
        string status,
        string message,
        int progress,
        DateTime createdAtUtc,
        string name,
        string code,
        string resolution,
        string bitrate)
    {
        return Validate(
            file, title, description, processingProfile, processingId,
            status, message, progress, createdAtUtc, name, code, resolution, bitrate
        ).Count == 0;
    }

    /// <summary>
    /// Ensures that the processing controller data is valid, throwing an exception if not
    /// </summary>
    /// <param name="file">The IFormFile instance</param>
    /// <param name="title">The title string</param>
    /// <param name="description">The description string</param>
    /// <param name="processingProfile">The processing profile string</param>
    /// <param name="processingId">The processing identifier</param>
    /// <param name="status">The status string</param>
    /// <param name="message">The message string</param>
    /// <param name="progress">The progress percentage</param>
    /// <param name="createdAtUtc">The creation timestamp</param>
    /// <param name="name">The profile name</param>
    /// <param name="code">The profile code</param>
    /// <param name="resolution">The video resolution</param>
    /// <param name="bitrate">The video bitrate</param>
    /// <exception cref="ArgumentException">Thrown when the data is invalid</exception>
    public static void EnsureValid(
        IFormFile? file,
        string title,
        string description,
        string processingProfile,
        Guid processingId,
        string status,
        string message,
        int progress,
        DateTime createdAtUtc,
        string name,
        string code,
        string resolution,
        string bitrate)
    {
        var errors = Validate(
            file, title, description, processingProfile, processingId,
            status, message, progress, createdAtUtc, name, code, resolution, bitrate
        );

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Processing controller data is invalid. Errors: {string.Join(", ", errors)}");
        }
    }

    /// <summary>
    /// Checks if a status string is valid
    /// </summary>
    private static bool IsValidStatus(string status)
    {
        return status is "Queued" or "Processing" or "Completed" or "Failed" or "Cancelled";
    }
}