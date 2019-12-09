// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete reference for all REST API endpoints.

## Base URL

```
http://localhost:5000/api
```

## Authentication

All endpoints require API key in headers:

```
X-API-Key: your-api-key-here
```

## Response Format

All responses use JSON format:

```json
{
  "status": "success",
  "data": { },
  "error": null,
  "timestamp": "2026-05-04T10:30:00Z"
}
```

## Processing Endpoints

### Upload Video

**POST** `/processing/videos`

Upload and process a video file.

**Headers**:
```
X-API-Key: your-api-key
Content-Type: multipart/form-data
```

**Parameters**:
- `videoFile` (File, required): Video file to upload
- `profileId` (int, optional): Processing profile ID (default: 1)
- `generateThumbnail` (bool, optional): Auto-generate thumbnail (default: true)

**Request**:
```bash
curl -X POST http://localhost:5000/api/processing/videos \
  -H "X-API-Key: your-api-key" \
  -F "videoFile=@video.mp4" \
  -F "profileId=1"
```

**Response (201 Created)**:
```json
{
  "data": {
    "jobId": 1,
    "videoId": 5,
    "status": "Processing",
    "progress": 0,
    "createdAt": "2026-05-04T10:30:00Z"
  }
}
```

### Get Processing Job

**GET** `/processing/jobs/{jobId}`

Retrieve processing job details and progress.

**Parameters**:
- `jobId` (int, required): Job ID

**Request**:
```bash
curl -X GET http://localhost:5000/api/processing/jobs/1 \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "id": 1,
    "uploadJobId": 5,
    "status": "Completed",
    "progress": 100,
    "errorMessage": null,
    "startedAt": "2026-05-04T10:30:00Z",
    "completedAt": "2026-05-04T10:45:00Z",
    "elapsedSeconds": 900
  }
}
```

### List Processing Profiles

**GET** `/processing/profiles`

List all available video processing profiles.

**Query Parameters**:
- `skip` (int, optional): Skip N results (default: 0)
- `take` (int, optional): Return N results (default: 10)

**Request**:
```bash
curl -X GET http://localhost:5000/api/processing/profiles \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": [
    {
      "id": 1,
      "name": "HD Quality",
      "videoWidth": 1080,
      "videoHeight": 1920,
      "videoBitrate": 4000,
      "audioCodec": "aac",
      "frameRate": 30,
      "createdAt": "2026-05-01T00:00:00Z"
    },
    {
      "id": 2,
      "name": "Standard Quality",
      "videoWidth": 720,
      "videoHeight": 1280,
      "videoBitrate": 2000,
      "audioCodec": "aac",
      "frameRate": 24,
      "createdAt": "2026-05-01T00:00:00Z"
    }
  ]
}
```

### Create Processing Profile

**POST** `/processing/profiles`

Create a new video processing profile.

**Request Body**:
```json
{
  "name": "Ultra HD",
  "videoWidth": 1440,
  "videoHeight": 2560,
  "videoBitrate": 8000,
  "audioCodec": "aac",
  "frameRate": 60,
  "presetName": "slow"
}
```

**Request**:
```bash
curl -X POST http://localhost:5000/api/processing/profiles \
  -H "X-API-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ultra HD",
    "videoWidth": 1440,
    "videoHeight": 2560,
    "videoBitrate": 8000
  }'
```

**Response (201 Created)**:
```json
{
  "data": {
    "id": 3,
    "name": "Ultra HD",
    "videoWidth": 1440,
    "videoHeight": 2560,
    "videoBitrate": 8000,
    "createdAt": "2026-05-04T10:30:00Z"
  }
}
```

## Scheduling Endpoints

### Create Upload Job

**POST** `/schedule/jobs`

Schedule a video for upload to YouTube.

**Request Body**:
```json
{
  "videoShortId": 1,
  "youtubeChannelId": 1,
  "title": "Amazing Short Video",
  "description": "This is an amazing short video",
  "tags": ["entertainment", "viral", "shorts"],
  "scheduledUploadTime": "2026-05-05T14:00:00Z"
}
```

**Request**:
```bash
curl -X POST http://localhost:5000/api/schedule/jobs \
  -H "X-API-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "videoShortId": 1,
    "youtubeChannelId": 1,
    "title": "Amazing Short",
    "scheduledUploadTime": "2026-05-05T14:00:00Z"
  }'
```

