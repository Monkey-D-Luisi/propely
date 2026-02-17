# Walkthrough: audit-0021-billing-integration-tests

## Summary
Created 18 integration tests for BillingController covering all 5 endpoints. Updated ApiWebApplicationFactory to include billing plan configuration and a mock IPaymentService (with IsEnabled=true and test URLs).

## Key Decisions
- **Mock IPaymentService in factory**: Registered a NSubstitute mock for IPaymentService with `IsEnabled=true` and pre-configured return URLs, allowing checkout and portal success-path tests without Stripe SDK.
- **Billing plans in-memory config**: Added free and pro plan configurations to the factory's in-memory collection so PlanProvider returns real plans.
- **No separate test factory**: Extended the existing `ApiWebApplicationFactory` rather than creating a billing-specific one. The additions are harmless to existing tests.
- **Webhook test**: Tests invalid signature rejection only. Valid webhook processing requires Stripe signature generation which is better covered in handler unit tests.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/tests/.../Api/BillingEndpointTests.cs` | New — 18 integration tests |
| `services/orgs-api/tests/.../Fixtures/ApiWebApplicationFactory.cs` | Added billing config, webhook secret, and mock IPaymentService |

## Tests
All 18 tests pass. Full suite: 498 tests (5 arch + 340 unit + 153 integration).
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Verification
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln  # All 498 tests pass
```
