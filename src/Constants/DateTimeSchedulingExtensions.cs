// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace YouTubeShortAutomator.Constants
{
    /// <summary>
    /// Extension methods for DateTime useful for scheduling logic.
    /// </summary>
    public static class DateTimeSchedulingExtensions
    {
        /// <summary>
        /// Determines whether the specified DateTime is within business hours (Monday-Friday, 9 AM to 5 PM).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <param name="startHour">The start hour of business hours (inclusive). Default is 9.</param>
        /// <param name="endHour">The end hour of business hours (exclusive). Default is 17.</param>
        /// <returns>true if the DateTime is within business hours; otherwise, false.</returns>
        public static bool IsWithinBusinessHours(this DateTime dateTime, int startHour = 9, int endHour = 17)
        {
            // Check if day is Monday to Friday (0=Sunday, 6=Saturday in .NET DayOfWeek)
            if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return dateTime.Hour >= startHour && dateTime.Hour < endHour;
        }

        /// <summary>
        /// Returns the next occurrence of the specified day of the week, including today if the current day matches.
        /// </summary>
        /// <param name="dateTime">The DateTime to start from.</param>
        /// <param name="dayOfWeek">The day of the week to find the next occurrence of.</param>
        /// <returns>A DateTime representing the next occurrence of the specified day of the week.</returns>
        public static DateTime NextOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            // Calculate the days to add to get to the next [dayOfWeek]
            int daysToAdd = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
            // If daysToAdd is 0, today is the day. We return today.
            return dateTime.AddDays(daysToAdd);
        }
    }
}