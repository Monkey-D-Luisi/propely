# Audit Executive Summary: Epic 006 — Billing & Subscriptions

## Audit Metadata
- **Epic:** `docs/backlog/epic-006-billing.md`
- **Date:** 2026-02-11
- **Auditor:** Agent
- **Status:** In Progress
- **Tasks audited:** 4 (0042, 0043, 0044, 0045)
- **Services affected:** orgs-api, web
- **Commits analyzed:** 18 (4 feat + 9 review + 2 docs + 3 branch-only)

> **Status values:**
> - `In Progress` — There are still `Not started` items in the Prioritized Action Plan.
> - `Complete` — All action plan items are `Done`. The agent will skip this audit when running `next audit action`.

## Scores

| Area | Score | Verdict |
|------|-------|---------|
| Architecture | 80/100 | Good overall; one layer violation (API→Infrastructure) |
| Security (Backend) | 70/100 | Stripe verification solid; CSRF gap on billing POST endpoints |
| Security (Frontend) | 78/100 | Good CSRF handling pattern; redirect validation missing |
| Code Quality | 82/100 | Consistent patterns; DRY violations and one oversized handler |
| Test Coverage | 55/100 | Strong handler coverage; 0% frontend, 0% controller, 0% validators |
| Documentation | 88/100 | Walkthroughs aligned; epic tracker stale |
| **Overall** | **73/100** | Solid foundation with addressable gaps |

---

## Security Findings

### HIGH

#### F1. Missing CSRF Validation on Billing POST Endpoints
- **Severity:** HIGH
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs:54`, `:102`
- **Problem:** The POST `/billing/checkout` and POST `/billing/customer-portal` endpoints do not call `ValidateCsrf()`. The `ValidateCsrf()` method is private to `AuthController` (line 314) and is called on all 8 of its state-changing endpoints. BillingController, which also uses cookie-based auth, lacks this validation entirely.
- **Impact:** An attacker could craft a page that tricks an authenticated user into triggering a Stripe checkout or portal session redirect. While the user would still need to confirm payment on Stripe's side (limiting financial damage), the attacker could force-redirect authenticated users.
- **Recommendation:** Extract `ValidateCsrf()` to a shared base controller or action filter. Apply it to both `CreateCheckout` and `CreateCustomerPortal` endpoints. Example:
  ```csharp
  // Option A: Move to a shared BaseController
  // Option B: Create a CsrfValidationFilter as an [ServiceFilter]
  if (!ValidateCsrf()) return StatusCode(403, new { error = "Invalid CSRF token." });
  ```

### MEDIUM

#### F2. Protocol-Relative URL Bypass in Path Validators
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/CheckoutRequestValidator.cs:29-41`, `CustomerPortalRequestValidator.cs:20-32`
- **Problem:** The `IsAllowedRelativePath` method checks `path.StartsWith('/')` and `!path.Contains("://")` but does not reject protocol-relative URLs like `//attacker.com`. While the `_frontendBaseUrl` prepend (`http://localhost:3000//attacker.com`) mitigates exploitation by keeping the redirect on the frontend domain, this is a defense-in-depth gap.
- **Recommendation:** Add `path.StartsWith("//")` rejection:
  ```csharp
  if (path.StartsWith("//")) return false;
  ```

#### F3. No Logging on Webhook Signature Verification Failure
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs:141-143`
- **Problem:** When `EventUtility.ConstructEvent()` throws `StripeException` (invalid signature), the controller returns `BadRequest()` with no logging. Failed signature verifications could indicate an attack attempt.
- **Recommendation:** Log the failure:
  ```csharp
  catch (StripeException ex)
  {
      _logger.LogWarning("Webhook signature verification failed: {Message}", ex.Message);
      return BadRequest();
  }
  ```

#### F4. No Max Length Validation on URL Inputs
- **Severity:** MEDIUM
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/CheckoutRequestValidator.cs:16-26`, `CustomerPortalRequestValidator.cs:11-16`
- **Problem:** URL string fields (`SuccessUrl`, `CancelUrl`, `ReturnUrl`) have no maximum length constraint. An attacker could send extremely long URLs that consume memory during processing.
- **Recommendation:** Add `.MaximumLength(2048)` to each URL rule.

### LOW

