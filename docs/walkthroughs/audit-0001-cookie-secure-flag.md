# Walkthrough: audit-0001-cookie-secure-flag

## Task Reference
- Task: `docs/tasks/audit-0001-cookie-secure-flag.md`
- Walkthrough: `docs/walkthroughs/audit-0001-cookie-secure-flag.md`
- Branch/PR: `fix/epic-002-audit-remediation` / PR #231
- Date: `2026-02-09`

## Summary
Made the cookie `Secure` flag environment-aware in `AuthController` by injecting `IWebHostEnvironment` and computing a `_useSecureCookies` boolean. Applied to both the CSRF double-submit cookie and the JWT access token cookie. This eliminates a CRITICAL vulnerability where tokens could be intercepted over HTTP in production.

## Context
- Background: Epic 002 implemented authentication features (OAuth, password reset, email verification, rate limiting, soft-delete). The audit found that both auth cookies had `Secure = false` hardcoded regardless of environment.
- Problem statement: Cookies without the `Secure` flag are transmitted over plain HTTP, allowing session hijacking via network sniffing in production.
- Constraints: Integration tests run in a "Testing" environment where HTTPS is not available, so the fix must be environment-aware rather than unconditionally `true`.

## Decisions & Trade-offs
- **Decision:** Use `!IsDevelopment() && EnvironmentName != "Testing"` instead of `IsProduction()`
  - Options considered: (a) `IsProduction()` only, (b) `!IsDevelopment()`, (c) `!IsDevelopment() && != "Testing"`
  - Why this choice: Option (a) would leave staging/preview environments insecure. Option (b) would break integration tests running in "Testing" environment. Option (c) covers all non-local environments while keeping tests functional.
  - Consequences / risks: If a new environment name is introduced (e.g., "Preview"), cookies will be secure by default (safe default).

- **Decision:** Follow existing pattern from `OAuthConfiguration.cs:17-19`
  - Options considered: (a) Custom configuration key in appsettings.json, (b) Follow `OAuthConfiguration.cs` pattern
  - Why this choice: The codebase already had an established pattern for environment-based cookie policy. Reusing it improves consistency and reduces cognitive load.
  - Consequences / risks: None — consistent with established patterns.

## Implementation Notes
- Key changes:
  - Added `IWebHostEnvironment environment` parameter to `AuthController` constructor (line 51)
  - Added `private readonly bool _useSecureCookies` field (line 39)
  - Computed value in constructor: `_useSecureCookies = !environment.IsDevelopment() && environment.EnvironmentName != "Testing"` (line 64)
  - Replaced `Secure = false` at CSRF cookie (line 77) and access token cookie (line 416)
- Edge cases handled: Testing environment correctly excluded so integration tests continue to work
- Known limitations: The `Secure` flag is determined at startup and cannot be changed at runtime. This is acceptable because the hosting environment does not change during application lifetime.

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
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` — Added `IWebHostEnvironment` injection, `_useSecureCookies` field, and applied to both cookie `Secure` flags

## Tests
### Unit
- What was added/updated: None — cookie configuration is tested at the integration level
- How to run: N/A

### Integration
- What was added/updated: No new tests. Existing tests in `AuthEndpointTests.cs` continue to pass because `ApiWebApplicationFactory` uses "Testing" environment where `_useSecureCookies = false`.
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~IntegrationTests"`

### Manual
- What you verified: N/A (would verify in HTTPS staging environment)
- Steps: Deploy to environment with HTTPS, open browser DevTools > Application > Cookies, verify `Secure` attribute is present on `access_token` and `csrf_token` cookies

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Cookie `Secure` flag now enforced in all non-development/testing environments
- AuthN/AuthZ impact: Prevents session hijacking via HTTP interception in production
- Sensitive data handling: JWT tokens are now protected from transmission over insecure channels

## Follow-ups / Backlog
- None

## Checklist
- [x] Task scope matches `docs/tasks/audit-0001-cookie-secure-flag.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
