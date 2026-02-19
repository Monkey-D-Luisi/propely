# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.0.x   | Yes       |

## Reporting a Vulnerability

If you discover a security vulnerability in Propely, please report it responsibly.

**Do NOT open a public GitHub issue for security vulnerabilities.**

Instead, please report vulnerabilities by emailing the maintainer directly or using [GitHub's private vulnerability reporting](https://github.com/Monkey-D-Luisi/propely/security/advisories/new).

### What to include

- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

### What to expect

- Acknowledgment within 48 hours
- A fix or mitigation plan within 7 days for critical issues
- Credit in the release notes (unless you prefer to remain anonymous)

## Security Practices

Propely includes several security measures:

- **Authentication**: JWT with HTTP-only cookies, refresh token rotation, CSRF protection (HMAC-signed with TTL)
- **Authorization**: Role-based access control (RBAC) with Owner/Admin/Agent/Viewer hierarchy and granular permission overrides
- **Rate limiting**: Per-endpoint and global rate limits on authentication and destructive endpoints
- **Input validation**: Server-side validation on all endpoints with max-length constraints
- **Multi-tenancy isolation**: EF Core global query filters prevent cross-tenant data access
- **Dependency scanning**: Automated NuGet audit, npm audit, and Trivy container scanning in CI
- **Secret management**: Secret Manager integration with version pinning (no `latest` in production)
- **Infrastructure**: VPC networking, least-privilege IAM, Workload Identity Federation (keyless CI/CD)

For deployment security guidance, see [docs/production-hardening.md](docs/production-hardening.md).
