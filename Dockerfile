# =============================================================================
# YouTube Shorts Automator - Dockerfile
# Minimal multi-stage build with Alpine images
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

WORKDIR /src

# Copy project files and restore dependencies
COPY ["YouTubeShortsAutomator.csproj", "./"]
RUN dotnet restore "YouTubeShortsAutomator.csproj"

# Copy source files
COPY . .
RUN dotnet build "YouTubeShortsAutomator.csproj" -c Release -o /app/build

# Publish stage
FROM builder AS publish
RUN dotnet publish "YouTubeShortsAutomator.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

# Install FFmpeg and required dependencies
RUN apk add --no-cache \
    ffmpeg \
    curl \
    ca-certificates \
    ttf-dejavu \
    && rm -rf /var/cache/apk/*

WORKDIR /app

# Create non-root user
RUN adduser -D -u 1001 appuser

# Copy published application
COPY --from=publish /app/publish .

# Create directories for processing and logs
RUN mkdir -p /app/logs /app/processing /app/output && \
    chown -R appuser:appuser /app/logs /app/processing /app/output && \
    chmod -R 755 /app/logs /app/processing /app/output

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV PATH="/app:$PATH"

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health/ready || exit 1

# Expose ports
EXPOSE 8080

# Switch to non-root user
USER appuser

# Run application
ENTRYPOINT ["dotnet", "YouTubeShortsAutomator.dll"]
