using System;
using System.Collections.Generic;
using System.Globalization;

namespace YouTubeShortsAutomator.Benchmarks;

public static class CollectionUtilityBenchmarksExtensionsValidation
{
    /// <summary>
    /// Validates the collection utility benchmarks extensions instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this CollectionUtilityBenchmarksExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ChunkByEmptyCollection
        var chunkByEmptyCollection = CollectionUtilityBenchmarksExtensions.ChunkByEmptyCollection();
        if (chunkByEmptyCollection == null)
        {
            problems.Add($"ChunkByEmptyCollection() must not return null.");
        }
        else
        {
            if (chunkByEmptyCollection.Count != 0)
            {
                problems.Add($"ChunkByEmptyCollection() must return empty list but has {chunkByEmptyCollection.Count} items.");
            }
        }

        // Validate ChunkBySingleItem
        var chunkBySingleItem = CollectionUtilityBenchmarksExtensions.ChunkBySingleItem();
        if (chunkBySingleItem == null)
        {
            problems.Add($"ChunkBySingleItem() must not return null.");
        }
        else
        {
            if (chunkBySingleItem.Count != 1 || chunkBySingleItem[0].Count != 1)
            {
                problems.Add($"ChunkBySingleItem() must return list containing exactly one chunk with one item.");
            }
        }

        // Validate ChunkByUnevenDivision
        var chunkByUnevenDivision = CollectionUtilityBenchmarksExtensions.ChunkByUnevenDivision();
        if (chunkByUnevenDivision == null)
        {
            problems.Add($"ChunkByUnevenDivision() must not return null.");
        }
        else
        {
            var totalItems = chunkByUnevenDivision.Sum(c => c.Count);
            if (totalItems != 105)
            {
                problems.Add($"ChunkByUnevenDivision() must return 105 items across all chunks but has {totalItems}.");
            }

            var expectedChunks = new[] { 25, 25, 25, 30 };
            for (var i = 0; i < expectedChunks.Length; i++)
            {
                if (i >= chunkByUnevenDivision.Count)
                {
                    problems.Add($"ChunkByUnevenDivision() is missing expected chunk at index {i}.");
                    break;
                }

                if (chunkByUnevenDivision[i].Count != expectedChunks[i])
                {
                    problems.Add($"ChunkByUnevenDivision() chunk at index {i} must have {expectedChunks[i]} items but has {chunkByUnevenDivision[i].Count}.");
                }
            }

            if (chunkByUnevenDivision.Count != expectedChunks.Length)
            {
                problems.Add($"ChunkByUnevenDivision() must return {expectedChunks.Length} chunks but has {chunkByUnevenDivision.Count}.");
            }
        }

        // Validate GroupByToDictionaryEmptyCollection
        var groupByToDictionaryEmptyCollection = CollectionUtilityBenchmarksExtensions.GroupByToDictionaryEmptyCollection();
        if (groupByToDictionaryEmptyCollection == null)
        {
            problems.Add($"GroupByToDictionaryEmptyCollection() must not return null.");
        }
        else
        {
            if (groupByToDictionaryEmptyCollection.Count != 0)
            {
                problems.Add($"GroupByToDictionaryEmptyCollection() must return empty dictionary but has {groupByToDictionaryEmptyCollection.Count} items.");
            }
        }

        // Validate GroupByToDictionarySingleGroup
        var groupByToDictionarySingleGroup = CollectionUtilityBenchmarksExtensions.GroupByToDictionarySingleGroup();
        if (groupByToDictionarySingleGroup == null)
        {
            problems.Add($"GroupByToDictionarySingleGroup() must not return null.");
        }
        else
        {
            if (groupByToDictionarySingleGroup.Count != 1)
            {
                problems.Add($"GroupByToDictionarySingleGroup() must return dictionary with exactly one group but has {groupByToDictionarySingleGroup.Count}.");
            }
            else
            {
                // Get the first (and only) group's value list
                var firstGroupValue = groupByToDictionarySingleGroup.Values.FirstOrDefault();
                if (firstGroupValue == null || firstGroupValue.Count != 100)
                {
                    problems.Add($"GroupByToDictionarySingleGroup()'s group must have 100 items but has {(firstGroupValue?.Count ?? 0)}.");
                }
            }
        }

