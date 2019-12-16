// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace YouTubeShortsAutomator.CLI;

/// <summary>
/// Parses command-line arguments for CLI operations
/// Supports flags, options, and positional arguments with validation
/// </summary>
public class CliArgumentParser
{
    private readonly Dictionary<string, CliCommand> _commands;
    private readonly Dictionary<string, string> _parsedArgs;

    public CliArgumentParser()
    {
        _commands = new Dictionary<string, CliCommand>();
        _parsedArgs = new Dictionary<string, string>();
    }

    public void RegisterCommand(string commandName, CliCommand command)
    {
        if (!_commands.ContainsKey(commandName))
        {
            _commands[commandName] = command;
        }
    }

    public bool TryParseArguments(string[] args, out ParsedCliArguments result)
    {
        result = new ParsedCliArguments();

        if (args.Length == 0)
        {
            PrintHelp();
            return false;
        }

        var command = args[0];

        if (command == "--help" || command == "-h")
        {
            PrintHelp();
            return false;
        }

        if (!_commands.TryGetValue(command, out var cliCommand))
        {
            Console.WriteLine($"Unknown command: {command}");
            PrintHelp();
            return false;
        }

        result.Command = command;

        // Parse remaining arguments
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--"))
            {
                var parts = arg.Substring(2).Split('=');
                var key = parts[0];
                var value = parts.Length > 1 ? parts[1] : (i + 1 < args.Length ? args[++i] : "true");
                result.Options[key] = value;
            }
            else if (arg.StartsWith("-"))
            {
                var key = arg.Substring(1);
                var value = i + 1 < args.Length ? args[++i] : "true";
                result.Options[key] = value;
            }
            else
            {
                result.PositionalArguments.Add(arg);
            }
        }

        return ValidateArguments(result, cliCommand);
    }

    private bool ValidateArguments(ParsedCliArguments args, CliCommand command)
    {
        // Validate required options
        foreach (var required in command.RequiredOptions)
        {
            if (!args.Options.ContainsKey(required))
            {
                Console.WriteLine($"Missing required option: --{required}");
                return false;
            }
        }

        return true;
    }

    private void PrintHelp()
    {
        Console.WriteLine("YouTube Shorts Automator CLI");
        Console.WriteLine("============================");
        Console.WriteLine();
        Console.WriteLine("Usage: youtube-shorts-automator [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");

        foreach (var cmd in _commands)
        {
            Console.WriteLine($"  {cmd.Key,-20} {cmd.Value.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help, -h          Show this help message");
        Console.WriteLine("  --version, -v       Show version information");
    }
}

public class CliCommand
{
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredOptions { get; set; } = new();
    public List<string> OptionalOptions { get; set; } = new();
    public Func<ParsedCliArguments, Task<int>>? Handler { get; set; }
}

public class ParsedCliArguments
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, string> Options { get; set; } = new();
    public List<string> PositionalArguments { get; set; } = new();

    public bool TryGetOption(string key, out string value)
    {
        return Options.TryGetValue(key, out value!);
    }

    public string GetOption(string key, string defaultValue = "")
    {
        return Options.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public bool GetBoolOption(string key, bool defaultValue = false)
    {
        if (Options.TryGetValue(key, out var value))
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("1", StringComparison.OrdinalIgnoreCase);
        }
        return defaultValue;
    }
}
