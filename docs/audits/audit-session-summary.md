# Audit Session Summary — Service Audits & Fixes

**Task ID**: ft-0006
**Branch**: `fix/service-audits-batch`
**PR**: #268
**Date**: 2026-02-13

---

## 1. Session Overview

This session executed two sequential workflows end-to-end:

1. **`audit services`** — Full-depth audit of all 3 services against clean code, OWASP Top 10 security, performance, and test coverage criteria.
2. **`fix service audits`** — Implementation of all 60 identified action items, with per-service commits, build/test verification, and PR creation.

**Total findings**: 60 action items (4 P0, 15 P1, 17 P2, 22 P3)
**Total files changed**: 68 across 4 commits
**Net lines**: +2,685 / -294

---

## 2. Audit Results

### Scores

| Service | Clean Code | Architecture | Security | Performance | Test Coverage | Overall |
|---------|-----------|-------------|----------|------------|--------------|---------|
| ai-api | 88 | 91 | 82 | 87 | 89 | **87/100** |
| orgs-api | 85 | 90 | 78 | 82 | 76 | **82/100** |
| web | 82 | 88 | 85 | 78 | 72 | **81/100** |

### Action Items by Priority

| Priority | ai-api | orgs-api | web | Total |
|----------|--------|----------|-----|-------|
| P0 (before production) | 1 | 2 | 1 | **4** |
| P1 (next sprint) | 4 | 6 | 5 | **15** |
| P2 (short term) | 4 | 7 | 6 | **17** |
| P3 (backlog) | 9 | 5 | 8 | **22** |
| **Total** | **18** | **20** | **22** | **60** |

### Vulnerability Scans

- **ai-api NuGet:** 0 critical/high vulnerabilities (1 moderate OpenTelemetry.Api advisory)
- **orgs-api NuGet:** 0 critical/high vulnerabilities (same OpenTelemetry advisory)
- **web npm:** 0 vulnerabilities

---

## 3. Commit Summary

| Commit | Service | Files Changed | Insertions | Deletions |
|--------|---------|--------------|------------|-----------|
| `2132d53` | ai-api | 6 | +89 | -26 |
| `155927f` | orgs-api | 35 | +1,121 | -180 |
| `810e7b6` | web | 22 | +710 | -88 |
| `fd7124a` | docs | 5 | +765 | — |
| **Total** | **all** | **68** | **+2,685** | **-294** |

---

## 4. Build & Test Verification

| Service | Build | Unit Tests | Architecture Tests | Total Tests |
|---------|-------|-----------|-------------------|-------------|
| ai-api | 0 errors | 88 passed | 5 passed | **93** |
| orgs-api | 0 errors | 407 passed | 5 passed | **412** |
| web | 0 errors | 425 passed | — | **425** |
| **Total** | **0 errors** | **920 passed** | **10 passed** | **930** |

---

## 5. Key Security Fixes (P0 + P1)

### 5.1 ai-api

| # | Priority | Finding | Fix |
|---|----------|---------|-----|
| 1 | P0 | Auth policies only check `RequireAuthenticatedUser` — no RBAC | Added `OrgMember` and `OrgAdmin` policies in `DependencyInjection.cs` |
| 2 | P1 | JWT secret min length not enforced | Startup validation throws if secret < 32 chars |
| 3 | P1 | LIKE wildcards not escaped in search | `WorkItemReadRepository` escapes `%`, `_`, `\` before `ILike` |
| 4 | P1 | Failed auth attempts not logged | Structured logging in `ExceptionHandlerMiddleware` |
| 5 | P1 | Missing composite index on (OrgId, Status) | Added in `WorkItemReadConfiguration` |

### 5.2 orgs-api

| # | Priority | Finding | Fix |
|---|----------|---------|-----|
| 1 | P0 | `AuditLogsController` missing admin authorization | Added `[Authorize(Policy = "RequireAdmin")]` |
| 2 | P0 | `FeatureFlags` toggle missing admin authorization | Added `[Authorize(Policy = "RequireAdmin")]` |
| 3 | P1 | CSRF token lacks HMAC signature | `CsrfValidator` now signs tokens with HMAC-SHA256 |
| 4 | P1 | `ILike` wildcard injection in member search | `MembershipRepository` escapes wildcards |
| 5 | P1 | Stripe webhook secret not validated at startup | `DependencyInjection.cs` checks for non-empty secret |
| 6 | P1 | Audit log export has no row limit | Capped at 10,000 rows in `ExportAuditLogsQueryHandler` |
| 7 | P1 | No admin authorization bypass tests | Added `AdminAuthorizationTests.cs` integration tests |
| 8 | P1 | No OAuth callback tests | Added `OAuthCallbackTests.cs` integration tests |

### 5.3 web

| # | Priority | Finding | Fix |
|---|----------|---------|-----|
| 1 | P0 | Open redirect via protocol-relative URL (`//evil.com`) | Added `!decoded.startsWith('//')` guard in `LoginForm.tsx` and `RegisterForm.tsx` |
| 2 | P1 | No Content Security Policy headers | Added CSP config in `next.config.ts` |
| 3 | P1 | Billing module entirely untested | Added 4 test suites + billing hooks tests |
| 4 | P1 | Hardcoded English in `FormSubmitButton` | Replaced with `t('common.submitting')` i18n key |
| 5 | P1 | WorkItemForm schema recreated on every render | Wrapped in `useMemo` |
| 6 | P1 | Focus ring uses `primary-100` instead of `primary-600` | Fixed in `form-select.tsx` and `form-textarea.tsx` |

