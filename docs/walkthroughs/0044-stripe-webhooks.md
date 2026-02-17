# Walkthrough: 0044-stripe-webhooks

## Task Reference
- Task: `docs/tasks/0044-stripe-webhooks.md`
- Walkthrough: `docs/walkthroughs/0044-stripe-webhooks.md`
- Branch/PR: `feat/0044-stripe-webhooks`
- Date: 2026-02-11

## Summary
Implemented a Stripe webhook endpoint that receives billing events, verifies signatures, processes events idempotently, and updates subscription state accordingly. Handles five event types: checkout completed, invoice paid/failed, subscription updated/deleted.

## Context
- Background: Tasks 0042/0043 established Stripe SDK integration, Subscription entity, checkout session creation, and entitlement enforcement. After a user completes checkout, Stripe sends webhook events to notify the application about payment outcomes and subscription changes.
- Problem statement: No mechanism to receive and process Stripe billing events, so subscription status would never update after initial checkout.
- Constraints: Clean Architecture layers respected. Webhook endpoint must be unauthenticated (Stripe sends requests). Application layer cannot depend on Stripe SDK.

## Decisions & Trade-offs

- **Decision:** Created a separate `ProcessedWebhookEvent` entity instead of reusing `ProcessedEvent`
  - Options considered: Reuse existing ProcessedEvent (Guid PK), new entity with string PK
  - Why this choice: Stripe event IDs are strings (`evt_xxxx`), not GUIDs. Separate entity avoids type mismatch and keeps domain event tracking separate from external webhook tracking.

- **Decision:** Single MediatR command handler with private methods per event type
  - Options considered: Separate command per event type, strategy pattern, single handler with switch
  - Why this choice: All event types share the same idempotency check and save pattern. Private methods keep related logic together without over-abstracting for 5 event types.

- **Decision:** `WebhookEventMapper` static class in Infrastructure maps Stripe SDK types to `WebhookEventData` DTO
  - Options considered: Raw JSON parsing in handler, mapper in API layer, mapper in Infrastructure
  - Why this choice: Keeps Stripe SDK dependency in Infrastructure. Application layer handler only sees `WebhookEventData` (a plain record). Controller calls mapper after signature verification.

- **Decision:** Controller returns 200 even on processing errors (after signature verification)
  - Options considered: Let exceptions propagate (500), always return 200 after verification
  - Why this choice: Returning 500 causes Stripe to retry, which wastes resources since our idempotency check handles duplicates. Processing errors are logged for investigation.

- **Decision:** Added `plan_id` to Stripe checkout session metadata
  - Options considered: Reverse-map from Stripe price ID, store plan_id in metadata
  - Why this choice: Checkout metadata is the cleanest way to pass our plan ID through the Stripe flow. For subscription.updated events, we reverse-map from Stripe price ID using the billing configuration.

## Implementation Notes
- `WebhookEventMapper` handles Stripe.net v50 breaking changes: `Invoice.Parent.SubscriptionDetails.SubscriptionId` (was `Invoice.SubscriptionId`), `SubscriptionItem.CurrentPeriodEnd` (was `Subscription.CurrentPeriodEnd`).
- Controller builds a `priceIdToPlanId` reverse lookup dictionary from `BillingConfiguration.Plans` at construction time, used by the mapper for subscription.updated events.
- Payment failure notifications go to all org owners (not just the user who set up billing).
- The `SendPaymentFailureEmailAsync` implementation uses inline HTML (no Razor template) - a Razor template can be added later.
- Audit logging happens automatically via `AppDbContext.SaveChangesAsync` which intercepts entity changes.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # 0 errors
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln    # 437 tests pass (297 unit + 135 integration + 5 architecture)
```

## Files Changed

### Created
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IWebhookEventRepository.cs` - Repository interface for idempotent webhook event tracking
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommand.cs` - MediatR command: EventId, EventType, RawJson
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs` - Handles 5 event types with idempotency check
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/WebhookEventData.cs` - DTO for deserialized webhook event data
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Entities/ProcessedWebhookEvent.cs` - Entity with string PK for Stripe event IDs
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/ProcessedWebhookEventConfiguration.cs` - EF config: table `processed_webhook_events`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/WebhookEventRepository.cs` - IWebhookEventRepository implementation
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/WebhookEventMapper.cs` - Maps Stripe Event objects to WebhookEventData DTOs

### Modified
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Subscription.cs` - Added `UpdatePlan(string planId)` method
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs` - Added `SendPaymentFailureEmailAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` - Added DbSet<ProcessedWebhookEvent> + configuration
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` - Registered IWebhookEventRepository
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/EmailService.cs` - Implemented SendPaymentFailureEmailAsync
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs` - Added plan_id to checkout metadata
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs` - Added POST /billing/webhook endpoint with Stripe signature verification

### EF Migration
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Migrations/*_AddProcessedWebhookEvents.cs`

### Tests Created
- `tests/SaasTemplate.OrgsApi.UnitTests/Application/Billing/Commands/ProcessWebhookEventCommandHandlerTests.cs` - 12 tests: idempotency, all event types, email notification, edge cases

## Tests
### Automated
- 297 unit tests pass (12 new tests added)
- 135 integration tests pass
- 5 architecture tests pass
- Total: 437 tests, 0 failures

## Follow-ups / Backlog
- Task 0045: Pricing page and billing management UI
- Consider adding Razor email template for payment failure notifications
- N+1 query in EntitlementService (acknowledged in cr-0048, acceptable at current scale)

## Checklist
- [x] Task scope matches `docs/tasks/0044-stripe-webhooks.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
