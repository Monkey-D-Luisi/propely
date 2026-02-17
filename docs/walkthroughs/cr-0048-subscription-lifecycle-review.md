# Walkthrough: cr-0048-subscription-lifecycle-review

## Task Reference
- Task: `docs/tasks/cr-0048-subscription-lifecycle-review.md`
- Walkthrough: `docs/walkthroughs/cr-0048-subscription-lifecycle-review.md`
- PR: #245
- Date: 2026-02-11

## Summary
Addressed Copilot and Gemini code review feedback on PR #245 (subscription lifecycle). Applied 1 MUST_FIX (open-redirect in checkout URLs) and 6 SHOULD_FIX items (exception types, plan validation, defensive copy, input validation, returnUrl validation, validation error format). Deferred 1 performance optimization (N+1 query) as out of scope. Rejected 1 suggestion (LINQ Select for loop with side effects).

## Comment Classification

| # | Source | File | Classification | Action |
|---|--------|------|---------------|--------|
| C1 | Copilot | StripePaymentService.cs:77 | SHOULD_FIX | Fixed: `InvalidOperationException` -> `NotFoundException` |
| C2 | Copilot | CreateCheckoutSessionCommandHandler.cs:46 | SHOULD_FIX | Fixed: Added `IPlanProvider` plan validation |
| C3 | Copilot | CheckoutRequestValidator.cs:27 | MUST_FIX | Fixed: Restricted to relative paths, server-side URL construction |
| C4 | Copilot | EntitlementService.cs:35 | OUT_OF_SCOPE | N+1 query acceptable at current scale |
| C5 | Copilot | PlanProvider.cs:37 | SHOULD_FIX | Fixed: Defensive copy of Features list |
| C6 | Copilot | BillingController.cs:57 | SHOULD_FIX | Fixed: Added `Guid.Empty` validation |
| C7 | Copilot | EntitlementService.cs:49 | REJECTED | Foreach has side effects, not suitable for Select() |
| G1 | Gemini | EntitlementService.cs:48 | OUT_OF_SCOPE | Duplicate of C4 - N+1 query acknowledged |
| G2 | Gemini | CheckoutRequestValidator.cs:26 | ALREADY_FIXED | Duplicate of C3 - open redirect already fixed |
| G3 | Gemini | StripePaymentService.cs:93 | SHOULD_FIX | Fixed: Added `_frontendBaseUrl` validation for returnUrl |
| G4 | Gemini | BillingController.cs:38 | SHOULD_FIX | Fixed: Return all validation errors via `ValidationProblem()` |

## Changes Made

### MUST_FIX C3/G2: Open-redirect in checkout URLs
- `CheckoutRequestValidator.cs`: Changed SuccessUrl/CancelUrl validation from accepting any absolute URI to requiring relative paths starting with `/`. Added `IsAllowedRelativePath` helper that rejects URLs containing `://`.
- `BillingController.cs`: Injected `IConfiguration` to read `Auth:FrontendBaseUrl`. Now constructs absolute URLs server-side by prepending the frontend base URL to the relative paths before passing to the command handler.

### SHOULD_FIX C1: StripePaymentService exception type
- `StripePaymentService.cs`: Replaced `InvalidOperationException` with `NotFoundException` when no subscription exists for an org in `CreateCustomerPortalSessionAsync`. This returns a 404 instead of 500 for a predictable client condition.

### SHOULD_FIX C2: PlanId validation in handler
- `CreateCheckoutSessionCommandHandler.cs`: Added `IPlanProvider` dependency. Validates `request.PlanId` against configured plans before calling `IPaymentService`. Throws `DomainException` (400) for unknown plan IDs instead of letting it surface as a 500 from Stripe.

### SHOULD_FIX C5: Defensive copy of Features list
- `PlanProvider.cs`: Changed `ToPlanInfo` to create a defensive copy of the Features list via `new List<string>(plan.Features)` to prevent downstream mutation of config-owned data.

### SHOULD_FIX C6: Guid.Empty validation on GET subscription
- `BillingController.cs`: Added early return with 400 when `orgId == Guid.Empty` on the GET /billing/subscription endpoint.

### SHOULD_FIX G3: returnUrl open-redirect in CreateCustomerPortalSessionAsync
- `StripePaymentService.cs`: Added `_frontendBaseUrl` field from `IConfiguration`. Validates that `returnUrl` starts with the configured frontend base URL before passing to Stripe. Throws `DomainException` for invalid URLs.

### SHOULD_FIX G4: Validation error format
- `BillingController.cs`: Replaced `BadRequest(new { error = ... })` with `ModelState.AddModelError` + `ValidationProblem()` to return all validation errors in standard `ValidationProblemDetails` format.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # 0 errors
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln    # 425 tests pass (285 unit + 135 integration + 5 architecture)
```

## Checklist
- [x] Task scope matches review feedback
- [x] All Copilot comments analyzed and addressed
- [x] All Gemini comments analyzed and addressed
- [x] Tests updated and passing
- [x] No secrets committed
