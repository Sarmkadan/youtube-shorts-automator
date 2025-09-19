// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Utilities;

/// <summary>
/// Collection utility methods for batch operations and transformations
/// Provides safe operations on IEnumerable with null handling
/// </summary>
public static class CollectionUtility
{
    public static List<T> ToSafeList<T>(IEnumerable<T>? collection)
    {
        return collection?.ToList() ?? new List<T>();
    }

    public static T[] ToSafeArray<T>(IEnumerable<T>? collection)
    {
        return collection?.ToArray() ?? Array.Empty<T>();
    }

    public static bool IsNullOrEmpty<T>(IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    public static bool HasElements<T>(IEnumerable<T>? collection)
    {
        return collection != null && collection.Any();
    }

    public static List<TOutput> SelectSafe<TInput, TOutput>(
        IEnumerable<TInput>? collection,
        Func<TInput, TOutput> selector)
    {
        if (IsNullOrEmpty(collection))
            return new List<TOutput>();

        try
        {
            return collection!.Select(selector).ToList();
        }
        catch
        {
            return new List<TOutput>();
        }
    }

    public static List<T> WhereSafe<T>(
        IEnumerable<T>? collection,
        Func<T, bool> predicate)
    {
        if (IsNullOrEmpty(collection))
            return new List<T>();

        try
        {
            return collection!.Where(predicate).ToList();
        }
        catch
        {
            return new List<T>();
        }
    }

    public static List<List<T>> ChunkBy<T>(IEnumerable<T> collection, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0");

        var chunks = new List<List<T>>();
        var chunk = new List<T>();

        foreach (var item in collection)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                chunks.Add(new List<T>(chunk));
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    public static Dictionary<TKey, List<T>> GroupByToDictionary<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var result = new Dictionary<TKey, List<T>>();

        foreach (var item in collection)
        {
            var key = keySelector(item);
            if (!result.ContainsKey(key))
            {
                result[key] = new List<T>();
            }
            result[key].Add(item);
        }

        return result;
    }

    public static T? FirstOrDefault<T>(IEnumerable<T>? collection, T? defaultValue = default)
    {
        if (IsNullOrEmpty(collection))
            return defaultValue;

        return collection!.FirstOrDefault() ?? defaultValue;
    }

    public static T? LastOrDefault<T>(IEnumerable<T>? collection, T? defaultValue = default)
    {
        if (IsNullOrEmpty(collection))
            return defaultValue;

        return collection!.LastOrDefault() ?? defaultValue;
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in collection)
        {
            var key = keySelector(item);
            if (seen.Add(key))
            {
                yield return item;
            }
        }
    }

    public static bool ContainsAny<T>(IEnumerable<T> collection, params T[] values)
    {
        var set = new HashSet<T>(collection);
        return values.Any(v => set.Contains(v));
    }

    public static bool ContainsAll<T>(IEnumerable<T> collection, params T[] values)
    {
        var set = new HashSet<T>(collection);
        return values.All(v => set.Contains(v));
    }

    public static List<T> Shuffle<T>(IEnumerable<T> collection)
    {
        var list = collection.ToList();
        var random = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            var randomIndex = random.Next(i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
        return list;
    }

    public static IEnumerable<T> TakeRandom<T>(IEnumerable<T> collection, int count)
    {
        return Shuffle(collection).Take(count);
    }

    public static decimal Average(IEnumerable<decimal> collection)
    {
        if (IsNullOrEmpty(collection))
            return 0;

        return collection!.Average();
    }

    public static int Sum(IEnumerable<int> collection)
    {
        if (IsNullOrEmpty(collection))
            return 0;

        return collection!.Sum();
    }
}
