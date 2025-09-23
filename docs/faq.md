// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

## Installation & Setup

### Q: Which operating system is supported?
**A:** YouTube Shorts Automator runs on:
- Windows 10/11 and Windows Server 2019+
- macOS 10.15+
- Linux (Ubuntu 18.04+, Debian 10+, CentOS 8+)

### Q: Do I need a paid SQL Server license?
**A:** No. You can use:
- **SQL Server Express** (free, up to 10GB)
- **SQL Server LocalDB** (free, for development)
- **Docker with SQL Server Linux container** (free)
- Or pay for full SQL Server 2019+

### Q: What's the minimum .NET version required?
**A:** .NET 10.0 or later is required. Download from https://dotnet.microsoft.com/download

### Q: Can I run this on Raspberry Pi?
**A:** YouTube Shorts Automator requires at least 2GB RAM and dual-core processor. Raspberry Pi 4 (4GB+) might work with reduced concurrency settings.

### Q: How much disk space do I need?
**A:** Minimum 20GB recommended:
- 2GB for application and dependencies
- 10GB+ for processing temporary files
- 5GB+ for output videos
- 2GB+ for logs and database

## Configuration

### Q: How do I get YouTube API credentials?
**A:** Follow these steps:
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create new project
3. Enable "YouTube Data API v3"
4. Create OAuth 2.0 credentials (Desktop application)
5. Download JSON and extract values
6. Add to `appsettings.json`

### Q: Is my API key visible in logs?
**A:** No. We never log credentials. API keys are stored securely and excluded from all logging.

### Q: Can I use multiple API keys for rate limiting?
**A:** Yes. Configure multiple channels with different API keys in database and rotate between them for uploads.

### Q: How often should I sync analytics?
**A:** Default is every 6 hours. Adjust `AnalyticsSyncIntervalHours` based on your needs:
- Every hour: Most up-to-date analytics
- Every 6 hours: Balanced approach (default)
- Every 24 hours: Less API calls

## Video Processing

### Q: What video formats are supported?
**A:** Any format supported by FFmpeg:
- **Video**: MP4, MKV, WebM, AVI, MOV, FLV
- **Audio**: AAC, MP3, FLAC, Opus, Vorbis
- **Resolution**: Any, but will be resized to profile settings
- **Duration**: Up to 60 seconds (YouTube Shorts requirement)

### Q: Can I process 4K videos?
**A:** Yes, but consider:
- Processing takes longer
- Output size may exceed YouTube Shorts limits
- High CPU/memory usage
- Consider downscaling in processing profile

### Q: How do I add a watermark?
**A:** 1. Set `EnableWatermark: true` in appsettings.json
2. Provide path to watermark image: `WatermarkImagePath: "path/to/logo.png"`
3. Watermark applied during transcoding

### Q: Can I apply color grading?
**A:** Yes, specify color grading profile during processing:
```json
{
  "name": "Cinematic",
  "colorGradingPreset": "cinematic_dark"
}
```

### Q: Why is transcoding slow?
**A:** Check:
- FFmpeg quality preset (fast/medium/slow)
- Output bitrate (higher = slower)
- System CPU usage
- Concurrent processing count
- Video resolution

## Uploading

### Q: How many videos can I upload simultaneously?
**A:** Configurable via `MaxConcurrentUploads` (default: 3).
Increase carefully based on:
- API rate limits (YouTube enforces quotas)
- Available bandwidth
- Server resources

### Q: Will uploads resume if interrupted?
**A:** Partially. We track upload progress but:
- YouTube handles resumable uploads
- Long interruptions require restart
- Completed uploads are marked done

### Q: Can I schedule uploads for specific times?
**A:** Yes. Set `ScheduledUploadTime` to any future datetime.
System checks every `ScheduleCheckIntervalSeconds` (default: 60).

### Q: What happens if upload fails?
**A:** Automatic retry with exponential backoff:
- Attempt 1: Immediately
- Attempt 2: After 60 seconds
- Attempt 3: After 300 seconds
- After 3 failures: Marked as failed

Configure max retries: `DefaultRetryCount`

### Q: Can I upload to multiple channels?
**A:** Yes. Register multiple YouTube channels and specify channel when creating upload job.

## Analytics

### Q: How often are analytics updated?
**A:** YouTube updates metrics with 48-hour delay. Our sync interval: `AnalyticsSyncIntervalHours` (default: 6 hours)

### Q: What metrics are tracked?
**A:** Views, likes, comments, shares, watch time, engagement rate, and more.

### Q: Can I export analytics?
**A:** Yes, via API:
- CSV format
- JSON format
- Custom date ranges
- Filtered by channel or video

### Q: Do you track real-time viewer data?
**A:** No. YouTube doesn't provide real-time viewer info in API. 
Consider YouTube Analytics dashboard for real-time stats.

## Performance & Scaling

