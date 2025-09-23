// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Production deployment instructions for YouTube Shorts Automator.

## Pre-Deployment Checklist

- [ ] All tests passing
- [ ] Code reviewed and merged
- [ ] Environment variables configured
- [ ] Database backups configured
- [ ] SSL certificates obtained
- [ ] Firewall rules configured
- [ ] Monitoring setup complete
- [ ] Backup and recovery plan documented

## Deployment Methods

### Method 1: Docker Deployment (Recommended)

#### Build Docker Image

```bash
# Clone repository
git clone https://github.com/sarmkadan/youtube-shorts-automator.git
cd youtube-shorts-automator

# Build image
docker build -t youtube-shorts-automator:1.2.0 .

# Tag for registry
docker tag youtube-shorts-automator:1.2.0 yourusername/youtube-shorts-automator:1.2.0

# Push to Docker Hub
docker push yourusername/youtube-shorts-automator:1.2.0
```

#### Deploy with Docker Compose

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  app:
    image: youtube-shorts-automator:1.2.0
    container_name: youtube-shorts-automator
    restart: always
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://+:5001;http://+:5000
      ConnectionString: "Server=db;Database=YouTubeShortsAutomator;User Id=sa;Password=${SA_PASSWORD};"
      YouTubeApiKey: ${YOUTUBE_API_KEY}
      YouTubeClientId: ${YOUTUBE_CLIENT_ID}
      YouTubeClientSecret: ${YOUTUBE_CLIENT_SECRET}
    depends_on:
      - db
    volumes:
      - ./logs:/app/logs
      - ./processing:/app/processing
      - ./output:/app/output
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: youtube-shorts-automator-db
    restart: always
    environment:
      SA_PASSWORD: ${SA_PASSWORD}
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"
    volumes:
      - db-data:/var/opt/mssql/data
      - db-log:/var/opt/mssql/log
    networks:
      - app-network

  redis:
    image: redis:7-alpine
    container_name: youtube-shorts-automator-cache
    restart: always
    ports:
      - "6379:6379"
    volumes:
      - cache-data:/data
    networks:
      - app-network

volumes:
  db-data:
  db-log:
  cache-data:

networks:
  app-network:
    driver: bridge
```

Create `.env` file:

```bash
SA_PASSWORD=YourSecurePassword123!
YOUTUBE_API_KEY=your-api-key
YOUTUBE_CLIENT_ID=your-client-id
YOUTUBE_CLIENT_SECRET=your-client-secret
```

Deploy:

```bash
# Start services
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f app

# Check status
docker-compose -f docker-compose.prod.yml ps

# Stop services
docker-compose -f docker-compose.prod.yml down
```

### Method 2: Linux Server Deployment

#### Prerequisites

```bash
# Update system
sudo apt-get update
sudo apt-get upgrade -y

# Install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
export PATH="$PATH:$HOME/.dotnet"

# Install SQL Server
# Follow https://docs.microsoft.com/sql/linux/quickstart-install-connect-ubuntu

# Install FFmpeg
sudo apt-get install ffmpeg

# Install systemd service manager
sudo apt-get install systemctl
```

#### Create System Service

Create `/etc/systemd/system/youtube-shorts-automator.service`:

```ini
[Unit]
Description=YouTube Shorts Automator
After=network.target

[Service]
Type=notify
User=youtube-automator
WorkingDirectory=/opt/youtube-shorts-automator
ExecStart=/opt/youtube-shorts-automator/YouTubeShortsAutomator
Restart=always
RestartSec=10
SyslogIdentifier=youtube-automator
Environment="ASPNETCORE_ENVIRONMENT=Production"

[Install]
WantedBy=multi-user.target
```

#### Setup Application

```bash
# Create user
sudo useradd -r -s /bin/false youtube-automator

# Create directory
sudo mkdir -p /opt/youtube-shorts-automator
sudo chown youtube-automator:youtube-automator /opt/youtube-shorts-automator

# Publish application
dotnet publish -c Release -o /opt/youtube-shorts-automator

# Set permissions
sudo chown -R youtube-automator:youtube-automator /opt/youtube-shorts-automator

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable youtube-shorts-automator
sudo systemctl start youtube-shorts-automator

# Check status
sudo systemctl status youtube-shorts-automator

# View logs
sudo journalctl -u youtube-shorts-automator -f
```

### Method 3: Azure App Service Deployment

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Resource group created

#### Deployment Steps

```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name youtube-shorts-automator \
  --location eastus

# Create SQL Database
az sql server create \
  --resource-group youtube-shorts-automator \
  --name yt-shorts-server \
  --admin-user sqladmin \
  --admin-password YourPassword123!

az sql db create \
  --resource-group youtube-shorts-automator \
  --server yt-shorts-server \
  --name YouTubeShortsAutomator

# Create App Service Plan
az appservice plan create \
  --name youtube-shorts-plan \
  --resource-group youtube-shorts-automator \
  --sku B2 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group youtube-shorts-automator \
  --plan youtube-shorts-plan \
  --name youtube-shorts-automator \
  --runtime "dotnet:10"

