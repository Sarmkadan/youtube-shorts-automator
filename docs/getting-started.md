// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started Guide

This guide walks you through setting up YouTube Shorts Automator from scratch.

## Prerequisites Checklist

- [ ] .NET 10 SDK installed
- [ ] SQL Server 2019+ or LocalDB
- [ ] FFmpeg 4.0+ installed
- [ ] Git installed
- [ ] YouTube API credentials (from Google Cloud Console)
- [ ] Code editor (VS Code or Visual Studio 2024)

## Step 1: Install .NET 10

### Windows
```powershell
# Using Windows Package Manager
winget install Microsoft.DotNet.SDK.10

# Or download from
# https://dotnet.microsoft.com/download/dotnet/10.0
```

### macOS
```bash
brew install dotnet
```

### Linux (Ubuntu/Debian)
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
export PATH="$PATH:$HOME/.dotnet"
```

Verify installation:
```bash
dotnet --version  # Should show 10.x.x
```

## Step 2: Install FFmpeg

### Windows (using Chocolatey)
```powershell
choco install ffmpeg
```

### macOS
```bash
brew install ffmpeg
```

### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install ffmpeg
```

Verify installation:
```bash
ffmpeg -version
ffprobe -version
```

## Step 3: Set Up SQL Server

### Option A: SQL Server 2019+ (Production)

Download from: https://www.microsoft.com/sql-server/sql-server-downloads

After installation, verify connection:
```bash
# Windows (using sqlcmd)
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"

# Linux/macOS (using sqlcli)
mssql-cli -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

### Option B: LocalDB (Development - Windows only)

```powershell
# Install LocalDB
SqlLocalDB install mssqllocaldb
SqlLocalDB start mssqllocaldb

# Verify
SqlLocalDB info mssqllocaldb
```

Update connection string in `appsettings.json`:
```json
{
  "AppSettings": {
    "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=YouTubeShortsAutomator;Integrated Security=true;"
  }
}
```

## Step 4: Set Up YouTube API Credentials

1. Visit [Google Cloud Console](https://console.cloud.google.com)
2. Create new project:
   - Click "Select a Project"
   - Click "NEW PROJECT"
   - Enter "YouTubeShortsAutomator"
   - Click "CREATE"

3. Enable YouTube Data API v3:
   - Click the search bar
   - Search "YouTube Data API v3"
   - Click "YouTube Data API v3"
   - Click "ENABLE"

4. Create OAuth 2.0 Credentials:
   - Click "Credentials" in left sidebar
   - Click "Create Credentials"
   - Select "OAuth client ID"
   - Application type: "Desktop application"
   - Name: "YouTubeShortsAutomator"
   - Click "CREATE"
   - Download the JSON file

5. Add to `appsettings.json`:
```json
{
  "AppSettings": {
    "YouTubeApiKey": "AIza...",
    "YouTubeClientId": "...-abc.apps.googleusercontent.com",
    "YouTubeClientSecret": "GOCSP-..."
  }
}
```

## Step 5: Clone Repository

```bash
git clone https://github.com/sarmkadan/youtube-shorts-automator.git
cd youtube-shorts-automator

# Verify repository
ls -la
# Should see: README.md, appsettings.json, YouTubeShortsAutomator.csproj, etc.
```

## Step 6: Configure Application

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "ConnectionString": "Server=localhost;Database=YouTubeShortsAutomator;Integrated Security=true;",
    "LogDirectory": "logs",
    "ProcessingDirectory": "processing",
    "OutputDirectory": "output",
    "MaxConcurrentUploads": 3,
    "MaxConcurrentProcessing": 2,
    "DefaultRetryCount": 3,
    "UploadTimeoutSeconds": 7200,
    "YouTubeApiKey": "your-api-key",
    "YouTubeClientId": "your-client-id",
    "YouTubeClientSecret": "your-client-secret",
    "ScheduleCheckIntervalSeconds": 60,
    "ApiKey": "your-api-key-for-requests"
  }
}
```

