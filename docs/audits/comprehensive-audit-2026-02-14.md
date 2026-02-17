# Comprehensive Audit Report -- SaaS Starter Kit

**Date**: 2026-02-14
**Scope**: Full monorepo (~400+ source files)
**Methodology**: Exhaustive static analysis by 7 specialized agents in parallel
**Priority**: User > Security > Architecture > Performance

---

## Executive Summary

The project is a monorepo with three services (Next.js 16 frontend, two .NET 10 microservices) following Clean Architecture + CQRS. The overall quality level is **high** -- zero `any` in TypeScript, zero `@ts-ignore`, architecture tests enforcing layer rules, CSRF with HMAC-SHA256, transactional outbox pattern, and 46+ frontend unit tests + 86+ orgs-api tests + 28+ ai-api tests.

However, there are **3 critical security findings** that must be resolved before selling to clients, along with significant improvements needed in UX, documentation, and operations.

---

## Severity Summary

| Severity | Count | Primary Area |
|----------|-------|-------------|
| CRITICAL | 3 | Security (broken access control) |
| HIGH | 10 | Security + UX + DevOps |
| MEDIUM | 15 | Architecture + UX + Docs |
| LOW | 12 | Performance + DX + cosmetic |

---

## 1. UX/FRONTEND AUDIT

### 1.1 Strengths

- Zero `any` types, zero `@ts-ignore`, zero TODO/FIXME across all TypeScript code
- Complete i18n with `next-intl` (en + es), 740+ message lines, Zod schemas with translated messages via factory functions
- 46 test files covering virtually every component and hook
- 4 E2E specs with Playwright (login, register, create-org, invite-member)
- Consistent design system: semantic tokens in `globals.css`, `primary-*` classes, `bg-surface`
- Custom toast system with context provider and variants (default, success, destructive)
- Real-time notification system with polling, bell icon, unread badge
- Feature flags with `FeatureGate` component and `useFeatureFlag` hook
- Empty states for all lists
- Skeleton loaders for all data views
- DialogOverlay with focus trapping, Escape key, focus restoration
- CSRF protection with token freshness checking
- CSP headers configured with `frame-ancestors 'none'`, `X-Frame-Options: DENY`

### 1.2 Critical and High Findings

| ID | Severity | Finding | File | Line |
|----|----------|---------|------|------|
| UX-1 | HIGH | No mobile navigation/hamburger menu. Admin links hidden with `hidden sm:inline` with no mobile alternative. | `apps/web/src/components/layout/AppHeader.tsx` | 93-115 |
| UX-2 | HIGH | Skip-to-content defined but not rendered. i18n key `common.skipToContent` exists but unused in any layout. Violates WCAG 2.4.1. | `apps/web/src/app/[locale]/layout.tsx` | -- |
| UX-3 | HIGH | StatusBadge shows untranslated English enum values. Renders "Pending", "Active" regardless of locale. | `apps/web/src/components/work-items/StatusBadge.tsx` | -- |
| UX-4 | HIGH | NotificationDropdown lacks keyboard accessibility. No `role="menu"`, no keyboard navigation, no Escape key, no focus trap. | `apps/web/src/components/layout/NotificationDropdown.tsx` | -- |
| UX-5 | HIGH | No AbortController in any data fetching hook. Risk of setting state on unmounted components. | `apps/web/src/hooks/*.ts` | -- |

### 1.3 Medium Findings

