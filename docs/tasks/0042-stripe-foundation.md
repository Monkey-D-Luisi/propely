# Task: 0042 - Stripe Integration Foundation (SDK, API Keys, Config)

## Metadata
- ID: 0042
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #194
- Epic: `docs/backlog/epic-006-billing.md`
- Old Issue: #24

## Goal
Set up the foundational Stripe integration: SDK, configuration, API key management, and the IPaymentService abstraction with a BILLING_MODE toggle.

## Context
This SaaS template needs billing for monetization. Stripe is the industry standard with excellent .NET support. This task sets up the foundation; subsequent tasks (0043-0046) build on it for subscriptions, webhooks, and UI. A `BILLING_MODE` toggle allows running the app without Stripe during development.

## Scope
### In scope
- Add `Stripe.net` NuGet package to orgs-api
- Create `IPaymentService` interface in Application layer
- Create `StripePaymentService` implementation in Infrastructure
- Create `NoOpPaymentService` for when billing is disabled
- `BILLING_MODE` toggle: "stripe" (real) or "free" (no billing)
- Configure Stripe API keys via environment variables
- Define subscription plans in `appsettings.json`
- Create `Subscription` domain entity (basic, for future tasks)
- EF migration for subscriptions table

### Out of scope
- Checkout flow (task 0043)
- Webhook handling (task 0044)
- Pricing page (task 0045)
- One-time payments (task 0046)

## Requirements
- R1: `Stripe.net` SDK installed and configured
- R2: API keys stored in environment variables (never in code)
- R3: `BILLING_MODE=free` disables all billing (NoOp implementation)
- R4: `BILLING_MODE=stripe` enables real Stripe integration
- R5: Subscription plans configurable in appsettings
- R6: Subscription entity linked to Organization

## Acceptance Criteria
- AC1: `StripePaymentService` can communicate with Stripe test API
- AC2: `BILLING_MODE=free` results in NoOpPaymentService being registered
- AC3: `BILLING_MODE=stripe` without API keys fails at startup with clear error
- AC4: Subscription entity and table exist in database
- AC5: `dotnet build` passes
- AC6: `dotnet test` passes
- AC7: Migration applies cleanly

## Constraints (non-negotiable)
- Clean Architecture layers respected
- No Stripe API keys committed
- BILLING_MODE default is "free" (safe for development)
- Update walkthrough

## Implementation Steps

1. **Add Stripe.net NuGet** to `SaasTemplate.OrgsApi.Infrastructure`
   - `dotnet add services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure package Stripe.net`

2. **Create IPaymentService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IPaymentService.cs`)
   ```csharp
   public interface IPaymentService
   {
       Task<string> CreateCheckoutSessionAsync(Guid orgId, string planId, string successUrl, string cancelUrl, CancellationToken ct);
       Task<string> CreateCustomerPortalSessionAsync(Guid orgId, string returnUrl, CancellationToken ct);
       bool IsEnabled { get; }
   }
   ```

3. **Create StripePaymentService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs`)
   - Initialize Stripe client with API key from config
   - Implement interface methods using Stripe SDK
   - `IsEnabled => true`

4. **Create NoOpPaymentService** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/NoOpPaymentService.cs`)
   - All methods return success/no-op results
   - `IsEnabled => false`

5. **Create BillingConfiguration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/BillingConfiguration.cs`)
   - `Mode`: "stripe" | "free"
   - `Stripe__SecretKey`, `Stripe__PublishableKey`, `Stripe__WebhookSecret`
   - `Plans`: list of plan definitions (Id, Name, StripePriceId, Features)

6. **Create Subscription entity** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Subscription.cs`)
   - `Id` (Guid), `OrgId` (Guid), `StripeCustomerId` (string), `StripeSubscriptionId` (string)
   - `PlanId` (string), `Status` (enum: Active, PastDue, Cancelled, Trialing)
   - `CurrentPeriodEnd` (DateTime), `CancelledAtUtc` (DateTime?)
   - Factory method `Create()`

7. **Create SubscriptionStatus enum** (`services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/SubscriptionStatus.cs`)

8. **Create EF configuration** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/SubscriptionConfiguration.cs`)
   - Map to `subscriptions` table
   - Index on OrgId (one subscription per org)
   - Index on StripeSubscriptionId (for webhook lookups)

9. **Create migration**
   - `dotnet ef migrations add AddSubscriptions --project services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure --startup-project services/orgs-api/src/SaasTemplate.OrgsApi.Api`

10. **Register in DI** (`services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`)
    ```csharp
    var billingMode = configuration.GetValue<string>("Billing:Mode") ?? "free";
    if (billingMode == "stripe")
        services.AddScoped<IPaymentService, StripePaymentService>();
    else
        services.AddScoped<IPaymentService, NoOpPaymentService>();
    ```

11. **Add env vars** to `.env`
    - `ORGSAPI_Billing__Mode=free`
    - `ORGSAPI_Billing__Stripe__SecretKey=`
    - `ORGSAPI_Billing__Stripe__PublishableKey=`
    - `ORGSAPI_Billing__Stripe__WebhookSecret=`

12. **Add plan configuration** to `appsettings.json`
    ```json
    "Billing": {
      "Plans": [
        { "Id": "free", "Name": "Free", "StripePriceId": "", "Features": ["5 members", "1 org"] },
        { "Id": "pro", "Name": "Pro", "StripePriceId": "price_xxx", "Features": ["Unlimited members", "5 orgs"] },
        { "Id": "enterprise", "Name": "Enterprise", "StripePriceId": "price_yyy", "Features": ["Unlimited everything"] }
      ]
    }
    ```

### Testing

13. **Unit tests**
    - NoOpPaymentService: all methods return expected defaults
    - DI registration: correct service based on BILLING_MODE
    - Subscription entity: Create factory method

## Files to Create / Modify

### Create
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IPaymentService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Subscription.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/SubscriptionStatus.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/NoOpPaymentService.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/BillingConfiguration.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/SubscriptionConfiguration.cs`
- `docs/walkthroughs/0042-stripe-foundation.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj` (add Stripe.net)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs` (add Subscription DbSet)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` (add billing config)
- `.env` (add billing env vars)

## Testing Plan
- Unit tests: NoOpPaymentService, DI registration, Subscription entity
- Integration tests: Migration applies, Subscription CRUD
- Manual: Verify BILLING_MODE toggle works

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Migration applies cleanly
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
