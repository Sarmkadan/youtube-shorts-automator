# CliArgumentParser

The `CliArgumentParser` class provides a structured mechanism for defining, registering, and parsing command-line arguments within the `youtube-shorts-automator` application. It facilitates the creation of CLI interfaces by enabling the definition of supported commands, required and optional parameters, and asynchronous execution handlers.

## API

### Constructors

#### `CliArgumentParser()`
Initializes a new instance of the `CliArgumentParser` class.

### Methods

#### `void RegisterCommand(string command, string description, Func<ParsedCliArguments, Task<int>> handler)`
Registers a new command with the parser.
*   **Parameters:** 
    *   `command`: The name of the command to register.
    *   `description`: A brief description of the command's functionality.
    *   `handler`: An asynchronous function to execute if the command is successfully parsed.
*   **Remarks:** This method registers the command and associates it with a specific execution logic.

#### `bool TryParseArguments(string[] args)`
Attempts to parse the provided command-line arguments.
*   **Parameters:** `args`: An array of command-line arguments strings.
*   **Returns:** `true` if the arguments were parsed successfully according to the registered configuration; otherwise, `false`.

#### `bool TryGetOption(string key, out string value)`
Attempts to retrieve the value of a specific option.
*   **Parameters:** 
    *   `key`: The name of the option.
    *   `value`: When this method returns, contains the value associated with the option if found; otherwise, an empty string.
*   **Returns:** `true` if the option exists and was successfully retrieved; otherwise, `false`.

#### `string GetOption(string key)`
Retrieves the value of a specific option.
*   **Parameters:** `key`: The name of the option.
*   **Returns:** The string value of the option.
*   **Throws:** `KeyNotFoundException` if the option is not present.

#### `bool GetBoolOption(string key)`
Checks for the presence of a boolean flag option.
*   **Parameters:** `key`: The name of the option.
*   **Returns:** `true` if the flag is present; otherwise, `false`.

### Properties

*   **`string Description`**: Gets or sets the description for the command-line interface or registered command.
*   **`List<string> RequiredOptions`**: Gets or sets the list of mandatory options that must be provided for the command to succeed.
*   **`List<string> OptionalOptions`**: Gets or sets the list of optional flags or parameters.
*   **`Func<ParsedCliArguments, Task<int>>? Handler`**: The asynchronous handler function invoked if the command is successfully parsed.
*   **`string Command`**: Gets or sets the name of the registered command.
*   **`Dictionary<string, string> Options`**: A dictionary containing the key-value pairs of parsed options.
*   **`List<string> PositionalArguments`**: A list containing positional arguments extracted during the parsing process.

## Usage

### Basic Command Setup

```csharp
var parser = new CliArgumentParser();
parser.RegisterCommand("process", "Processes a video file.", async (args) => {
    Console.WriteLine("Processing started...");
    return await Task.FromResult(0);
});

if (parser.TryParseArguments(args))
{
    // Execution flow handled by the registered handler
}
```

### Retrieving Options and Positional Arguments

```csharp
var parser = new CliArgumentParser();
// Assuming command configuration is done here

if (parser.TryParseArguments(args))
{
    if (parser.TryGetOption("input", out string inputFile))
    {
        Console.WriteLine($"Input file: {inputFile}");
    }

    bool verbose = parser.GetBoolOption("verbose");
    
    foreach (var arg in parser.PositionalArguments)
    {
        Console.WriteLine($"Positional argument: {arg}");
    }
}
```

## Notes

*   **Thread Safety**: `CliArgumentParser` is not designed to be thread-safe. Instances should not be shared across threads during the parsing or registration phases.
*   **Parsing Logic**: `TryParseArguments` expects a standard CLI argument format. If required options defined in `RequiredOptions` are missing from the input, the parsing operation will fail, returning `false`.
*   **Handler Execution**: The `Handler` is only invoked if `TryParseArguments` returns `true`. It is the responsibility of the caller to invoke the handler if the automatic execution flow is not utilized.
