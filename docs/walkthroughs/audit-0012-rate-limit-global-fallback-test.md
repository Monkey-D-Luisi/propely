# Walkthrough: audit-0012-rate-limit-global-fallback-test

## Task Reference
- Task: `docs/audits/epic-002-executive-summary.md` (action #12)
- Walkthrough: `docs/walkthroughs/audit-0012-rate-limit-global-fallback-test.md`
- Branch/PR: `fix/audit-0006-session-invalidation`
- Date: `2026-02-09`

## Summary
Added an integration test that verifies the global rate limit fallback (100 requests/min for `*` endpoint) returns HTTP 429 on the 101st request from the same IP address.

## Context
- Background: The existing test `CsrfEndpoint_ShouldKeepGlobalRateLimitFallback` only verified that 20 requests succeed (proving endpoint-specific limits don't apply to CSRF), but never tested the actual global cap.
- Problem statement: The global 100/min rate limit could silently break during refactoring without test detection.
- Constraints: Must use a unique IP per test to avoid cross-test interference with other rate limit tests.

## Decisions & Trade-offs
- **Decision: Use `/auth/csrf` GET endpoint for the test**
  - Options considered: (1) Use `/auth/csrf` (GET, no auth needed), (2) Use a non-auth endpoint like `/feature-flags`
  - Why this choice: `/auth/csrf` is the simplest unauthenticated GET endpoint that doesn't have its own endpoint-specific rate limit rule, so only the global `*` fallback applies.
  - Consequences: The test sends 101 sequential requests which takes a few seconds but accurately validates the limit.

## Files Changed
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs` — Added `GlobalRateLimit_ShouldReturn429After100RequestsFromSameIp` test

## Tests
### Integration
- `GlobalRateLimit_ShouldReturn429After100RequestsFromSameIp` — Sends 101 requests from the same simulated IP, verifies the 101st returns 429 with `Retry-After` header

## Checklist
- [x] Task scope matches audit action #12 in `docs/audits/epic-002-executive-summary.md`
- [x] Tests updated and passing (310 total: 5 arch + 184 unit + 121 integration)
- [x] No secrets committed
