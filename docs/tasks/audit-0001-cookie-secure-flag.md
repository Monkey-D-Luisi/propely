# Audit Action: audit-0001-cookie-secure-flag

## Metadata
- ID: audit-0001
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P0
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#1 - Cookie Secure flag environment-aware"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0001-cookie-secure-flag.md`

## Goal
Make JWT access token and CSRF cookie `Secure` flag environment-aware to prevent session hijacking over HTTP in production.

## Context
Both the CSRF cookie (`AuthController.cs:74`) and the access token cookie (`AuthController.cs:413`) had `Secure = false` hardcoded. This allowed tokens to be transmitted over plain HTTP in any environment, including production. An attacker on the same network could intercept the JWT and impersonate the user. The pattern for environment-aware cookie configuration already existed in `OAuthConfiguration.cs:17-19`.

## Scope
### In scope
- Inject `IWebHostEnvironment` into `AuthController` constructor
- Compute `_useSecureCookies` field based on environment
- Apply to CSRF cookie and access token cookie

### Out of scope
- HSTS configuration
- Cookie `SameSite` attribute changes
- Cookie `Domain` or `Path` changes

## Requirements
- R1: Cookie `Secure = true` in Production and Staging environments
- R2: Cookie `Secure = false` only in Development and Testing environments
- R3: Follow the same pattern as `OAuthConfiguration.cs` for consistency

## Acceptance Criteria
- [x] AC1: CSRF cookie uses `Secure = _useSecureCookies`
- [x] AC2: Access token cookie uses `Secure = _useSecureCookies`
- [x] AC3: `_useSecureCookies` is `false` when `IsDevelopment()` or `EnvironmentName == "Testing"`
- [x] AC4: Build passes
- [x] AC5: All tests pass

## Constraints
- Must not break integration tests (which run in "Testing" environment)
- Must follow existing environment-check patterns in the codebase

## Implementation Steps
1. Add `IWebHostEnvironment environment` parameter to `AuthController` constructor
2. Add `private readonly bool _useSecureCookies` field
3. Compute: `_useSecureCookies = !environment.IsDevelopment() && environment.EnvironmentName != "Testing"`
4. Replace `Secure = false` with `Secure = _useSecureCookies` in CSRF cookie (line 74)
5. Replace `Secure = false` with `Secure = _useSecureCookies` in access token cookie (line 413)

## Testing Plan
- Unit tests: N/A — cookie configuration is an API-layer concern tested via integration tests
- Integration tests: Existing tests pass (ApiWebApplicationFactory runs in "Testing" environment where `_useSecureCookies = false`)
- Manual checks: Deploy to HTTPS environment, verify cookies have `Secure` attribute in browser DevTools

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