#### F5. Frontend Redirects Without Domain Validation
- **Severity:** LOW
- **Files:** `apps/web/src/hooks/billing.ts:98`, `:118`
- **Problem:** `window.location.href = data.checkoutUrl` and `window.location.href = data.portalUrl` redirect to URLs from the API without validating they belong to Stripe's domain. In practice, these URLs originate from Stripe's SDK and the risk is theoretical (requires server compromise).
- **Recommendation:** Validate URLs start with `https://checkout.stripe.com` or `https://billing.stripe.com` before redirect.

#### F6. Verbose Error Messages in Billing Handlers
- **Severity:** LOW
- **Files:** `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs:84-85`
- **Problem:** String-based exception matching (`ex.InnerException?.Message.Contains("duplicate key")`) is fragile and could leak database implementation details in edge cases.
- **Recommendation:** Catch `DbUpdateException` specifically and check for the inner `PostgresException` error code.

---

## Architecture Compliance

### Adherence Score: 80/100

The epic implements Clean Architecture with CQRS (MediatR) consistently across four tasks. Domain, Application, and Infrastructure layers are well-separated with one notable exception.

### Positive Observations
- Domain entities (`Subscription`, `SubscriptionStatus`) have zero framework dependencies and use proper factory patterns with `Create()` methods.
- Application layer uses record-based commands/queries with `IRequest<T>`, clean handler separation, and domain exception hierarchy (`NotFoundException`, `ForbiddenException`, `DomainException`).
- Infrastructure implements strategy pattern for `IPaymentService` (`StripePaymentService` / `NoOpPaymentService`), enabling the `BILLING_MODE` toggle cleanly.
- `IPlanProvider` abstraction in Application layer keeps plan configuration decoupled from Infrastructure.
- `WebhookEventMapper` keeps Stripe SDK types out of the Application layer.

### Violations / Concerns
- **API→Infrastructure import** (`BillingController.cs:8`): `using SaasTemplate.OrgsApi.Infrastructure.Billing;` imports `BillingConfiguration` to access `Stripe.WebhookSecret` and the `Plans` list for price-to-plan mapping. The API layer should not reference Infrastructure directly. Fix: expose needed config via Application-layer interfaces or move the config class.
- **ProcessWebhookEventCommandHandler** (254 lines): Contains 5 event-type handlers, email notification logic, and status mapping in a single class. This violates Single Responsibility Principle. Consider extracting to separate strategy-based handlers.

---

## Code Quality

### Backend
**Strengths:**
- Consistent PascalCase naming for public members, camelCase for locals.
- FluentValidation used consistently for request validation.
- Options pattern (`IOptions<BillingConfiguration>`) for configuration.
- Proper async/await throughout with `CancellationToken` propagation.
- Repository pattern with interface-based abstractions.

**Issues:**
- **DRY violation**: Authorization checks (membership + role) duplicated identically in `CreateCheckoutSessionCommandHandler.cs:30-39` and `CreateCustomerPortalSessionCommandHandler.cs:30-39`.
- **DRY violation**: `IsAllowedRelativePath` method duplicated verbatim in `CheckoutRequestValidator.cs:29-41` and `CustomerPortalRequestValidator.cs:20-32`.
- **Silent webhook failures**: `WebhookEventMapper.cs:28-29,49-50,67` returns empty `WebhookEventData` when Stripe object casts fail, which could mask data issues.

### Frontend
**Strengths:**
- Proper `"use client"` directives on interactive components.
- Clean hook composition with `useState`/`useEffect`/`useCallback`.
- Mounted-flag pattern prevents state updates after unmount (`billing.ts:39-40`).
- Zod schema validation in `apiFetch` calls.
- Responsive Tailwind CSS with mobile-first breakpoints (`md:grid-cols-3`).
- Full i18n coverage (EN + ES) via `useTranslations`.

**Issues:**
- **Unused error states**: `usePlans()` and `useSubscription()` hooks return `error` values, but `PricingContent.tsx` and `BillingManagement.tsx` only show toast notifications, not inline error UI.
- **Incomplete Plan schema**: `schemas.ts:265-269` has `id`, `name`, `features` but omits `maxMembers`, `maxOrganizations` which are returned by the backend `PlanDto`.

---

## Test Coverage

### Summary

| Layer | Tests | Gaps |
|-------|-------|------|
| Unit (handlers) | 33 across 5 handlers | None — all command/query handlers fully tested |
| Unit (infrastructure) | 14 across 2 services | `StripePaymentService`, `NoOpPaymentService`, `WebhookEventMapper` untested |
| Unit (domain) | 3 in SubscriptionTests | None |
| Integration (endpoints) | 0 | **BillingController has no integration tests** |
| Validators | 0 | **CheckoutRequestValidator and CustomerPortalRequestValidator untested** |
| Frontend | 0 | **All billing components and hooks untested** |

