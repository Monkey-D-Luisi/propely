# Task: cr-0048 - Subscription Lifecycle PR #245 Review

## PR Metadata
- PR: #245 `feat(orgs-api): add subscription lifecycle and entitlement enforcement (#0043)`
- Branch: `feat/0043-subscription-lifecycle` -> `main`
- Author: Monkey-D-Luisi
- CI: All checks passed (Orgs API Build & Test, Claude Code Review, Detect Changes)

## Changed Files (28)
- 13 new source files (Application interfaces, commands, queries, Infrastructure implementations, API controller/DTOs/validator)
- 6 modified source files (BillingConfiguration, StripePaymentService, DI, CreateOrganization/CreateInvitation handlers, appsettings)
- 6 new test files + 2 updated test files
- 3 doc files (task, walkthrough, epic)

## Review Threads

### Source 1: Inline Review Comments (7 from Copilot)

1. **StripePaymentService.cs:77** - `CreateCustomerPortalSessionAsync` throws `InvalidOperationException` (500) for predictable client condition (no subscription). Should use `NotFoundException`/`DomainException`.
2. **CreateCheckoutSessionCommandHandler.cs:46** - PlanId not validated against configured plans before passing to `IPaymentService`. Could surface as 500.
3. **CheckoutRequestValidator.cs:27** - SuccessUrl/CancelUrl accept any absolute URI, enabling open-redirect/phishing. Should restrict to relative paths or allow-listed hosts.
4. **EntitlementService.cs:35** - N+1 query pattern in `CanCreateOrganizationAsync` for users with many orgs.
5. **PlanProvider.cs:37** - `ToPlanInfo` passes mutable `List<string>` from config directly. Could be accidentally mutated.
6. **BillingController.cs:57** - GET /billing/subscription doesn't validate `orgId` against `Guid.Empty`.
7. **EntitlementService.cs:49** - Foreach loop should use `.Select(...)`.

### Source 2: Reviews (1 from Copilot)
- Summary only, no additional actionable items beyond the 7 inline comments.

### Source 3: Issue Comments (3)
- ChatGPT Codex: Usage limit notice (not actionable)
- Gemini: Summary only (not actionable)
- Claude: "No issues found" (not actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] **#3** CheckoutRequestValidator open-redirect: Restrict SuccessUrl/CancelUrl to relative paths, construct absolute URLs server-side using `Auth:FrontendBaseUrl`

### SHOULD_FIX
- [x] **#1** StripePaymentService: Replace `InvalidOperationException` with `NotFoundException` for missing subscription
- [x] **#2** CreateCheckoutSessionCommandHandler: Validate PlanId against `IPlanProvider` before calling `IPaymentService`
- [x] **#5** PlanProvider: Defensive copy of Features list in `ToPlanInfo`
- [x] **#6** BillingController: Validate `orgId != Guid.Empty` on GET /billing/subscription

### SUGGESTION (not applicable)
- [x] **#7** EntitlementService: foreach -> Select(). Not applicable: the loop has side effects (early returns, accumulator mutation), not suitable for LINQ projection.

### OUT_OF_SCOPE
- [x] **#4** EntitlementService N+1 pattern: Performance optimization for `CanCreateOrganizationAsync`. Free plan limit is 1 org (1 query), pro is 5 (5 queries max). Acceptable at current scale. Already documented in PR risk section.

## Behavioral Parity Checks
- [x] Redirect parity: Checkout URLs are the only redirect-related items. Addressed via MUST_FIX #3.
- [x] Locale source correctness: No localized flows in this PR. N/A.
- [x] API-to-UI contract parity: Backend-only PR. Frontend is task 0045. N/A.
- [x] Test parity: All new behavior has unit tests. Happy + error paths covered for checkout, subscription queries, entitlements.
