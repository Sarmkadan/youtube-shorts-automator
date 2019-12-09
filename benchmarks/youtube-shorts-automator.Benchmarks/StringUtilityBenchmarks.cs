// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Benchmarks for string manipulation hot paths used throughout the processing pipeline.
/// Covers truncation, slug generation, and case-conversion routines that run
/// on every video title / metadata field touched by the upload service.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class StringUtilityBenchmarks
{
    // Realistic video title length — 88 chars
    private const string LongTitle =
        "My Amazing YouTube Short Video With Extra Words For Testing Performance Characteristics";

    // 10-word hyphenated slug as typically returned by a CMS or tag system
    private const string HyphenatedInput =
        "my-video-title-with-multiple-words-for-camel-case-conversion";

    // Mixed-case slug candidate with symbols and numbers
    private const string SlugInput =
        "Hello World! This is a Test String with Symbols 123 & Special Chars";

    [Benchmark(Description = "Truncate 88-char title to 30")]
    public string Truncate() =>
        StringUtility.Truncate(LongTitle, 30);

    [Benchmark(Description = "CamelCase — 10-word hyphenated slug")]
    public string ToCamelCase() =>
        StringUtility.ToCamelCase(HyphenatedInput);

    [Benchmark(Description = "PascalCase — 10-word hyphenated slug")]
    public string ToPascalCase() =>
        StringUtility.ToPascalCase(HyphenatedInput);

    [Benchmark(Description = "ToSlug — 69-char mixed-symbol string")]
    public string ToSlug() =>
        StringUtility.ToSlug(SlugInput);

    [Benchmark(Description = "SplitByLength — 88 chars into 10-char chunks")]
    public string[] SplitByLength() =>
        StringUtility.SplitByLength(LongTitle, 10);

    [Benchmark(Description = "RemoveWhitespace — 88-char title")]
    public string RemoveWhitespace() =>
        StringUtility.RemoveWhitespace(LongTitle);

    [Benchmark(Description = "NormalizeWhitespace — 88-char title")]
    public string NormalizeWhitespace() =>
        StringUtility.NormalizeWhitespace(LongTitle);
}
