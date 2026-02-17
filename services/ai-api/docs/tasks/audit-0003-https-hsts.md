# Audit Action: audit-0003-https-hsts

## Metadata
- ID: audit-0003
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-30
- Priority: P1
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Enforce HTTPS redirection + HSTS in non-dev environments"
- Dependencies:
  - audit-0001, audit-0002 (security baseline work) - completed
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0003-https-hsts.md`
  - Security baseline: `docs/standards/security-baseline.md`
  - Security audit: `docs/audits/2026-01-30-security.md` (Finding F-02)

## Goal
Enforce HTTPS redirection and HSTS headers in non-development environments to prevent downgrade attacks and MITM risks.

## Context
The security audit (F-02) identified that:
- Pipeline does not include `UseHttpsRedirection()` or `UseHsts()`
- Security headers middleware only appends HSTS when requests are already HTTPS
- This increases risk of downgrade and MITM attacks in production

The security baseline requires HTTPS redirection and HSTS in non-development environments.

## Scope
### In scope
- Add `UseHttpsRedirection()` in non-development environments
- Add `UseHsts()` in non-development environments
- Configure forwarded headers for reverse proxy compatibility
- Remove deprecated X-XSS-Protection header (already covered by CSP)

### Out of scope
- SSL certificate configuration (infrastructure concern)
- Reverse proxy setup (deployment concern)

## Requirements
- R1: HTTPS redirection must be enabled in non-development environments
- R2: HSTS must be applied with appropriate max-age
- R3: Forwarded headers must be configured for reverse proxy scenarios
- R4: Development must continue to work without HTTPS

## Acceptance Criteria
- [x] `UseHttpsRedirection()` added for non-dev environments
- [x] `UseHsts()` added for non-dev environments
- [x] Forwarded headers configured
- [x] X-XSS-Protection header removed (deprecated)
- [x] Build passes
- [x] All tests pass (79 tests)

## Constraints
- C1: Must not break local development (HTTP allowed in dev)
- C2: Must align with security baseline patterns

## Implementation Steps
1. Add UseForwardedHeaders() at the start of the pipeline
2. Add conditional HTTPS redirection and HSTS for non-dev environments
3. Remove deprecated X-XSS-Protection header from SecurityHeadersMiddleware
4. Run build and tests

## Testing Plan
- Unit tests: None required (infrastructure configuration)
- Integration tests: Existing tests should pass
- Manual checks: Verify HTTPS headers in production mode

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (79 tests)
- [x] No secrets committed
- [x] Walkthrough updated
