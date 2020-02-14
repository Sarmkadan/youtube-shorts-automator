// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Credential Management Security Guide

Comprehensive guide for configuring, securing, and rotating credentials used by YouTube Shorts Automator.

---

## Credentials overview

The application requires three categories of secrets:

| Secret | Where it is used | Sensitivity |
|---|---|---|
| YouTube OAuth 2.0 client ID & secret | Authenticating uploads via Google APIs | **High** |
| YouTube API key | Read-only API calls (quota tracking) | **Medium** |
| Internal API key (`ApiKey`) | Protecting the local REST API | **Medium** |
| Database connection string | SQL Server access | **High** |

---

## Setting up YouTube OAuth 2.0

### 1. Create a Google Cloud project
1. Go to [Google Cloud Console](https://console.cloud.google.com).
2. Click **Select a project → NEW PROJECT**.
3. Name it (e.g. `youtube-shorts-automator`) and click **CREATE**.

### 2. Enable the YouTube Data API v3
1. Open **APIs & Services → Library**.
2. Search for *YouTube Data API v3* and click **ENABLE**.

### 3. Configure the OAuth consent screen
1. Open **APIs & Services → OAuth consent screen**.
2. Select **External** (or **Internal** for organisation accounts).
3. Fill in app name, support email, and developer contact.
4. Add the scope `https://www.googleapis.com/auth/youtube.upload`.
5. Add your own Google account as a **Test user** while the app is in testing.

### 4. Create OAuth 2.0 credentials
1. Open **APIs & Services → Credentials → Create Credentials → OAuth client ID**.
2. Application type: **Desktop application**.
3. Download the JSON file – treat it like a password.

### 5. Store credentials securely

**Never commit credentials to source control.**

**Development** – use `appsettings.local.json` (in `.gitignore`):
```json
{
  "AppSettings": {
    "YouTubeClientId": "...-abc.apps.googleusercontent.com",
    "YouTubeClientSecret": "GOCSP-...",
    "YouTubeApiKey": "AIza..."
  }
}
```

**CI/CD** – add secrets in your repository settings and reference them in the workflow:
```yaml
env:
  AppSettings__YouTubeClientId: ${{ secrets.YOUTUBE_CLIENT_ID }}
  AppSettings__YouTubeClientSecret: ${{ secrets.YOUTUBE_CLIENT_SECRET }}
  AppSettings__YouTubeApiKey: ${{ secrets.YOUTUBE_API_KEY }}
```

**Production (Docker)** – use environment variables from a secrets manager, not a `.env` file:
```bash
docker run \
  -e "AppSettings__YouTubeClientId=$YOUTUBE_CLIENT_ID" \
  -e "AppSettings__YouTubeClientSecret=$YOUTUBE_CLIENT_SECRET" \
  youtube-shorts-automator
```

---

## Token lifecycle

OAuth 2.0 access tokens expire after one hour. The application stores the `AccessToken` and `RefreshToken` in the `YouTubeChannels` table and automatically detects expiry via `IsTokenExpired()`.

### Refresh flow
1. At upload time the service checks `channel.IsTokenExpired()`.
2. If expired, call the token-refresh endpoint before uploading.
3. Persist the new `AccessToken` and updated `TokenExpiresAt` back to the database.

### Revoking a token
If credentials are compromised, revoke immediately:
```bash
curl -X POST "https://oauth2.googleapis.com/revoke" \
  --data "token=<access_or_refresh_token>"
```
Then generate a new client secret in Google Cloud Console and rotate all stored tokens.

---

## Internal API key

The `ApiKey` setting controls access to the local REST API (`X-API-Key` header). Generate a strong random value:

```bash
# Linux / macOS
openssl rand -hex 32

# PowerShell
[System.Web.Security.Membership]::GeneratePassword(64, 8)
```

Store it in `appsettings.local.json` or as an environment variable:
```bash
export AppSettings__ApiKey="<generated-value>"
```

Rotate the key by generating a new value and redeploying the application.

---

## Database connection string

### Principle of least privilege
Create a dedicated SQL Server login with only the permissions the application needs:

```sql
CREATE LOGIN YouTubeShortsApp WITH PASSWORD = 'StrongPassword!';
CREATE USER YouTubeShortsApp FOR LOGIN YouTubeShortsApp;

-- Grant only what is required
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO YouTubeShortsApp;
```

Use this account in the connection string instead of `sa`:
```
Server=localhost;Database=YouTubeShortsAutomator;User Id=YouTubeShortsApp;Password=StrongPassword!;
```

### Encrypt the connection
Add `Encrypt=True;TrustServerCertificate=False;` to enforce TLS:
```
Server=prod-db;Database=YouTubeShortsAutomator;User Id=app;Password=...;Encrypt=True;TrustServerCertificate=False;
```

---

## Secret storage options

| Environment | Recommended storage |
|---|---|
| Local development | `appsettings.local.json` (git-ignored) |
| CI/CD pipelines | Repository / pipeline secret variables |
| Docker / Kubernetes | Environment variables injected at runtime |
| Cloud (Azure) | Azure Key Vault + Managed Identity |
| Cloud (AWS) | AWS Secrets Manager + IAM role |
| Cloud (GCP) | Secret Manager + Workload Identity |

---

## Security checklist

- [ ] `appsettings.local.json` is listed in `.gitignore`
- [ ] No secrets appear in `appsettings.json` committed to the repository
- [ ] YouTube OAuth consent screen has only required scopes
- [ ] Database account follows least-privilege principle
- [ ] Internal API key is at least 32 characters of random data
- [ ] TLS is enforced for the database connection in production
- [ ] Tokens are refreshed before expiry; revoked if compromised
- [ ] CI/CD secrets are stored in the platform's secret store, not in workflow files

---

## Troubleshooting

### `token_expired` error during upload
The stored access token has expired and the refresh attempt failed. Re-authenticate:
1. Delete the stored `AccessToken` and `RefreshToken` for the channel.
2. Re-run the OAuth consent flow to obtain new tokens.

### `403 Forbidden` from YouTube API
Common causes:
- API key does not have YouTube Data API v3 enabled.
- Daily quota (10,000 units) exhausted – check **Google Cloud Console → APIs & Services → YouTube Data API v3 → Quotas**.
- OAuth consent screen not approved for the scope used.

### `Invalid API Key` on local REST endpoints
The `X-API-Key` header does not match the configured `AppSettings.ApiKey`. Verify the value in `appsettings.local.json` or the environment variable.

---

## Further reading

- [Getting started](getting-started.md)
- [Pipeline configuration](pipeline-configuration.md)
- [Deployment guide](deployment.md)
- [Google OAuth 2.0 documentation](https://developers.google.com/identity/protocols/oauth2)
- [YouTube Data API v3 quotas](https://developers.google.com/youtube/v3/getting-started#quota)