### Q: How many videos can the system handle?
**A:** Depends on:
- Processing concurrency setting
- Server resources (CPU, RAM, disk)
- Database size
- Network bandwidth

With default settings on 4-core/8GB server: ~100-200 videos/day

### Q: How do I scale horizontally?
**A:** Current architecture supports single-server deployment. For horizontal scaling:
- Use load balancer
- Share database across instances
- Use Redis for distributed caching
- Configure background services carefully

### Q: What's the database size limit?
**A:** SQL Server Express: 10GB
Full SQL Server: Terabytes

Expected per-video: ~1MB database storage

### Q: Can I optimize processing speed?
**A:** Yes:
- Use faster FFmpeg preset: `fast` instead of `slow`
- Increase `MaxConcurrentProcessing`
- Use SSD for temporary files
- Enable compression in network

## Troubleshooting

### Q: API returns 401 Unauthorized
**A:** Check:
- API key in header: `X-API-Key: your-key`
- API key correct in `appsettings.json`
- API key not expired
- Header capitalization (case-sensitive on Linux)

### Q: Database connection fails
**A:** Check:
- SQL Server running: `sqlcmd -S localhost -Q "SELECT 1"`
- Connection string syntax correct
- Firewall allows port 1433
- Credentials correct
- Database exists

### Q: FFmpeg command not found
**A:** Install FFmpeg:
```bash
# Linux
sudo apt-get install ffmpeg

# macOS
brew install ffmpeg

# Windows
choco install ffmpeg
```

### Q: Out of memory errors
**A:** Reduce:
- `MaxConcurrentProcessing` to 1
- Video bitrate in processing profile
- Increase server RAM if possible

### Q: High CPU usage
**A:** Check:
- Number of concurrent jobs
- FFmpeg quality setting (use `fast`)
- System load: `top` or Task Manager
- Consider disabling watermarking temporarily

### Q: Uploads timing out
**A:** Increase timeout in `appsettings.json`:
```json
{
  "UploadTimeoutSeconds": 14400  // 4 hours instead of 2
}
```

Also check internet bandwidth.

### Q: Port 5000 already in use
**A:** Change port in `Program.cs`:
```csharp
app.Listen("http://0.0.0.0:8080");
```

Or kill process using port:
```bash
# Linux/macOS
lsof -i :5000

# Windows
netstat -ano | findstr :5000
```

## Security

### Q: Is my data encrypted?
**A:** In transit: HTTPS (when configured)
At rest: Database-level encryption (if enabled)
Credentials: Secured in config

### Q: How do I protect the API?
**A:** Configure API key security:
- Use strong random API key
- Rotate periodically
- Store in environment variable
- Use HTTPS only
- Implement rate limiting

### Q: Can I use OAuth for API access?
**A:** Currently uses API keys. OAuth support planned for future versions.

### Q: Do you store video files?
**A:** Only processed videos in output directory. Original files in processing directory (can be deleted after successful upload).

## Advanced Usage

### Q: Can I use this as a library in my app?
**A:** Yes. Services are public and DI-compatible. Reference the project and use services directly.

### Q: Can I extend with custom video effects?
**A:** Yes. Create custom `ProcessingProfile` and implement FFmpeg filters.

### Q: How do I run tests?
**A:**
```bash
dotnet test
dotnet test --filter="TestName"
dotnet test /p:CollectCoverage=true
```

### Q: Can I use this in a cloud function?
**A:** Not directly. The system is designed for continuous operation with background services. Consider containerizing and deploying to:
- Azure Container Instances
- AWS ECS
- Google Cloud Run
- Kubernetes

### Q: Can I integrate with other platforms?
**A:** Yes, via webhooks:
- Configure `Webhook.Secret` in settings
- Receive notifications on upload completion
- Send to external systems

## Support

### Q: Where can I report bugs?
**A:** Create issue on [GitHub](https://github.com/sarmkadan/youtube-shorts-automator/issues)

### Q: How do I contribute?
**A:** Follow [CONTRIBUTING guidelines](../CONTRIBUTING.md)

### Q: Is there commercial support?
**A:** Contact at rutova2@gmail.com for consultation services.

### Q: What's the roadmap?
**A:** Check [CHANGELOG.md](../CHANGELOG.md) for planned features.

## Licensing

### Q: Can I use this commercially?
**A:** Yes. Licensed under MIT License - free for any use.

### Q: Do I need to attribute you?
**A:** Not required by license, but appreciated if mentioned.

### Q: Can I modify the code?
**A:** Yes, freely. See MIT License for details.

## More Help

- **Documentation**: See [docs/](../) folder
- **Examples**: See [examples/](../examples/) folder
- **API Docs**: See [docs/api-reference.md](api-reference.md)
- **GitHub**: [youtube-shorts-automator](https://github.com/sarmkadan/youtube-shorts-automator)
- **Email**: rutova2@gmail.com
