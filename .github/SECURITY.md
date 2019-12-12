# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in this project, please report it by emailing:

**rutova2@gmail.com**

Please include:
- A detailed description of the vulnerability
- Steps to reproduce or exploit
- Any proof-of-concept code
- Your assessment of the impact

We will respond within 3 business days and work with you to resolve the issue promptly.

## Supported Versions

| Version | Status | Notes |
|---------|--------|-------|
| v2.0.x | ✅ Supported | All current features and security fixes |
| v1.x | ⚠️ Security fixes only | No new features, limited support |

## Security Best Practices

- Never commit API keys, credentials, or sensitive configuration to the repository
- Use environment variables for all secrets
- Rotate credentials regularly
- Enable 2FA on your GitHub account
- Keep dependencies updated (see Dependabot configuration)

## Disclosure Policy

We follow responsible disclosure:
1. Report the vulnerability privately to rutova2@gmail.com
2. We acknowledge receipt within 3 business days
3. We investigate and develop a fix
4. We release a patch
5. We publicly disclose the vulnerability with credit to the reporter

## Security Updates

Security updates will be:
- Released as patch versions (e.g., v2.0.1)
- Announced in the CHANGELOG.md
- Available via NuGet and GitHub releases
- Backported to supported major versions when applicable

## Additional Resources

- [GitHub Security Advisories](https://github.com/advisories)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
