# Walkthrough: audit-0003-https-hsts

## Task Reference
- Task: `docs/tasks/audit-0003-https-hsts.md`
- Walkthrough: `docs/walkthroughs/audit-0003-https-hsts.md`
- Branch/PR: `audit-0003-https-hsts`
- Date: `2026-01-30`

## Summary
Implements HTTPS redirection and HSTS enforcement in non-development environments per security audit finding F-02. Also configures forwarded headers for reverse proxy compatibility and removes the deprecated X-XSS-Protection header.

## Context
- Background: Security audit F-02 identified missing HTTPS enforcement
- Problem statement: HTTP requests not forced to HTTPS, HSTS not consistently enforced
- Constraints: Must not break local HTTP development

## Decisions & Trade-offs
- **Decision:** Use conditional middleware based on environment
  - Options considered: (1) Always enable with dev cert, (2) Conditional by environment
  - Why this choice: Simpler local development, aligns with security baseline example
  - Consequences / risks: Development uses HTTP only

- **Decision:** Remove X-XSS-Protection header
  - Why this choice: Deprecated header, CSP already provides protection
  - Consequences / risks: None - CSP is the modern replacement

## Implementation Notes
- Key changes:
  - Added UseForwardedHeaders() for reverse proxy support
  - Added UseHttpsRedirection() and UseHsts() for non-dev environments
  - Removed X-XSS-Protection header from SecurityHeadersMiddleware
- Edge cases handled: Forwarded headers for load balancers
- Known limitations: Requires actual HTTPS certificate in production

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Full

## Commands Run
```bash
dotnet build
dotnet test
```

## Files Changed
- `src/SaasTemplate.AiApi.Api/Program.cs` — Added forwarded headers, HTTPS redirection, and HSTS middleware
- `src/SaasTemplate.AiApi.Api/Middleware/SecurityHeadersMiddleware.cs` — Removed deprecated X-XSS-Protection header
- `tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs` — Updated test to not expect X-XSS-Protection

## Tests
### Unit
- What was added/updated: None
- How to run: `dotnet test`

### Integration
- What was added/updated: None
- How to run: `dotnet test`

### Manual
- What you verified: HTTPS redirection active in non-dev mode
- Steps: Run API in Production mode, verify redirect behavior

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None
- Dashboards/alerts touched: None

## Security
- Validation: N/A
- AuthN/AuthZ impact: None
- Sensitive data handling: Transport layer security improved

## Performance
- Hot paths impacted: Every request (minimal overhead)
- Any profiling/bench notes: Standard ASP.NET Core middleware

## Docs Updated
- Files updated: This walkthrough
- Anything intentionally left for later: None

## Rollback Plan
- How to revert safely: Remove HTTPS middleware from Program.cs
- Data rollback considerations: None

## Follow-ups / Backlog
- [ ] Configure SSL certificates in deployment
- [ ] Set up reverse proxy forwarded headers in infrastructure

## Checklist
- [x] Task scope matches `docs/tasks/audit-0003-https-hsts.md`
- [x] Tests updated and passing (79 tests)
- [x] Docs updated where relevant
- [x] No secrets committed