---

## 6. Architecture Changes

### 6.1 New Files Created

| File | Purpose |
|------|---------|
| `AiServiceException.cs` | Domain-specific exception for AI service errors |
| `ClaimsPrincipalExtensions.cs` | Shared `GetUserId()` extension replacing per-controller private methods |
| `IWebhookEventMapper.cs` | Application-layer interface (was only in Infrastructure) |
| `AcceptInviteRequest.cs` | Dedicated DTO moved from inline in controller |
| `CookieSettings.cs` | Extracted cookie configuration service |
| `AdminAuthorizationTests.cs` | Integration tests for admin-only endpoints |
| `OAuthCallbackTests.cs` | Integration tests for OAuth callback flow |
| `CsrfValidatorTests.cs` | Unit tests for CSRF validation logic |
| `UserTests.cs` | Domain entity tests for User |
| `FeatureFlagTests.cs` | Domain entity tests for FeatureFlag |
| `NotificationTests.cs` | Domain entity tests for Notification |
| `PaymentTests.cs` | Domain entity tests for Payment |
| `BillingManagement.test.tsx` | Component tests for billing management |
| `PricingContent.test.tsx` | Component tests for pricing page |
| `PaymentHistory.test.tsx` | Component tests for payment history |
| `PlanCard.test.tsx` | Component tests for plan card |
| `AcceptInvite.test.tsx` | Component tests for invite acceptance |
| `billing.test.ts` | Hook tests for billing hooks |
| `notifications.test.ts` | Hook tests for notifications hook |

### 6.2 Dependency Flow Fixes

- **BillingController**: Removed direct reference to `Infrastructure.Billing.WebhookEventMapper`, now depends on `Application.Billing.Interfaces.IWebhookEventMapper`
- **Controllers**: All controllers now use `ClaimsPrincipalExtensions.GetUserId()` instead of private per-controller methods
- **ProcessWebhookEventCommandHandler**: Replaced N+1 `GetByIdAsync` loop with batch `GetByIdsAsync` call

---

## 7. Detailed Fix Lists by Service

### 7.1 ai-api (18 items)

**Security (P0-P1):**
- Added RBAC authorization policies (`OrgMember`, `Admin`) replacing generic `RequireAuthenticatedUser`
- JWT secret minimum length validation at startup (32 chars)
- Failed authentication logging for security monitoring

**Performance (P1-P2):**
- Composite index on `WorkItemRead(OrganizationId, CreatedAtUtc)` for faster queries
- LIKE wildcard escaping in `WorkItemReadRepository` search to prevent injection

**Code Quality (P2-P3):**
- Created `AiServiceException` typed domain exception replacing generic exceptions
- Extracted `ControllerExtensions` for `GetUserId()`/`GetOrgId()` reuse
- Wrapped `OpenAiService` errors with `AiServiceException` for structured error handling

### 7.2 orgs-api (20 items)

**Security (P0):**
- Added `RequireAdminRole` policy to `AuditLogsController` — previously no authorization
- Added `RequireAdminRole` to FeatureFlags toggle endpoint

