# Security Policy

## Reporting Security Vulnerabilities

**Do NOT open public GitHub issues for security vulnerabilities.** This could expose the vulnerability before a fix is available.

### Responsible Disclosure

If you discover a security vulnerability in YouTube Shorts Automator, please report it using one of these methods:

#### Option 1: GitHub Private Vulnerability Reporting (Recommended)

Use GitHub's private vulnerability reporting feature:

1. Go to [GitHub Security Advisories](https://github.com/sarmkadan/youtube-shorts-automator/security/advisories/new)
2. Click "Report a vulnerability"
3. Provide details about the vulnerability
4. Submit your report

#### Option 2: Direct Email

Send a detailed report to: **rutova2@gmail.com**

Include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Affected versions
- Suggested fix (if you have one)
- Your contact information

### What to Include in a Report

- **Type of Vulnerability**: SQL injection, XSS, authentication bypass, etc.
- **Location**: Specific file, function, or component affected
- **Severity**: Critical, High, Medium, Low (use CVSS v3.1 if possible)
- **Description**: Clear explanation of the issue
- **Impact**: What could an attacker do?
- **Reproduction Steps**: How to verify the vulnerability
- **Proof of Concept** (optional): Code or steps to demonstrate

## Supported Versions

Security updates will be provided for:

| Version | Status | Support Until |
|---------|--------|---|
| 1.x     | Active | 2027-05-06 |
| 0.x     | EOL    | Not supported |

Only the latest version receives active security support. We recommend always upgrading to the latest release.

## Security Response Timeline

Our commitment to security:

- **48 hours**: Initial acknowledgment of your report
- **7 days**: Security assessment and remediation plan
- **14 days**: Patch release (when possible)
- **30 days**: Public disclosure after patch is available

## Security Best Practices for Users

### When Using YouTube Shorts Automator

1. **Keep Dependencies Updated**
   ```bash
   dotnet list package --outdated
   ```

2. **Secure Configuration**
   - Never commit credentials to version control
   - Use environment variables or Azure Key Vault for secrets
   - Keep `appsettings.json` out of public repositories
   - Use strong API keys and passwords

3. **API Security**
   ```json
   {
     "AppSettings": {
       "ApiKey": "${API_KEY}",
       "YouTubeClientSecret": "${YOUTUBE_CLIENT_SECRET}"
     }
   }
   ```

4. **Network Security**
   - Use HTTPS for all API communications
   - Validate SSL/TLS certificates
   - Use VPN when accessing from untrusted networks
   - Implement rate limiting

5. **Database Security**
   - Use strong SQL Server passwords
   - Enable encryption at rest
   - Restrict database access by IP
   - Regular backups

6. **Authentication**
   - Rotate API keys periodically
   - Use OAuth 2.0 for YouTube integration
   - Implement proper session management
   - Enable multi-factor authentication where available

### Dependency Security

```bash
# Check for vulnerable dependencies
dotnet list package --vulnerable

# Update packages
dotnet add package PackageName --version 1.2.3
```

## Vulnerability Categories

We take the following vulnerability categories seriously:

- **Authentication & Authorization**: OAuth bypasses, session hijacking, privilege escalation
- **Data Protection**: Unencrypted sensitive data, SQL injection, data exposure
- **Cryptography**: Weak encryption, improper key management
- **Input Validation**: Command injection, path traversal, XSS
- **Configuration**: Exposed secrets, insecure defaults
- **Dependencies**: Vulnerable third-party packages

## Known Security Considerations

### YouTube API Security
- OAuth tokens are stored securely with encryption
- Refresh tokens are rotated periodically
- API calls use HTTPS with certificate validation
- Rate limiting prevents API abuse

### Video Processing
- FFmpeg subprocess execution with argument validation
- Temporary files stored in restricted directories
- Input validation prevents malicious file processing
- File deletion after processing

### Database Security
- SQL parameterization prevents injection attacks
- Connection strings use encryption
- Database credentials never logged
- Access logging for audit trails

## Security Advisories

Security advisories will be published at:
- [GitHub Security Advisories](https://github.com/sarmkadan/youtube-shorts-automator/security/advisories)
- [CHANGELOG.md](CHANGELOG.md) - Security fixes documented

Subscribe to releases to be notified of security patches:
1. Go to the [project releases page](https://github.com/sarmkadan/youtube-shorts-automator/releases)
2. Click "Watch" → "Releases only"

## Security Testing

This project includes:
- SAST (Static Application Security Testing)
- Dependency scanning via Dependabot
- CI/CD security checks
- Regular security audits

## Compliance

This project aims to comply with:
- OWASP Top 10
- CWE Top 25
- NIST Cybersecurity Framework

## Questions?

For security-related questions or concerns:
- Email: rutova2@gmail.com
- GitHub Issues: [Non-sensitive questions only](https://github.com/sarmkadan/youtube-shorts-automator/issues)

---

Thank you for helping keep YouTube Shorts Automator secure!
