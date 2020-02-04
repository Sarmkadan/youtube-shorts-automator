# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for YouTube Shorts Automator

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY ["YouTubeShortsAutomator.csproj", "./"]
RUN dotnet restore "YouTubeShortsAutomator.csproj"

COPY . .
RUN dotnet build "YouTubeShortsAutomator.csproj" -c Release -o /app/build

FROM builder AS publish
RUN dotnet publish "YouTubeShortsAutomator.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Install FFmpeg and required dependencies
RUN apt-get update && apt-get install -y \
    ffmpeg \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create directories for processing and logs
RUN mkdir -p /app/logs /app/processing /app/output && \
    chmod -R 755 /app/logs /app/processing /app/output

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
ENV PATH="/app:$PATH"

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

# Expose ports
EXPOSE 5000 5001

# Run application
ENTRYPOINT ["dotnet", "YouTubeShortsAutomator.dll"]