# Configure connection string
az webapp config appsettings set \
  --resource-group youtube-shorts-automator \
  --name youtube-shorts-automator \
  --settings \
    ConnectionString="Server=tcp:yt-shorts-server.database.windows.net,1433;Initial Catalog=YouTubeShortsAutomator;Persist Security Info=False;User ID=sqladmin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    YouTubeApiKey="your-api-key" \
    ASPNETCORE_ENVIRONMENT="Production"

# Deploy application
az webapp up \
  --resource-group youtube-shorts-automator \
  --name youtube-shorts-automator \
  --plan youtube-shorts-plan
```

## Nginx Reverse Proxy Setup

Create `/etc/nginx/sites-available/youtube-shorts-automator`:

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com;

    # Redirect to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Gzip compression
    gzip on;
    gzip_types text/plain text/css text/javascript application/json application/javascript;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;

        # Timeouts for long-running uploads
        proxy_read_timeout 3600s;
        proxy_connect_timeout 600s;
        proxy_send_timeout 600s;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/youtube-shorts-automator /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

## Database Backup Strategy

### Automated Backups

Create backup script `/opt/backups/backup.sh`:

```bash
#!/bin/bash

BACKUP_DIR="/opt/backups"
DB_SERVER="localhost"
DB_NAME="YouTubeShortsAutomator"
DB_USER="sa"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/backup_$TIMESTAMP.bak"

# Create backup
sqlcmd -S $DB_SERVER -U $DB_USER -P $SA_PASSWORD \
  -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_FILE' WITH FORMAT"

# Keep only last 30 days
find $BACKUP_DIR -name "backup_*.bak" -mtime +30 -delete

echo "Backup completed: $BACKUP_FILE"
```

Schedule with cron:

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /opt/backups/backup.sh
```

## Monitoring and Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry(
    new ApplicationInsightsServiceOptions
    {
        InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"]
    }
);
```

### Serilog Configuration

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/youtube-shorts/app.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

### Health Checks

```bash
# Monitor health endpoint
curl -f http://localhost:5000/api/health

# Setup monitoring alert
# Configure alert if health check fails more than 3 times in 5 minutes
```

## SSL/TLS Certificate Setup

### Using Let's Encrypt

```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Generate certificate
sudo certbot certonly --nginx -d yourdomain.com

# Auto-renewal
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

# Verify renewal
sudo certbot renew --dry-run
```

## Rollback Procedure

```bash
# List available versions
docker images | grep youtube-shorts-automator

# Rollback to previous version
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d --pull always \
  -e VERSION=1.1.0

# Or with systemd
sudo systemctl stop youtube-shorts-automator
# Restore previous binary version
sudo systemctl start youtube-shorts-automator
```

## Performance Tuning

### .NET Configuration

```json
{
  "AppSettings": {
    "MaxConcurrentUploads": 5,
    "MaxConcurrentProcessing": 3,
    "ProcessingQueueLimit": 500
  }
}
```

### SQL Server Tuning

```sql
-- Increase connection pool
-- Set min pool size to 10, max to 100 in connection string

-- Create indexes
CREATE INDEX idx_upload_jobs_status ON UploadJobs(Status);
CREATE INDEX idx_upload_jobs_scheduled ON UploadJobs(ScheduledUploadTime);
CREATE INDEX idx_videos_status ON Videos(Status);

-- Update statistics
UPDATE STATISTICS Videos;
UPDATE STATISTICS UploadJobs;
```

### Nginx Tuning

```nginx
worker_processes auto;
worker_connections 2048;
keepalive_timeout 65;
types_hash_max_size 2048;
client_max_body_size 512M;  # For large video uploads
```

## Disaster Recovery

### Database Recovery

```bash
# Restore from backup
sqlcmd -S localhost -U sa -P $SA_PASSWORD \
  -Q "RESTORE DATABASE YouTubeShortsAutomator FROM DISK = N'/opt/backups/backup_20260504_020000.bak' WITH REPLACE"
```

### Volume Recovery (Docker)

```bash
# List available backups
docker run --rm -v db-data:/data alpine ls -la /data

# Restore from backup
docker run --rm -v db-data:/data -v /opt/backups:/backups \
  alpine cp /backups/db_backup.tar.gz /data/
```

## Maintenance Windows

Schedule maintenance during low-traffic periods:

```bash
# Disable traffic
sudo systemctl stop youtube-shorts-automator

# Perform maintenance (upgrades, backups, etc.)
dotnet ef database update

# Re-enable traffic
sudo systemctl start youtube-shorts-automator
```

## Post-Deployment Validation

```bash
# Test API endpoints
curl -f http://localhost:5000/api/health

# Test processing
curl -X POST http://localhost:5000/api/processing/videos \
  -H "X-API-Key: your-api-key" \
  -F "videoFile=@test.mp4"

# Check logs
tail -f /var/log/youtube-shorts/app.log

# Verify database
sqlcmd -S localhost -U sa -Q "SELECT COUNT(*) FROM Videos;"
```
