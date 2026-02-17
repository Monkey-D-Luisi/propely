# Task: 0044 - Stripe Webhook Handler

## Metadata
- ID: 0044
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #196
- Epic: `docs/backlog/epic-006-billing.md`
- Old Issue: #27
- Dependencies: 0042 (Stripe foundation)

## Goal
Create a Stripe webhook endpoint that processes billing events (checkout completed, invoice paid/failed, subscription updated/deleted) and updates subscription status accordingly.

## Context
Stripe uses webhooks to notify the application about billing events. After a user completes checkout (task 0043), Stripe sends a `checkout.session.completed` event. Subscription renewals trigger `invoice.paid` or `invoice.payment_failed`. Cancellations trigger `customer.subscription.deleted`. Our webhook endpoint must verify the Stripe signature, process events idempotently, and update the Subscription entity.

## Scope
### In scope
- Webhook endpoint: `POST /billing/webhook`
- Stripe signature verification using webhook secret
- Event handling: `checkout.session.completed`, `invoice.paid`, `invoice.payment_failed`, `customer.subscription.updated`, `customer.subscription.deleted`
- Idempotent processing (same event processed twice = no side effects)
- Update Subscription status based on events
- Create audit log entries for billing events
- Send notifications on payment failures

### Out of scope
- Retry logic (Stripe handles retries automatically)
- Event sourcing / event store (just process and update)

## Requirements
- R1: Webhook verifies Stripe signature before processing
- R2: Invalid signatures return 400 (not processed)
- R3: Each event type updates Subscription correctly
- R4: Processing is idempotent (event ID tracked)
- R5: Payment failures trigger user notifications
- R6: Audit log entries created for all billing events

## Acceptance Criteria
- AC1: `POST /billing/webhook` with valid Stripe signature processes event
- AC2: `POST /billing/webhook` with invalid signature returns 400
- AC3: `checkout.session.completed` creates/updates Subscription to Active
- AC4: `invoice.payment_failed` updates Subscription to PastDue
- AC5: `customer.subscription.deleted` updates Subscription to Cancelled
- AC6: Duplicate event processing is harmless (idempotent)
- AC7: `dotnet build` and `dotnet test` pass

## Constraints (non-negotiable)
- Webhook endpoint must NOT have [Authorize] attribute (Stripe calls it)
- Must verify Stripe signature (security)
- Clean Architecture layers respected
- Update walkthrough

## Implementation Steps

1. **Create webhook endpoint** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs`)
   - `POST /billing/webhook` - [AllowAnonymous]
   - Read raw body for signature verification
   - Use `EventUtility.ConstructEvent()` from Stripe.net SDK

2. **Create event handlers** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/`)
   - `ProcessWebhookEventCommand.cs`: EventType, EventId, EventData (JSON)
   - Handler: switch on event type -> delegate to specific handlers

3. **Handle checkout.session.completed**
   - Extract customer ID, subscription ID, org ID (from metadata)
   - Create or update Subscription entity with Active status

4. **Handle invoice.paid**
   - Update subscription's `CurrentPeriodEnd`
   - Ensure status is Active

5. **Handle invoice.payment_failed**
   - Update subscription status to PastDue
   - Send notification to org owners about payment failure

6. **Handle customer.subscription.updated**
   - Update plan, status, period end
   - Handle downgrades/upgrades

7. **Handle customer.subscription.deleted**
   - Update subscription status to Cancelled
   - Set `CancelledAtUtc`

8. **Idempotency tracking**
   - Store processed event IDs (use ProcessedEvent table pattern already in codebase)
   - Before processing, check if event ID already processed -> skip if yes

9. **Create audit log entries** for each processed event

### Testing

10. **Unit tests**
    - Each event handler: correct subscription state changes
    - Invalid signature rejection
    - Duplicate event ID handling (idempotent)

11. **Integration tests**
    - Process mock webhook events -> verify DB state

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommand.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs`
- `docs/walkthroughs/0044-stripe-webhooks.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs` (add webhook endpoint)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Subscription.cs` (add status change methods)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/SubscriptionRepository.cs`

## Testing Plan
- Unit tests: All event handlers, signature verification
- Integration tests: End-to-end webhook processing with mock events
- Manual: Use Stripe CLI to forward test webhooks

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] All event types handled
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
