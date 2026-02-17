# Audit Executive Summary: Epic 002 — Auth Completion & Security

## Audit Metadata
- **Epic:** `docs/backlog/epic-002-auth-security.md`
- **Date:** 2026-02-09
- **Auditor:** Agent
- **Status:** In Progress
- **Tasks audited:** 5 (0027, 0028, 0029, 0030, 0031)
- **Services affected:** orgs-api, web
- **Commits analyzed:** 22+

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | 95/100 | Excellent — Clean Architecture strictly enforced with automated tests |
| Security (Backend) | 75/100 | Good with important findings requiring pre-production fixes |
| Security (Frontend) | 95/100 | Excellent — no dangerouslySetInnerHTML, proper redirect validation |
| Code Quality | 90/100 | Very good — consistent patterns, minor controller size concern |
| Test Coverage | 85/100 | Very good with specific gaps identified |
| Documentation | 85/100 | Good — minor DOD inconsistencies (now fixed) |
| **Overall** | **87/100** | **Production-ready with caveats** |

---

## Security Findings

### CRITICAL

#### F1. Cookie `Secure` flag hardcoded to `false` for access token and CSRF
- **Severity:** CRITICAL
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs:413` (access token), `AuthController.cs:74` (CSRF token)
- **Problem:** Both the JWT bearer cookie and the CSRF double-submit cookie used `Secure = false`, allowing tokens to be transmitted over plain HTTP. An attacker on the same network could intercept the JWT and impersonate the user.
- **Impact:** Session hijacking, unauthorized API access, full account takeover.
- **Recommendation:** Make the flag environment-aware: `Secure = true` in production, `false` only in Development/Testing. Use `IWebHostEnvironment.IsDevelopment()` pattern already established in `OAuthConfiguration.cs`.
- **Status:** REMEDIATED — Fixed during this audit session. Cookie `Secure` flag is now environment-aware using `IWebHostEnvironment`.

### HIGH

#### F2. Timing attack in login enables email enumeration
- **Severity:** HIGH
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs:25`
- **Problem:** The original short-circuit evaluation `user is null || !_passwordHasher.VerifyPassword(...)` caused different response times: ~5-10ms for non-existent emails (immediate return) vs ~150-300ms for existing emails (bcrypt computation). An attacker measuring response times could enumerate valid email addresses.
- **Impact:** Email enumeration — attacker discovers which emails have accounts, enabling targeted phishing or credential stuffing.
- **Recommendation:** Always execute a bcrypt verification, even when the user is not found, using a pre-computed dummy hash to consume equivalent time.
- **Status:** REMEDIATED — Fixed during this audit session. `TimingSafetyHash` constant added; bcrypt always runs regardless of user existence.

#### F3. No account lockout mechanism after failed login attempts
- **Severity:** HIGH
- **Files:** N/A (missing feature)
- **Problem:** Rate limiting is per-IP only (5 attempts/min via AspNetCoreRateLimit). A distributed attacker using multiple IPs can brute-force passwords without triggering any lockout. There is no per-user/per-email tracking of failed attempts.
- **Impact:** Password brute-force from distributed sources bypasses IP-based rate limiting.
- **Recommendation:** Implement an `ILoginAttemptTracker` service that tracks failed attempts per user ID. After 5 failures, lock the account for 15 minutes with exponential backoff. Store attempt counts in Redis for distributed state.

### MEDIUM

#### F4. Existing sessions not invalidated on password change/reset
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs:48-51`
- **Problem:** When a user changes or resets their password, all existing JWT tokens remain valid until their natural expiration (24h). If an attacker already has a stolen token, they can continue using it even after the victim changes their password.
- **Recommendation:** Add a `PasswordVersion` (int) field to the User entity, increment it on password change, include it as a JWT claim, and validate it in `ActiveUserMiddleware`. Alternatively, maintain a token blacklist in Redis.

#### F5. No maximum password length validation
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/RegisterRequestValidator.cs`, `ResetPasswordRequestValidator.cs`, `ChangePasswordRequestValidator.cs`
- **Problem:** Only minimum length (8 chars) was validated. Extremely long passwords (100KB+) could cause denial-of-service through bcrypt computation time.
- **Recommendation:** Add `MaximumLength(128)` to all password validators.
- **Status:** REMEDIATED — Fixed during this audit session. All four password validators now enforce max 128 characters.

