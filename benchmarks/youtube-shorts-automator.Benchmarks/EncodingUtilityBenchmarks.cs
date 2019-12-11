// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using YouTubeShortsAutomator.Utilities;

namespace YouTubeShortsAutomator.Benchmarks;

/// <summary>
/// Benchmarks for encoding and hashing utilities called on every cache key,
/// API credential verification, and upload token generation in the pipeline.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class EncodingUtilityBenchmarks
{
    // Typical cache key composed from channel id + video id + timestamp
    private const string MetadataKey =
        "yt-short:channel-42:video-2026-05-14T12:00:00Z:processed";

    // Bearer token payload length representative of OAuth responses
    private const string BearerPayload =
        "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6InVzZXItdG9rZW4ifQ";

    [Benchmark(Description = "SHA-256 — 58-char metadata cache key")]
    public string Sha256Hash() =>
        EncodingUtility.ComputeSha256Hash(MetadataKey);

    [Benchmark(Description = "MD5 — 58-char metadata cache key")]
    public string Md5Hash() =>
        EncodingUtility.ComputeMd5Hash(MetadataKey);

    [Benchmark(Description = "Base64 encode — 55-char bearer payload")]
    public string EncodeBase64() =>
        EncodingUtility.EncodeBase64(BearerPayload);

    [Benchmark(Description = "Base64 decode — 55-char bearer payload")]
    public string DecodeBase64() =>
        EncodingUtility.DecodeBase64(BearerPayload);

    [Benchmark(Description = "Random string — 32-char upload token")]
    public string GenerateRandomString() =>
        EncodingUtility.GenerateRandomString(32);

    [Benchmark(Description = "Random hex — 32-char request correlation id")]
    public string GenerateRandomHexString() =>
        EncodingUtility.GenerateRandomHexString(32);
}