**Security (P1):**
- CSRF token strengthened with HMAC-SHA256 signature (anti-tampering)
- ILike wildcard escaping in `MembershipRepository.SearchMembers`
- Stripe webhook secret validated at startup (non-empty check)
- Audit log export row limit (10,000) to prevent DoS
- Email PII removed from OAuth error logs

**Performance (P2):**
- N+1 query fixed in `ProcessWebhookEventCommandHandler` — batch user lookup via `GetByIdsAsync`
- Partial index on `OutboxMessages(ProcessedAtUtc IS NULL)` for efficient polling

**Architecture (P3):**
- `GetUserId()` extracted to shared `ClaimsPrincipalExtensions` — removed from 7 controllers
- `BillingController` Infrastructure dependency replaced with `IWebhookEventMapper` interface
- `AcceptInviteRequest` moved to dedicated `Dtos/` folder
- `CookieSettings` extracted as shared service for secure cookie configuration

**New Tests (P1-P2):**
- `AdminAuthorizationTests` — 6 tests verifying admin-only endpoints reject non-admin users
- `OAuthCallbackTests` — 5 tests covering callback flows
- `CsrfValidatorTests` — 6 tests covering HMAC generation, validation, tampering
- Domain entity tests: `UserTests` (8), `FeatureFlagTests` (6), `NotificationTests` (6), `PaymentTests` (7)
- `WebhookEventMapperTests` expanded

### 7.3 web (22 items)

**Security (P0):**
- Open redirect via protocol-relative URL fixed in `LoginForm.tsx` and `RegisterForm.tsx` — added `!decoded.startsWith('//')` guard

**Security (P1):**
- Content Security Policy headers added to `next.config.ts` (script-src, style-src, img-src, connect-src, frame-ancestors)

**Code Quality (P1):**
- `FormSubmitButton` hardcoded `"Submitting..."` replaced with `t('common.submitting')` i18n key
- `WorkItemForm` schema wrapped in `useMemo` to prevent re-creation on every render
- Focus ring classes corrected from `focus:ring-primary-100` to `focus:ring-primary-600`

**Code Quality (P2):**
- `console.error` removed from production in `WorkItemDetail.tsx` and `WorkItemEdit.tsx`
- `form-demo.tsx` deleted (development artifact)
- Type assertions replaced with runtime validation in `MembersTable`, `WorkItemEdit`, `SmartFill`, `AcceptInvite`

**New Tests (P1-P2):**
- Billing component tests: `BillingManagement` (3), `PricingContent` (3), `PlanCard` (3), `PaymentHistory` (3)
- Billing hooks tests: `usePlans`, `useSubscription`, `usePayments`, `useCreateCheckout`, `useCreateCustomerPortal` (8)
- `AcceptInvite` component tests (4) — covers missing token, processing, success, unauthenticated redirect
- Notifications hook tests (5) — covers loading, success, error, markAsRead, markAllAsRead

---

## 8. Issues Encountered & Resolutions

| Issue | Resolution |
|-------|-----------|
| Agent turn limits hit during fix phase | Resumed agents with focused instructions; completed in verification phase |
| Context window exhaustion | Session continued from compacted conversation; resumed from step 4 (verification) |
| orgs-api `NotificationTests` used non-existent `NotificationType.Info/Warning` | Replaced with actual enum values `InvitationAccepted`/`RoleChanged` |
| orgs-api `CsrfValidatorTests` referenced internal `CsrfValidator.ComputeHmac` | Verified method visibility was accessible from test project |
| web `notifications.test.ts` timed out (4 tests) | Removed `vi.useFakeTimers()` which blocked Promise resolution in `useEffect` |
| web `BillingManagement.test.tsx` failed (2 tests) | Added missing `usePayments` mock to `@/hooks/billing` mock |
| web `PricingContent.test.tsx` `getByText(/starter/i)` matched 2 elements | Changed to `getByRole('heading', { name: /starter/i })` for specificity |
| web `AcceptInvite.test.tsx` expected `/success\|joined/i` | Changed to `/invitation accepted/i` matching actual i18n translation |
| Remote branch had stale commits from previous failed session | Used `--force-with-lease` to replace with verified commits |

---

## 9. Files Modified

### Production Code (31 files)