#### F6. Missing CSRF rejection integration tests
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs`
- **Problem:** No integration tests verified that POST requests without CSRF tokens are properly rejected with 403. This means the CSRF protection could silently break during refactoring without test detection.
- **Recommendation:** Add integration tests that send POST requests to auth endpoints without the CSRF header and verify 403 response.
- **Status:** REMEDIATED — Fixed during this audit session. Four new CSRF rejection tests added (login, forgot-password, reset-password, verify-email).

### LOW

#### F7. CSRF token without expiration
- **Severity:** LOW
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs:64-79`
- **Problem:** Generated CSRF tokens have no TTL. Once issued, a token remains valid indefinitely as long as the cookie exists.
- **Recommendation:** Include a timestamp in the CSRF token payload and reject tokens older than 1 hour during validation.

#### F8. No security event audit logging
- **Severity:** LOW
- **Files:** N/A (missing feature)
- **Problem:** Security-sensitive events are not logged: failed login attempts, consumed/expired reset tokens, failed password verifications, rejected OAuth attempts. This makes incident investigation and forensics difficult.
- **Recommendation:** Add structured logging (ILogger) for all security events with correlation IDs. Consider a dedicated `SecurityAuditService` that writes to a separate log stream.

---

## Architecture Compliance

### Adherence Score: 95/100

Clean Architecture is strictly enforced across the entire epic implementation, verified by automated architecture tests in `ArchitectureTests.cs`.

### Positive Observations
- **Domain layer** has zero framework dependencies. Entities contain pure business logic with methods like `User.VerifyEmail()`, `User.ChangePassword()`, `User.SoftDelete()`.
- **Application layer** uses CQRS via MediatR with clean interfaces (`IUserRepository`, `IJwtTokenService`, `IPasswordHasher`, `IEmailService`). No imports from Infrastructure or Api layers.
- **Infrastructure layer** implements all external concerns (EF Core, BCrypt, JWT, SMTP) behind application-defined interfaces.
- **Api layer** is a thin delegation layer — controllers send MediatR commands/queries and map results to DTOs.
- **Architecture tests** (`ArchitectureTests.cs`) automatically verify that dependencies flow inward only, preventing regressions.
- **Stateless password reset** via JWT with SHA256 fingerprint of current password hash — eliminates need for a reset token table in DB.
- **Global query filters** for soft-delete — EF Core automatically excludes deleted entities, reducing error surface.

### Violations / Concerns
- `AuthController.cs` is 585 lines with 8 injected validators in the constructor. This is a sign the controller handles too many responsibilities (local auth + OAuth + CSRF + cookie management). Consider splitting OAuth endpoints into a dedicated `OAuthController`.

---

## Code Quality

### Backend
**Strengths:**
- Consistent PascalCase naming for public members, camelCase for locals throughout all handlers and services.
- All command/query handlers are `sealed` with single responsibility.
- FluentValidation validators are cleanly separated from business logic.
- Typed exceptions (`DomainException`, `ConflictException`, `NotFoundException`) with consistent error codes.
- `CancellationToken` properly propagated through all async chains.
- `ArgumentException.ThrowIfNullOrWhiteSpace` used for service constructor guards.
- BCrypt with automatic salt generation and work factor 11 (industry standard).
- JWT validation is thorough: issuer, audience, lifetime, signing key, and purpose claim all verified.
- Token purpose separation prevents token confusion attacks (access vs email-verification vs password-reset).

**Issues:**
- `AuthController.cs` at 585 lines exceeds typical controller size guidelines. The 8 validator injections signal excessive responsibility. `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs:1-585`
- Manual cookie parsing in CSRF logic instead of using a cookie helper or library.

