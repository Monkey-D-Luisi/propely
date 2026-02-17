# Service Audit: orgs-api

## Audit Metadata
- **Service:** `services/orgs-api/`
- **Date:** 2026-02-13
- **Auditor:** Agent
- **Status:** In Progress
- **Technology:** .NET 10 Clean Architecture + CQRS (MediatR)
- **Source files analyzed:** 290
- **Test files analyzed:** 76

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Clean Code | 85/100 | Good SOLID adherence, minor DRY violations, one arch boundary violation |
| Architecture | 90/100 | Excellent Clean Architecture with automated enforcement tests |
| Security | 78/100 | Strong foundations but two HIGH access control gaps |
| Performance | 82/100 | Well-optimized, one N+1 pattern, minor index gap |
| Test Coverage | 76/100 | Solid handler coverage, gaps in OAuth and admin endpoint testing |
| **Overall** | **82/100** | **Well-architected service with critical admin auth gaps to address** |

---

## Security Findings

### CRITICAL

*None identified.*

### HIGH

#### F1. Audit Logs Controller Missing Admin Role Authorization
- **Severity:** HIGH
- **OWASP:** A01 Broken Access Control
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuditLogsController.cs:16-17`
- **Problem:** The `AuditLogsController` is at route `admin/audit-logs` and requires `[Authorize]`, but has no admin role check or policy. Any authenticated user can query and export the entire audit log.
- **Impact:** Any authenticated user can access full audit logs including actions from all users across all organizations, entity changes, and correlation IDs. Significant information disclosure and privilege escalation.
- **Recommendation:** Add `[Authorize(Policy = "AdminOnly")]` or equivalent role-based policy.

#### F2. Feature Flags Controller Missing Admin Authorization
- **Severity:** HIGH
- **OWASP:** A01 Broken Access Control
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/FeatureFlagsController.cs:52-58`
- **Problem:** The `PUT /feature-flags/{name}` toggle endpoint only requires `[Authorize]`. Any authenticated user can enable/disable feature flags.
- **Impact:** A regular user could toggle `MaintenanceMode` to `true` (DoS), enable `BetaFeatures`, or modify other system flags.
- **Recommendation:** Add admin-only authorization policy to the toggle endpoint. Read endpoints (GET) may remain accessible.

### MEDIUM

#### F3. CSRF Token Lacks Cryptographic Binding
- **Severity:** MEDIUM
- **OWASP:** A01 Broken Access Control
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Services/CsrfValidator.cs:9-38`
- **Problem:** CSRF validation only checks format and age (timestamp + random hex). The token has no server-side binding -- any client that can craft a hex timestamp + random string can pass validation.
- **Impact:** If CORS is misconfigured or bypassed, an attacker could forge valid CSRF tokens.
- **Recommendation:** Sign the token with HMAC using a server-side secret, or reinstate double-submit cookie check.

#### F4. ILike Wildcard Injection in Member Search
- **Severity:** MEDIUM
- **OWASP:** A03 Injection
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs:102-105`
- **Problem:** Search parameter interpolated into `LIKE` pattern as `$"%{search}%"` without escaping PostgreSQL wildcard metacharacters (`%`, `_`).
- **Impact:** Users can craft search strings like `%` to match all members or `_` for single-character wildcards.
- **Recommendation:** Escape `%`, `_`, and `\` in search input before wrapping with wildcards.

#### F5. Stripe Webhook Secret Could Be Empty String
- **Severity:** MEDIUM
- **OWASP:** A02 Cryptographic Failures
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs:52`, `Application/Billing/BillingConfiguration.cs:18`
- **Problem:** `_webhookSecret` defaults to `string.Empty`. If not configured, `EventUtility.ConstructEvent` may behave unpredictably.
- **Impact:** Forged webhook events could be accepted if signature verification fails open with empty secret.
- **Recommendation:** Validate at startup that `Stripe.WebhookSecret` is non-empty when billing mode is not "free".

#### F6. Export Audit Logs Has No Pagination Limit
- **Severity:** MEDIUM
- **OWASP:** A04 Insecure Design
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Application/AuditLogs/Queries/ExportAuditLogs/ExportAuditLogsQueryHandler.cs:16-39`
- **Problem:** `ExportAuditLogsQuery` calls `GetAllFilteredAsync` without any row limit.
- **Impact:** Could export millions of rows, causing OOM or database strain.
- **Recommendation:** Add a hard maximum (e.g., 10,000 rows) or implement streaming/pagination.

### LOW

#### F7. JWT Minimum Key Length Not Enforced
- **Severity:** LOW
- **OWASP:** A02 Cryptographic Failures
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/JwtTokenService.cs:175-181`
- **Problem:** `BuildSigningKey()` accepts any secret length without minimum validation.
- **Recommendation:** Validate `Jwt:Secret` is at least 32 bytes at startup.