### Missing Tests
1. `BillingControllerIntegrationTests` — All 5 endpoints need integration tests covering auth, validation, and error responses.
2. `CheckoutRequestValidatorTests` — URL validation rules, including edge cases (`//`, empty strings, absolute URLs).
3. `CustomerPortalRequestValidatorTests` — Same URL validation patterns.
4. `WebhookEventMapperTests` — Mapping of all 5 Stripe event types, null/missing field handling, price-to-plan mapping.
5. `StripePaymentServiceTests` — Session creation, error handling, metadata population (requires Stripe SDK mocking).
6. Frontend billing hook tests (`usePlans`, `useSubscription`, `useCreateCheckout`, `useCreateCustomerPortal`).
7. Frontend component tests (`PlanCard`, `PricingContent`, `BillingManagement`).

---

## Documentation

### Task-Walkthrough Alignment

| Task | Task DOD | Walkthrough | Issue |
|------|----------|-------------|-------|
| 0042 | OK (7/7) | OK | Aligned |
| 0043 | OK (6/6) | OK | Minor: documented deviation from `HasFeatureAsync` to typed methods |
| 0044 | OK (7/7) | OK | Aligned |
| 0045 | OK (7/7) | OK | 2 items deferred to follow-ups: payment failure banner, BILLING_MODE=free hiding |

### Other Documentation Issues
- **Epic progress tracker stale** (`epic-006-billing.md:75-78`): Shows "3 done, 2 remaining" but should be "4 done, 1 remaining" since task 0045 is marked DONE.
- **Task 0045 deferred items**: Payment failure warning banner and conditional BILLING_MODE=free UI hiding are listed in walkthrough follow-ups but not tracked in the epic backlog.

---

## Commit History

### Pattern Compliance
- Conventional commits: **Yes** (9/10 — one `code-review` scope anomaly on cr-0042)
- Branch naming: **Yes** (10/10 — all `feat/NNNN-description`)
- Code review cycles: **Observed** — every task has 2 review cycles (thorough)

### Observations
- Squash merge workflow on main produces clean linear history.
- Task ID traceability is perfect: every commit references `(#NNNN)` or `(#cr-NNNN)`.
- Review fix commits (cr-NNNN) sometimes land on main before their parent feature commits due to independent merge timing. This is harmless but could confuse `git bisect`.
- Stray `nul` file at repo root (Windows artifact from `>nul` redirection creating a literal file).
- One non-conventional commit message on `fix/audit-epic-005-batch` branch: `task-0042-stripe-files-from-other-branch`.

---

## What's Done Well

1. **Strategy pattern for billing mode** (`StripePaymentService` / `NoOpPaymentService`): Clean toggle between Stripe and free modes without conditional logic scattered across handlers. `PlanProvider.cs:14-17`.
2. **Stripe webhook signature verification**: Proper use of `EventUtility.ConstructEvent()` with dedicated webhook secret. `BillingController.cs:139`.
3. **Idempotent webhook processing**: `ProcessedWebhookEvent` entity prevents duplicate processing with proper idempotency check. `ProcessWebhookEventCommandHandler.cs:47-53`.
4. **Domain entity design**: `Subscription.Create()` factory method with immutable properties and clear state transitions (`UpdateStatus`, `UpdatePlan`, `Cancel`). `Subscription.cs:14-62`.
5. **Comprehensive handler testing**: All 5 command/query handlers have thorough unit tests covering happy paths, error paths, security scenarios, and edge cases. 33 tests with proper AAA pattern and NSubstitute mocking.
6. **Two-pass code review discipline**: Every task went through 2 review cycles, catching issues like the `pastdue` vs `past_due` status mismatch (cr-0050, C6/C7).
7. **Frontend hook patterns**: Mounted-flag cleanup, proper dependency arrays, `useCallback` for memoized mutations, CSRF token integration. `billing.ts:1-121`.
8. **Full i18n coverage**: Both EN and ES locales have complete billing string sets, with locale-aware date formatting in `BillingManagement.tsx:129-132`.

---

## Prioritized Action Plan

> This table is consumed by the `next audit action` workflow.
> Items are ordered by priority (P0 first) and within priority by severity.

