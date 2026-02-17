# Audit Action: audit-0004-csrf-rejection-tests

## Metadata
- ID: audit-0004
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#6 - CSRF rejection integration tests"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0004-csrf-rejection-tests.md`

## Goal
Add integration tests verifying that POST requests to auth endpoints without CSRF tokens are properly rejected with 403 Forbidden, providing a regression safety net for the CSRF middleware.

## Context
The CSRF double-submit cookie pattern was implemented in `AuthController` during Epic 002, but no integration tests verified the rejection path. If someone accidentally disabled or misconfigured the CSRF middleware during a future refactoring, no test would catch it. The audit identified this as a test coverage gap.

## Scope
### In scope
- Add integration tests for 4 critical POST auth endpoints: login, forgot-password, reset-password, verify-email
- Each test sends a POST without the `X-CSRF-Token` header and asserts 403

### Out of scope
- Testing with invalid/expired CSRF tokens
- Testing GET endpoints (not protected by CSRF)
- Testing PATCH/PUT endpoints (same middleware, covered by implication)
- CSRF token generation tests

## Requirements
- R1: Each test sends a POST request without the `X-CSRF-Token` header
- R2: Each test asserts HTTP 403 Forbidden response
- R3: Tests are isolated from rate limiting (use `UniqueIpAddress()`)

## Acceptance Criteria
- [x] AC1: `Login_WithoutCsrfToken_ShouldReturn403` test exists and passes
- [x] AC2: `ForgotPassword_WithoutCsrfToken_ShouldReturn403` test exists and passes
- [x] AC3: `ResetPassword_WithoutCsrfToken_ShouldReturn403` test exists and passes
- [x] AC4: `VerifyEmail_WithoutCsrfToken_ShouldReturn403` test exists and passes
- [x] AC5: Build passes
- [x] AC6: All 295 tests pass (176 unit + 5 architecture + 114 integration)

## Constraints
- Must use existing `ApiWebApplicationFactory` test infrastructure
- Must not interfere with other tests (rate limiting isolation via `UniqueIpAddress()`)

## Implementation Steps
1. Add `Login_WithoutCsrfToken_ShouldReturn403` test to `AuthEndpointTests.cs`
2. Add `ForgotPassword_WithoutCsrfToken_ShouldReturn403` test
3. Add `ResetPassword_WithoutCsrfToken_ShouldReturn403` test
4. Add `VerifyEmail_WithoutCsrfToken_ShouldReturn403` test
5. Each test: create client, build `HttpRequestMessage` with POST method and JSON content, add `X-Real-IP` header for rate limit isolation, send via `SendAsync`, assert `HttpStatusCode.Forbidden`

## Testing Plan
- Unit tests: N/A — this task IS integration tests
- Integration tests: 4 new tests added (lines 1093-1164 in `AuthEndpointTests.cs`)
- Manual checks: N/A

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
