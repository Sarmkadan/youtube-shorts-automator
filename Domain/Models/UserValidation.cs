// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="User"/> class
/// </summary>
public static class UserValidation
{
    /// <summary>
    /// Validates a User object and returns any validation errors
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <returns>Read-only list of validation errors (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this User? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
            errors.Add("User ID must be a valid GUID");

        // Validate Email
        if (string.IsNullOrWhiteSpace(value.Email))
            errors.Add("Email is required");
        else if (value.Email.Length > 254)
            errors.Add("Email address cannot exceed 254 characters");
        else if (!value.Email.Contains('@'))
            errors.Add("Email must contain a valid @ symbol");
        else if (value.Email.Split('@')[1].Length < 3)
            errors.Add("Email domain must be at least 3 characters");

        // Validate DisplayName
        if (string.IsNullOrWhiteSpace(value.DisplayName))
            errors.Add("Display name is required");
        else if (value.DisplayName.Length > 100)
            errors.Add("Display name cannot exceed 100 characters");
        else if (value.DisplayName.Trim().Length == 0)
            errors.Add("Display name cannot contain only whitespace");

        // Validate ChannelId
        if (string.IsNullOrWhiteSpace(value.ChannelId))
            errors.Add("YouTube channel ID is required");
        else if (value.ChannelId.Length != 24 && value.ChannelId.Length != 26)
            errors.Add("YouTube channel ID must be 24 or 26 characters long");

        // Validate EmailConfirmed
        // No specific validation needed for boolean

        // Validate StorageQuotaBytes
        if (value.StorageQuotaBytes <= 0)
            errors.Add("Storage quota must be greater than zero");
        else if (value.StorageQuotaBytes > 100_000_000_000) // 100GB max
            errors.Add("Storage quota cannot exceed 100GB");

        // Validate StorageUsedBytes
        if (value.StorageUsedBytes < 0)
            errors.Add("Storage used cannot be negative");
        else if (value.StorageUsedBytes > value.StorageQuotaBytes)
            errors.Add("Storage used cannot exceed storage quota");

        // Validate SubscriptionTier
        // Enum validation handled by compiler

        // Validate SubscriptionExpiresAt
        if (value.SubscriptionExpiresAt < DateTime.UtcNow.AddDays(-1))
            errors.Add("Subscription expiration date cannot be in the past");
        else if (value.SubscriptionExpiresAt < value.CreatedAt)
            errors.Add("Subscription cannot expire before account creation");

        // Validate CreatedAt
        if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Creation date cannot be in the future");
        else if (value.CreatedAt < DateTime.UnixEpoch)
            errors.Add("Creation date must be a valid date");

        // Validate LastActivityAt
        if (value.LastActivityAt.HasValue)
        {
            if (value.LastActivityAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("Last activity date cannot be in the future");
            else if (value.LastActivityAt.Value < value.CreatedAt)
                errors.Add("Last activity cannot occur before account creation");
        }

        // Validate IsActive
        // No specific validation needed for boolean

        // Validate Videos
        if (value.Videos == null)
            errors.Add("Videos collection cannot be null");
        else if (value.Videos.Any(v => v == null))
            errors.Add("Videos collection cannot contain null entries");

        // Validate ApiCredential
        if (value.ApiCredential != null)
        {
            if (value.ApiCredential.UserId != value.Id)
                errors.Add("API credential must belong to the same user");

            var credentialValidation = value.ApiCredential.Validate();
            if (credentialValidation.IsValid == false)
                errors.AddRange(credentialValidation.Errors.Select(e => $"API credential: {e}"));
        }

        // Validate UploadSchedule
        if (value.UploadSchedule != null)
        {
            var scheduleValidation = value.UploadSchedule.Validate();
            if (scheduleValidation.IsValid == false)
                errors.AddRange(scheduleValidation.Errors.Select(e => $"Upload schedule: {e}"));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a User object is valid
    /// </summary>
    /// <param name="value">The user to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this User? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a User object is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with error details</exception>
    public static void EnsureValid(this User? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"User validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}