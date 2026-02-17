# Walkthrough: ft-0006 Fix Service Audits

## Summary

Addresses findings from the comprehensive service audit across ai-api, orgs-api, and web services. This includes security hardening, performance improvements, clean code fixes, and test coverage gaps.

## Decisions

- Process services in order: ai-api, orgs-api, web (matching workflow specification)
- Within each service, process items in priority order (P0 first)
- Write tests alongside production code changes
- **RBAC policies deferred**: JWT tokens issued by orgs-api `JwtTokenService` do not include a `role` claim. All `RequireRole`/`RequireClaim("role", ...)` policies were reverted to `RequireAuthenticatedUser()` with a comment explaining the deferral. Role-based access will be implemented when `JwtTokenService` is updated to emit role claims.
- **AdminOnly policy deferred**: Same root cause as RBAC — `AdminOnly` policy in orgs-api also uses `RequireAuthenticatedUser()` pending JWT role claims.

## Changes by Service

### ai-api (PR #270)

| # | Action Item | Resolution |
|---|------------|------------|
| 1 | Implement role-based authorization policies | **Deferred** — JWT lacks role claims; kept `RequireAuthenticatedUser()` with explanatory comment |
| 2 | Add JWT secret minimum length validation | **Done** — Already implemented at `DependencyInjection.cs:112-117` |
| 7 | Extract userId extraction helper | **Done** — Created `ClaimsPrincipalExtensions.cs` with `TryGetUserId()` and `TryGetOrgId()` |

Additional: Removed stale `role` claim from `DevAuthenticationHandler.cs`, added `ParseWorkItemRequestValidator.cs` for input validation.

### orgs-api (PR #270)

| # | Action Item | Resolution |
|---|------------|------------|
| 1 | Add admin authorization to AuditLogsController | **Deferred** — JWT lacks role claims |
| 2 | Add admin authorization to FeatureFlags toggle | **Deferred** — JWT lacks role claims |
| 7 | Add authorization bypass tests for admin endpoints | **Deferred** — Blocked by #1, #2 |
| 9 | Enforce minimum JWT secret length | **Done** — Already implemented in `JwtTokenService.cs:175-181` |

Additional: Reverted `AdminOnly` policy to `RequireAuthenticatedUser()`, rewrote `AdminAuthorizationTests.cs` (removed 403-for-non-admin tests), fixed password regex in `RegisterRequestValidator.cs` to exclude whitespace, fixed `CreateInvitationCommandHandler` validation.

### web (PR #270)

| # | Action Item | Resolution |
|---|------------|------------|
| 1 | Fix open redirect via protocol-relative URL | **Done** — Added `!decoded.startsWith('//')` check in LoginForm, RegisterForm, OAuthButtons |
| 2 | Add Content Security Policy headers | **Done** — CSP configured in `next.config.ts` headers with proper env var names |
| 3 | Add billing module tests | **Done** — 4 component tests + hook tests added |
| 4 | Add AcceptInvite component tests | **Done** — Tests in `src/components/orgs/__tests__/AcceptInvite.test.tsx` |
| 5 | Fix hardcoded English in FormSubmitButton | **Done** — Now uses `t('submitting')` i18n key |
| 6 | Wrap WorkItemForm schema in useMemo | **Done** — Schema wrapped in `useMemo` |
| 7 | Fix focus ring classes to match design system | **Done** — Updated to `focus:ring-primary-600` |
| 9 | Add notifications hook tests | **Done** — Tests in `src/hooks/__tests__/notifications.test.ts` |
| 14 | Remove form-demo.tsx from production | **Done** — File already removed from codebase |

Additional: Fixed `global-error.tsx` error boundary, fixed `useMyOrgs` refetch loading flash, fixed E2E `org.helper.ts` race condition.

## Tests Added

### ai-api
- No new test files (validator file added for request validation)

### orgs-api
- Rewrote `AdminAuthorizationTests.cs` (removed stale 403 tests for non-admin, kept 401 unauthenticated tests)
- Updated `FeatureFlagEndpointTests.cs` (Toggle test expects 200 for authenticated users)
- Updated `CreateInvitationCommandHandlerTests.cs`

### web
- `src/hooks/__tests__/billing.test.ts` — billing hooks (plans, subscription, payments, checkout, portal, one-time)
- `src/hooks/__tests__/notifications.test.ts` — notifications hook (loading, success, error, markAsRead, markAllAsRead)
- `src/components/billing/__tests__/BillingManagement.test.tsx`
- `src/components/billing/__tests__/PaymentHistory.test.tsx`
- `src/components/billing/__tests__/PlanCard.test.tsx`
- `src/components/billing/__tests__/PricingContent.test.tsx`
- `src/components/orgs/__tests__/AcceptInvite.test.tsx`

## Verification Results

### ai-api
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — PASS
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 169 tests passed

### orgs-api
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — PASS
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 574 tests passed

### web
- `cd apps/web && npm run build` — PASS
- `cd apps/web && npm test` — 431 tests passed
- E2E Smoke Tests — PASS (5/5 CI checks green)

## Follow-ups

Items still pending (see audit reports for full details):
- ai-api: 15 remaining items (P1: auth logging, composite index, read repo tests; P2: projector tests, OpenAi exception, LIKE escaping, GIN index; P3: various unit tests, caching, docs)
- orgs-api: 16 remaining items (P1: CSRF HMAC, ILike escape, Stripe webhook, export limit, OAuth tests; P2: PII logging, N+1 fix, outbox index, CSRF tests, domain tests, health check; P3: various)
- web: 13 remaining items (P2: console.error, roles tests, dialog tests, pagination tests, type assertions; P3: font, dynamic imports, decompose, i18n, auth-events tests, next/image, error.body)
