# Code Review: cr-0038 — PR #235 epic-002 audit remediations

## PR Metadata
- PR: #235 (`fix/audit-0006-session-invalidation`)
- Target branch: `main`
- CI Status: All checks pass (Detect Changes: SUCCESS, Orgs API Build & Test: SUCCESS, AI API: SKIPPED, Web: SKIPPED)

## Changed Files
- docs/audits/epic-002-executive-summary.md
- docs/tasks/audit-0006-session-invalidation.md, audit-0007-*.md, audit-0008-*.md
- docs/walkthroughs/audit-0006-* through audit-0012-*
- services/orgs-api/src/.../Api/Controllers/AuthController.cs
- services/orgs-api/src/.../Api/Controllers/OAuthController.cs (new)
- services/orgs-api/src/.../Api/Middleware/ActiveUserMiddleware.cs
- services/orgs-api/src/.../Application/Auth/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs
- services/orgs-api/src/.../Application/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs
- services/orgs-api/src/.../Application/Users/Commands/ChangePassword/ChangePasswordCommandHandler.cs
- services/orgs-api/src/.../Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs
- services/orgs-api/src/.../Application/Users/Interfaces/IUserRepository.cs
- services/orgs-api/src/.../Domain/Users/User.cs
- services/orgs-api/src/.../Infrastructure/Migrations/20260209195818_AddPasswordVersionToUsers*
- services/orgs-api/src/.../Infrastructure/Persistence/Configurations/UserConfiguration.cs
- services/orgs-api/src/.../Infrastructure/Persistence/Repositories/UserRepository.cs
- services/orgs-api/src/.../Infrastructure/Services/JwtTokenService.cs
- services/orgs-api/tests/.../IntegrationTests/Api/AuthEndpointTests.cs
- services/orgs-api/tests/.../UnitTests/Api/Middleware/ActiveUserMiddlewareTests.cs
- services/orgs-api/tests/.../UnitTests/Application/Auth/Commands/*Tests.cs
- services/orgs-api/tests/.../UnitTests/Application/Users/Commands/*Tests.cs

## Review Comments (5 inline, 2 reviews, 2 issue comments)

### Inline Comments

| # | Source | File | Classification | Summary |
|---|--------|------|---------------|---------|
| 1 | gemini-code-assist | AuthController.cs:350 | SHOULD_FIX | CSRF token not signed/HMACed; future timestamp bypass; hardcoded TTL |
| 2 | gemini-code-assist | OAuthController.cs:137 | SHOULD_FIX | SetAccessTokenCookie duplicated between AuthController and OAuthController |
| 3 | Copilot | AuthController.cs:350 | SHOULD_FIX | CSRF TTL bypassed via future timestamp; parse with invariant culture |
| 4 | Copilot | ActiveUserMiddleware.cs:50 | SHOULD_FIX | pwd_ver claim type string literal duplicated vs JwtTokenService constant |
| 5 | Copilot | AuthEndpointTests.cs:1213 | SHOULD_FIX | Unused `email` variable assignment |

### Review Bodies
- gemini-code-assist: "Critical vulnerability in custom CSRF TTL" + hardcoded value + duplicated code
- copilot: Summary of 3 inline comments

### Issue Comments
- chatgpt-codex-connector: Usage limit notice (not actionable)
- gemini-code-assist: PR summary (not actionable)

## Comment Resolution Plan

### SHOULD_FIX
- [x] **#1/#3 — CSRF future timestamp bypass**: Add `tokenAge >= TimeSpan.Zero` check in `ValidateCsrf()` to reject tokens with future timestamps
- [x] **#1 partial — Extract TTL constant**: Extract hardcoded `TimeSpan.FromHours(1)` to a named constant `CsrfTokenMaxAge`
- [x] **#2 — SetAccessTokenCookie duplication**: Extract to shared extension method on `HttpResponse` in `Api/Extensions/HttpResponseCookieExtensions.cs`
- [x] **#4 — pwd_ver duplication**: Create `AuthClaimTypes` constant class in Application layer, reference from both JwtTokenService and ActiveUserMiddleware
- [x] **#5 — Unused email variable**: Discard the unused `email` variable in test

### OUT_OF_SCOPE
- **#1 partial — HMAC signing of CSRF tokens**: The CSRF mechanism uses a double-submit cookie pattern where security comes from the SameSite cookie attribute preventing cross-origin cookie reads. HMAC signing is defense-in-depth but constitutes a separate architectural change. The TTL is already defense-in-depth on top of the double-submit pattern. Deferring to a future task if needed.

## Parity Verification Checklist
- [x] Redirect parity checked — `next` propagation and sanitization consistent between AuthController and OAuthController
- [x] Locale source correctness checked — AuthController uses explicit payload locale with Accept-Language fallback; OAuthController resolves locale from redirect path
- [x] API/UI contract parity checked — no new frontend-facing fields; PasswordVersion is internal JWT claim
- [x] Test parity checked — session invalidation, rate limit, OAuth split all have integration tests; security logging tested with NullLogger
