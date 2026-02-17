# Task: 0043 - Subscription Plans and Lifecycle

## Metadata
- ID: 0043
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #195
- Epic: `docs/backlog/epic-006-billing.md`
- Old Issue: #25
- Dependencies: 0042 (Stripe foundation)

## Goal
Implement the subscription lifecycle: creating checkout sessions, handling subscription status changes, linking subscriptions to organizations, and enforcing entitlements.

## Context
Task 0042 establishes the Stripe SDK, configuration, and Subscription entity. This task implements the actual subscription flow: users select a plan, go through Stripe Checkout, and the resulting subscription is linked to their organization. Entitlements (what features/limits each plan grants) are enforced application-wide.

## Scope
### In scope
- Create Stripe Checkout session endpoint (redirect user to Stripe)
- Link Stripe customer to Organization
- Store subscription status after checkout completion
- Entitlement checking: `IEntitlementService.HasFeature(orgId, feature)`
- Plan limits: max members, max orgs per user, feature gates
- Update Subscription entity on status changes
- `GET /billing/subscription` - get current org's subscription
- `POST /billing/checkout` - create checkout session for a plan

### Out of scope
- Webhook handling (task 0044 - this task handles the initial checkout redirect only)
- Pricing page UI (task 0045)
- Upgrade/downgrade (handled via Stripe Customer Portal in task 0045)
- One-time payments (task 0046)

## Requirements
- R1: User can start a checkout session for a specific plan
- R2: Checkout session links to the user's current organization
- R3: After checkout, subscription is stored and linked to organization
- R4: `IEntitlementService` checks plan features at application boundaries
- R5: Free plan (no Stripe subscription) has limited features
- R6: When `BILLING_MODE=free`, all features are unlocked (no limits)

## Acceptance Criteria
- AC1: `POST /billing/checkout` returns a Stripe Checkout URL
- AC2: `GET /billing/subscription` returns current subscription status
- AC3: Organization without subscription is on "free" plan
- AC4: Entitlement checks correctly restrict based on plan
- AC5: `BILLING_MODE=free` bypasses all entitlement checks
- AC6: `dotnet build` and `dotnet test` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected
- Stripe Checkout (hosted page), not custom payment form
- English-only repo content
- Update walkthrough

## Implementation Steps

1. **Create ISubscriptionRepository** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/ISubscriptionRepository.cs`)
   - `GetByOrgIdAsync()`, `GetByStripeSubscriptionIdAsync()`, `AddAsync()`, `UpdateAsync()`

2. **Implement SubscriptionRepository** in Infrastructure

3. **Create CreateCheckoutSessionCommand** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCheckoutSession/`)
   - Properties: `OrgId`, `PlanId`, `SuccessUrl`, `CancelUrl`
   - Handler: look up plan config -> call IPaymentService.CreateCheckoutSessionAsync -> return checkout URL

4. **Create GetSubscriptionQuery** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetSubscription/`)
   - Property: `OrgId`
   - Handler: get subscription from repo -> return DTO with plan, status, period end

5. **Create IEntitlementService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IEntitlementService.cs`)
   ```csharp
   public interface IEntitlementService
   {
       Task<bool> HasFeatureAsync(Guid orgId, string feature, CancellationToken ct);
       Task<int> GetLimitAsync(Guid orgId, string limitName, CancellationToken ct);
   }
   ```

6. **Implement EntitlementService** in Infrastructure
   - Check org's subscription -> get plan -> check plan features/limits
   - When `BILLING_MODE=free`, return true/unlimited for everything

7. **Create BillingController** (`services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs`)
   - `POST /billing/checkout` -> CreateCheckoutSessionCommand
   - `GET /billing/subscription` -> GetSubscriptionQuery

8. **Add entitlement checks** to existing commands where limits apply
   - Example: CreateOrganizationCommand -> check max orgs for user's plan
   - Example: InviteMemberCommand -> check max members for org's plan

### Testing

9. **Unit tests**
   - CreateCheckoutSessionCommandHandler
   - GetSubscriptionQueryHandler
   - EntitlementService with different plans
   - EntitlementService with BILLING_MODE=free

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/ISubscriptionRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IEntitlementService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCheckoutSession/`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetSubscription/`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/EntitlementService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/SubscriptionRepository.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs`
- `docs/walkthroughs/0043-subscription-lifecycle.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs`

## Testing Plan
- Unit tests: Command/query handlers, EntitlementService
- Integration tests: Checkout session creation, subscription retrieval
- Manual: Full checkout flow with Stripe test mode

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
