// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortAutomator.Exceptions;

public class ValidationException : Exception
{
    public string? FieldName { get; set; }
    public string? FieldValue { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, string fieldName, string fieldValue)
        : base(message)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
    }

    public ValidationException(string message, Dictionary<string, string> errors)
        : base(message)
    {
        Errors = errors;
    }
}
