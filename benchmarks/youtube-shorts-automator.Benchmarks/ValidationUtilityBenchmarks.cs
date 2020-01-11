// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Benchmarks for validation utilities, measuring performance of regex-based 
/// validation of email addresses, URLs, and YouTube IDs.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ValidationUtilityBenchmarks
{
    private const string ValidEmail = "user@example.com";
    private const string InvalidEmail = "invalid-email";
    
    private const string ValidUrl = "https://example.com/video";
    private const string InvalidUrl = "ftp://invalid-url";
    
    private const string ValidChannelId = "UC123456789012345678901Q";
    private const string ValidVideoId = "aBcDeFgHiJk";

    [Benchmark(Description = "ValidateEmail — valid")]
    public (bool IsValid, string? Error) ValidateEmail_Valid() =>
        ValidationUtility.ValidateEmail(ValidEmail);

    [Benchmark(Description = "ValidateEmail — invalid")]
    public (bool IsValid, string? Error) ValidateEmail_Invalid() =>
        ValidationUtility.ValidateEmail(InvalidEmail);

    [Benchmark(Description = "ValidateUrl — valid")]
    public (bool IsValid, string? Error) ValidateUrl_Valid() =>
        ValidationUtility.ValidateUrl(ValidUrl);

    [Benchmark(Description = "ValidateChannelId — valid")]
    public (bool IsValid, string? Error) ValidateYouTubeChannelId_Valid() =>
        ValidationUtility.ValidateYouTubeChannelId(ValidChannelId);

    [Benchmark(Description = "ValidateVideoId — valid")]
    public (bool IsValid, string? Error) ValidateYouTubeVideoId_Valid() =>
        ValidationUtility.ValidateYouTubeVideoId(ValidVideoId);
}
