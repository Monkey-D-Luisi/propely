# Walkthrough: 0046 - One-Time Payments (Add-On)

## Summary

Added one-time payment support via Stripe Checkout (payment mode), separate from recurring subscriptions. Organizations can now make single payments for add-ons or credit packs, with a full payment history visible on the billing page.

## Architecture

### Domain Layer

- **`PaymentStatus.cs`** — Enum: `Pending`, `Succeeded`, `Failed`
- **`Payment.cs`** — Sealed entity with private constructor, static `Create()` factory, and `MarkSucceeded()`/`MarkFailed()` methods. Follows the same aggregate pattern as `Subscription`.

### Application Layer

- **`IPaymentRepository`** — Interface with `GetByOrgIdAsync`, `GetByStripePaymentIntentIdAsync`, `AddAsync`
- **`IPaymentService.CreatePaymentCheckoutSessionAsync`** — New method on existing interface for payment-mode checkout
- **`WebhookEventData`** — Added 4 nullable properties: `StripePaymentIntentId`, `Amount`, `Currency`, `CheckoutMode`
- **`CreatePaymentSessionCommand`** — MediatR command that validates org membership (owner/admin), checks billing enabled, then delegates to `IPaymentService`
- **`GetPaymentHistoryQuery`** — MediatR query returning `List<PaymentDto>` ordered by date descending
- **`ProcessWebhookEventCommandHandler`** — Extended `checkout.session.completed` handler to disambiguate by `CheckoutMode`. When mode is `"payment"`, creates a `Payment` record instead of a `Subscription`.

### Infrastructure Layer

- **`PaymentConfiguration.cs`** — EF Core config with snake_case columns, UUID PK, string-stored enum, unique index on `stripe_payment_intent_id`
- **`PaymentRepository.cs`** — Standard EF implementation
- **`StripePaymentService`** — New `CreatePaymentCheckoutSessionAsync` using `mode: "payment"` with inline `PriceData` (amount + currency + description)
- **`NoOpPaymentService`** — No-op stub returning empty string
- **`WebhookEventMapper`** — Populates `CheckoutMode`, `StripePaymentIntentId`, `Amount`, `Currency` from Stripe `Session` object
- **`DependencyInjection`** — Registered `IPaymentRepository → PaymentRepository`
- **`AppDbContext`** — Added `DbSet<Payment>` and `PaymentConfiguration`

### API Layer

- **`PaymentRequest`** — DTO record with `OrgId`, `Amount`, `Currency`, `Description`, `SuccessUrl`, `CancelUrl`
- **`PaymentRequestValidator`** — FluentValidation with amount > 0, relative URL paths, max lengths
- **`BillingController`** — Two new endpoints:
  - `POST /billing/payment` — Creates one-time payment checkout session
  - `GET /billing/payments?orgId=` — Returns payment history for org

### Frontend

- **`schemas.ts`** — Added `PaymentSchema`, `Payment` type, `PaymentsResponseSchema`
- **`billing.ts`** — Added `usePayments(orgId)` hook and `useCreatePayment()` hook
- **`PaymentHistory.tsx`** — New component with table showing date, description, amount (formatted), and status badge
- **`BillingManagement.tsx`** — Integrated `PaymentHistory` component below subscription card
- **`en.json` / `es.json`** — Added `billing.payments.*` and `billing.errors.paymentFailed` i18n keys

## Webhook Disambiguation

The key design decision was how to handle `checkout.session.completed` events for both subscription and payment modes. The solution uses the `CheckoutMode` field (populated from Stripe's `Session.Mode`):

- `"payment"` → routes to `HandlePaymentCheckoutCompleted` (creates `Payment` record)
- `null` or `"subscription"` → existing subscription handler (unchanged)

This is fully backward-compatible: existing subscription webhooks continue working since they have no `CheckoutMode` set.

## Files Changed

### Created
- `Domain/Billing/PaymentStatus.cs`
- `Domain/Billing/Payment.cs`
- `Application/Billing/Interfaces/IPaymentRepository.cs`
- `Application/Billing/Commands/CreatePaymentSession/CreatePaymentSessionCommand.cs`
- `Application/Billing/Commands/CreatePaymentSession/CreatePaymentSessionCommandHandler.cs`
- `Application/Billing/Queries/GetPaymentHistory/GetPaymentHistoryQuery.cs`
- `Application/Billing/Queries/GetPaymentHistory/GetPaymentHistoryQueryHandler.cs`
- `Infrastructure/Persistence/Configurations/PaymentConfiguration.cs`
- `Infrastructure/Persistence/Repositories/PaymentRepository.cs`
- `Api/Dtos/PaymentRequest.cs`
- `Api/Validators/PaymentRequestValidator.cs`
- `apps/web/src/components/billing/PaymentHistory.tsx`
- EF Core migration `AddPaymentsTable`

### Modified
- `Application/Billing/Interfaces/IPaymentService.cs` — Added `CreatePaymentCheckoutSessionAsync`
- `Application/Billing/Commands/ProcessWebhookEvent/WebhookEventData.cs` — Added 4 payment fields
- `Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs` — Added payment webhook handling
- `Infrastructure/Billing/StripePaymentService.cs` — Implemented payment checkout
- `Infrastructure/Billing/NoOpPaymentService.cs` — No-op stub
- `Infrastructure/Billing/WebhookEventMapper.cs` — Maps payment fields from Stripe Session
- `Infrastructure/DependencyInjection.cs` — Registered `IPaymentRepository`
- `Infrastructure/Persistence/AppDbContext.cs` — Added `DbSet<Payment>` and config
- `Api/Controllers/BillingController.cs` — Added payment/payments endpoints
- `apps/web/src/lib/schemas.ts` — Added Payment schema
- `apps/web/src/hooks/billing.ts` — Added payment hooks
- `apps/web/src/components/billing/BillingManagement.tsx` — Integrated PaymentHistory
- `apps/web/messages/en.json` — Added payment i18n keys
- `apps/web/messages/es.json` — Added payment i18n keys (Spanish)

## Tests

- **`CreatePaymentSessionCommandHandlerTests`** — 5 tests: valid owner, org not found, not member, regular member (forbidden), billing disabled
- **`GetPaymentHistoryQueryHandlerTests`** — 3 tests: valid member returns payments, not member throws, empty list
- **`ProcessWebhookEventCommandHandlerTests`** — 3 new tests: payment checkout completed, duplicate payment intent skip, missing fields skip
- **`PaymentRequestValidatorTests`** — 8 tests: valid request, empty fields, zero/negative amount, absolute URL rejection
