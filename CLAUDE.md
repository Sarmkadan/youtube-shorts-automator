# CLAUDE.md

YouTube Shorts upload automation on .NET 10: an ASP.NET Core web API (repo root) plus an older console pipeline (`src/`), with FFmpeg processing, YouTube Data API v3 upload, SQL Server persistence, scheduling and analytics.

## Build

Requires .NET SDK 10.0.100+ (`global.json`, rollForward latestMinor).

```bash
dotnet restore                                   # whole solution
dotnet build --configuration Release --no-restore
make build                                       # web API only (YouTubeShortsAutomator.csproj, Release)
make run / make debug                            # run web API (Release / Debug)
dotnet run --project youtube-shorts-automator.csproj   # console pipeline app
make docker / make docker-up / make docker-down  # Docker image + compose (app :5000, SQL :1433, Redis :6379)
```

Solution: `youtube-shorts-automator.sln` (4 projects). Two csproj files share the repo root; `Directory.Build.props` gives each its own `obj/<Project>/` and `bin/<Project>/` to avoid collisions. Do not remove the `Compile Remove` globs in either csproj.

## Test

```bash
dotnet test --configuration Release --no-build --verbosity normal   # CI command
make test
make coverage                                    # /p:CollectCoverage=true, writes coverage.xml
dotnet test tests/youtube-shorts-automator.Tests --filter "FullyQualifiedName~SchedulingServiceTests"
```

Test project: `tests/youtube-shorts-automator.Tests/` - xUnit 2.9, FluentAssertions 7, Moq 4.20. It references the console project (`youtube-shorts-automator.csproj`), not the web API. Benchmarks: `benchmarks/youtube-shorts-automator.Benchmarks/` (BenchmarkDotNet).

## Lint / format

```bash
make lint      # dotnet build /p:EnforceCodeStyleInBuild=true
make format    # dotnet format YouTubeShortsAutomator.csproj
make audit     # dotnet list package --vulnerable
```

Style comes from `.editorconfig`: 4-space indent, 120 col, Allman braces (`csharp_new_line_before_open_brace = all`), PascalCase public members, camelCase private, `I` prefix on interfaces. `TreatWarningsAsErrors` is false.

## Key directories and entry points

Web API (`YouTubeShortsAutomator.csproj`, namespace `YouTubeShortsAutomator`):

- `Program.cs` - composition root (Serilog, controllers, Swagger in Development, `EnsureCreatedAsync` on startup, no EF migrations)
- `Controllers/`, `API/` - MVC controllers (Video, Processing, Schedule, Analytics, Metrics, Webhook, Health, System, Configuration, JobStatus)
- `Application/Services`, `Application/Repositories` - service and repository interfaces + application services
- `Domain/` - entities, enums, constants, domain exceptions
- `Infrastructure/` - `ApplicationDbContext` (EF Core, SQL Server), repository implementations, `AddInfrastructureServices`
- `Extensions/ServiceCollectionExtensions.cs` - `AddApplicationServices`, `AddBackgroundServices`, `AddRateLimitingOptions`
- `BackgroundServices/` - `ProcessingBackgroundService` (DB-table-as-queue poller), `CleanupBackgroundService`; `AnalyticsBackgroundService` exists but is not registered
- `Integration/` - all outbound calls: `FFmpegWrapper`, `GoogleApiClient`, `WebhookPublisher`, custom `DefaultHttpClientFactory` (not `System.Net.Http.IHttpClientFactory`)
- `Middleware/` - order is RequestLogging -> ErrorHandling -> RateLimiting -> ApiKeyValidation (`UseApplicationMiddleware`)
- `Events/`, `Caching/`, `Metrics/`, `Formatters/`, `Utilities/` - in-process pub/sub, `ICacheService` over IMemoryCache, homegrown metrics, JSON/CSV formatters, static helpers
- `appsettings.json` - config; `RateLimit:*` keys, connection strings

Console app (`youtube-shorts-automator.csproj`, namespace `YouTubeShortAutomator` - no "s", intentional):

- `src/Program.cs` - manual `ServiceCollection`, hand-parsed sub-commands (e.g. `history`)
- `src/Services/` - pipeline stages, `JobOrchestrationService` coordinator, Quartz-based `SchedulingService`
- `src/Data/` - raw `System.Data.SqlClient` repositories
- `src/Configuration/`, `src/Constants/`, `src/Exceptions/`

The two apps share no code by design. `docs/ARCHITECTURE.md` is the authoritative architecture description; `docs/architecture.md` is outdated.

CI: `.github/workflows/` (build.yml matrix on ubuntu/windows/macos, ci.yml, codeql, docker, nuget-publish, release). Branches: `main`, `develop`.

## Conventions

- File-per-concern split: each type `Foo.cs` has siblings `FooExtensions.cs`, `FooValidation.cs`, `FooJsonExtensions.cs`. Keep validation and JSON mapping out of the core type; follow this shape when adding types.
- Dependency direction: Controllers -> Application services -> repository interfaces -> Infrastructure implementations. Domain has no outward dependencies.
- New API area: controller + service/interface in `Application/Services` + repository interface in `Application/Repositories` + impl in `Infrastructure/Repositories` + register in `AddInfrastructureServices`.
- New outbound integration: interface + impl in `Integration/`, register in `AddApplicationServices`, fake it in tests. Tests never touch ffmpeg, Google or the network.
- New job type: extend `ProcessingJobType`, handle in `VideoProcessingService.ProcessVideoAsync`; the worker picks it up automatically.
- Throw domain exceptions (`Domain/Exceptions`); `ErrorHandlingMiddleware` maps them to responses.
- Every source file starts with the `Author: Vladyslav Zaiets | https://sarmkadan.com` header block.
- Nullable and ImplicitUsings enabled everywhere; `GlobalUsings.cs` per project.
- Tests: class `<Subject>Tests`, xUnit `[Fact]`/`[Theory]`, FluentAssertions `.Should()`, Moq for interfaces.
- Config keys and defaults live in `AppDefaults` constants, not string literals.
