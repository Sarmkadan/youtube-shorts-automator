## [Unreleased]
### Added
- Startup validation for `VideoProcessor` and `ThumbnailGenerator` options (`IOptions<T>` with `ValidateOnStart`): FFmpeg/FFprobe paths, writable output directory, max duration 1-60 s, 9:16 target resolution, thumbnail quality 1-100. Misconfiguration stops the app at startup with a message naming each setting.
### Fixed
- Startup failures are logged before the process exits, instead of `Environment.Exit` dropping the message

## [2.0.2] - 2026-05-27
### Fixed
- Fix upload retry not resuming from last chunk on network timeout
- Added regression test for the fix

## [2.0.1] - 2026-05-26
### Security
- Added input validation and length limits to prevent potential DoS from overly long strings
- Added request timeout configuration to HttpClient calls to prevent hanging requests
- Added CancellationToken parameters to async methods without timeout handling
- Added security policy and vulnerability reporting process
- Enhanced logging to prevent accidental credential exposure in logs

## [2.0.0] - 2026-05-18
### Added
- Add content calendar with smart title/description optimization
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x
### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency
### Fixed
- Various edge cases found through testing

## [2.0.1] - 2026-05-26
### Security
- Added input validation and length limits to prevent potential DoS from overly long strings
- Added request timeout configuration to HttpClient calls to prevent hanging requests
- Added CancellationToken parameters to async methods without timeout handling
- Added security policy and vulnerability reporting process
- Enhanced logging to prevent accidental credential exposure in logs