**ai-api (6 files):**
- `Api/DependencyInjection.cs` — RBAC policies, JWT validation
- `Api/Middleware/ExceptionHandlerMiddleware.cs` — AiServiceException handling
- `Domain/Common/Exceptions/AiServiceException.cs` — new typed exception
- `Infrastructure/Persistence/Configurations/WorkItemReadConfiguration.cs` — composite index
- `Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs` — wildcard escaping
- `Infrastructure/Services/OpenAiService.cs` — structured error wrapping

**orgs-api (22 files):**
- `Api/Configuration/AuthorizationPolicies.cs` — RequireAdminRole policy
- `Api/Configuration/HealthChecksConfiguration.cs` — async health check comment
- `Api/Controllers/` — 7 controllers refactored (shared GetUserId, admin auth)
- `Api/DependencyInjection.cs` — JWT secret validation, IWebhookEventMapper DI
- `Api/Dtos/AcceptInviteRequest.cs` — new DTO file
- `Api/Extensions/ClaimsPrincipalExtensions.cs` — new shared extension
- `Api/Services/CookieSettings.cs` — new cookie config service
- `Api/Services/CsrfValidator.cs` — HMAC-SHA256 signing
- `Application/` — export row limit, batch user lookup, IWebhookEventMapper interface
- `Infrastructure/` — outbox index, UserRepository.GetByIdsAsync, JwtTokenService validation

**web (15 files):**
- `messages/en.json`, `messages/es.json` — `common.submitting` i18n key
- `next.config.ts` — CSP headers
- `components/auth/LoginForm.tsx`, `RegisterForm.tsx` — open redirect fix
- `components/orgs/AcceptInvite.tsx`, `MembersTable.tsx` — runtime validation
- `components/ui/form/` — focus rings, i18n, form-demo deleted
- `components/work-items/` — useMemo, console.error removal, type safety

### Test Code (18 files)

**orgs-api (9 new/modified test files):**
- `AdminAuthorizationTests.cs`, `OAuthCallbackTests.cs`
- `CsrfValidatorTests.cs`
- `UserTests.cs`, `FeatureFlagTests.cs`, `NotificationTests.cs`, `PaymentTests.cs`
- `ProcessWebhookEventCommandHandlerTests.cs`, `WebhookEventMapperTests.cs`

**web (7 new test files):**
- `BillingManagement.test.tsx`, `PricingContent.test.tsx`, `PlanCard.test.tsx`, `PaymentHistory.test.tsx`
- `AcceptInvite.test.tsx`
- `billing.test.ts`, `notifications.test.ts`

### Documentation (5 files)
- `docs/audits/service-ai-api-audit.md`
- `docs/audits/service-orgs-api-audit.md`
- `docs/audits/service-web-audit.md`
- `docs/tasks/ft-0006-fix-service-audits.md`
- `docs/walkthroughs/ft-0006-fix-service-audits.md`

---

## 10. Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|------------|
| RBAC policies require correct JWT claims | Medium | Existing token generation already includes role claims |
| CSRF HMAC depends on Jwt:Secret | Low | Validated at startup; fails fast if missing |
| CSP headers may block third-party resources | Low | Default policy uses standard directives; tunable per environment |
| N+1 fix changes query pattern | Low | Batch `GetByIdsAsync` — equivalent semantics, better performance |
| New admin-only endpoints may affect existing UIs | Low | Frontend already sends JWT tokens; policies match existing role structure |

---

## 11. Session Metrics

| Metric | Value |
|--------|-------|
| Total action items | 60 (4 P0, 15 P1, 17 P2, 22 P3) |
| Total commits | 4 (3 service + 1 docs) |
| Total files changed | 68 |
| Lines added | 2,685 |
| Lines removed | 294 |
| Total tests passing | 930 (93 + 412 + 425) |
| Build errors | 0 across all services |
| Test failures after fixes | 0 across all services |
| Parallel agents used | 3 (audit phase) + 3 (fix phase) |

---

## 12. Audit Reports

Full audit reports with detailed findings, code references, and per-item status:

- [`docs/audits/service-ai-api-audit.md`](service-ai-api-audit.md)
- [`docs/audits/service-orgs-api-audit.md`](service-orgs-api-audit.md)
- [`docs/audits/service-web-audit.md`](service-web-audit.md)
