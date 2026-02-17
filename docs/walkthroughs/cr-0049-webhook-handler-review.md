# Walkthrough: cr-0049-webhook-handler-review

## Task Reference
- Task: `docs/tasks/cr-0049-webhook-handler-review.md`
- Walkthrough: `docs/walkthroughs/cr-0049-webhook-handler-review.md`
- PR: #246
- Date: 2026-02-11

## Summary
Addressed Gemini, Copilot, and Claude code review feedback on PR #246 (Stripe webhook handler). Applied 1 MUST_FIX (HTML injection) and 5 SHOULD_FIX items (redundant serialization, error handling, plan ID fallback, race condition, input validation).

## Comment Classification

| # | Source | File | Classification | Action |
|---|--------|------|---------------|--------|
| G1 | Gemini | EmailService.cs:219 | MUST_FIX | Fixed: HTML-encode orgName |
| C2 | Copilot | EmailService.cs:220 | ALREADY_FIXED | Duplicate of G1 |
| CL1 | Claude | EmailService.cs:217 | ALREADY_FIXED | Duplicate of G1 |
| G2-G9 | Gemini | Multiple files | SHOULD_FIX | Fixed: Pass WebhookEventData directly |
| C1/C5 | Copilot | BillingController.cs | SHOULD_FIX | Fixed: Remove try-catch, let exceptions propagate |
| C3 | Copilot | WebhookEventMapper.cs:80 | SHOULD_FIX | Fixed: PlanId = planId (null when unmapped) |
| C4 | Copilot | ProcessWebhookEventCommandHandler.cs:48 | SHOULD_FIX | Fixed: Catch DbUpdateException for duplicate PK |
| C6 | Copilot | ProcessWebhookEventCommandHandler.cs:104 | SHOULD_FIX | Fixed: Use IsNullOrWhiteSpace guard |

## Changes Made

### Fix 1: MUST_FIX — HTML-encode orgName in payment failure email (G1/C2/CL1)
- **File:** `EmailService.cs`
- Added `WebUtility.HtmlEncode(orgName)` before interpolating into HTML body to prevent HTML injection

### Fix 2: SHOULD_FIX — Pass WebhookEventData directly (G2-G9)
- **File:** `ProcessWebhookEventCommand.cs` — Changed `string RawJson` to `WebhookEventData EventData`
- **File:** `BillingController.cs` — Removed `JsonSerializer.Serialize(eventData)`, pass DTO directly
- **File:** `ProcessWebhookEventCommandHandler.cs` — Replaced 5 `JsonSerializer.Deserialize` calls with `request.EventData`
- **File:** `ProcessWebhookEventCommandHandlerTests.cs` — Updated `CreateCommand` helper for direct DTO, removed unused `using System.Text.Json`

### Fix 3: SHOULD_FIX — Let exceptions propagate for transient failures (C1/C5)
- **File:** `BillingController.cs` — Removed try-catch around `_mediator.Send()` so transient failures return 500 and Stripe retries

### Fix 4: SHOULD_FIX — Remove Stripe price ID fallback (C3)
- **File:** `WebhookEventMapper.cs` — Changed `PlanId = planId ?? stripePriceId` to `PlanId = planId` (null when no mapping exists)

### Fix 5: SHOULD_FIX — Handle race condition in idempotency (C4)
- **File:** `ProcessWebhookEventCommandHandler.cs` — Wrapped `SaveChangesAsync` in try-catch with `when` filter for duplicate key/unique constraint violations; logs and returns gracefully on concurrent duplicate processing

### Fix 6: SHOULD_FIX — Guard PlanId against empty/whitespace (C6)
- **File:** `ProcessWebhookEventCommandHandler.cs` — Changed `if (data.PlanId is not null)` to `if (!string.IsNullOrWhiteSpace(data.PlanId))` in both `HandleCheckoutSessionCompleted` and `HandleSubscriptionUpdated`

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # 0 errors, 12 warnings
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln    # All 437 tests pass (297 unit + 135 integration + 5 architecture)
```

## Checklist
- [x] Task scope matches review feedback
- [x] All Gemini comments analyzed and addressed
- [x] All Copilot comments analyzed and addressed
- [x] All Claude comments analyzed and addressed
- [x] Tests updated and passing
- [x] No secrets committed
