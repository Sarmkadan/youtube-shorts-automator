# Architecture

This document describes the solution as it actually exists in the repo today - what compiles,
how it is wired, and why some things look the way they do. For the older, more aspirational
guide see [architecture.md](architecture.md); this file is the source of truth.

## Solution layout

The solution (`youtube-shorts-automator.sln`) contains four projects:

| Project | Location | What it is |
|---|---|---|
| `YouTubeShortsAutomator` | repo root | ASP.NET Core web API (net10.0) - the main application |
| `youtube-shorts-automator` | `src/` | Console pipeline app (net10.0) - the original CLI iteration |
| `youtube-shorts-automator.Tests` | `tests/` | xUnit test project |
| `youtube-shorts-automator.Benchmarks` | `benchmarks/` | BenchmarkDotNet project for the utility classes |

Two application projects share the repo root, which is unusual and worth explaining:

- The **web API** (`YouTubeShortsAutomator.csproj`) compiles everything at the root except
  `src/`, `tests/`, `benchmarks/` (explicit `Compile Remove` globs in the csproj).
- The **console app** (`youtube-shorts-automator.csproj`) does the inverse: it removes all
  root sources and compiles only `src/**/*.cs` plus `GlobalUsings.cs`. It has its own
  namespace root (`YouTubeShortAutomator`, no "s") to avoid type collisions.

**Rationale / trade-off:** the console app came first (video pipeline as a one-shot process,
Quartz for scheduling, raw `SqlClient` for persistence). The web API was built as the second
phase with a proper layered structure, EF Core and hosted services. Keeping both in one repo
preserves history and lets the CLI remain a working reference implementation, at the cost of
csproj glob gymnastics and two parallel service sets with overlapping names
(e.g. two `VideoProcessingService`, two `SchedulingService`). If the CLI is ever retired,
delete `src/` and the second csproj and the glob excludes can go away.

## Web API (root project)

### Layering

```
Controllers/, API/          HTTP surface (ASP.NET Core MVC controllers)
Application/                service interfaces + application services, repository interfaces
Domain/                     entities, enums, domain exceptions, constants
Infrastructure/             EF Core DbContext, repository implementations, DI registration
BackgroundServices/         IHostedService workers
Integration/                outbound adapters: FFmpeg, Google API, webhooks, HttpClient factory
Middleware/                 request logging, error handling, rate limiting, API-key validation
Events/, Caching/, Metrics/ in-process eventing, IMemoryCache wrapper, metrics collector
Utilities/, Formatters/     static helpers, JSON/CSV output formatters
```

Dependencies point inward: Controllers -> Application services -> repository interfaces
(`Application/Repositories/I*Repository.cs`) -> implemented in `Infrastructure/Repositories`
over `ApplicationDbContext` (EF Core, SQL Server provider). Domain models have no outward
dependencies.

### Composition root

`Program.cs` is a minimal-hosting top-level file. Wiring is split into three extension
entry points so the root stays readable:

- `Infrastructure/Extensions/ServiceCollectionExtensions.AddInfrastructureServices` -
  DbContext, repositories, the core application services (video processing, upload,
  analytics, scheduling, configuration, credentials).
- `Extensions/ServiceCollectionExtensions.AddApplicationServices` - cross-cutting pieces:
  `ICacheService` (IMemoryCache-backed), formatters, `IFFmpegWrapper`, `IWebhookPublisher`,
  the custom `Integration.IHttpClientFactory`, `IEventPublisher`.
- `AddBackgroundServices` - hosted workers.

Serilog is configured before the host builds (console + rolling file sink under `logs/`),
so startup failures are still captured; the whole startup is wrapped in
try/catch/`Log.CloseAndFlush()`.

The database is created with `EnsureCreatedAsync()` at startup, not migrations.
**Trade-off:** zero-friction local/dev bootstrap, but no schema evolution path - moving to
`Database.Migrate()` with EF migrations is the known next step before any production use.

### Request pipeline

`UseApplicationMiddleware()` applies custom middleware in a deliberate order:

1. `RequestLoggingMiddleware` - log everything, even requests that later fail
2. `ErrorHandlingMiddleware` - catch-all so lower middleware/controllers can throw domain
   exceptions (`Domain/Exceptions`) and get consistent error responses
3. `RateLimitingMiddleware` - fixed-window limiter, window/limit come from
   `RateLimit:*` configuration (defaults 100 req / 60 s), options registered as a
   singleton via `AddRateLimitingOptions`
4. `ApiKeyValidationMiddleware` - simple shared-key auth, applied last so rejected
   requests are still logged and rate-counted

### Background processing

Job execution is decoupled from HTTP by a polling worker model:

- Controllers (`ProcessingController`, `VideoController`) create `ProcessingJob` rows in
  Pending state via the repositories.