**Response (201 Created)**:
```json
{
  "data": {
    "jobId": 5,
    "videoShortId": 1,
    "title": "Amazing Short Video",
    "status": "Scheduled",
    "scheduledUploadTime": "2026-05-05T14:00:00Z",
    "createdAt": "2026-05-04T10:30:00Z"
  }
}
```

### Get Upload Job

**GET** `/schedule/jobs/{jobId}`

Retrieve details of a scheduled upload job.

**Request**:
```bash
curl -X GET http://localhost:5000/api/schedule/jobs/5 \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "id": 5,
    "videoShortId": 1,
    "youtubeChannelId": 1,
    "title": "Amazing Short Video",
    "description": "This is an amazing short video",
    "tags": ["entertainment", "viral", "shorts"],
    "status": "Uploaded",
    "scheduledUploadTime": "2026-05-05T14:00:00Z",
    "youtubeVideoId": "dQw4w9WgXcQ",
    "retryCount": 0,
    "createdAt": "2026-05-04T10:30:00Z",
    "completedAt": "2026-05-05T14:05:00Z"
  }
}
```

### List Upload Jobs

**GET** `/schedule/jobs`

List all scheduled upload jobs.

**Query Parameters**:
- `status` (string, optional): Filter by status (Scheduled, Uploading, Uploaded, Failed)
- `youtubeChannelId` (int, optional): Filter by channel
- `skip` (int, optional): Skip N results
- `take` (int, optional): Return N results

**Request**:
```bash
curl -X GET "http://localhost:5000/api/schedule/jobs?status=Scheduled&take=20" \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": [
    {
      "id": 5,
      "title": "Amazing Short Video",
      "status": "Scheduled",
      "scheduledUploadTime": "2026-05-05T14:00:00Z"
    }
  ],
  "pagination": {
    "total": 1,
    "skip": 0,
    "take": 20
  }
}
```

### Update Upload Job

**PUT** `/schedule/jobs/{jobId}`

Update scheduled upload job details.

**Request Body**:
```json
{
  "title": "Updated Title",
  "description": "Updated description",
  "tags": ["new", "tags"],
  "scheduledUploadTime": "2026-05-05T15:00:00Z"
}
```

**Request**:
```bash
curl -X PUT http://localhost:5000/api/schedule/jobs/5 \
  -H "X-API-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Title",
    "scheduledUploadTime": "2026-05-05T15:00:00Z"
  }'
```

**Response (200 OK)**:
```json
{
  "data": {
    "id": 5,
    "title": "Updated Title",
    "status": "Scheduled",
    "scheduledUploadTime": "2026-05-05T15:00:00Z"
  }
}
```

### Cancel Upload Job

**DELETE** `/schedule/jobs/{jobId}`

Cancel a scheduled upload job.

**Request**:
```bash
curl -X DELETE http://localhost:5000/api/schedule/jobs/5 \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "jobId": 5,
    "cancelled": true,
    "message": "Job cancelled successfully"
  }
}
```

## Analytics Endpoints

### Get Overall Metrics

**GET** `/analytics/metrics`

Get aggregated metrics across all videos.

**Query Parameters**:
- `startDate` (datetime, optional): Start date (default: 30 days ago)
- `endDate` (datetime, optional): End date (default: now)

**Request**:
```bash
curl -X GET "http://localhost:5000/api/analytics/metrics?startDate=2026-04-04&endDate=2026-05-04" \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "totalViews": 50000,
    "totalLikes": 2500,
    "totalComments": 500,
    "totalShares": 300,
    "avgEngagementRate": 0.065,
    "topPerformer": {
      "videoId": "dQw4w9WgXcQ",
      "title": "Most Viewed Video",
      "views": 15000
    }
  }
}
```

### Get Video Metrics

**GET** `/analytics/videos/{videoId}`

Get detailed metrics for specific video.

**Request**:
```bash
curl -X GET http://localhost:5000/api/analytics/videos/dQw4w9WgXcQ \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "videoId": "dQw4w9WgXcQ",
    "title": "Amazing Short Video",
    "views": 5000,
    "likes": 250,
    "comments": 50,
    "shares": 30,
    "watches": 4200,
    "engagementRate": 0.06,
    "uploadedAt": "2026-05-01T10:00:00Z",
    "lastUpdatedAt": "2026-05-04T10:00:00Z"
  }
}
```

### Get Trending Videos

**GET** `/analytics/trends`

Get trending videos based on engagement.

