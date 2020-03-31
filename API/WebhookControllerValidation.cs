// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace YouTubeShortsAutomator.API;

/// <summary>
/// Provides validation helpers for webhook models
/// </summary>
public static class WebhookControllerValidation
{
    /// <summary>
    /// Validates a WebhookRegistration instance
    /// </summary>
    /// <param name="value">The webhook registration to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this WebhookRegistration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate WebhookId (should not be empty/default)
        if (value.WebhookId == Guid.Empty)
        {
            errors.Add("WebhookId must be a non-empty GUID");
        }

        // Validate Url (required field)
        if (string.IsNullOrWhiteSpace(value.Url))
        {
            errors.Add("Url is required and cannot be empty or whitespace");
        }
        else if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uriResult) ||
                 !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            errors.Add("Url must be a valid HTTP or HTTPS URL");
        }

        // Validate Events array (required field)
        if (value.Events == null)
        {
            errors.Add("Events collection cannot be null");
        }
        else if (value.Events.Length == 0)
        {
            errors.Add("Events collection cannot be empty");
        }
        else
        {
            // Validate each event string
            for (int i = 0; i < value.Events.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(value.Events[i]))
                {
                    errors.Add($"Events[{i}] cannot be null, empty, or whitespace");
                }
            }
        }

        // Validate CreatedAtUtc (should not be default DateTime)
        if (value.CreatedAtUtc == default)
        {
            errors.Add("CreatedAtUtc must be a valid DateTime");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a WebhookDetails instance
    /// </summary>
    /// <param name="value">The webhook details to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this WebhookDetails value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate WebhookId (should not be empty/default)
        if (value.WebhookId == Guid.Empty)
        {
            errors.Add("WebhookId must be a non-empty GUID");
        }

        // Validate Url (required field)
        if (string.IsNullOrWhiteSpace(value.Url))
        {
            errors.Add("Url is required and cannot be empty or whitespace");
        }
        else if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uriResult) ||
                 !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            errors.Add("Url must be a valid HTTP or HTTPS URL");
        }

        // Validate Events array (required field)
        if (value.Events == null)
        {
            errors.Add("Events collection cannot be null");
        }
        else if (value.Events.Length == 0)
        {
            errors.Add("Events collection cannot be empty");
        }
        else
        {
            // Validate each event string
            for (int i = 0; i < value.Events.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(value.Events[i]))
                {
                    errors.Add($"Events[{i}] cannot be null, empty, or whitespace");
                }
            }
        }

        // Validate IsActive (no validation needed for boolean)

        // Validate CreatedAtUtc (should not be default DateTime)
        if (value.CreatedAtUtc == default)
        {
            errors.Add("CreatedAtUtc must be a valid DateTime");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a WebhookListItem instance
    /// </summary>
    /// <param name="value">The webhook list item to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this WebhookListItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate WebhookId (should not be empty/default)
        if (value.WebhookId == Guid.Empty)
        {
            errors.Add("WebhookId must be a non-empty GUID");
        }

        // Validate Url (required field)
        if (string.IsNullOrWhiteSpace(value.Url))
        {
            errors.Add("Url is required and cannot be empty or whitespace");
        }
        else if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uriResult) ||
                 !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            errors.Add("Url must be a valid HTTP or HTTPS URL");
        }

        // Validate IsActive (no validation needed for boolean)

        // Validate CreatedAtUtc (should not be default DateTime)
        if (value.CreatedAtUtc == default)
        {
            errors.Add("CreatedAtUtc must be a valid DateTime");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a WebhookRegistration instance is valid
    /// </summary>
    /// <param name="value">The webhook registration to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this WebhookRegistration value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether a WebhookDetails instance is valid
    /// </summary>
    /// <param name="value">The webhook details to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this WebhookDetails value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether a WebhookListItem instance is valid
    /// </summary>
    /// <param name="value">The webhook list item to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this WebhookListItem value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a WebhookRegistration instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The webhook registration to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors</exception>
    public static void EnsureValid(this WebhookRegistration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookRegistration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a WebhookDetails instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The webhook details to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors</exception>
    public static void EnsureValid(this WebhookDetails value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookDetails validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Ensures that a WebhookListItem instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The webhook list item to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value contains validation errors</exception>
    public static void EnsureValid(this WebhookListItem value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookListItem validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}