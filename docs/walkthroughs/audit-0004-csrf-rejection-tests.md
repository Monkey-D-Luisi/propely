# Walkthrough: audit-0004-csrf-rejection-tests

## Task Reference
- Task: `docs/tasks/audit-0004-csrf-rejection-tests.md`
- Walkthrough: `docs/walkthroughs/audit-0004-csrf-rejection-tests.md`
- Branch/PR: `fix/epic-002-audit-remediation` / PR #231
- Date: `2026-02-09`

## Summary
Added 4 integration tests to `AuthEndpointTests.cs` that verify POST requests to auth endpoints without the CSRF token header are rejected with 403 Forbidden. This provides a regression safety net: if the CSRF middleware is accidentally disabled or misconfigured, these tests will fail immediately.

## Context
- Background: Epic 002 implemented a CSRF double-submit cookie pattern in `AuthController`. POST endpoints validate that the `X-CSRF-Token` header matches the `csrf_token` cookie. The implementation was correct, but no test verified the rejection path.
- Problem statement: Without CSRF rejection tests, the protection could silently break during future refactoring with no test detection. This was identified as a MEDIUM test coverage gap in the audit.
- Constraints: Tests must be isolated from rate limiting and from each other. The existing `ApiWebApplicationFactory` and test helpers must be reused.

## Decisions & Trade-offs
- **Decision:** Test 4 POST endpoints rather than all auth endpoints
  - Options considered: (a) Test all POST/PATCH/PUT endpoints, (b) Test only POST endpoints, (c) Test only the 4 most critical POST endpoints
  - Why this choice: The 4 selected endpoints (login, forgot-password, reset-password, verify-email) are the most security-critical and all go through the same CSRF middleware. PATCH/PUT endpoints (change-password, update-profile) share the same middleware path, so testing them would add coverage breadth but not depth.
  - Consequences / risks: If someone adds endpoint-specific CSRF bypass for PATCH/PUT, these tests wouldn't catch it. Acceptable risk given the middleware is applied globally.

- **Decision:** Use `SendAsync` with raw `HttpRequestMessage` instead of helper methods
  - Options considered: (a) Use existing `PostAsync` helpers (which may inject CSRF automatically), (b) Build raw `HttpRequestMessage` via `SendAsync`
  - Why this choice: The test needs to explicitly NOT send the CSRF header. Using `SendAsync` with a manually constructed request ensures no automatic CSRF injection from test helpers. This makes the test intent explicit.
  - Consequences / risks: Slightly more verbose test code, but clearer intent.

## Implementation Notes
- Key changes: Added 4 test methods to `AuthEndpointTests.cs` (lines 1093-1164):
  - `Login_WithoutCsrfToken_ShouldReturn403` — Registers a user first, then attempts login without CSRF
  - `ForgotPassword_WithoutCsrfToken_ShouldReturn403` — Sends forgot-password without CSRF
  - `ResetPassword_WithoutCsrfToken_ShouldReturn403` — Sends reset-password with dummy token without CSRF
  - `VerifyEmail_WithoutCsrfToken_ShouldReturn403` — Sends verify-email with dummy token without CSRF
- Edge cases handled: Each test uses `UniqueIpAddress()` via `X-Real-IP` header to avoid rate limit interference from other tests
- Known limitations: Tests verify the middleware rejection at the HTTP level but do not test the token validation logic itself (valid token matching). Existing tests that successfully call these endpoints implicitly test the happy path.

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Files Changed
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs` — Added 4 new CSRF rejection test methods (lines 1093-1164, ~72 lines)

## Tests
### Unit
- What was added/updated: None
- How to run: N/A

### Integration
- What was added/updated: 4 new tests:
  - `Login_WithoutCsrfToken_ShouldReturn403`
  - `ForgotPassword_WithoutCsrfToken_ShouldReturn403`
  - `ResetPassword_WithoutCsrfToken_ShouldReturn403`
  - `VerifyEmail_WithoutCsrfToken_ShouldReturn403`
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~WithoutCsrfToken"`
- Results: All 4 tests pass. Total suite: 295 tests (176 unit + 5 architecture + 114 integration)

### Manual
- What you verified: N/A
- Steps: N/A — fully covered by automated integration tests

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Confirms CSRF middleware correctly rejects unauthenticated POST requests
- AuthN/AuthZ impact: None — tests verify existing security behavior
- Sensitive data handling: N/A

## Follow-ups / Backlog
- None

## Checklist
- [x] Task scope matches `docs/tasks/audit-0004-csrf-rejection-tests.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