**Query Parameters**:
- `days` (int, optional): Look back N days (default: 7)
- `limit` (int, optional): Return top N videos (default: 10)

**Request**:
```bash
curl -X GET "http://localhost:5000/api/analytics/trends?days=7&limit=5" \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": [
    {
      "rank": 1,
      "videoId": "dQw4w9WgXcQ",
      "title": "Viral Short Video",
      "views": 15000,
      "likes": 1200,
      "engagementRate": 0.08,
      "trend": "up"
    },
    {
      "rank": 2,
      "videoId": "jNgzyJa_3xk",
      "title": "Amazing Highlights",
      "views": 12000,
      "likes": 900,
      "engagementRate": 0.075,
      "trend": "stable"
    }
  ]
}
```

### Export Metrics

**POST** `/analytics/export`

Export analytics data in specified format.

**Request Body**:
```json
{
  "format": "csv",
  "startDate": "2026-04-04",
  "endDate": "2026-05-04",
  "includeMetrics": ["views", "likes", "comments", "shares"],
  "groupBy": "day"
}
```

**Request**:
```bash
curl -X POST http://localhost:5000/api/analytics/export \
  -H "X-API-Key: your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "format": "csv",
    "startDate": "2026-04-04",
    "endDate": "2026-05-04"
  }' \
  > analytics.csv
```

**Response (200 OK)**:
```
File download (CSV/JSON)
```

## System Endpoints

### Health Check

**GET** `/health`

Check system health status.

**Request**:
```bash
curl -X GET http://localhost:5000/api/health
```

**Response (200 OK)**:
```json
{
  "status": "healthy",
  "timestamp": "2026-05-04T10:30:00Z",
  "uptime": "2 days 5 hours"
}
```

### Get Version

**GET** `/version`

Get application version information.

**Request**:
```bash
curl -X GET http://localhost:5000/api/version
```

**Response (200 OK)**:
```json
{
  "data": {
    "version": "1.2.0",
    "buildDate": "2026-05-01T00:00:00Z",
    "environment": "Production"
  }
}
```

### Get Metrics

**GET** `/metrics`

Get system performance metrics.

**Request**:
```bash
curl -X GET http://localhost:5000/api/metrics \
  -H "X-API-Key: your-api-key"
```

**Response (200 OK)**:
```json
{
  "data": {
    "cpuUsagePercent": 35.2,
    "memoryUsageMB": 512,
    "availableMemoryMB": 1024,
    "processingQueueCount": 5,
    "uploadQueueCount": 2,
    "failedJobsCount": 1,
    "totalProcessedVideos": 150
  }
}
```

## Error Responses

### 400 Bad Request

```json
{
  "status": "error",
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input parameters",
    "details": {
      "videoFile": "File is required",
      "profileId": "Must be positive integer"
    }
  }
}
```

### 401 Unauthorized

```json
{
  "status": "error",
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Missing or invalid API key"
  }
}
```

### 404 Not Found

```json
{
  "status": "error",
  "error": {
    "code": "NOT_FOUND",
    "message": "Resource not found",
    "details": {
      "resourceId": 999,
      "resourceType": "UploadJob"
    }
  }
}
```

### 429 Too Many Requests

```json
{
  "status": "error",
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests",
    "retryAfter": 60
  }
}
```

### 500 Internal Server Error

```json
{
  "status": "error",
  "error": {
    "code": "INTERNAL_ERROR",
    "message": "An unexpected error occurred",
    "traceId": "0HNF3L6OQ1H7P:00000001"
  }
}
```

## Rate Limiting

All endpoints are rate limited:
- **Default**: 100 requests per minute
- **Burst**: Up to 20 requests per second

Rate limit headers in response:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1714920660
```

## Status Codes

| Code | Meaning |
|------|---------|
| 200  | Success |
| 201  | Created |
| 400  | Bad Request |
| 401  | Unauthorized |
| 403  | Forbidden |
| 404  | Not Found |
| 429  | Rate Limited |
| 500  | Server Error |

## Enum Values

### ProcessingStatus
- `Pending`
- `Processing`
- `Completed`
- `Failed`
- `Cancelled`

### UploadStatus
- `Scheduled`
- `Uploading`
- `Uploaded`
- `Failed`
- `Cancelled`

### VideoQuality
- `Low` (480p)
- `Standard` (720p)
- `High` (1080p)
- `VeryHigh` (1440p)
- `Ultra` (2160p+)
