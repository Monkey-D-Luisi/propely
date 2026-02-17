# Task: 0046 - One-Time Payments (Add-On)

## Metadata
- ID: 0046
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #198
- Epic: `docs/backlog/epic-006-billing.md`
- Old Issue: #26
- Dependencies: 0042 (Stripe foundation)
- Milestone: v1.0

## Goal
Add support for one-time payments via Stripe Checkout for add-on purchases or credit packs, separate from recurring subscriptions.

## Context
Some SaaS products sell one-time add-ons (extra API credits, premium reports, data exports). This builds on the Stripe foundation from task 0042 to support `payment` mode (vs `subscription` mode) in Stripe Checkout.

## Scope
### In scope
- One-time payment via Stripe Checkout (payment mode)
- Payment record entity to track completed payments
- `POST /billing/payment` endpoint to create payment session
- Handle `payment_intent.succeeded` webhook event
- Payment history page (list of past one-time payments)
- Audit log entries for payments

### Out of scope
- Refunds (handled via Stripe dashboard)
- Metered/usage-based billing
- Custom payment form

## Requirements
- R1: Users can initiate a one-time payment for a specific product/add-on
- R2: Payment processed via Stripe Checkout (hosted page)
- R3: Successful payment creates a Payment record linked to the org
- R4: Payment history is viewable by org owners
- R5: Webhook processes `payment_intent.succeeded`

## Acceptance Criteria
- AC1: `POST /billing/payment` returns Stripe Checkout URL for one-time payment
- AC2: `payment_intent.succeeded` webhook creates Payment record
- AC3: `GET /billing/payments` returns payment history for org
- AC4: `dotnet build` and `dotnet test` pass
- AC5: `npm run build` and `npm test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Stripe Checkout payment mode (not subscription)
- Update walkthrough

## Implementation Steps

1. **Create Payment entity** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Payment.cs`)
   - Id, OrgId, StripePaymentIntentId, Amount, Currency, Description, Status, CreatedAtUtc

2. **Create PaymentConfiguration** for EF Core and migration

3. **Create IPaymentRepository** and implement

4. **Create CreatePaymentSessionCommand** - calls Stripe Checkout in payment mode

5. **Create GetPaymentHistoryQuery** - list payments for org

6. **Handle payment_intent.succeeded** in webhook handler (from task 0044)

7. **Add endpoints** to BillingController
   - `POST /billing/payment` (create session)
   - `GET /billing/payments` (history)

8. **Frontend: Payment history component** on billing page

9. **Add i18n strings**

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Payment.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreatePaymentSession/`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetPaymentHistory/`
- `docs/walkthroughs/0046-one-time-payments.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs`
- `apps/web/src/app/[locale]/orgs/[orgId]/billing/page.tsx`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Unit tests: Payment entity, command handlers
- Integration tests: Payment session creation, webhook processing
- Manual: Full payment flow with Stripe test mode

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