| # | Priority | Severity | Title | Description | Files | Dependencies | Status |
|---|----------|----------|-------|-------------|-------|--------------|--------|
| 1 | P0 | HIGH | Add CSRF validation to billing endpoints | Extract `ValidateCsrf()` to shared base or filter. Apply to `CreateCheckout` and `CreateCustomerPortal` POST endpoints. | `BillingController.cs:54,102`, `AuthController.cs:314` | None | Done |
| 2 | P1 | MEDIUM | Fix protocol-relative URL bypass in validators | Add `path.StartsWith("//")` check to `IsAllowedRelativePath`. Extract duplicate method to shared helper. | `CheckoutRequestValidator.cs:29-41`, `CustomerPortalRequestValidator.cs:20-32` | None | Done |
| 3 | P1 | MEDIUM | Add webhook signature failure logging | Log `StripeException` message at Warning level when signature verification fails in `HandleWebhook`. | `BillingController.cs:141-143` | None | Done |
| 4 | P1 | MEDIUM | Add max length validation on URL inputs | Add `.MaximumLength(2048)` to `SuccessUrl`, `CancelUrl`, and `ReturnUrl` rules in both validators. | `CheckoutRequestValidator.cs:16-26`, `CustomerPortalRequestValidator.cs:11-16` | #2 | Done |
| 5 | P1 | MEDIUM | Fix API→Infrastructure architecture violation | Move `BillingConfiguration` to Application layer, or expose webhook secret and price map through Application-layer interfaces. Remove `using Infrastructure.Billing` from BillingController. | `BillingController.cs:8,38,45-49` | None | Done |
| 6 | P1 | MEDIUM | Add BillingController integration tests | Create integration tests for all 5 endpoints: checkout, subscription, plans, customer-portal, webhook. Cover auth, validation, CSRF, and error responses. | `tests/.../Controllers/BillingControllerTests.cs` (new) | #1 | Done |
| 7 | P1 | MEDIUM | Add request validator tests | Create unit tests for `CheckoutRequestValidator` and `CustomerPortalRequestValidator` covering URL validation, empty inputs, and edge cases. | `tests/.../Validators/CheckoutRequestValidatorTests.cs` (new), `tests/.../Validators/CustomerPortalRequestValidatorTests.cs` (new) | #2, #4 | Done |
| 8 | P2 | LOW | Add frontend redirect domain validation | Validate that `checkoutUrl` starts with `https://checkout.stripe.com` and `portalUrl` starts with `https://billing.stripe.com` before `window.location.href` assignment. | `apps/web/src/hooks/billing.ts:98,118` | None | Done |
| 9 | P2 | LOW | Update epic progress tracker | Change progress table from "3 done, 2 remaining" to "4 done, 1 remaining" to reflect task 0045 completion. | `docs/backlog/epic-006-billing.md:75-78` | None | Done |
| 10 | P2 | LOW | Add WebhookEventMapper tests | Create unit tests for mapping all 5 Stripe event types, null field handling, and price-to-plan mapping. | `tests/.../Infrastructure/Billing/WebhookEventMapperTests.cs` (new) | None | Done |
| 11 | P2 | LOW | Delete stray `nul` file | Remove the `nul` file at repo root (Windows artifact) and optionally add it to `.gitignore`. | `nul` | None | Done |
| 12 | P3 | LOW | Refactor ProcessWebhookEventCommandHandler | Extract the 5 event-type handlers to separate classes following strategy pattern. Extract email notification logic. 254 lines → ~5 smaller handlers. | `ProcessWebhookEventCommandHandler.cs` | None | Not started |
| 13 | P3 | LOW | Extract duplicate authorization checks | Create shared authorization helper or MediatR pipeline behavior for membership + role validation used by both checkout and portal handlers. | `CreateCheckoutSessionCommandHandler.cs:30-39`, `CreateCustomerPortalSessionCommandHandler.cs:30-39` | None | Not started |
| 14 | P3 | LOW | Add error state display in billing components | Consume `error` states from `usePlans()` and `useSubscription()` hooks in `PricingContent.tsx` and `BillingManagement.tsx` with inline fallback UI. | `apps/web/src/components/billing/PricingContent.tsx`, `apps/web/src/components/billing/BillingManagement.tsx` | None | Not started |
| 15 | P3 | LOW | Add frontend billing component tests | Create component tests for `PlanCard`, `PricingContent`, and `BillingManagement` covering rendering, loading states, and error handling. | `apps/web/src/components/billing/` | None | Not started |

---

## Verification Commands

```bash
# Run after all audit actions are implemented
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build && npm test
```
