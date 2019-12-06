# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Makefile for YouTube Shorts Automator
# Provides convenient commands for development and deployment

.PHONY: help clean build test run debug release docker docker-up docker-down database-update restore lint format

DOTNET = dotnet
PROJECT = YouTubeShortsAutomator.csproj
CONFIGURATION = Release
OUTPUT_DIR = ./bin/$(CONFIGURATION)
PUBLISH_DIR = ./publish

# Default target
help:
	@echo "YouTube Shorts Automator - Build Targets"
	@echo "========================================"
	@echo ""
	@echo "Development:"
	@echo "  make restore    - Restore NuGet packages"
	@echo "  make build      - Build project"
	@echo "  make rebuild    - Clean and rebuild project"
	@echo "  make test       - Run unit tests"
	@echo "  make run        - Run application locally"
	@echo "  make debug      - Run in debug mode"
	@echo ""
	@echo "Quality:"
	@echo "  make lint       - Run code style analysis"
	@echo "  make format     - Auto-format code"
	@echo "  make coverage   - Generate test coverage report"
	@echo ""
	@echo "Deployment:"
	@echo "  make release    - Create release build"
	@echo "  make publish    - Publish for deployment"
	@echo "  make docker     - Build Docker image"
	@echo "  make docker-up  - Start Docker Compose services"
	@echo "  make docker-down- Stop Docker Compose services"
	@echo ""
	@echo "Database:"
	@echo "  make db-migrate - Apply database migrations"
	@echo "  make db-create  - Create new migration"
	@echo "  make db-revert  - Revert last migration"
	@echo ""
	@echo "Utility:"
	@echo "  make clean      - Remove build artifacts"
	@echo "  make info       - Display project information"
	@echo ""

# Restore NuGet packages
restore:
	@echo "Restoring NuGet packages..."
	$(DOTNET) restore $(PROJECT)
	@echo "✓ Restore complete"

# Build project
build: restore
	@echo "Building project..."
	$(DOTNET) build $(PROJECT) -c $(CONFIGURATION)
	@echo "✓ Build complete"

# Rebuild project
rebuild: clean build

# Run tests
test:
	@echo "Running tests..."
	$(DOTNET) test --configuration $(CONFIGURATION) --verbosity normal
	@echo "✓ Tests complete"

# Generate coverage report
coverage:
	@echo "Generating coverage report..."
	$(DOTNET) test /p:CollectCoverage=true \
		/p:CoverageFormat=opencover \
		/p:CoverageFileName=coverage.xml \
		--configuration $(CONFIGURATION)
	@echo "✓ Coverage report generated: coverage.xml"

# Run application locally
run: build
	@echo "Starting application..."
	$(DOTNET) run --project $(PROJECT) -c $(CONFIGURATION)

# Debug mode
debug: restore
	@echo "Starting in debug mode..."
	$(DOTNET) run --project $(PROJECT)

# Release build
release: clean test
	@echo "Creating release build..."
	$(DOTNET) build $(PROJECT) -c Release -o $(OUTPUT_DIR)
	@echo "✓ Release build complete: $(OUTPUT_DIR)"

# Publish for deployment
publish: release
	@echo "Publishing for deployment..."
	$(DOTNET) publish $(PROJECT) -c Release -o $(PUBLISH_DIR)
	@echo "✓ Published to: $(PUBLISH_DIR)"

# Code style analysis
lint:
	@echo "Running code style analysis..."
	$(DOTNET) build /p:EnforceCodeStyleInBuild=true $(PROJECT)
	@echo "✓ Style analysis complete"

# Auto-format code
format:
	@echo "Formatting code..."
	$(DOTNET) format $(PROJECT)
	@echo "✓ Code formatted"

# Build Docker image
docker:
	@echo "Building Docker image..."
	docker build -t youtube-shorts-automator:latest .
	@echo "✓ Docker image built: youtube-shorts-automator:latest"

# Start Docker Compose services
docker-up:
	@echo "Starting Docker Compose services..."
	docker-compose up -d
	@echo "✓ Services started"
	@echo "  App: http://localhost:5000"
	@echo "  Database: localhost:1433"
	@echo "  Cache: localhost:6379"

# Stop Docker Compose services
docker-down:
	@echo "Stopping Docker Compose services..."
	docker-compose down
	@echo "✓ Services stopped"

# Apply database migrations
db-migrate:
	@echo "Applying database migrations..."
	$(DOTNET) ef database update
	@echo "✓ Migrations applied"

# Create new migration
db-create:
	@echo "Enter migration name:"
	@read migration_name; \
	$(DOTNET) ef migrations add $$migration_name
	@echo "✓ Migration created"

# Revert last migration
db-revert:
	@echo "Reverting last migration..."
	$(DOTNET) ef migrations remove
	@echo "✓ Migration reverted"

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	$(DOTNET) clean $(PROJECT)
	rm -rf $(OUTPUT_DIR) $(PUBLISH_DIR) coverage.xml
	@echo "✓ Clean complete"

# Display project information
info:
	@echo "Project Information"
	@echo "==================="
	@echo "Name: YouTube Shorts Automator"
	@echo "Framework: .NET 10.0"
	@echo "Language: C#"
	@echo "Author: Vladyslav Zaiets"
	@echo ""
	@echo "Build Configuration:"
	@echo "  Configuration: $(CONFIGURATION)"
	@echo "  Output Dir: $(OUTPUT_DIR)"
	@echo "  Publish Dir: $(PUBLISH_DIR)"
	@echo ""
	@echo ".NET Version:"
	@$(DOTNET) --version
	@echo ""

# Watch for changes and rebuild
watch:
	@echo "Watching for changes..."
	$(DOTNET) watch run --project $(PROJECT)

# Check dependencies for vulnerabilities
audit:
	@echo "Checking for vulnerable dependencies..."
	$(DOTNET) list package --vulnerable
	@echo "✓ Audit complete"

# Format and lint combined
quality: format lint
	@echo "✓ Code quality checks complete"

# Full CI pipeline
ci: clean restore lint build test coverage
	@echo "✓ CI pipeline complete"

# Development workflow (build + test + run)
dev: build test debug

# Production workflow (clean + build + test + publish + docker)
prod: clean build test publish docker
	@echo "✓ Production build complete"

# Display version
version:
	@grep "Version" $(PROJECT) | head -1 | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/'
