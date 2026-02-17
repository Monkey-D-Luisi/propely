# Code Review: cr-0062 - One-Time Payments Review

## Metadata
- PR: #262
- Branch: `feat/0046-one-time-payments`
- Target: `main`
- Task: 0046 (One-Time Payments)
- GitHub Issue: #198
- CI Status: E2E Smoke Tests FAILED (expected — no Stripe keys in CI), all others passed

## Changed Files
36 files (see PR for full list). Key areas: Domain, Application, Infrastructure, API layers in orgs-api; Frontend components, hooks, schemas, i18n in web app.

## Review Sources
- Inline review comments: 9 (5 Copilot, 4 Gemini)
- General reviews: 2 (Copilot summary, Gemini summary)
- Issue comments: 3 (none actionable — usage limit notice, /gemini trigger, Gemini summary)

## Comment Resolution Plan

### MUST_FIX

- [x] **MF-1**: Validate Amount/Currency non-null in `HandlePaymentCheckoutCompleted` (Copilot #2799817688, Gemini #2799863019)
  - Remove unsafe `data.Amount ?? 0` and `data.Currency ?? "usd"` fallbacks
  - Add null checks with warning log, skip record creation when missing
  - Files: `ProcessWebhookEventCommandHandler.cs`

- [x] **MF-2**: Map description from Stripe session metadata (Copilot #2799817731, Gemini #2799863019)
  - Add `Description` property to `WebhookEventData`
  - Populate from `session.Metadata["description"]` in `WebhookEventMapper.MapCheckoutSession`
  - Use mapped description (with fallback) in `HandlePaymentCheckoutCompleted`
  - Files: `WebhookEventData.cs`, `WebhookEventMapper.cs`, `ProcessWebhookEventCommandHandler.cs`

### SHOULD_FIX

- [x] **SF-1**: Add CancelUrl validator tests (Copilot #2799817756)
  - Add test cases for empty CancelUrl, absolute CancelUrl, protocol-relative CancelUrl
  - Mirrors existing `CheckoutRequestValidatorTests` pattern
  - Files: `PaymentRequestValidatorTests.cs`

- [x] **SF-2**: Fix `rounded-lg` → `rounded-xl` for design system compliance (Copilot #2799817782)
  - PaymentHistory outer container, skeleton, and empty state all use `rounded-lg`
  - CLAUDE.md design system specifies `rounded-xl` for cards/containers
  - Files: `PaymentHistory.tsx`

- [x] **SF-3**: Simplify FluentValidation rules — remove redundant `.When()` guard (Gemini #2799863034)
  - Chain `.NotEmpty()` + `.MaximumLength()` + `.Must()` in single rule per field
  - `.When(x => !string.IsNullOrEmpty(...))` is redundant after `.NotEmpty()`
  - Files: `PaymentRequestValidator.cs`

- [x] **SF-4**: Move `OrderByDescending` from handler to repository (Gemini #2799863039, #2799863044)
  - Add `.OrderByDescending(p => p.CreatedAtUtc)` to `PaymentRepository.GetByOrgIdAsync`
  - Remove in-memory `.OrderByDescending()` from `GetPaymentHistoryQueryHandler`
  - Files: `PaymentRepository.cs`, `GetPaymentHistoryQueryHandler.cs`

### OUT_OF_SCOPE

- **OOS-1**: Create payment as `Pending` instead of `Succeeded` (Copilot #2799817642)
  - Rationale: Would require adding `payment_intent.succeeded` and `checkout.session.async_payment_succeeded` event handlers. Without those, payments would remain `Pending` forever. Standard Stripe Checkout with card payments (default) confirms payment at `checkout.session.completed`. Full async payment method support is a separate feature.

## Parity Verification Checklist
- [x] Redirect parity checked — N/A: no auth `next` parameter. Payment checkout creates Stripe-hosted URL.
- [x] Locale source correctness checked — `PaymentHistory` uses `useLocale()` (route locale). i18n keys added for EN + ES.
- [x] API/UI contract parity checked — `PaymentRequest` DTO fields match frontend `useCreatePayment` payload. `PaymentDto` fields match `PaymentSchema` Zod schema.
- [x] Test parity checked — 19 tests covering happy paths + error paths for commands, queries, validators, and webhook handler. CancelUrl test gap addressed in SF-1.
