# Contributing to YouTube Shorts Automator

We appreciate your interest in contributing! This document provides guidelines for participating in the project.

## Code of Conduct

Please review our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing. All contributors are expected to follow these guidelines.

## Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **Git** for version control
- **FFmpeg 4.0+** for video processing
- **SQL Server 2019+** or LocalDB (for development)

### Development Setup

1. **Fork the Repository**
   ```bash
   # Click "Fork" on GitHub
   ```

2. **Clone Your Fork**
   ```bash
   git clone https://github.com/YOUR-USERNAME/youtube-shorts-automator.git
   cd youtube-shorts-automator
   ```

3. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/sarmkadan/youtube-shorts-automator.git
   git fetch upstream
   ```

4. **Install Dependencies**
   ```bash
   dotnet restore
   ```

5. **Build the Project**
   ```bash
   dotnet build
   ```

6. **Verify Setup**
   ```bash
   dotnet test
   ```

## Making Changes

### Create a Feature Branch

Always create a new branch for your work:

```bash
git checkout -b feature/YourFeatureName
# or
git checkout -b bugfix/YourBugFixName
```

Use descriptive branch names:
- `feature/` prefix for new features
- `bugfix/` prefix for bug fixes
- `docs/` prefix for documentation updates
- `refactor/` prefix for code refactoring

### Code Style

Follow these conventions to maintain consistency:

#### C# Coding Standards

- **Naming**: Use PascalCase for public members, camelCase for private members
- **Async/Await**: Use async/await for all I/O operations
- **Methods**: Keep methods focused, preferably under 50 lines
- **Comments**: Only comment non-obvious logic; code should be self-documenting
- **XML Documentation**: Public APIs require XML documentation comments:

```csharp
/// <summary>
/// Processes a video file with the specified profile.
/// </summary>
/// <param name="inputPath">Path to the input video file</param>
/// <param name="outputPath">Path for the processed output</param>
/// <param name="profile">Processing profile with encoding settings</param>
/// <returns>Path to the processed video file</returns>
/// <exception cref="VideoProcessingException">Thrown when processing fails</exception>
public async Task<string> TranscodeVideoAsync(
    string inputPath,
    string outputPath,
    ProcessingProfile profile)
```

#### Code Organization

- One public class per file (with rare exceptions for tightly-coupled types)
- Logical grouping within files: properties, constructors, public methods, private methods
- Keep using statements organized and minimal
- Use appropriate access modifiers (public, internal, private)

#### Architecture Patterns

- **Dependency Injection**: Use constructor injection; avoid service locators
- **Async All the Way**: Async methods should be fully async (no blocking calls)
- **Repository Pattern**: Implement interfaces for data access abstraction
- **SOLID Principles**: Follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion

### Commit Messages

Write clear, descriptive commit messages:

```
Add ability to schedule uploads with recurring patterns

- Implement RecurrencePattern domain model
- Add SchedulingService.ScheduleRecurringUploadAsync method
- Add database migration for recurring schedules table
- Update API documentation

Closes #123
```

Guidelines:
- First line is a concise summary (50 characters or less)
- Leave a blank line before the body
- Reference related issues: `Closes #123` or `Related to #456`
- Explain *what* and *why*, not *how* (the code shows how)

## Testing

### Test Requirements

All contributions must include tests. We aim for high code coverage.

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter="FullyQualifiedName~VideoProcessingServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Test Guidelines

- **Test Naming**: Use clear names describing what is being tested:
  - `TranscodeVideoAsync_WithValidInput_ReturnsProcessedPath`
  - `ScheduleUploadAsync_WithPastTime_ThrowsArgumentException`

- **Arrange-Act-Assert Pattern**:
  ```csharp
  [Fact]
  public async Task TranscodeVideoAsync_WithValidProfile_SuccessfullyProcessesVideo()
  {
      // Arrange
      var inputPath = "test_input.mp4";
      var outputPath = "test_output.mp4";
      var profile = new ProcessingProfile { VideoWidth = 1080, VideoHeight = 1920 };
      
      // Act
      var result = await _service.TranscodeVideoAsync(inputPath, outputPath, profile);
      
      // Assert
      Assert.NotNull(result);
      Assert.True(File.Exists(result));
  }
  ```

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **Mock External Dependencies**: Use mocks for YouTube API, FFmpeg, database calls

## Pull Request Process

### Before Submitting

1. **Update Branches**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Build and Test Locally**
   ```bash
   dotnet build
   dotnet test
   ```

3. **Check Code Quality**
   - Verify no compiler warnings
   - Ensure proper error handling
   - Review for security issues

### Submitting a Pull Request

1. **Push Your Branch**
   ```bash
   git push origin feature/YourFeatureName
   ```

2. **Create Pull Request on GitHub**
   - Title: Clear, concise description of changes
   - Description: Explain what changed and why
   - Link related issues: `Closes #123`

3. **PR Description Template**
   ```markdown
   ## Description
   Brief explanation of the changes made.
   
   ## Type of Change
   - [ ] New feature
   - [ ] Bug fix
   - [ ] Documentation update
   - [ ] Performance improvement
   
   ## Testing
   Describe how to test these changes.
   
   ## Checklist
   - [ ] Tests pass locally
   - [ ] New tests added for new functionality
   - [ ] Documentation updated
   - [ ] Commit messages are clear
   - [ ] No breaking changes (or documented)
   ```

### Review Process

- **CI Checks**: All GitHub Actions workflows must pass
- **Code Review**: At least one maintainer review required
- **Test Coverage**: New code should maintain or improve coverage
- **Documentation**: Changes to public APIs require documentation updates

### Addressing Feedback

- Make requested changes in new commits
- Don't rebase after review starts (keeps conversation in context)
- Reply to all comments
- Request re-review after addressing feedback

## Documentation

### Types of Documentation

**API Documentation**: Update XML docs for public methods
```csharp
/// <summary>
/// Gets the processing status of a video job.
/// </summary>
/// <param name="jobId">The job identifier</param>
/// <returns>Current status of the processing job</returns>
public async Task<ProcessingStatus> GetJobStatusAsync(int jobId)
```

**README Updates**: For significant new features, update the README with examples

**Changelog**: Add entry to [CHANGELOG.md](CHANGELOG.md) following Keep a Changelog format:
```markdown
## [1.3.0] - 2026-05-06
### Added
- New recurring schedule feature for automated uploads
- Support for custom webhook events

### Fixed
- Fixed race condition in concurrent processing
```

**Examples**: Add code examples to `/examples` directory for new features

## Reporting Issues

### Bug Reports

Include the following information:

```markdown
## Description
Clear description of the bug.

## Steps to Reproduce
1. Step one
2. Step two
3. Result

## Expected Behavior
What should happen.

## Actual Behavior
What actually happens.

## Environment
- .NET Version: 10.0
- OS: Windows/Linux/macOS
- FFmpeg Version: 4.x.x
```

### Feature Requests

```markdown
## Description
Clear description of the desired feature.

## Use Case
Why this feature is needed.

## Proposed Solution
Your suggested approach (optional).

## Alternatives
Other approaches you considered (optional).
```

## Questions?

- **GitHub Issues**: Ask questions by [opening an issue](https://github.com/sarmkadan/youtube-shorts-automator/issues)
- **Discussion**: Check existing issues before asking
- **Email**: For security-related questions, see [SECURITY.md](SECURITY.md)

## License

By contributing, you agree that your contributions will be licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Thank you for contributing to YouTube Shorts Automator! Your efforts help make this project better for everyone.