### Create Directories

```bash
mkdir logs processing output
```

## Step 7: Build Project

```bash
# Restore NuGet packages
dotnet restore
# This downloads all dependencies (~500MB)

# Build project
dotnet build
# Compiles .cs files to IL

# Verify build
dotnet build -c Release
# Creates optimized Release build
```

## Step 8: Database Setup

```bash
# Apply migrations
dotnet ef database update
# Creates database and tables

# Verify database
# Connect to your SQL Server instance and run:
# USE YouTubeShortsAutomator;
# SELECT * FROM Videos;
```

## Step 9: Run Application

```bash
# Development mode
dotnet run
# Output: Building...
#         info: YouTubeShortsAutomator.Program[0]
#         Application started. Press Ctrl+C to shut down.

# Application runs at:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
```

Visit `http://localhost:5000/swagger` to test API endpoints.

## Step 10: Create First Processing Profile

Use Swagger UI or curl:

```bash
curl -X POST http://localhost:5000/api/processing/profiles \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-for-requests" \
  -d '{
    "name": "HD Quality",
    "videoWidth": 1080,
    "videoHeight": 1920,
    "videoBitrate": 4000,
    "frameRate": 30
  }'

# Response: {"id": 1, "name": "HD Quality", ...}
```

## Step 11: Upload First Video

```bash
# Prepare a video file (MP4, MKV, etc.)
# Create a simple test video or use existing one

# Upload via API
curl -X POST http://localhost:5000/api/processing/videos \
  -H "X-API-Key: your-api-key-for-requests" \
  -F "videoFile=@test_video.mp4" \
  -F "profileId=1"

# Response: {"jobId": 1, "status": "Processing"}
```

## Step 12: Schedule Upload to YouTube

```bash
curl -X POST http://localhost:5000/api/schedule/jobs \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-for-requests" \
  -d '{
    "videoShortId": 1,
    "title": "My First Automated Short",
    "description": "Testing YouTube Shorts Automator",
    "tags": ["automation", "youtube", "shorts"],
    "scheduledUploadTime": "2026-05-05T14:00:00Z"
  }'

# Response: {"jobId": 1, "status": "Scheduled"}
```

## Step 13: Monitor Progress

```bash
# Check job status
curl -X GET http://localhost:5000/api/schedule/jobs/1 \
  -H "X-API-Key: your-api-key-for-requests"

# Response: {"id": 1, "title": "...", "status": "Scheduled", ...}

# View logs
tail -f logs/application.log
```

## Next Steps

1. **Explore API**: Visit http://localhost:5000/swagger
2. **Read Architecture**: See [architecture.md](architecture.md)
3. **Check Examples**: See [../examples](../examples)
4. **Deploy**: See [deployment.md](deployment.md)
5. **Configure Dashboard**: See [../docs](../) for admin setup

## Troubleshooting

### Connection String Issues
```bash
# Test connection
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT 1"

# Verify in appsettings.json format
# Server=localhost;Database=YouTubeShortsAutomator;Integrated Security=true;
```

### FFmpeg Not Found
```bash
# Add FFmpeg to PATH
export PATH="/usr/bin:$PATH"

# Or configure in appsettings.json
{
  "Processing": {
    "FFmpegPath": "/usr/local/bin/ffmpeg",
    "FFprobePath": "/usr/local/bin/ffprobe"
  }
}
```

### Port Already in Use
```bash
# Change port in Program.cs
app.Listen("http://0.0.0.0:8080");

# Or kill process using port
# Windows: netstat -ano | findstr :5000
# Linux: lsof -i :5000
```

## Getting Help

- **Issues**: https://github.com/sarmkadan/youtube-shorts-automator/issues
- **Email**: rutova2@gmail.com
- **Documentation**: https://sarmkadan.com
