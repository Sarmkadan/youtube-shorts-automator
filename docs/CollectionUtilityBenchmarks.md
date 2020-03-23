# CollectionUtilityBenchmarks

Provides benchmarking utilities for common collection operations, including chunking, grouping, and deduplication. Designed to measure performance of `List<T>` and `Dictionary<TKey,TValue>` transformations under various conditions.

## API

### `public void Setup()`
Initializes the benchmarking environment. Must be called before any benchmark methods to ensure consistent state.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: `InvalidOperationException` if called after any benchmark method has executed.

---

### `public List<List<int>> ChunkBy(List<int> source, int chunkSize)`
Partitions the input list into sublists of the specified size. The last sublist may contain fewer elements if the input size is not evenly divisible by the chunk size.

- **Parameters**:
  - `source`: The input list to partition.
  - `chunkSize`: The maximum number of elements per sublist. Must be greater than zero.
- **Return value**: A list of lists, where each inner list contains at most `chunkSize` elements from the input.
- **Throws**:
  - `ArgumentNullException` if `source` is `null`.
  - `ArgumentOutOfRangeException` if `chunkSize` is less than or equal to zero.

---

### `public Dictionary<int, List<int>> GroupByToDictionary(List<int> source, Func<int, int> keySelector)`
Groups elements of the input list into a dictionary where keys are derived from each element using the provided selector, and values are lists of elements sharing the same key.

- **Parameters**:
  - `source`: The input list to group.
  - `keySelector`: A function to extract the key for each element.
- **Return value**: A dictionary mapping each unique key to the list of elements that produced that key.
- **Throws**:
  - `ArgumentNullException` if `source` is `null` or `keySelector` is `null`.
  - `ArgumentException` if `keySelector` returns `null` for any element.

---
### `public List<int> DistinctBy(List<int> source, Func<int, int> keySelector)`
Returns a new list containing only the distinct elements from the input, where distinctness is determined by the key returned from the provided selector. The first occurrence of each key is retained.

- **Parameters**:
  - `source`: The input list to deduplicate.
  - `keySelector`: A function to extract the key used to determine uniqueness.
- **Return value**: A list of distinct elements, preserving the original order of first occurrences.
- **Throws**:
  - `ArgumentNullException` if `source` is `null` or `keySelector` is `null`.

## Usage

### Example 1: Chunking a large dataset
