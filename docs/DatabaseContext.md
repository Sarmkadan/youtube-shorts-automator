# DatabaseContext
The `DatabaseContext` class is designed to provide a convenient interface for interacting with a database, allowing users to execute commands, retrieve data, and manage connections in a straightforward and efficient manner. It serves as a central point for database operations, encapsulating the underlying complexity and providing a simple, async-friendly API.

## API
* `public DatabaseContext`: The constructor for the `DatabaseContext` class, used to create a new instance.
* `public async Task<SqlConnection> GetConnectionAsync`: Retrieves a `SqlConnection` object asynchronously. This method is used to obtain a connection to the database, which can then be used for various database operations. It does not take any parameters and returns a `SqlConnection` object wrapped in a `Task`. It may throw exceptions related to database connection failures.
* `public async Task<int> ExecuteCommandAsync`: Executes a database command asynchronously and returns the number of rows affected. This method takes no parameters and returns an integer value wrapped in a `Task`, indicating the number of rows affected by the command. It may throw exceptions related to command execution failures.
* `public async Task<T?> ExecuteScalarAsync<T>`: Executes a database query that returns a single value asynchronously. This method takes no parameters and returns a value of type `T` wrapped in a `Task`, which represents the scalar value returned by the query. It may throw exceptions related to query execution failures.
* `public async Task<DataTable> ExecuteDataTableAsync`: Retrieves a `DataTable` object from the database asynchronously. This method takes no parameters and returns a `DataTable` object wrapped in a `Task`, which contains the data retrieved from the database. It may throw exceptions related to data retrieval failures.
* `public async ValueTask DisposeAsync`: Disposes of the `DatabaseContext` instance asynchronously, releasing any unmanaged resources. This method takes no parameters and returns a `ValueTask`, indicating the completion of the disposal operation. It does not throw exceptions.

## Usage
The following examples demonstrate how to use the `DatabaseContext` class:
```csharp
// Example 1: Executing a command
var context = new DatabaseContext();
await context.ExecuteCommandAsync();
```

```csharp
// Example 2: Retrieving data
var context = new DatabaseContext();
var dataTable = await context.ExecuteDataTableAsync();
foreach (DataRow row in dataTable.Rows)
{
    Console.WriteLine(row["column_name"]);
}
```

## Notes
When using the `DatabaseContext` class, it is essential to consider the following:
* The class is designed to be used asynchronously, and all methods return `Task` or `ValueTask` objects. This allows for efficient and non-blocking database operations.
* The `DisposeAsync` method should be called when the `DatabaseContext` instance is no longer needed to ensure proper disposal of unmanaged resources.
* The class is not thread-safe by default. If multiple threads need to access the same `DatabaseContext` instance, proper synchronization mechanisms should be implemented to avoid concurrency issues.
* Error handling is crucial when working with database operations. The `DatabaseContext` class may throw exceptions related to connection failures, command execution errors, or data retrieval issues. These exceptions should be caught and handled accordingly to ensure robust and reliable database interactions.
