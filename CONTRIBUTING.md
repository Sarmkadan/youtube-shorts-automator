# Contributing to YouTube Shorts Automator

Thanks for your interest in contributing. This guide covers the setup, conventions, and pull request process.

By participating you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md). Report security issues privately as described in [SECURITY.md](SECURITY.md), not in public issues.

## Prerequisites

- **.NET SDK 10.0.100 or later** (`global.json` pins the SDK with `rollForward: latestMinor`; CI uses `10.0.x`)
- **Git**
- **FFmpeg** if you run the video processing pipeline locally
- **Docker** (for `make docker-up`) or a local **SQL Server 2019+** if you run the web API against a database. Unit tests do not need a database.

## Clone and Restore

```bash
git clone https://github.com/sarmkadan/youtube-shorts-automator.git
cd youtube-shorts-automator
dotnet restore youtube-shorts-automator.slnx
```

Use your own fork if you do not have write access to the repository.

## Build

The repository root contains two project files and a solution, so a bare `dotnet build` fails with `MSB1011`. Pass the solution file:

```bash
dotnet build youtube-shorts-automator.slnx --configuration Release --no-restore
```

## Run Tests

```bash
dotnet test youtube-shorts-automator.slnx --configuration Release --no-build --verbosity normal

# A single test class
dotnet test tests/youtube-shorts-automator.Tests --filter "FullyQualifiedName~SchedulingServiceTests"
```

Tests use xUnit, FluentAssertions, and Moq. They must not touch FFmpeg, Google APIs, or the network. Fake those dependencies.

## Code Style

[`.editorconfig`](.editorconfig) is the source of truth:

- UTF-8, spaces (4 for C#), final newline, no trailing whitespace
- Maximum line length of 120 columns
- Allman braces: the opening brace goes on its own line
- `PascalCase` for public members, `camelCase` for private fields, `I` prefix for interfaces
- Nullable reference types and implicit usings are enabled; keep new code warning-free

Other conventions:

- Each source file starts with the project's author header block. Copy it from an existing file.
- Keep one type per file. Put validation and JSON mapping in sibling files (`FooValidation.cs`, `FooJsonExtensions.cs`) rather than in the core type.
- Respect the layering: Controllers, then Application services, then repository interfaces, then Infrastructure implementations. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
- Throw domain exceptions from `Domain/Exceptions`; the error middleware maps them to HTTP responses.
- Test classes are named `<Subject>Tests`.

## Branch Naming

Branch from an up-to-date `main`. Use lowercase, hyphen-separated names with a type prefix:

- `feature/<short-name>`, for example `feature/recurring-schedules`
- `fix/<short-name>`
- `refactor/<short-name>`
- `docs/<short-name>`

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<optional scope>): <summary>

<optional body: what changed and why>

<optional footer: Closes #123>
```

- Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`
- Summary: imperative mood, lowercase, no trailing period, 72 characters or fewer
- Separate the body from the summary with a blank line
- Explain *what* and *why*. The diff shows *how*.

Example:

```
feat(scheduling): add recurring upload schedules

Uploads can now repeat on a daily or weekly pattern instead of
requiring one schedule entry per video.

Closes #123
```

## Pull Requests

1. **Keep it focused.** One concern per PR. Split unrelated changes.
2. **Build and test locally** with the commands above before you push.
3. **Push your branch and open a PR against `main`.**
4. **Title:** use the commit message summary format, for example `feat(scheduling): add recurring upload schedules`.
5. **Description:** explain what changed and why, how you tested it, and any follow-up work. Link related issues with `Closes #123` or `Related to #456`.
6. **Add tests** for new behavior and bug fixes.
7. **Update docs** when you change public behavior. For user-visible changes, add an entry under `[Unreleased]` in [CHANGELOG.md](CHANGELOG.md) (Keep a Changelog format).
8. **CI must pass.** The build and test workflows in `.github/workflows/` run on pull requests to `main`. A PR with failing checks will not be merged.
9. **Address review comments** in new commits. Rebase only before review starts, or when asked.

## Questions

Open a [GitHub issue](https://github.com/sarmkadan/youtube-shorts-automator/issues). Search existing issues first.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
