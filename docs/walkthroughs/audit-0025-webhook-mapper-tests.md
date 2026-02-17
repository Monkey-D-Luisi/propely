# Walkthrough: audit-0025-webhook-mapper-tests

## Summary
Created unit tests for `WebhookEventMapper`, the static utility that maps raw Stripe `Event` objects to the application-layer `WebhookEventData` record. Tests cover all three supported event types (checkout.session.completed, customer.subscription.updated, customer.subscription.deleted), edge cases (missing metadata, invalid GUIDs, unknown price IDs, wrong object types), and the unknown event type fallback.

## Key Decisions
- **Direct Stripe SDK types**: Tests construct `Stripe.Event`, `Stripe.Checkout.Session`, and `Stripe.Subscription` objects directly since `WebhookEventMapper` is a pure static mapper with no external dependencies.
- **Price-to-plan mapping**: A static dictionary simulates the configuration-driven mapping to verify that subscription events resolve price IDs to plan IDs correctly.
- **Defense-in-depth**: Tests verify that wrong object types (e.g., a Subscription inside a checkout.session.completed event) return empty data rather than throwing.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/tests/.../Infrastructure/Billing/WebhookEventMapperTests.cs` | New — 8 unit tests |

## Tests
All 8 tests pass:
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~WebhookEventMapperTests"
```

## Verification
```bash
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln  # All tests pass
```
