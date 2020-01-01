#nullable enable
using FluentAssertions;
using YouTubeShortsAutomator.CLI;
using Xunit;

namespace YouTubeShortAutomator.Tests;

public sealed class CliArgumentParserEdgeCaseTests
{
    [Fact]
    public void TryParseArguments_EmptyArgs_ReturnsFalse()
    {
        var parser = new CliArgumentParser();
        var result = parser.TryParseArguments([], out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseArguments_HelpFlag_ReturnsFalse()
    {
        var parser = new CliArgumentParser();
        var result = parser.TryParseArguments(["--help"], out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseArguments_ShortHelpFlag_ReturnsFalse()
    {
        var parser = new CliArgumentParser();
        var result = parser.TryParseArguments(["-h"], out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryParseArguments_UnknownCommand_ReturnsFalse()
    {
        var parser = new CliArgumentParser();
        var result = parser.TryParseArguments(["nonexistent"], out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void RegisterCommand_DuplicateCommand_DoesNotThrow()
    {
        var parser = new CliArgumentParser();
        var cmd = new CliCommand { Description = "Test" };

        var act = () =>
        {
            parser.RegisterCommand("test", cmd);
            parser.RegisterCommand("test", cmd);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void TryParseArguments_RegisteredCommand_ReturnsTrue()
    {
        var parser = new CliArgumentParser();
        parser.RegisterCommand("process", new CliCommand { Description = "Process videos" });

        var result = parser.TryParseArguments(["process"], out var parsed);

        result.Should().BeTrue();
        parsed.Command.Should().Be("process");
    }
}
