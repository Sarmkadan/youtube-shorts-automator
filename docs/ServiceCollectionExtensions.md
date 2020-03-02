# ServiceCollectionExtensions
The `ServiceCollectionExtensions` class provides a set of extension methods for the `IServiceCollection` interface, allowing for the registration of various infrastructure components, services, and features in a .NET application. These methods simplify the process of setting up the application's service container, enabling the addition of logging, caching, HTTP clients, and other essential components.

## API
The `ServiceCollectionExtensions` class offers the following public members:
* `AddInfrastructure`: Registers the application's infrastructure components.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddRepositories`: Registers the application's data repositories.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddApplicationServices`: Registers the application's business logic services.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddHttpClients`: Registers the application's HTTP clients.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddLogging`: Registers the application's logging components.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddCaching`: Registers the application's caching components.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None
* `AddInfrastructureServices`: Registers the application's infrastructure services.
	+ Parameters: None
	+ Return value: The `IServiceCollection` instance, allowing for method chaining.
	+ Throws: None

## Usage
Here are two examples of using the `ServiceCollectionExtensions` class:
```csharp
// Example 1: Registering infrastructure components
var services = new ServiceCollection();
services.AddInfrastructure()
       .AddRepositories()
       .AddApplicationServices();

// Example 2: Registering logging and caching components
var services = new ServiceCollection();
services.AddLogging()
       .AddCaching()
       .AddHttpClients();
```

## Notes
When using the `ServiceCollectionExtensions` class, keep in mind the following:
* These methods are designed to be used in a .NET application's startup configuration, typically in the `Startup.cs` or `Program.cs` file.
* The order in which these methods are called may affect the application's behavior, as some components may depend on others being registered first.
* These methods are thread-safe, as they only modify the `IServiceCollection` instance and do not access any shared state.
* In a Dockerized application, the `AddInfrastructure` and `AddInfrastructureServices` methods may need to be called before the `AddHttpClients` method, as the infrastructure components may be required for the HTTP clients to function correctly.
* When using BenchmarkDotNet performance benchmarks, the `AddCaching` method may need to be called before the `AddLogging` method, as caching may be used to improve the performance of logging operations.
