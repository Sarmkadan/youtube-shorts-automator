# CollectionUtility

CollectionUtility provides a suite of static methods designed to perform robust, null-safe operations on `IEnumerable<T>` collections. It facilitates common tasks such as filtering, grouping, sampling, and safe conversion, reducing the need for explicit null checks throughout the codebase.

## API

*   **`public static List<T> ToSafeList<T>(IEnumerable<T>? source)`**
    Converts the source collection to a list. Returns an empty list if `source` is null.
*   **`public static T[] ToSafeArray<T>(IEnumerable<T>? source)`**
    Converts the source collection to an array. Returns an empty array if `source` is null.
*   **`public static bool IsNullOrEmpty<T>(IEnumerable<T>? source)`**
    Determines whether the source collection is null or has no elements.
*   **`public static bool HasElements<T>(IEnumerable<T>? source)`**
    Determines whether the source collection is not null and has at least one element.
*   **`public static List<TOutput> SelectSafe<TInput, TOutput>(IEnumerable<TInput>? source, Func<TInput, TOutput> selector)`**
    Projects each element of a sequence into a new form. Returns an empty list if `source` is null.
*   **`public static List<T> WhereSafe<T>(IEnumerable<T>? source, Func<T, bool> predicate)`**
    Filters a sequence of values based on a predicate. Returns an empty list if `source` is null.
*   **`public static List<List<T>> ChunkBy<T>(IEnumerable<T>? source, int chunkSize)`**
    Splits the source collection into smaller lists of the specified size.
*   **`public static Dictionary<TKey, List<T>> GroupByToDictionary<T, TKey>(IEnumerable<T>? source, Func<T, TKey> keySelector)`**
    Groups the elements of a sequence according to a specified key selector function and returns a dictionary.
*   **`public static T? FirstOrDefault<T>(IEnumerable<T>? source)`**
    Returns the first element of a sequence, or a default value if the sequence is null or empty.
*   **`public static T? LastOrDefault<T>(IEnumerable<T>? source)`**
    Returns the last element of a sequence, or a default value if the sequence is null or empty.
*   **`public static IEnumerable<T> DistinctBy<T, TKey>(IEnumerable<T>? source, Func<T, TKey> keySelector)`**
    Returns distinct elements from a sequence according to a specified key selector function.
*   **`public static bool ContainsAny<T>(IEnumerable<T>? source, IEnumerable<T> subset)`**
    Determines whether the source collection contains any element present in the specified subset.
*   **`public static bool ContainsAll<T>(IEnumerable<T>? source, IEnumerable<T> subset)`**
    Determines whether the source collection contains all elements present in the specified subset.
*   **`public static List<T> Shuffle<T>(IEnumerable<T>? source)`**
    Randomizes the order of elements in the source collection and returns a new list.
*   **`public static IEnumerable<T> TakeRandom<T>(IEnumerable<T>? source, int count)`**
    Returns a specified number of contiguous, randomly selected elements from the sequence.
*   **`public static decimal Average`**
    Static property providing access to collection average functionality.
*   **`public static int Sum`**
    Static property providing access to collection summation functionality.

## Usage

```csharp
// Example 1: Safe filtering and conversion
var items = new List<int> { 1, 2, 3, 4, 5 };
var evenNumbers = CollectionUtility.WhereSafe(items, x => x % 2 == 0);
var safeList = CollectionUtility.ToSafeList(evenNumbers);

// Example 2: Grouping and FirstOrDefault
var users = new List<User> { new User("A"), new User("B"), new User("A") };
var grouped = CollectionUtility.GroupByToDictionary(users, u => u.Name);
var firstUser = CollectionUtility.FirstOrDefault(users);
```

## Notes

*   **Null Handling**: The majority of methods are designed to treat a `null` input collection as an empty collection rather than throwing a `NullReferenceException`. This behavior should be accounted for when expecting explicit null validation.
*   **Thread Safety**: This class is generally thread-safe as it primarily performs read operations on the provided `IEnumerable<T>` inputs. However, if the underlying collections passed to these methods are modified by other threads during execution, the behavior is subject to the thread-safety guarantees of the specific `IEnumerable` implementation.
*   **Performance**: While these methods optimize for readability and safety, some operations (like `Shuffle` or `DistinctBy`) require full enumeration of the source collection, which may have performance implications for very large datasets.
