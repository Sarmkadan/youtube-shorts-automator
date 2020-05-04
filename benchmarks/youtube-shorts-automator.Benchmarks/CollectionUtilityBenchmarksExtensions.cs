// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Extension benchmarks for collection utilities, providing additional scenarios
/// and edge cases for chunking, grouping, and distinct operations.
/// </summary>
public sealed class CollectionUtilityBenchmarksExtensions
{
    [Benchmark(Description = "ChunkBy — empty collection")]
    public static List<List<int>> ChunkByEmptyCollection()
    {
        var empty = new List<int>();
        return CollectionUtility.ChunkBy(empty, 10);
    }

    [Benchmark(Description = "ChunkBy — single item")]
    public static List<List<int>> ChunkBySingleItem()
    {
        var single = new List<int> { 42 };
        return CollectionUtility.ChunkBy(single, 10);
    }

    [Benchmark(Description = "ChunkBy — uneven division")]
    public static List<List<int>> ChunkByUnevenDivision()
    {
        var numbers = Enumerable.Range(0, 105).ToList();
        return CollectionUtility.ChunkBy(numbers, 25);
    }

    [Benchmark(Description = "GroupByToDictionary — empty collection")]
    public static Dictionary<int, List<int>> GroupByToDictionaryEmptyCollection()
    {
        var empty = new List<int>();
        return CollectionUtility.GroupByToDictionary(empty, n => n % 10);
    }

    [Benchmark(Description = "GroupByToDictionary — single group")]
    public static Dictionary<int, List<int>> GroupByToDictionarySingleGroup()
    {
        var numbers = Enumerable.Repeat(5, 100).ToList();
        return CollectionUtility.GroupByToDictionary(numbers, n => n);
    }

    [Benchmark(Description = "GroupByToDictionary — many groups")]
    public static Dictionary<int, List<int>> GroupByToDictionaryManyGroups()
    {
        var numbers = Enumerable.Range(0, 1000).ToList();
        return CollectionUtility.GroupByToDictionary(numbers, n => n % 100);
    }

    [Benchmark(Description = "DistinctBy — empty collection")]
    public static List<int> DistinctByEmptyCollection()
    {
        var empty = new List<int>();
        return CollectionUtility.DistinctBy(empty, n => n % 10).ToList();
    }

    [Benchmark(Description = "DistinctBy — all unique")]
    public static List<int> DistinctByAllUnique()
    {
        var numbers = Enumerable.Range(0, 1000).ToList();
        return CollectionUtility.DistinctBy(numbers, n => n).ToList();
    }

    [Benchmark(Description = "DistinctBy — all same")]
    public static List<int> DistinctByAllSame()
    {
        var numbers = Enumerable.Repeat(42, 1000).ToList();
        return CollectionUtility.DistinctBy(numbers, n => n).ToList();
    }
}