// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Pipeline Configuration Guide

Detailed configuration examples and best practices for the YouTube Shorts Automator pipeline.

## Overview

The upload pipeline consists of four stages:
1. **Input validation** – file type, size, and duration checks
2. **Video processing** – FFmpeg transcoding using a processing profile
3. **Scheduling** – queuing uploads to fit within quota limits
4. **Upload & publish** – authenticated upload to YouTube Data API v3

---

## Processing Profiles

Processing profiles define how raw video is transcoded for YouTube Shorts (9:16 vertical, up to 60 s).

### Minimal profile (fast, low quality)
```json
{
  "name": "Fast Preview",
  "videoWidth": 720,
  "videoHeight": 1280,
  "videoBitrate": 2000,
  "audioBitrate": 96,
  "frameRate": 24,
  "videoCodec": "h264",
  "audioCodec": "aac",
  "container": "mp4",
  "applyWatermark": false,
  "applyColorGrading": false,
  "compressionLevel": 8
}
```

### Standard profile (recommended)
```json
{
  "name": "Standard Shorts",
  "videoWidth": 1080,
  "videoHeight": 1920,
  "videoBitrate": 4000,
  "audioBitrate": 128,
  "frameRate": 30,
  "videoCodec": "h264",
  "audioCodec": "aac",
  "container": "mp4",
  "applyWatermark": false,
  "applyColorGrading": false,
  "compressionLevel": 5
}
```

### High-quality profile (slower, larger file)
```json
{
  "name": "High Quality",
  "videoWidth": 1080,
  "videoHeight": 1920,
  "videoBitrate": 8000,
  "audioBitrate": 192,
  "frameRate": 60,
  "videoCodec": "h264",
  "audioCodec": "aac",
  "container": "mp4",
  "applyWatermark": true,
  "applyColorGrading": true,
  "compressionLevel": 2
}
```

---

## Application Settings

All pipeline parameters live in `appsettings.json` under the `AppSettings` key.

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
    "ScheduleCheckIntervalSeconds": 60,
    "YouTubeApiKey": "<your-api-key>",
    "YouTubeClientId": "<your-client-id>",
    "YouTubeClientSecret": "<your-client-secret>",
    "ApiKey": "<your-internal-api-key>"
  }
}
```

### Key parameters

| Parameter | Default | Description |
|---|---|---|
| `MaxConcurrentUploads` | `3` | Parallel upload workers |
| `MaxConcurrentProcessing` | `2` | Parallel FFmpeg workers |
| `DefaultRetryCount` | `3` | Upload retry limit per job |
| `UploadTimeoutSeconds` | `7200` | Per-upload timeout (2 h) |
| `ScheduleCheckIntervalSeconds` | `60` | How often the scheduler polls the queue |

---

## Environment-specific overrides

Create `appsettings.local.json` (excluded from version control) for local secrets:

```json
{
  "AppSettings": {
    "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=YouTubeShortsAutomator;Integrated Security=true;",
    "YouTubeApiKey": "AIza...",
    "YouTubeClientId": "...-abc.apps.googleusercontent.com",
    "YouTubeClientSecret": "GOCSP-..."
  }
}
```

Environment variables override JSON settings and follow this naming scheme:

```bash
export AppSettings__YouTubeApiKey="AIza..."
export AppSettings__ConnectionString="Server=prod-db;..."
```

---

## Docker configuration

`docker-compose.yml` mounts a secrets file and passes connection details via environment variables:

```yaml
services:
  automator:
    build: .
    environment:
      - AppSettings__ConnectionString=Server=db;Database=YouTubeShortsAutomator;User Id=sa;Password=${DB_PASSWORD};
      - AppSettings__YouTubeApiKey=${YOUTUBE_API_KEY}
      - AppSettings__YouTubeClientId=${YOUTUBE_CLIENT_ID}
      - AppSettings__YouTubeClientSecret=${YOUTUBE_CLIENT_SECRET}
    volumes:
      - ./processing:/app/processing
      - ./output:/app/output
      - ./logs:/app/logs
```

Use a `.env` file (not committed) to supply the variable values:

```dotenv
DB_PASSWORD=StrongPassword123
YOUTUBE_API_KEY=AIza...
YOUTUBE_CLIENT_ID=....apps.googleusercontent.com
YOUTUBE_CLIENT_SECRET=GOCSP-...
```

---

## Scheduling configuration

Schedule uploads at specific times to stay within the YouTube quota (10,000 units/day by default).

```bash
# Schedule a processed video for upload
curl -X POST http://localhost:5000/api/schedule/jobs \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "videoShortId": 42,
    "title": "Daily Tip #7",
    "description": "Quick productivity tip",
    "tags": ["tips", "productivity", "shorts"],
    "scheduledUploadTime": "2026-06-01T09:00:00Z"
  }'
```

To distribute uploads evenly over a week, stagger `scheduledUploadTime` by 24 hours between jobs.

---

## FFmpeg path override

If `ffmpeg` is not on the system `PATH`, set explicit paths:

```json
{
  "Processing": {
    "FFmpegPath": "/usr/local/bin/ffmpeg",
    "FFprobePath": "/usr/local/bin/ffprobe"
  }
}
```

---

## Retry and error handling

Transient upload failures are retried automatically up to `DefaultRetryCount` times. Each attempt is logged to the upload history (see `dotnet run history`). To disable retries for a specific job, set `maxRetries` to `0` when creating the upload job.

---

## Further reading

- [Getting started](getting-started.md)
- [API reference](api-reference.md)
- [Deployment guide](deployment.md)
- [FAQ](faq.md)
