# CollectionUtilityBenchmarksExtensions

The `CollectionUtilityBenchmarksExtensions` class serves as a static container for predefined data sets used exclusively in performance benchmarking scenarios. It provides a variety of `List<int>` and `Dictionary<int, List<int>>` instances configured to represent specific edge cases and common distribution patterns, such as empty collections, single-item scenarios, uneven divisions, and distinct value distributions. These members are designed to be consumed directly by benchmarking frameworks to evaluate the performance of collection manipulation methods under controlled and repeatable conditions without the overhead of dynamic data generation during test execution.

## API

### ChunkByEmptyCollection
```csharp
public static List<List<int>> ChunkByEmptyCollection
```
Provides a list of lists representing the result of chunking an empty source collection. This member returns an empty `List<List<int>>` (count is 0) and is used to benchmark how chunking algorithms handle input with no elements. It does not accept parameters and never throws exceptions.

### ChunkBySingleItem
```csharp
public static List<List<int>> ChunkBySingleItem
```
Provides a list of lists representing the result of chunking a source collection containing exactly one element. The returned `List<List<int>>` contains a single inner list with one integer. This is used to verify overhead and correctness when processing minimal non-empty datasets. It does not accept parameters and never throws exceptions.

### ChunkByUnevenDivision
```csharp
public static List<List<int>> ChunkByUnevenDivision
```
Provides a list of lists representing a scenario where the source collection size is not evenly divisible by the chunk size, resulting in a final chunk with fewer elements than the others. The returned `List<List<int>>` contains multiple inner lists of varying lengths. This member is critical for testing boundary logic in partitioning algorithms. It does not accept parameters and never throws exceptions.

### GroupByToDictionaryEmptyCollection
```csharp
public static Dictionary<int, List<int>> GroupByToDictionaryEmptyCollection
```
Provides a dictionary representing the result of grouping an empty collection. This member returns an empty `Dictionary<int, List<int>>` (count is 0). It is used to benchmark grouping operations when the input sequence contains no items. It does not accept parameters and never throws exceptions.

### GroupByToDictionarySingleGroup
```csharp
public static Dictionary<int, List<int>> GroupByToDictionarySingleGroup
```
Provides a dictionary where all input items belong to a single group key. The returned `Dictionary<int, List<int>>` contains exactly one key, with the associated value being a list of integers. This tests the performance of grouping logic when no hash collisions or multiple bucket allocations are required beyond the initial entry. It does not accept parameters and never throws exceptions.

### GroupByToDictionaryManyGroups
```csharp
public static Dictionary<int, List<int>> GroupByToDictionaryManyGroups
```
Provides a dictionary representing a high-cardinality grouping scenario where input items are distributed across many distinct keys. The returned `Dictionary<int, List<int>>` contains numerous entries, typically with small lists per key. This is used to stress-test dictionary resizing and hashing mechanisms during grouping operations. It does not accept parameters and never throws exceptions.

### DistinctByEmptyCollection
```csharp
public static List<int> DistinctByEmptyCollection
```
Provides a list representing the result of applying a distinct filter to an empty collection. This member returns an empty `List<int>`. It serves as a baseline for measuring the cost of distinct operations on zero-length inputs. It does not accept parameters and never throws exceptions.

### DistinctByAllUnique
```csharp
public static List<int> DistinctByAllUnique
```
Provides a list where every integer is unique, representing the result of a distinct operation on a set with no duplicates. The returned `List<int>` contains a sequence of unique integers. This benchmarks the worst-case scenario for distinct filters where no elements are removed, requiring full traversal and tracking of all items. It does not accept parameters and never throws exceptions.

### DistinctByAllSame
```csharp
public static List<int> DistinctByAllSame
```
Provides a list containing a single integer, representing the result of applying a distinct filter to a collection where all original items were identical. This benchmarks the scenario where the maximum number of elements are filtered out, leaving only one unique value. It does not accept parameters and never throws exceptions.

## Usage

The following examples demonstrate how these static members can be utilized within a BenchmarkDotNet test suite to evaluate collection processing performance.

### Example 1: Benchmarking Chunking Strategies
This example uses the predefined chunking datasets to compare two different implementation strategies for splitting collections.

```csharp
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class ChunkingBenchmarks
{
    [Benchmark]
    public List<List<int>> ProcessEmpty()
    {
        // Simulates processing logic on the empty case
        return CollectionUtilityBenchmarksExtensions.ChunkByEmptyCollection;
    }

    [Benchmark]
    public List<List<int>> ProcessUneven()
    {
        // Simulates processing logic on the uneven division case
        var data = CollectionUtilityBenchmarksExtensions.ChunkByUnevenDivision;
        // Additional processing logic would occur here in a real test
        return data;
    }
}
```

### Example 2: Benchmarking Grouping Overhead
This example utilizes the grouping datasets to measure the retrieval and iteration costs associated with different group cardinalities.

```csharp
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class GroupingBenchmarks
{
    [Benchmark]
    public int CountSingleGroup()
    {
        var data = CollectionUtilityBenchmarksExtensions.GroupByToDictionarySingleGroup;
        int count = 0;
        foreach (var group in data)
        {
            count += group.Value.Count;
        }
        return count;
    }

    [Benchmark]
    public int CountManyGroups()
    {
        var data = CollectionUtilityBenchmarksExtensions.GroupByToDictionaryManyGroups;
        int count = 0;
        foreach (var group in data)
        {
            count += group.Value.Count;
        }
        return count;
    }
}
```

## Notes

*   **Immutability of References**: While the collections returned by these properties are mutable `List<T>` and `Dictionary<TKey, TValue>` instances, they are intended to be treated as read-only fixtures during benchmark execution. Modifying the contents of these static members in one benchmark iteration will affect all subsequent tests using the same member within the same process lifecycle, potentially skewing results.
*   **Thread Safety**: The members themselves are static references to pre-instantiated objects. Reading from these collections (e.g., iterating or counting) is generally safe if the collections are not modified. However, concurrent writes to these static instances from multiple benchmark threads are not synchronized and will lead to undefined behavior. Benchmark frameworks typically isolate instances per iteration, but care should be taken not to modify the shared static state.
*   **Data Integrity**: The data within these collections is hardcoded to match specific structural expectations (e.g., `ChunkByUnevenDivision` guarantees a remainder). If a benchmark requires a specific count or value range different from what is provided, a new member should be added rather than modifying the existing data at runtime.
*   **Exception Handling**: Accessing these properties never throws exceptions. They are initialized at type load time. Any exceptions arising during their use would stem from the consumer's logic (e.g., invalid casting or null dereferencing if the consumer incorrectly assumes nullability), not from the properties themselves.
