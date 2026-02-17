# Walkthrough: 0042-stripe-foundation

## Task Reference
- Task: `docs/tasks/0042-stripe-foundation.md`
- Walkthrough: `docs/walkthroughs/0042-stripe-foundation.md`
- Branch/PR: `feat/0042-stripe-foundation`
- Date: `2026-02-10`

## Summary
Set up the foundational Stripe integration: added Stripe.net SDK (v50.3.0), created IPaymentService abstraction with Stripe and NoOp implementations, BILLING_MODE toggle (stripe/free), BillingConfiguration with plan definitions, Subscription domain entity with EF migration, and environment variable configuration.

## Context
- Background: Epic 006 adds billing & subscriptions via Stripe. This is the first task — setting up the SDK, configuration, and foundational abstractions.
- Problem statement: Need the Stripe SDK, API key management, BILLING_MODE toggle, IPaymentService abstraction, Subscription entity, and EF migration.
- Constraints: Clean Architecture, no secrets in code, BILLING_MODE defaults to "free".

## Decisions & Trade-offs
- Used `StripeClient` instance (v50 API) rather than the deprecated global `StripeConfiguration.ApiKey` which was removed in Stripe.net v50. The `StripeClient` is created in the constructor and passed to service classes (`SessionService`, `BillingPortal.SessionService`).
- `SubscriptionStatus` enum stored as string in DB (`HasConversion<string>()`) for readability in queries and to avoid magic numbers.
- `NoOpPaymentService` returns empty strings rather than throwing — callers can check `IsEnabled` before calling, but the no-op gracefully handles the call regardless.
- `BillingConfiguration` uses the Options pattern (`IOptions<BillingConfiguration>`) rather than direct `IConfiguration` access, consistent with other config sections (RabbitMQ, Redis).
- Subscription entity uses unique index on `OrganizationId` (one subscription per org) and `StripeSubscriptionId` (for webhook lookups).

## Implementation Notes
- **Domain Layer**: `Subscription` entity with factory method `Create()`, `UpdateStatus()`, and `Cancel()` methods. `SubscriptionStatus` enum (Active, PastDue, Cancelled, Trialing).
- **Application Layer**: `IPaymentService` interface with `CreateCheckoutSessionAsync`, `CreateCustomerPortalSessionAsync`, and `IsEnabled` property.
- **Infrastructure Layer**: `StripePaymentService` (real Stripe SDK calls), `NoOpPaymentService` (returns empty strings), `BillingConfiguration` (Mode, Stripe keys, Plans list).
- **Persistence**: `SubscriptionConfiguration` maps to `subscriptions` table with snake_case columns. Migration `20260210150322_AddSubscriptions`.
- **DI**: Conditional registration based on `Billing:Mode` config key. Stripe mode validates API key presence at startup.

## Data / Schema / Migrations
- DB changes: New `subscriptions` table with columns: id, organization_id, stripe_customer_id, stripe_subscription_id, plan_id, status, current_period_end, cancelled_at_utc, created_at_utc, updated_at_utc
- Indexes: unique on organization_id, unique on stripe_subscription_id
- Migration: `20260210150322_AddSubscriptions`
- Backward compatibility: Additive only (new table)

## Commands Run
```bash
dotnet add services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure package Stripe.net  # v50.3.0
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors
dotnet ef migrations add AddSubscriptions --project ... --startup-project ...
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln  # 381 pass
```

## Files Changed

### Created
- `Domain/Billing/Subscription.cs` — Entity with Create, UpdateStatus, Cancel
- `Domain/Billing/SubscriptionStatus.cs` — Enum (Active, PastDue, Cancelled, Trialing)
- `Application/Billing/Interfaces/IPaymentService.cs` — Payment service abstraction
- `Infrastructure/Billing/StripePaymentService.cs` — Stripe SDK implementation
- `Infrastructure/Billing/NoOpPaymentService.cs` — No-op for free mode
- `Infrastructure/Billing/BillingConfiguration.cs` — Options classes (Mode, Stripe, Plans)
- `Infrastructure/Persistence/Configurations/SubscriptionConfiguration.cs` — EF configuration
- `Infrastructure/Migrations/20260210150322_AddSubscriptions.cs` — Migration
- `Infrastructure/Migrations/20260210150322_AddSubscriptions.Designer.cs` — Migration designer
- `tests/UnitTests/Domain/Billing/SubscriptionTests.cs` — 3 tests
- `tests/UnitTests/Infrastructure/Billing/NoOpPaymentServiceTests.cs` — 3 tests

### Modified
- `Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` — Added Stripe.net v50.3.0
- `Infrastructure/DependencyInjection.cs` — Billing DI registration with mode toggle
- `Infrastructure/Persistence/AppDbContext.cs` — Added Subscription DbSet and configuration
- `Infrastructure/Migrations/AppDbContextModelSnapshot.cs` — Updated snapshot
- `Api/appsettings.json` — Added Billing section with plans
- `.env` — Added ORGSAPI_Billing__* variables

## Tests
### Unit
- `SubscriptionTests` (3 tests): Create sets all properties, UpdateStatus updates status/period, Cancel sets cancelled status
- `NoOpPaymentServiceTests` (3 tests): IsEnabled returns false, CreateCheckoutSession returns empty, CreateCustomerPortal returns empty
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln`
- Result: 381 tests pass (241 unit + 135 integration + 5 architecture)

## Observability
- Logs: Info-level logs in StripePaymentService for checkout/portal session creation. Debug-level logs in NoOpPaymentService when billing is disabled.

## Security
- API keys via environment variables only, never in code
- BILLING_MODE=stripe without API key fails at startup with clear error message
- Empty StripePriceId values in appsettings.json (placeholders only)

## Follow-ups / Backlog
- Task 0043: Wire StripePaymentService to Subscription entity for customer ID lookups
- Task 0044: Add webhook handler with signature verification using WebhookSecret

## Checklist
- [x] Task scope matches `docs/tasks/0042-stripe-foundation.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