| ID | Severity | Finding | File |
|----|----------|---------|------|
| UX-6 | MEDIUM | Missing `loading.tsx` in 8 routes: billing, work-items (list, new, detail, edit), admin/*, profile, pricing | `apps/web/src/app/[locale]/` |
| UX-7 | MEDIUM | Duplicated `formatDate` logic in 4+ components (WorkItemsTable:36, WorkItemDetail:31, PaymentHistory:32, AuditLogTable:112) | Various |
| UX-8 | MEDIUM | Duplicated error display pattern copy-pasted instead of reusable `ErrorMessage` component | Various |
| UX-9 | MEDIUM | WorkItemForm uses raw `register` while all other forms use `FormProvider` + `FormField` | `apps/web/src/components/work-items/WorkItemForm.tsx` |
| UX-10 | MEDIUM | No retry logic despite `ApiError` capturing `retryAfterSeconds` from `Retry-After` header | `apps/web/src/lib/api.ts` |
| UX-11 | MEDIUM | CSRF token fetching duplicated in 4+ components. Should be a `useCsrfToken()` hook. | Various |
| UX-12 | MEDIUM | No SWR/React Query -- all data fetching is manual `useState` + `useEffect` + `useCallback` | `apps/web/src/hooks/*.ts` |
| UX-13 | MEDIUM | No `<main>` landmark in many pages | Various pages |

### 1.4 Low Findings

| ID | Severity | Finding |
|----|----------|---------|
| UX-14 | LOW | Duplicate loading skeleton patterns -- most use raw `div` with `animate-pulse` instead of existing `Skeleton` components |
| UX-15 | LOW | `getStatusColor` duplicated in BillingManagement.tsx:53 and PaymentHistory.tsx:39 |
| UX-16 | LOW | Duplicated error handling branches in AppHeader.tsx:54-67 |
| UX-17 | LOW | `console.error` in production code (hooks/work-items.ts:47,77,139) |

### 1.5 Frontend Test Gaps

- No tests for: `auth-layout.tsx`, `LanguageSwitcher.tsx`, `Pagination.tsx`, `DialogOverlay.tsx`, `RoleBadge.tsx`, `VersionInfo.tsx`, `DeleteAccountSection.tsx`, `TrustedBySection.tsx`
- No test for the `version.ts` hook
- No page-level tests except `orgs/mine/page.test.tsx`
- No E2E for: work items, billing, profile, admin features, password reset

---

## 2. SECURITY AUDIT

### 2.1 Critical Findings (OWASP A01:2021 -- Broken Access Control)

| ID | Severity | Finding | Impact |
|----|----------|---------|--------|
| SEC-1 | CRITICAL | `UpdateMemberRoleCommand` does not include `RequestingUserId`. Handler does not verify identity or role of requester. Any authenticated user can promote anyone to Owner in ANY organization. | Complete privilege escalation |
| SEC-2 | CRITICAL | `CreateInvitationCommandHandler` does not verify requester is a member of the org nor checks their role. Any authenticated user can invite people to ANY organization. | Unauthorized org access |
| SEC-3 | CRITICAL | `AdminOnly` policy only calls `RequireAuthenticatedUser()`. Self-documenting comment admits the bug. Any authenticated user accesses audit logs, feature flags, version endpoint. | Admin data exposure |

**Affected files**:
- `services/orgs-api/src/.../Commands/UpdateMemberRole/UpdateMemberRoleCommandHandler.cs`:33-51
- `services/orgs-api/src/.../Commands/CreateInvitation/CreateInvitationCommandHandler.cs`:39-58
- `services/orgs-api/src/.../Api/Configuration/AuthorizationPolicies.cs`

### 2.2 High Findings

| ID | Severity | Finding | File |
|----|----------|---------|------|
| SEC-4 | HIGH | Hardcoded development secrets in .env.example and docker-compose | `.env.example`, `docker-compose.yml` |
| SEC-5 | HIGH | JWT secret has no rotation mechanism, no minimum key length validation at runtime | `JwtTokenService.cs` |
| SEC-6 | HIGH | Access token cookie expires in 24h with no refresh token mechanism | `HttpResponseCookieExtensions.cs`:12-19 |
| SEC-7 | HIGH | DevAuthenticationHandler bypasses all auth when `Security:AllowAnonymous=true` with no environment guard | `DevAuthenticationHandler.cs` |
| SEC-8 | HIGH | CSP allows `script-src 'unsafe-inline'` significantly weakening XSS protection | `next.config.ts`:4-16 |

### 2.3 Medium Security Findings

| ID | Severity | Finding |
|----|----------|---------|
| SEC-9 | MEDIUM | CSRF token cookie lacks `Secure` flag |
| SEC-10 | MEDIUM | Missing `Strict-Transport-Security` (HSTS) header in frontend |
| SEC-11 | MEDIUM | Invitation token returned in API response (combined with SEC-2 = unauthorized token generation) |
| SEC-12 | MEDIUM | Billing webhook endpoint has no rate limiting |
| SEC-13 | MEDIUM | Open redirect risk in LoginForm (`/\example.com` pattern) |
| SEC-14 | MEDIUM | Swagger potentially exposed in production |

### 2.4 Low Security Findings

| ID | Severity | Finding |
|----|----------|---------|
| SEC-15 | LOW | BCrypt uses default work factor (10) -- OWASP recommends 12+ for 2025+ |
| SEC-16 | LOW | `SameSite=Lax` instead of `Strict` (necessary for OAuth but widens CSRF surface) |
| SEC-17 | LOW | No PII data retention policy -- audit logs stored indefinitely |
| SEC-18 | LOW | Soft-delete does not revoke access tokens -- JWT valid up to 24h after deletion |
| SEC-19 | LOW | No SSL/TLS in PostgreSQL connection strings |
| SEC-20 | LOW | Error messages expose internal IDs |

### 2.5 Positive Security Controls

- CSRF with HMAC-SHA256 and timestamped tokens, timing-safe via `CryptographicOperations.FixedTimeEquals`
- Multi-tenancy with EF Core global query filters + `TenantMismatchException` on writes
- JWT via HttpOnly cookies (not Authorization header)
- IP-based rate limiting with per-endpoint overrides
- Outbox with `FOR UPDATE SKIP LOCKED`
- Pessimistic locking with `FOR UPDATE` for membership mutations
- Password version tracking -- token invalidation on password change
- OAuth redirect sanitization
- Audit logging integrated in DbContext with sensitive property exclusion
- Architecture tests enforcing layer rules

---

## 3. ARCHITECTURE AUDIT

### 3.1 Compliance: EXCELLENT with minor exceptions

- Architecture tests enforce layer dependencies in both services
- Domain layer has zero framework dependencies
- Clean CQRS with separate read/write models in ai-api
- Transactional outbox pattern
- Idempotent event projection via `ProcessedEvents` table

### 3.2 Architecture Findings

| ID | Severity | Finding | File |
|----|----------|---------|------|
| ARCH-1 | HIGH | No MediatR validation pipeline behavior. Application-layer validators registered but never auto-invoked. | `Application/DependencyInjection.cs`:12 |
| ARCH-2 | MEDIUM | User entity is anemic -- public setters, no factory methods, business logic in handlers | `Domain/Users/User.cs` |
| ARCH-3 | MEDIUM | No concrete domain events in orgs-api Domain layer | `Domain/` (orgs-api) |
| ARCH-4 | MEDIUM | Email failures silently swallowed -- users may miss critical emails | `EmailService.cs`:287-309 |
| ARCH-5 | MEDIUM | Outbox lacks dead-letter handling -- failed messages retry indefinitely | `OutboxDispatcherService.cs`:15-24 |
| ARCH-6 | LOW | InvitationRepository.CancelByOrgAsync loads all entities into memory | `InvitationRepository.cs`:31-38 |
| ARCH-7 | LOW | PaymentRepository.GetByOrgIdAsync has no pagination | `PaymentRepository.cs`:19-25 |
| ARCH-8 | LOW | Test method name misleading: `ShouldHaveExactlyThreeValues` asserts 5 | `WorkItemStatusTests.cs`:18 |

---

## 4. PERFORMANCE AUDIT

### 4.1 N+1 Query Risk: LOW

Queries are generally well-optimized with join projections, `AsNoTracking()`, and no lazy loading.

### 4.2 Performance Findings

| ID | Severity | Finding |
|----|----------|---------|
| PERF-1 | MEDIUM | No SWR/React Query in frontend -- no client-side cache invalidation, no stale-while-revalidate |
| PERF-2 | LOW | InvitationRepository.CancelByOrgAsync loads N entities for soft-delete |
| PERF-3 | LOW | PaymentRepository.GetByOrgIdAsync without pagination |
| PERF-4 | LOW | HealthChecksConfiguration creates RabbitMQ connection synchronously with `.GetAwaiter().GetResult()` |
| PERF-5 | LOW | No `maxmemory-policy` configured in Redis |
| PERF-6 | INFO | `min_instances = 0` in Cloud Run for all services -- significant cold starts |

---

## 5. DEVOPS AUDIT

### 5.1 Strengths

- Multi-stage Docker builds, alpine images, non-root users
- Comprehensive CI/CD with change detection, parallel builds, vulnerability scanning
- Full deployment pipeline: CI -> Publish -> Deploy -> Rollback
- Terraform IaC with modules for VPC, Cloud SQL, Redis, Cloud Run, IAM, Secrets
- Cross-platform scripts (PowerShell + Bash)

### 5.2 DevOps Findings

| ID | Severity | Finding | File |
|----|----------|---------|------|
| OPS-1 | HIGH | Mailhog ports exposed to ALL interfaces (not localhost-bound) | `docker-compose.yml`:76-77 |
| OPS-2 | HIGH | No OTLP endpoint configured in production Terraform | `staging/main.tf` |
| OPS-3 | MEDIUM | Verify Dependabot scope covers all 3 ecosystems | `.github/dependabot.yml` |
| OPS-4 | MEDIUM | Shared PostgreSQL superuser for both services | `docker-compose.yml`, `.env` |
| OPS-5 | MEDIUM | Mailhog image unmaintained -- consider Mailpit | `docker-compose.yml`:72 |
| OPS-6 | MEDIUM | No structured logging (Serilog/NLog) in .NET services | `Program.cs` |
| OPS-7 | MEDIUM | No resource limits in docker-compose.production.yml | `docker-compose.production.yml` |
| OPS-8 | LOW | Aspire dashboard unpinned version | `docker-compose.yml`:61 |
| OPS-9 | LOW | seed-dev.sh has no PowerShell equivalent | `scripts/` |
| OPS-10 | LOW | scaffold-module.ps1 has no Bash equivalent | `services/*/scripts/` |
| OPS-11 | LOW | Run scripts don't install dependencies or check versions | `scripts/run-*.*` |
| OPS-12 | LOW | Health check responses expose exception messages | `HealthChecksConfiguration.cs`:164 |

---

## 6. DOCUMENTATION & DX AUDIT

### 6.1 Ratings

| Category | Score | Detail |
|----------|-------|--------|
| Root-level docs | 9/10 | Buyer README with screenshots, feature matrix, architecture diagram |
| Operational guides | 9/10 | getting-started, cookbook, configuration, production-hardening, recovery-runbook |
| Developer onboarding | 8/10 | One-command setup; missing IDE guide |
| API Documentation | 4/10 | Swagger exists at runtime but no static API reference |
| Code Documentation | 3/10 | Only 30 .cs files with XML docs, 6 .tsx with JSDoc |
| Buyer customization | 5/10 | Cookbook covers extension but no dedicated branding guide |
| Backlog management | 10/10 | 13 epics, 80+ tasks, matching walkthroughs |
| Legal/Compliance | 9/10 | EULA, licensing guide, third-party notices |
| Agent governance | 10/10 | 10 workflow rules, 7 templates |

### 6.2 Documentation Findings

| ID | Severity | Finding |
|----|----------|---------|
| DOC-1 | HIGH | No static API Reference document |
| DOC-2 | HIGH | Very low XML doc coverage in .NET (30/200+ files) |
| DOC-3 | HIGH | Very low JSDoc coverage in TypeScript (6/60+ files) |
| DOC-4 | MEDIUM | No customization/branding guide for buyers |
| DOC-5 | MEDIUM | Zero ADRs written (template exists, directory empty) |
| DOC-6 | MEDIUM | No IDE/editor setup guide |
| DOC-7 | MEDIUM | No UPGRADING.md for buyers merging upstream changes |
| DOC-8 | LOW | No troubleshooting FAQ in getting-started |
| DOC-9 | LOW | Placeholder URLs in getting-started.md |
| DOC-10 | LOW | Demo video still pending (task 0064) |

---

## 7. IMPLEMENTATION PLAN

Improvements are organized in **sequential phases**. Within each phase, tasks marked with `+` can run **in parallel** without file collisions. Tasks marked with `->` are **sequential** (depend on prior task).

### PHASE 0: Critical Security (IMMEDIATE -- Blocks sale)

| # | Task | Parallel? | Key Files |
|---|------|-----------|-----------|
| 0.1 | + Fix SEC-1: Add `RequestingUserId` to `UpdateMemberRoleCommand`, verify membership and role | Yes with 0.2, 0.3 | `UpdateMemberRoleCommand.cs`, handler, controller |
| 0.2 | + Fix SEC-2: Add `RequestingUserId` to `CreateInvitationCommand`, verify membership Admin+ | Yes with 0.1, 0.3 | `CreateInvitationCommand.cs`, handler, controller |
| 0.3 | + Fix SEC-3: Implement real admin role. Add `role` claim to JWT, update `AdminOnly` policy | Yes with 0.1, 0.2 | `AuthorizationPolicies.cs`, `JwtTokenService.cs`, `LoginUserCommandHandler.cs` |

### PHASE 1: High Security (Urgent)

| # | Task | Parallel? |
|---|------|-----------|
| 1.1 | + Fix SEC-7: Add `ASPNETCORE_ENVIRONMENT` guard in DevAuthenticationHandler | Yes with 1.2-1.5 |
| 1.2 | + Fix SEC-5: Validate JWT secret minimum length at runtime (>=32 bytes) | Yes with 1.1, 1.3-1.5 |
| 1.3 | + Fix SEC-8: Implement CSP nonces or migrate to `strict-dynamic` | Yes with 1.1-1.2, 1.4-1.5 |
| 1.4 | + Fix OPS-1: Bind Mailhog ports to localhost | Yes with 1.1-1.3, 1.5 |
| 1.5 | + Fix SEC-6: Implement refresh token mechanism | Yes with 1.1-1.4 |

### PHASE 2: Critical UX (High priority for product)

| # | Task | Parallel? |
|---|------|-----------|
| 2.1 | + Fix UX-1: Implement mobile navigation (hamburger menu) | Yes with 2.2-2.5 |
| 2.2 | + Fix UX-2: Render skip-to-content link in layout | Yes with 2.1, 2.3-2.5 |
| 2.3 | + Fix UX-3: Translate StatusBadge using i18n messages | Yes with 2.1-2.2, 2.4-2.5 |
| 2.4 | + Fix UX-4: Add keyboard accessibility to NotificationDropdown | Yes with 2.1-2.3, 2.5 |
| 2.5 | + Fix UX-13: Add `<main>` landmarks to all pages | Yes with 2.1-2.4 |

### PHASE 3: Architecture and Robustness

| # | Task | Parallel? |
|---|------|-----------|
| 3.1 | -> Fix ARCH-1: Implement `ValidationBehavior` MediatR pipeline. DO FIRST. | FIRST |
| 3.2 | + Fix ARCH-2: Enrich User entity with factory methods | Yes with 3.3-3.6 |
| 3.3 | + Fix ARCH-3: Define concrete domain events in orgs-api | Yes with 3.2, 3.4-3.6 |
| 3.4 | + Fix ARCH-4: Propagate critical email errors to caller | Yes with 3.2-3.3, 3.5-3.6 |
| 3.5 | + Fix ARCH-5: Implement dead-letter handling in outbox | Yes with 3.2-3.4, 3.6 |
| 3.6 | + Fix OPS-6: Add Serilog with JSON structured logging | Yes with 3.2-3.5 |

### PHASE 4: UX Consolidation

| # | Task | Parallel? |
|---|------|-----------|
| 4.1 | + Fix UX-5: Add AbortController to all data fetching hooks | Yes with 4.2-4.6 |
| 4.2 | + Fix UX-7: Extract `formatDate` to shared utility | Yes with 4.1, 4.3-4.6 |
| 4.3 | + Fix UX-8: Create reusable `ErrorMessage` component | Yes with 4.1-4.2, 4.4-4.6 |
| 4.4 | + Fix UX-9: Migrate WorkItemForm to FormProvider + FormField | Yes with 4.1-4.3, 4.5-4.6 |
| 4.5 | + Fix UX-11: Extract `useCsrfToken()` hook | Yes with 4.1-4.4, 4.6 |
| 4.6 | + Fix UX-6: Add `loading.tsx` to 8 missing routes | Yes with 4.1-4.5 |
| 4.7 | -> Fix UX-12: Integrate TanStack Query for data fetching. AFTER 4.1 and 4.5. | AFTER 4.1, 4.5 |

### PHASE 5: DevOps and Operations

| # | Task | Parallel? |
|---|------|-----------|
| 5.1 | + Fix OPS-2: Configure OTLP endpoint in Terraform | Yes with 5.2-5.6 |
| 5.2 | + Fix OPS-4: Create separate PostgreSQL users per service | Yes with 5.1, 5.3-5.6 |
| 5.3 | + Fix OPS-5: Migrate from Mailhog to Mailpit | Yes with 5.1-5.2, 5.4-5.6 |
| 5.4 | + Fix OPS-7: Add resource limits to docker-compose.production.yml | Yes with 5.1-5.3, 5.5-5.6 |
| 5.5 | + Fix OPS-8: Pin Aspire dashboard version | Yes with 5.1-5.4, 5.6 |
| 5.6 | + Fix PERF-5: Configure `maxmemory-policy allkeys-lru` in Redis | Yes with 5.1-5.5 |

### PHASE 6: Documentation (Buyer-facing)

| # | Task | Parallel? |
|---|------|-----------|
| 6.1 | + Fix DOC-1: Create `docs/api-reference.md` | Yes with 6.2-6.6 |
| 6.2 | + Fix DOC-4: Create `docs/customization-guide.md` | Yes with 6.1, 6.3-6.6 |
| 6.3 | + Fix DOC-5: Write 5-10 ADRs for key decisions | Yes with 6.1-6.2, 6.4-6.6 |
| 6.4 | + Fix DOC-6: Create IDE setup guide | Yes with 6.1-6.3, 6.5-6.6 |
| 6.5 | + Fix DOC-7: Create `UPGRADING.md` | Yes with 6.1-6.4, 6.6 |
| 6.6 | + Fix DOC-8+9: Add troubleshooting FAQ and update placeholder URLs | Yes with 6.1-6.5 |
| 6.7 | -> Fix DOC-2+3: Increase XML docs and JSDoc coverage. AFTER code stabilized. | AFTER phases 3-4 |

### PHASE 7: Polish and Performance

| # | Task | Parallel? |
|---|------|-----------|
| 7.1 | + Fix ARCH-6: Migrate InvitationRepository.CancelByOrgAsync to ExecuteUpdateAsync | Yes |
| 7.2 | + Fix ARCH-7: Add pagination to PaymentRepository.GetByOrgIdAsync | Yes |
| 7.3 | + Fix UX-10: Implement retry logic using ApiError.retryAfterSeconds | Yes |
| 7.4 | + Add tests for untested components | Yes |
| 7.5 | + Add tests for WorkItemEventProjector and ai-api validators | Yes |
| 7.6 | + Fix SEC-10: Add HSTS header to frontend | Yes |
| 7.7 | + Fix SEC-15: Configure BCrypt work factor to 12 | Yes |
| 7.8 | + Fix SEC-12: Add rate limiting to billing webhook endpoint | Yes |

### Phase Dependency Matrix

```
Phase 0 (Critical Security) --> Phase 1 (High Security)
                                      |
Phase 2 (Critical UX) <-- parallel --> Phase 1
                                      |
                                      v
                               Phase 3 (Architecture)
                                      |
                                      v
                               Phase 4 (UX Consolidation)
                                      |
Phase 5 (DevOps) <-- parallel --> Phase 4
                                      |
                                      v
                               Phase 6 (Documentation)
                                      |
                                      v
                               Phase 7 (Polish)
```

**Key notes**:
- Phase 0 is blocking -- nothing else should start until the 3 critical security issues are resolved
- Phases 1 and 2 can run in parallel -- high security and critical UX don't overlap on files
- Phase 3 must precede Phase 4 -- architectural changes affect the API surface consumed by frontend
- Phases 5 and 4 can run in parallel -- DevOps doesn't touch application code
- Phase 6 goes after 3-4 -- API documentation must reflect stabilized code
- Phase 7 is independent -- polish and performance with no hard dependencies
