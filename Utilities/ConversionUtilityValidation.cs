// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Validation utilities for ConversionUtility to ensure data integrity
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Validation helpers for ConversionUtility to ensure converted values meet expected criteria
/// </summary>
public static class ConversionUtilityValidation
{
    /// <summary>
    /// Validates ConversionUtility conversion behavior and returns a list of human-readable problems
    /// </summary>
    /// <returns>List of validation problems; empty list if valid</returns>
    public static IReadOnlyList<string> ValidateConversionUtility()
    {
        var problems = new List<string>();

        // Validate ToInt behavior (no specific constraints beyond null handling)
        // ToInt uses defaultValue=0, so no range validation needed

        // Validate ToLong behavior (no specific constraints beyond null handling)
        // ToLong uses defaultValue=0, so no range validation needed

        // Validate ToDecimal behavior (no specific constraints beyond null handling)
        // ToDecimal uses defaultValue=0, so no range validation needed

        // Validate ToDouble behavior (no specific constraints beyond null handling)
        // ToDouble uses defaultValue=0, so no range validation needed

        // Validate ToFloat behavior (no specific constraints beyond null handling)
        // ToFloat uses defaultValue=0, so no range validation needed

        // Validate ToBoolean behavior (no specific constraints beyond null handling)
        // ToBoolean uses defaultValue=false, so no validation needed

        // Validate ToDateTime - check for default/min value which indicates failure
        // ToDateTime uses DateTime.MinValue as default when parsing fails
        var testDateTime = ConversionUtility.ToDateTime(null);
        if (testDateTime == DateTime.MinValue)
        {
            problems.Add("ToDateTime conversion defaults to DateTime.MinValue on null input, indicating potential parsing failure");
        }

        // Validate ToGuid - check for empty Guid which indicates failure
        var testGuid = ConversionUtility.ToGuid(null);
        if (testGuid == Guid.Empty)
        {
            problems.Add("ToGuid conversion defaults to Guid.Empty on null input, indicating potential parsing failure");
        }

        // Validate ToEnum - check if default value is returned (null)
        var testEnum = ConversionUtility.ToEnum<DayOfWeek>(null);
        if (testEnum == null)
        {
            problems.Add("ToEnum<T> returns null when parsing fails, which may indicate invalid enum value");
        }

        // Validate ToString - check for empty string which might indicate null input
        var testString = ConversionUtility.ToString(null);
        if (string.IsNullOrEmpty(testString))
        {
            problems.Add("ToString returns empty string for null input");
        }

        // Validate ToByteArray - should return empty array for null input
        var testByteArray = ConversionUtility.ToByteArray(null);
        if (testByteArray.Length != 0)
        {
            problems.Add("ToByteArray should return empty byte array for null input");
        }

        // Validate JsonDeserialize - should return null on failure
        var testJsonDeserialize = ConversionUtility.JsonDeserialize<object>("invalid json");
        if (testJsonDeserialize == null)
        {
            problems.Add("JsonDeserialize<T> returns null when deserialization fails");
        }

        // Validate JsonSerialize - should return empty string on failure
        var testJsonSerialize = ConversionUtility.JsonSerialize(new { });
        if (string.IsNullOrEmpty(testJsonSerialize))
        {
            problems.Add("JsonSerialize<T> returns empty string when serialization fails");
        }

        // Validate ToList - should return empty list for null input
        var testToList = ConversionUtility.ToList<string>(null);
        if (testToList.Count != 0)
        {
            problems.Add("ToList<T> should return empty list for null input");
        }

        // Validate ObjectToDictionary - should return empty dictionary for null input
        var testObjectToDictionary = ConversionUtility.ObjectToDictionary(null);
        if (testObjectToDictionary.Count != 0)
        {
            problems.Add("ObjectToDictionary should return empty dictionary for null input");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if ConversionUtility conversion behavior is valid
    /// </summary>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsConversionUtilityValid()
    {
        return ValidateConversionUtility().Count == 0;
    }

    /// <summary>
    /// Ensures ConversionUtility conversion behavior is valid, throwing ArgumentException if not
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails with list of problems</exception>
    public static void EnsureConversionUtilityValid()
    {
        var problems = ValidateConversionUtility();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "ConversionUtility validation failed:" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", problems)
            );
        }
    }
}