### Frontend
**Strengths:**
- React Hook Form + Zod for type-safe form validation across all auth forms.
- Cancel-on-unmount pattern (`active` variable in useEffect cleanup) prevents state updates on unmounted components.
- No `dangerouslySetInnerHTML` in any auth component.
- No `any` types in auth components — fully typed.
- Accessibility: `aria-invalid`, `aria-describedby`, `role="alert"` properly applied to form fields.
- Complete i18n (EN + ES, 351+ lines per locale file).
- `Retry-After` header parsed correctly in both numeric and date formats for 429 responses.
- `SanitizeNext()` validates redirect paths: rejects absolute URLs, `//` protocol-relative URLs, and CRLF injection.
- Double-click prevention on OAuth buttons to prevent duplicate auth flows.

**Issues:**
- Manual cookie parsing in `csrf.ts` — could use a lightweight cookie library for consistency. `apps/web/src/lib/csrf.ts`

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (handlers) | ~25 across all auth handlers | No explicit timing attack test; OAuth email-not-verified scenario at controller level |
| Integration (endpoints) | ~20 auth endpoint tests + 5 soft-delete tests | CSRF rejection tests added during audit |
| Architecture | 5 dependency rule tests | None |
| Frontend | ~56 component tests | None significant |

### Missing Tests
1. **OAuth email-not-verified scenario unit test** — The check `if (!emailVerified)` exists at `AuthController.cs:299-302` (controller level), so a handler-level unit test doesn't apply. An integration test covering the full OAuth callback with unverified provider email would be the correct approach.
2. **Rate limiting global fallback test** — Only auth-specific rate limits are tested. No test verifies the global 100/min fallback applies to non-auth endpoints.
3. **Explicit token expiry tests** — `ResetPasswordCommandHandler` tests cover valid/invalid/consumed tokens but no explicit test for the 1-hour expiry boundary.

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DOD | Walkthrough | Issue |
|------|----------|-------------|-------|
| 0027 | Complete | Complete | Aligned |
| 0028 | Complete | Complete | Aligned |
| 0029 | Complete | Complete | Aligned (DOD fixed during audit) |
| 0030 | Complete | Complete | Aligned (DOD fixed during audit) |
| 0031 | Complete | Complete | Aligned |

### Other Documentation Issues
- Tasks 0029 and 0030 had unchecked DOD checkboxes despite being marked DONE — fixed during this audit.
- Missing coverage matrix mapping acceptance criteria to specific test names.
- Manual testing steps in tasks 0029 and 0030 were not formally documented as completed.

---

## Commit History

### Pattern Compliance
- Conventional commits: **Yes** — all commits use `feat`, `fix`, `docs`, `test`, `chore` prefixes.
- Branch naming: **Yes** — `feat/<description>-<task-number>` format consistently applied.
- Code review cycles: **Observed** — `fix(code-review): address PR #XXX feedback (#cr-XXXX)` pattern seen for tasks 0028, 0029, 0030, 0031.

### Observations
- Each task follows a consistent workflow: feature commit -> code review artifacts -> review feedback fixes -> docs completion.
- 22+ commits analyzed across all 5 tasks.
- No force pushes or unusual patterns detected.
- Issue references consistently included in commit messages (e.g., `#0027`, `#0028`).
- Clean merge history with proper PR-based integration.

---

## What's Done Well

This section recognizes the strong implementation choices that should be preserved and replicated in future epics.

