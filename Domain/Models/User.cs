// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public long StorageQuotaBytes { get; set; }
    public long StorageUsedBytes { get; set; }
    public UserSubscriptionTier SubscriptionTier { get; set; } = UserSubscriptionTier.Free;
    public DateTime SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Video> Videos { get; set; } = new();
    public ApiCredential? ApiCredential { get; set; }
    public UploadSchedule? UploadSchedule { get; set; }

    /// <summary>
    /// Validates user email and basic information
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Valid email address is required");

        if (Email.Length > 254)
            errors.Add("Email address is too long");

        if (string.IsNullOrWhiteSpace(DisplayName) || DisplayName.Length > 100)
            errors.Add("Display name must be between 1 and 100 characters");

        if (string.IsNullOrWhiteSpace(ChannelId))
            errors.Add("YouTube channel ID is required");

        if (StorageQuotaBytes <= 0)
            errors.Add("Storage quota must be greater than zero");

        if (StorageUsedBytes < 0 || StorageUsedBytes > StorageQuotaBytes)
            errors.Add("Storage used cannot exceed quota");

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Checks if user has reached storage limit
    /// </summary>
    public bool IsStorageFull()
    {
        return StorageUsedBytes >= StorageQuotaBytes;
    }

    /// <summary>
    /// Adds storage usage
    /// </summary>
    public void AddStorageUsage(long bytes)
    {
        StorageUsedBytes += bytes;
    }

    /// <summary>
    /// Removes storage usage
    /// </summary>
    public void RemoveStorageUsage(long bytes)
    {
        StorageUsedBytes = Math.Max(0, StorageUsedBytes - bytes);
    }

    /// <summary>
    /// Updates last activity timestamp
    /// </summary>
    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if subscription is active
    /// </summary>
    public bool HasActiveSubscription()
    {
        return SubscriptionTier != UserSubscriptionTier.Free &&
               SubscriptionExpiresAt > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if user can upload new videos based on tier
    /// </summary>
    public bool CanUploadNewVideo()
    {
        if (!IsActive) return false;
        if (IsStorageFull()) return false;

        return SubscriptionTier switch
        {
            UserSubscriptionTier.Free => Videos.Count(v => v.CreatedAt > DateTime.UtcNow.AddDays(-30)) < 5,
            UserSubscriptionTier.Pro => Videos.Count(v => v.CreatedAt > DateTime.UtcNow.AddDays(-30)) < 100,
            UserSubscriptionTier.Enterprise => true,
            _ => false
        };
    }
}

public enum UserSubscriptionTier
{
    Free = 0,
    Pro = 1,
    Enterprise = 2
}
