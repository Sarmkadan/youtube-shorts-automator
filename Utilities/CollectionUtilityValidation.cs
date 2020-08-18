// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// CollectionUtility validation helpers
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Provides validation helpers for CollectionUtility operations
/// Validates collection parameters, chunk sizes, predicates, selectors, and other inputs
/// to CollectionUtility methods
/// </summary>
public static class CollectionUtilityValidation
{
    /// <summary>
    /// Validates a collection parameter for CollectionUtility methods
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollection<T>(IEnumerable<T>? collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = new List<string>();

        // Check for empty collections where semantic validation makes sense
        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and chunkSize parameter for CollectionUtility.ChunkBy
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="chunkSize">The chunk size to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndChunkSize<T>(
        IEnumerable<T> collection,
        int chunkSize)
    {
        var baseProblems = ValidateCollection(collection);
        var problems = new List<string>(baseProblems);

        // Validate chunkSize is positive
        if (chunkSize <= 0)
        {
            problems.Add("Chunk size must be greater than 0");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and selector parameter for CollectionUtility.SelectSafe
    /// </summary>
    /// <typeparam name="TInput">The input element type</typeparam>
    /// <typeparam name="TOutput">The output element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="selector">The selector function to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or selector is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndSelector<TInput, TOutput>(
        IEnumerable<TInput>? collection,
        Func<TInput, TOutput> selector)
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var problems = new List<string>();

        // Validate collection separately
        if (collection == null)
        {
            problems.Add("Collection parameter cannot be null");
        }
        else if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and predicate parameter for CollectionUtility.WhereSafe
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="predicate">The predicate function to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or predicate is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndPredicate<T>(
        IEnumerable<T>? collection,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var problems = new List<string>();

        // Validate collection separately
        if (collection == null)
        {
            problems.Add("Collection parameter cannot be null");
        }
        else if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and keySelector parameter for CollectionUtility.GroupByToDictionary
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="keySelector">The key selector function to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or keySelector is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndKeySelector<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and values parameter for CollectionUtility.ContainsAny/ContainsAll
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="values">The values to check for presence</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or values is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndValues<T>(
        IEnumerable<T> collection,
        params T[] values)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        if (values.Length == 0)
        {
            problems.Add("Values array is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection and count parameter for CollectionUtility.TakeRandom
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="count">The count parameter to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollectionAndCount<T>(
        IEnumerable<T> collection,
        int count)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        if (count <= 0)
        {
            problems.Add("Count must be greater than 0");
        }
        else if (count > collection.Count())
        {
            problems.Add("Count exceeds collection size");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection parameter for CollectionUtility.Shuffle
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollectionForShuffle<T>(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collections for CollectionUtility.Average
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollectionForAverage(IEnumerable<decimal> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates collections for CollectionUtility.Sum
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>List of human-readable problem descriptions; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static IReadOnlyList<string> ValidateCollectionForSum(IEnumerable<int> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = new List<string>();

        if (!collection.Any())
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a collection parameter is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid (non-empty); false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollection<T>(IEnumerable<T> collection)
    {
        try
        {
            return ValidateCollection(collection).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with chunkSize is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="chunkSize">The chunk size to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollectionAndChunkSize<T>(
        IEnumerable<T> collection,
        int chunkSize)
    {
        try
        {
            return ValidateCollectionAndChunkSize(collection, chunkSize).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with selector is valid
    /// </summary>
    /// <typeparam name="TInput">The input element type</typeparam>
    /// <typeparam name="TOutput">The output element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="selector">The selector function to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or selector is null</exception>
    public static bool IsValidCollectionAndSelector<TInput, TOutput>(
        IEnumerable<TInput> collection,
        Func<TInput, TOutput> selector)
    {
        try
        {
            return ValidateCollectionAndSelector(collection, selector).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with predicate is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="predicate">The predicate function to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or predicate is null</exception>
    public static bool IsValidCollectionAndPredicate<T>(
        IEnumerable<T> collection,
        Func<T, bool> predicate)
    {
        try
        {
            return ValidateCollectionAndPredicate(collection, predicate).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with keySelector is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="keySelector">The key selector function to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or keySelector is null</exception>
    public static bool IsValidCollectionAndKeySelector<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        try
        {
            return ValidateCollectionAndKeySelector(collection, keySelector).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with values is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="values">The values to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection or values is null</exception>
    public static bool IsValidCollectionAndValues<T>(
        IEnumerable<T> collection,
        params T[] values)
    {
        try
        {
            return ValidateCollectionAndValues(collection, values).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection with count is valid
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="count">The count parameter to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollectionAndCount<T>(
        IEnumerable<T> collection,
        int count)
    {
        try
        {
            return ValidateCollectionAndCount(collection, count).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection parameter is valid for Shuffle operation
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollectionForShuffle<T>(IEnumerable<T> collection)
    {
        try
        {
            return ValidateCollectionForShuffle(collection).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection of decimals is valid for Average operation
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollectionForAverage(IEnumerable<decimal> collection)
    {
        try
        {
            return ValidateCollectionForAverage(collection).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a collection of integers is valid for Sum operation
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    public static bool IsValidCollectionForSum(IEnumerable<int> collection)
    {
        try
        {
            return ValidateCollectionForSum(collection).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures a collection parameter is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollection<T>(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollection(collection);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with chunkSize is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="chunkSize">The chunk size to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndChunkSize<T>(
        IEnumerable<T> collection,
        int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollectionAndChunkSize(collection, chunkSize);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with selector is valid, throwing if not
    /// </summary>
    /// <typeparam name="TInput">The input element type</typeparam>
    /// <typeparam name="TOutput">The output element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="selector">The selector function to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection or selector is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndSelector<TInput, TOutput>(
        IEnumerable<TInput> collection,
        Func<TInput, TOutput> selector)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var problems = ValidateCollectionAndSelector(collection, selector);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with predicate is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="predicate">The predicate function to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection or predicate is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndPredicate<T>(
        IEnumerable<T> collection,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var problems = ValidateCollectionAndPredicate(collection, predicate);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with keySelector is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="keySelector">The key selector function to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection or keySelector is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndKeySelector<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        var problems = ValidateCollectionAndKeySelector(collection, keySelector);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with values is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="values">The values to check</param>
    /// <exception cref="ArgumentNullException">Thrown if collection or values is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndValues<T>(
        IEnumerable<T> collection,
        params T[] values)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        var problems = ValidateCollectionAndValues(collection, values);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection with count is valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="count">The count parameter to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionAndCount<T>(
        IEnumerable<T> collection,
        int count)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollectionAndCount(collection, count);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection parameter is valid for Shuffle operation, throwing if not
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionForShuffle<T>(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollectionForShuffle(collection);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection of decimals is valid for Average operation, throwing if not
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionForAverage(IEnumerable<decimal> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollectionForAverage(collection);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }

    /// <summary>
    /// Ensures a collection of integers is valid for Sum operation, throwing if not
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if collection is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static void EnsureValidCollectionForSum(IEnumerable<int> collection)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var problems = ValidateCollectionForSum(collection);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Collection validation failed: " + string.Join("; ", problems));
        }
    }
}