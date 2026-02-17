# Task: audit-0025-webhook-mapper-tests

## Summary
Add unit tests for `WebhookEventMapper` covering all Stripe event types, null/missing field handling, and price-to-plan mapping.

## Source
Epic 006 Audit — Action #10 (P2, LOW)

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/tests/.../Infrastructure/Billing/WebhookEventMapperTests.cs` | New — 8 unit tests |

## Tests Added
1. `MapToEventData_CheckoutSessionCompleted_ShouldMapFields`
2. `MapToEventData_CheckoutSessionCompleted_WithMissingMetadata_ShouldReturnNulls`
3. `MapToEventData_CheckoutSessionCompleted_WithInvalidOrgId_ShouldReturnNullOrgId`
4. `MapToEventData_SubscriptionUpdated_ShouldMapFieldsWithPriceMapping`
5. `MapToEventData_SubscriptionUpdated_WithUnknownPriceId_ShouldReturnNullPlanId`
6. `MapToEventData_SubscriptionDeleted_ShouldMapFields`
7. `MapToEventData_UnknownEventType_ShouldReturnEmptyData`
8. `MapToEventData_CheckoutWithWrongObjectType_ShouldReturnEmptyData`

## Status
DONE
