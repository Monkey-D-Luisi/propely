# Walkthrough: audit-0020-architecture-violation

## Summary
Moved `BillingConfiguration`, `StripeSettings`, and `PlanConfiguration` POCOs from `Infrastructure.Billing` to `Application.Billing`. Updated all references in Infrastructure services, DI registration, API controller, and tests.

## Key Decisions
- **Application layer**: Config POCOs are pure data classes with no framework dependencies — they belong where the interface contracts are defined.
- **WebhookEventMapper stays in Infrastructure**: It references Stripe SDK types, so `BillingController` still imports `Infrastructure.Billing` for `WebhookEventMapper`. This is a remaining minor coupling that can be addressed by moving webhook mapping into the command handler.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/src/.../Application/Billing/BillingConfiguration.cs` | New — config POCOs in Application layer |
| `services/orgs-api/src/.../Infrastructure/Billing/BillingConfiguration.cs` | Deleted |
| `services/orgs-api/src/.../Api/Controllers/BillingController.cs` | Changed import from `Infrastructure.Billing` to `Application.Billing` |
| `services/orgs-api/src/.../Infrastructure/Billing/PlanProvider.cs` | Added `using Application.Billing` |
| `services/orgs-api/src/.../Infrastructure/Billing/StripePaymentService.cs` | Added `using Application.Billing` |
| `services/orgs-api/src/.../Infrastructure/DependencyInjection.cs` | Added `using Application.Billing` |
| `services/orgs-api/tests/.../Infrastructure/Billing/PlanProviderTests.cs` | Added `using Application.Billing` |

## Verification
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # 0 errors
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln    # 445 passed, 0 failed
```
