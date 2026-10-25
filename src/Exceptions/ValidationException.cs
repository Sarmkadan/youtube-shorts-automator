// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Thrown when validation of input data fails.
/// </summary>
public sealed class ValidationException : YoutubeShortsAutomatorException
{
    public string? FieldName { get; set; }
    public string? FieldValue { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();

    public ValidationException(string message) : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    public ValidationException(string message, string fieldName, string fieldValue)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(fieldName);
        ArgumentNullException.ThrowIfNull(fieldValue);

        FieldName = fieldName;
        FieldValue = fieldValue;
    }

    public ValidationException(string message, Dictionary<string, string> errors)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(errors);

        Errors = errors;
    }
}