#### F8. OAuth Controller Logs Email in Warning Messages
- **Severity:** LOW
- **OWASP:** A09 Logging Failures
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/OAuthController.cs:104-107,122`
- **Problem:** PII (email addresses) logged in warning/error messages.
- **Recommendation:** Log masked/hashed email or omit, relying on correlation IDs.

#### F9. FromSqlRaw with Interpolated Column Names
- **Severity:** LOW
- **OWASP:** A03 Injection
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/MembershipRepository.cs:56-58`
- **Problem:** Uses `FromSqlRaw` with column names from EF metadata interpolated into SQL. Currently safe (from trusted metadata), but fragile pattern.
- **Recommendation:** Add documentation comment noting names are from trusted EF metadata.

#### F10. BillingController Imports Infrastructure Layer Directly
- **Severity:** LOW
- **OWASP:** N/A (Architecture)
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs:12`
- **Problem:** Controller imports `SaasTemplate.OrgsApi.Infrastructure.Billing` for `WebhookEventMapper`. Clean Architecture boundary violation.
- **Recommendation:** Move `WebhookEventMapper` to Application layer or create an interface.

---

## Clean Code

### Architecture Compliance

**Score:** 90/100

| Layer | References | Compliance |
|---|---|---|
| Domain (`Domain.csproj`) | No project references, no NuGet packages | PASS -- zero framework dependencies |
| Application (`Application.csproj`) | Domain only; MediatR, FluentValidation abstractions | PASS |
| Infrastructure (`Infrastructure.csproj`) | Application only | PASS |
| Api (`Api.csproj`) | Infrastructure only | PASS (correct: Api -> Infra -> App -> Domain) |

**One violation**: `BillingController.cs:12` imports `Infrastructure.Billing` directly (F10).

Architecture tests exist at `tests/SaasTemplate.OrgsApi.ArchitectureTests/ArchitectureTests.cs` (5 tests).

### Strengths
- Consistent CQRS pattern with Command/Query + Handler pairs across all features
- Thin controllers delegating everything to MediatR
- Proper exception mapping in `ExceptionHandlerMiddleware.cs`
- Sensitive property filtering excludes `PasswordHash` and `Token` from audit logs (`AppDbContext.cs:31-35`)
- Sealed classes throughout (entities, handlers, controllers, middleware)
- Active user middleware validates user not soft-deleted and password version matches (`ActiveUserMiddleware.cs`)

### Issues
- **DRY:** `GetUserId()` duplicated in 5 controllers: `AuthController.cs:324`, `OrgsController.cs:218`, `BillingController.cs:215`, `NotificationsController.cs:71`, `AuditLogsController.cs:106`
- **DRY:** `_useSecureCookies` computation duplicated in `AuthController.cs:59` and `OAuthController.cs:31`
- `AcceptInviteRequest` record defined at bottom of `OrgsController.cs:232` instead of in `Dtos` folder

### Dead Code / Duplication
- `OutboxRepository.UpdateAsync` wraps synchronous operation in `Task.CompletedTask` (functional but could use `ValueTask`)
- No significant dead code detected

---

## Performance

### Database / Data Access
- **N+1 queries:** Found in `ProcessWebhookEventCommandHandler.cs:284-292` -- `SendPaymentFailureNotification` loads memberships, then calls `_userRepository.GetByIdAsync` inside a `foreach` loop for each owner.
- **Missing indexes:** Outbox table has index on `occurred_at_utc` but `GetUnprocessedMessagesAsync` queries `WHERE processed_at_utc IS NULL ORDER BY occurred_at_utc`. A partial index `WHERE processed_at_utc IS NULL` would improve polling.
- **Connection pooling:** Properly configured. EF Core scoped lifetime, Redis singleton with semaphore-guarded lazy init.
- **Query optimization:** No other issues detected.

### Caching
- Redis caching with configurable TTL and key prefix. Graceful fallback on connection failure.
- Cache invalidation not explicitly shown for org/membership changes (potential stale reads).

### Async Patterns
- One blocking call: `HealthChecksConfiguration.cs:81` uses `.GetAwaiter().GetResult()` for RabbitMQ health check factory (constrained by library API).
- All other I/O operations properly async.

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (Domain) | 3 files | Missing tests for User, FeatureFlag, Notification, Payment entities |
| Unit (Application) | 28 handler files | Missing `GetOrganizationQueryHandler` test |
| Integration (Infrastructure) | 8 files | Missing `MembershipRepository` search/locking tests |
| Integration (Api) | 8 files | Missing OAuth, FeatureFlags, AuditLogs endpoint tests |
| Architecture | 1 file (5 tests) | None |

### Missing Tests
1. **HIGH:** `OAuthController` -- no unit or integration tests for OAuth callback flow
2. **HIGH:** `AuditLogsController` -- no endpoint tests (especially authorization gap testing)
3. **HIGH:** No tests for authorization bypass scenarios (regular user accessing admin endpoints)
4. **MEDIUM:** `GetOrganizationQueryHandler` -- no dedicated test file
5. **MEDIUM:** `FeatureFlagsController` -- no controller-level endpoint tests
6. **MEDIUM:** `CsrfValidator` -- no unit tests for token validation logic
7. **MEDIUM:** Domain entity tests for `User`, `FeatureFlag`, `Notification`, `Payment`
8. **LOW:** `MembershipRepository.GetByOrgIdForUpdateAsync` (FOR UPDATE locking)
9. **LOW:** `RabbitMqPublisher` -- no unit tests

---

## What's Done Well

1. **Clean Architecture with automated enforcement.** 5 architecture tests verify layer boundaries, handler naming conventions, and sealed domain events. (`ArchitectureTests.cs`)
2. **Active user session validation.** Checks user exists, not soft-deleted, and password version matches JWT on every request. (`ActiveUserMiddleware.cs`)
3. **Stripe webhook signature verification.** Properly uses `EventUtility.ConstructEvent` to verify Stripe-Signature header. (`BillingController.cs:196-204`)
4. **Webhook idempotency.** Checks `IsProcessedAsync` before processing with graceful duplicate key handling. (`ProcessWebhookEventCommandHandler.cs:47-51, 86-94`)
5. **Outbox with `FOR UPDATE SKIP LOCKED`.** PostgreSQL advisory locking prevents duplicate event publishing in scaled deployments. (`OutboxRepository.cs:31-39`)
6. **PII exclusion from audit logs.** `SensitiveProperties` set excludes `PasswordHash` and `Token`. (`AppDbContext.cs:31-35`)
7. **OAuth open redirect prevention.** `SanitizeNext` rejects absolute URLs, double-slash paths, and newline injection. (`OAuthController.cs:181-207`)
8. **Rate limiting on sensitive endpoints.** Per-endpoint limits: login (5/min), forgot-password (3/min), register (10/min), resend-verification (1/5min). (`appsettings.json:34-64`)
9. **Password reset token bound to password hash.** Includes `pwd_fgp` claim, preventing token reuse after password change. (`JwtTokenService.cs:76-86`, `ResetPasswordCommandHandler.cs:51`)
10. **HttpOnly + SameSite cookies.** Access token cookie set with `HttpOnly = true`, `SameSite = Lax`, `Secure` based on environment. (`HttpResponseCookieExtensions.cs:9-16`)
11. **Pagination clamping.** `Math.Clamp(request.PageSize, 1, 100)` prevents oversized page requests. (Multiple query handlers)
12. **Error stripping in production.** Internal error details replaced with generic message for 500 errors in non-dev environments. (`ExceptionHandlerMiddleware.cs:76-79`)

---

## Prioritized Action Plan

> This table is consumed by the `fix service audits` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Category | Title | Description | Files | Dependencies | Status |
|---|----------|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | HIGH | Security | Add admin authorization to AuditLogsController | Add `[Authorize(Policy = "AdminOnly")]` or role-based policy. Any authenticated user can currently access all audit logs. | `Api/Controllers/AuditLogsController.cs:16-17` | Requires admin policy definition | Deferred — JWT issued by orgs-api does not include `role` claim; revisit when JwtTokenService emits roles |
| 2 | P0 | HIGH | Security | Add admin authorization to FeatureFlags toggle | Add admin-only policy to `PUT /feature-flags/{name}`. Regular users can currently toggle system flags. | `Api/Controllers/FeatureFlagsController.cs:52-58` | Requires admin policy definition | Deferred — JWT issued by orgs-api does not include `role` claim; revisit when JwtTokenService emits roles |
| 3 | P1 | MEDIUM | Security | Strengthen CSRF token with HMAC signature | Sign CSRF tokens with server-side secret so they cannot be forged | `Api/Services/CsrfValidator.cs:9-38` | None | Not started |
| 4 | P1 | MEDIUM | Security | Escape ILike wildcards in member search | Escape `%`, `_`, `\` in search input before `ILike` | `Infrastructure/Persistence/Repositories/MembershipRepository.cs:102` | None | Not started |
| 5 | P1 | MEDIUM | Security | Validate Stripe webhook secret at startup | When billing mode is not "free", validate `Stripe.WebhookSecret` is non-empty | `Api/Controllers/BillingController.cs:52` | None | Not started |
| 6 | P1 | MEDIUM | Security | Add row limit to audit log export | Hard max (10,000 rows) in `ExportAuditLogsQueryHandler` to prevent resource exhaustion | `Application/AuditLogs/Queries/ExportAuditLogs/ExportAuditLogsQueryHandler.cs:16-39` | None | Not started |
| 7 | P1 | HIGH | Test Coverage | Add authorization bypass tests for admin endpoints | Integration tests verifying non-admin users get 403 on admin endpoints | New test files | Depends on #1, #2 | Deferred — blocked by #1 and #2 (JWT role claims not yet implemented) |
| 8 | P1 | MEDIUM | Test Coverage | Add OAuth callback integration tests | Test successful login, provider mismatch, missing email, conflict scenarios | New test file | None | Not started |
| 9 | P2 | LOW | Security | Enforce minimum JWT secret length | Validate `Jwt:Secret` >= 32 bytes at startup | `Infrastructure/Services/JwtTokenService.cs:175-181` | None | Done (already implemented in JwtTokenService) |
| 10 | P2 | LOW | Security | Remove email PII from OAuth error logs | Replace raw email with masked version in log statements | `Api/Controllers/OAuthController.cs:104-107,122` | None | Not started |
| 11 | P2 | MEDIUM | Performance | Fix N+1 query in SendPaymentFailureNotification | Replace per-owner `GetByIdAsync` loop with batch `GetByIdsAsync` | `Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs:284-292` | Add `GetByIdsAsync` to `IUserRepository` | Not started |
| 12 | P2 | LOW | Performance | Add partial index for outbox polling | Add PostgreSQL partial index on `outbox_messages (occurred_at_utc) WHERE processed_at_utc IS NULL` | `Infrastructure/Persistence/Configurations/OutboxMessageConfiguration.cs` | EF migration | Not started |
| 13 | P2 | MEDIUM | Test Coverage | Add CsrfValidator unit tests | Test valid/expired/malformed tokens, missing header, future timestamp | New test file | None | Not started |
| 14 | P2 | MEDIUM | Test Coverage | Add domain entity tests (User, FeatureFlag, Notification, Payment) | Cover Create factory methods, validation, state transitions | New test files | None | Not started |
| 15 | P2 | LOW | Performance | Replace blocking call in health check setup | Replace `.GetAwaiter().GetResult()` with async pattern | `Api/Configuration/HealthChecksConfiguration.cs:81` | None | Not started |
| 16 | P3 | LOW | Clean Code | Extract GetUserId() to shared extension method | Eliminate 5x duplication across controllers | `Api/Controllers/AuthController.cs:324`, `OrgsController.cs:218`, `BillingController.cs:215`, `NotificationsController.cs:71`, `AuditLogsController.cs:106` | None | Not started |
| 17 | P3 | LOW | Clean Code | Fix BillingController Infrastructure dependency | Move `WebhookEventMapper` to Application layer or create interface | `Api/Controllers/BillingController.cs:12` | None | Not started |
| 18 | P3 | LOW | Clean Code | Move AcceptInviteRequest to Dtos folder | Record defined at bottom of controller instead of Dtos folder | `Api/Controllers/OrgsController.cs:232` | None | Not started |
| 19 | P3 | LOW | Performance | Add dead-letter handling for outbox messages | Add `RetryCount`/`FailedAtUtc` columns to prevent infinite retry of poison messages | `Infrastructure/Messaging/OutboxDispatcherService.cs` | EF migration | Not started |
| 20 | P3 | LOW | Clean Code | Extract _useSecureCookies to shared service | Both AuthController and OAuthController compute same boolean | `Api/Controllers/AuthController.cs:59`, `OAuthController.cs:31` | None | Not started |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```