- `ProcessingBackgroundService` loops: create a DI scope, pick up queued jobs, run them,
  `Task.Delay(checkInterval)`. Errors are logged and the loop backs off 10 s rather than
  crashing the host.
- `CleanupBackgroundService` prunes temp files and stale job records on its own interval.

**Rationale:** a DB-table-as-queue with a polling worker is the simplest thing that
survives restarts (state is in SQL Server, not in memory). The trade-off is latency
(bounded by the poll interval) and no competing-consumers story; if throughput ever
demands it, the seam to replace is `ProcessQueuedJobsAsync` - swap the poll for a real
queue without touching controllers.

Note: `BackgroundServices/AnalyticsBackgroundService.cs` exists but is **not registered**
in `AddBackgroundServices` - analytics aggregation currently only happens on demand through
`AnalyticsService`. Wire it up there if periodic aggregation is wanted.

### Eventing, caching, metrics

- `Events/` implements a small in-process pub/sub (`IEventPublisher` singleton,
  `VideoProcessingEvents`, handlers in `EventHandlers.cs`). It is intentionally not a
  message bus: same-process, best-effort, used to decouple side effects (webhook
  notifications, metrics) from the main job flow.
- `Caching/CacheService` wraps `IMemoryCache` behind `ICacheService` so callers are not
  tied to the Microsoft cache API and a distributed cache can be substituted later.
- `Metrics/MetricsCollector` (`IMetricsCollector`, singleton) collects counters exposed
  through `MetricsController` - deliberately homegrown instead of pulling in
  OpenTelemetry for what is currently a handful of counters.

### Outbound integrations

All process-external calls live in `Integration/` behind interfaces:

- `FFmpegWrapper` (`IFFmpegWrapper`) - shells out to ffmpeg for transcode/thumbnail work;
  the only place process invocation happens.
- `GoogleApiClient` - YouTube Data API v3 via `Google.Apis.YouTube.v3`.
- `WebhookPublisher` (`IWebhookPublisher`) - outbound HTTP notifications.
- `DefaultHttpClientFactory` (custom `Integration.IHttpClientFactory`) - predates use of
  the framework factory; kept because tests fake it easily. Do not confuse it with
  `System.Net.Http.IHttpClientFactory`.

This is the main testability boundary: unit tests fake these four interfaces and never
touch ffmpeg, Google or the network.

### A note on the file-per-concern pattern

Most types are split across sibling files: `Foo.cs`, `FooExtensions.cs`,
`FooValidation.cs`, `FooJsonExtensions.cs`. Validation and JSON mapping are kept out of
the core type so the entity files stay small and serialization concerns never leak into
domain logic. The cost is file count; the convention is mechanical, so follow it when
adding types rather than inventing a new shape.

## Console app (`src/`)

A self-contained pipeline runner with its own composition root (`src/Program.cs`,
manual `ServiceCollection`, no generic host):

- `Services/` - pipeline stages: `FileValidationService`, `VideoProcessingService`,
  `MetadataService`, `ThumbnailGeneratorService`, `ThumbnailAbTestService`,
  `TitleOptimizationEngine`, `YouTubeUploadService`, `AnalyticsService`,
  `ChannelService`, `ContentCalendarService`, with `JobOrchestrationService`
  coordinating and `SchedulingService` on Quartz cron triggers.
- `Data/` - repositories on raw `System.Data.SqlClient`; `UploadHistoryRepository`
  creates its own table on startup (`EnsureTableExistsAsync`).
- Sub-commands are parsed by hand in `Program.Main` (e.g. `history` prints the upload
  log); there is no framework CLI parser.

It shares no code with the web API - duplication was accepted over a shared library to
keep the two projects independently deletable.

## Extension points

- **New API area:** controller in `Controllers/` or `API/`, service + interface in
  `Application/Services`, repository interface in `Application/Repositories`,
  implementation in `Infrastructure/Repositories`, register in
  `AddInfrastructureServices`.
- **New job type:** extend `ProcessingJobType`, handle it in
  `VideoProcessingService.ProcessVideoAsync`; the background worker picks it up with no
  further wiring.
- **New outbound integration:** interface + implementation in `Integration/`, register in
  `AddApplicationServices`, fake it in tests.
- **Replacing the queue:** implement pickup/dispatch inside
  `ProcessingBackgroundService` against a broker; job creation call sites are already
  repository-only.

## Known limitations

- `EnsureCreatedAsync` instead of EF migrations - no schema upgrade path yet.
- `AnalyticsBackgroundService` is written but not registered.
- Rate limiting is per-instance, in-memory - multiple replicas would not share a window.
- The in-process event bus gives no delivery guarantees; a crash loses queued handlers.
- Several services are registered by concrete type rather than interface, which makes
  controller unit tests heavier than they need to be (being tightened incrementally).
- Two projects rooted in one directory means IDE tooling occasionally attributes a file
  to the wrong project; check the `Compile` globs in both csproj files when in doubt.