        // Validate GroupByToDictionaryManyGroups
        var groupByToDictionaryManyGroups = CollectionUtilityBenchmarksExtensions.GroupByToDictionaryManyGroups();
        if (groupByToDictionaryManyGroups == null)
        {
            problems.Add($"GroupByToDictionaryManyGroups() must not return null.");
        }
        else
        {
            var expectedGroupSizes = new[] { 100, 100 };
            var totalItems = 0;
            var groupsList = groupByToDictionaryManyGroups.Values.ToList();

            for (var i = 0; i < expectedGroupSizes.Length; i++)
            {
                if (i >= groupsList.Count)
                {
                    problems.Add($"GroupByToDictionaryManyGroups() is missing expected group at index {i}.");
                    break;
                }

                var actualCount = groupsList[i].Count;
                if (actualCount != expectedGroupSizes[i])
                {
                    problems.Add($"GroupByToDictionaryManyGroups() group at index {i} must have {expectedGroupSizes[i]} items but has {actualCount}.");
                }

                totalItems += actualCount;
            }

            if (groupsList.Count != expectedGroupSizes.Length)
            {
                problems.Add($"GroupByToDictionaryManyGroups() must have {expectedGroupSizes.Length} groups but has {groupsList.Count}.");
            }

            if (totalItems != 1000)
            {
                problems.Add($"GroupByToDictionaryManyGroups() must contain exactly 1000 items but has {totalItems}.");
            }
        }

        // Validate DistinctByEmptyCollection
        var distinctByEmptyCollection = CollectionUtilityBenchmarksExtensions.DistinctByEmptyCollection();
        if (distinctByEmptyCollection == null)
        {
            problems.Add($"DistinctByEmptyCollection() must not return null.");
        }
        else
        {
            if (distinctByEmptyCollection.Count != 0)
            {
                problems.Add($"DistinctByEmptyCollection() must return empty list but has {distinctByEmptyCollection.Count} items.");
            }
        }

        // Validate DistinctByAllUnique
        var distinctByAllUnique = CollectionUtilityBenchmarksExtensions.DistinctByAllUnique();
        if (distinctByAllUnique == null)
        {
            problems.Add($"DistinctByAllUnique() must not return null.");
        }
        else
        {
            if (distinctByAllUnique.Count != 1000)
            {
                problems.Add($"DistinctByAllUnique() must have exactly 1000 items but has {distinctByAllUnique.Count}.");
            }
            else
            {
                var expected = new HashSet<int>();
                var hasDuplicates = false;
                for (var i = 0; i < distinctByAllUnique.Count; i++)
                {
                    if (!expected.Add(distinctByAllUnique[i]))
                    {
                        problems.Add($"DistinctByAllUnique() contains duplicate value {distinctByAllUnique[i]} at index {i}.");
                        hasDuplicates = true;
                    }
                }

                if (!hasDuplicates && expected.Count != 1000)
                {
                    problems.Add($"DistinctByAllUnique() must contain 1000 unique values but has only {expected.Count} unique values.");
                }
            }
        }

        // Validate DistinctByAllSame
        var distinctByAllSame = CollectionUtilityBenchmarksExtensions.DistinctByAllSame();
        if (distinctByAllSame == null)
        {
            problems.Add($"DistinctByAllSame() must not return null.");
        }
        else
        {
            if (distinctByAllSame.Count != 1)
            {
                problems.Add($"DistinctByAllSame() must have exactly 1 item but has {distinctByAllSame.Count}.");
            }
            else if (distinctByAllSame[0] != 42)
            {
                problems.Add($"DistinctByAllSame() must return list containing only the value 42 but has {distinctByAllSame[0]}.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the collection utility benchmarks extensions instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this CollectionUtilityBenchmarksExtensions value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the collection utility benchmarks extensions instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this CollectionUtilityBenchmarksExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"CollectionUtilityBenchmarksExtensions is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
