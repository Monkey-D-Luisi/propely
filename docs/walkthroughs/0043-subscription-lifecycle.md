# Walkthrough: 0043-subscription-lifecycle

## Task Reference
- Task: `docs/tasks/0043-subscription-lifecycle.md`
- Walkthrough: `docs/walkthroughs/0043-subscription-lifecycle.md`
- Branch/PR: `feat/0043-subscription-lifecycle`
- Date: 2026-02-11

## Summary
Implemented the subscription lifecycle: checkout session creation, subscription queries, plan provider abstraction, and entitlement enforcement. Organizations can now be gated by plan limits (max members, max organizations) with automatic bypass when `BILLING_MODE=free`.

## Context
- Background: Task 0042 established Stripe SDK integration, `Subscription` entity, `IPaymentService` with Stripe/NoOp implementations, and `BillingConfiguration`. This task builds on that foundation.
- Problem statement: No way to create checkout sessions, query subscriptions, or enforce plan-based limits on organizations and members.
- Constraints: Clean Architecture layers respected, Stripe Checkout (hosted page) only, no webhook handling (task 0044).

## Decisions & Trade-offs

- **Decision:** Used task-specific entitlement methods (`CanCreateOrganizationAsync`, `CanAddMemberAsync`) instead of generic `HasFeatureAsync(orgId, feature)`
  - Options considered: Generic feature string lookup, task-specific typed methods
  - Why this choice: More explicit, easier to test, avoids stringly-typed feature keys. The task spec suggested `HasFeature` but typed methods are more maintainable.
  - Consequences / risks: Adding new entitlement checks requires adding new methods to the interface.

- **Decision:** Added `MaxMembers` and `MaxOrganizations` as typed properties on `PlanConfiguration` (0 = unlimited)
  - Options considered: Parsing feature strings like "5 members", typed properties, separate limits config
  - Why this choice: Typed properties are type-safe and avoid parsing. 0 as unlimited is a common convention.

- **Decision:** Created `IPlanProvider` abstraction in Application layer
  - Options considered: Injecting `IOptions<BillingConfiguration>` directly, dedicated interface
  - Why this choice: `BillingConfiguration` lives in Infrastructure. Clean Architecture requires Application layer to define its own interfaces. `IPlanProvider` maps `PlanConfiguration` to `PlanInfo` records.

- **Decision:** Implemented `CreateCustomerPortalSessionAsync` in `StripePaymentService` with customer ID lookup
  - Options considered: Leave as NotImplementedException, implement with subscription lookup
  - Why this choice: Unblocks task 0045 (billing management UI). Looks up the Stripe customer ID from the org's subscription.

## Implementation Notes
- `EntitlementService.CanCreateOrganizationAsync` finds the highest org limit across all of a user's subscriptions (user may belong to multiple orgs with different plans).
- `EntitlementService.CanAddMemberAsync` checks the specific org's subscription plan for member limits.
- `GetSubscriptionQueryHandler` falls back to free plan when no active/trialing subscription exists, or when the plan ID is unknown.
- `PlanProvider.GetFreePlan()` returns hardcoded defaults (5 members, 1 org) if no "free" plan is configured.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # 0 errors
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln    # 424 tests pass (284 unit + 135 integration + 5 architecture)
```

## Files Changed

### Created
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/ISubscriptionRepository.cs` - Repository interface for Subscription entity
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IPlanProvider.cs` - Plan lookup abstraction with PlanInfo record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IEntitlementService.cs` - Entitlement checking interface
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCheckoutSession/CreateCheckoutSessionCommand.cs` - Command + result records
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCheckoutSession/CreateCheckoutSessionCommandHandler.cs` - Verifies org/membership/billing, delegates to IPaymentService
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetSubscription/GetSubscriptionQuery.cs` - Query + SubscriptionDto records
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetSubscription/GetSubscriptionQueryHandler.cs` - Returns active subscription or free plan fallback
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/SubscriptionRepository.cs` - EF Core implementation
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/PlanProvider.cs` - IPlanProvider implementation using IOptions<BillingConfiguration>
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/EntitlementService.cs` - Checks billing mode, subscription plan, and limits
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs` - POST /billing/checkout, GET /billing/subscription
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/CheckoutRequest.cs` - Request DTO
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/CheckoutRequestValidator.cs` - FluentValidation for CheckoutRequest

### Modified
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/BillingConfiguration.cs` - Added MaxMembers, MaxOrganizations to PlanConfiguration
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs` - Implemented CreateCustomerPortalSessionAsync with customer ID lookup
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs` - Registered ISubscriptionRepository, IPlanProvider, IEntitlementService
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommandHandler.cs` - Added IEntitlementService org limit check
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Organizations/Commands/CreateInvitation/CreateInvitationCommandHandler.cs` - Added IEntitlementService member limit check
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` - Added MaxMembers/MaxOrganizations to plan entries

### Tests Created
- `tests/SaasTemplate.OrgsApi.UnitTests/Application/Billing/Commands/CreateCheckoutSessionCommandHandlerTests.cs` - 6 tests: auth, billing enabled, happy path
- `tests/SaasTemplate.OrgsApi.UnitTests/Application/Billing/Queries/GetSubscriptionQueryHandlerTests.cs` - 6 tests: no sub, active, trialing, cancelled, unknown plan, auth
- `tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Billing/EntitlementServiceTests.cs` - 11 tests: billing disabled bypass, under/at limit, unlimited plan, cancelled fallback
- `tests/SaasTemplate.OrgsApi.UnitTests/Infrastructure/Billing/PlanProviderTests.cs` - 7 tests: billing enabled flag, plan lookup, free plan defaults

### Tests Updated
- `tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/CreateOrganizationCommandHandlerTests.cs` - Added IEntitlementService mock + org limit test
- `tests/SaasTemplate.OrgsApi.UnitTests/Application/Organizations/Commands/CreateInvitationCommandHandlerTests.cs` - Added IEntitlementService mock + member limit test

## Tests
### Automated
- 284 unit tests pass (30 new tests added)
- 135 integration tests pass
- 5 architecture tests pass
- Total: 424 tests, 0 failures

## Follow-ups / Backlog
- Task 0044: Stripe webhook handler to update subscription status on events
- Task 0045: Pricing page and billing management UI (CreateCustomerPortalSessionAsync is now implemented)

## Checklist
- [x] Task scope matches `docs/tasks/0043-subscription-lifecycle.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