1. **Stateless password reset** — Using JWT with SHA256 fingerprint of the current password hash (`JwtTokenService.cs:88-103`) eliminates the need for a database token table. The fingerprint automatically invalidates tokens when the password changes. Elegant and secure.
2. **Email enumeration prevention in forgot-password** — `ForgotPasswordCommandHandler.cs:28-31` returns success silently even for non-existent emails, preventing attackers from discovering valid accounts.
3. **CSRF double-submit cookie pattern** — Proper implementation with `StringComparer.Ordinal` comparison (`AuthController.cs:444-455`), rejecting timing-based bypass attempts.
4. **Token purpose separation** — JWT claims include `purpose` (access, email-verification, password-reset) preventing token confusion attacks (`JwtTokenService.cs:13-16`).
5. **OAuth provider binding validation** — Prevents CSRF in OAuth flow by verifying the provider parameter matches the OAuth state (`AuthController.cs:550-571`).
6. **Open redirect prevention** — `SanitizeNext()` validates redirect paths, rejecting absolute URLs, `//` protocol-relative URLs, and CRLF injection (`AuthController.cs:485-511`).
7. **Security headers middleware** — `SecurityHeadersMiddleware.cs` applies nosniff, DENY frame-options, restrictive CSP, and Referrer-Policy to all responses.
8. **Active user middleware** — Centralized soft-delete enforcement via `ActiveUserMiddleware.cs` instead of per-handler checks, reducing error surface.
9. **Architecture tests** — Automated verification that dependencies flow inward only, catching violations at build time (`ArchitectureTests.cs`).
10. **Comprehensive frontend accessibility** — All auth forms use `aria-invalid`, `aria-describedby`, and `role="alert"` for screen reader support.
11. **Rate limiting per endpoint sensitivity** — Different rate limits for different auth endpoints (5/min login, 3/min forgot-password, 1/5min resend-verification) rather than a one-size-fits-all approach.
12. **BCrypt with automatic salt** — `BcryptPasswordHasher.cs` uses BCrypt.Net with automatic salt generation and work factor 11 (industry standard).

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | CRITICAL | Cookie Secure flag environment-aware | Make JWT and CSRF cookie `Secure` flag configurable per environment — `true` in production, `false` only in Development/Testing | `AuthController.cs:74,413` | None | Done |
| 2 | P0 | HIGH | Timing attack mitigation in login | Always execute bcrypt verification even when user not found, using a pre-computed dummy hash to prevent timing-based email enumeration | `LoginUserCommandHandler.cs:25` | None | Done |
| 3 | P0 | HIGH | Account lockout after failed attempts | Implement per-user login attempt tracking with exponential lockout (15 min after 5 failures) to prevent distributed brute-force | New: `ILoginAttemptTracker` + Redis impl | None | Not started |
| 4 | P1 | MEDIUM | Invalidate sessions on password change | Add `PasswordVersion` claim to JWT and validate in `ActiveUserMiddleware` so password changes invalidate all existing tokens | `User.cs`, `JwtTokenService.cs`, `ActiveUserMiddleware.cs` | None | Done |
| 5 | P1 | MEDIUM | Max password length validation | Add `MaximumLength(128)` to all password validators to prevent DoS via bcrypt computation on extremely long inputs | `RegisterRequestValidator.cs`, `ResetPasswordRequestValidator.cs`, `ChangePasswordRequestValidator.cs` | None | Done |
| 6 | P1 | MEDIUM | CSRF rejection integration tests | Add integration tests verifying POST requests without CSRF token return 403 for all auth endpoints | `AuthEndpointTests.cs` | None | Done |
| 7 | P1 | MEDIUM | OAuth unverified email integration test | Add integration test for OAuth callback when provider returns unverified email — verify it's properly rejected | `AuthEndpointTests.cs` | None | Done |
| 8 | P2 | LOW | CSRF token TTL | Add timestamp to CSRF tokens and reject tokens older than 1 hour | `AuthController.cs:64-79` | None | Done |
| 9 | P2 | LOW | Security event audit logging | Add structured logging for failed logins, consumed tokens, rejected OAuth attempts, and password change events | Multiple handlers | None | Done |
| 10 | P2 | LOW | Per-user rate limiting | Extend rate limiting beyond IP-only to include per-user/per-email tracking for auth endpoints | `appsettings.json`, new middleware | #3 | Not started |
| 11 | P3 | LOW | Split AuthController | Extract OAuth endpoints into a dedicated `OAuthController` to reduce AuthController size (currently 585 lines) | `AuthController.cs` | None | Done |
| 12 | P3 | LOW | Rate limiting global fallback test | Add integration test verifying the global 100/min rate limit applies to non-auth endpoints | `AuthEndpointTests.cs` | None | Done |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build && npm test
```
