# Task: audit-0021-billing-integration-tests

## Summary
Add integration tests for all 5 BillingController endpoints covering authentication, CSRF validation, input validation, authorization, and happy paths.

## Source
Epic 006 Audit — Action #6 (P1, MEDIUM)

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/tests/.../Api/BillingEndpointTests.cs` | New — 18 integration tests |
| `services/orgs-api/tests/.../Fixtures/ApiWebApplicationFactory.cs` | Added billing config and IPaymentService mock |

## Tests Added
### GET /billing/plans
1. `GetPlans_WhenNotAuthenticated_ShouldReturn200` (AllowAnonymous)
2. `GetPlans_ShouldReturnConfiguredPlans`

### GET /billing/subscription
3. `GetSubscription_WhenNotAuthenticated_ShouldReturn401`
4. `GetSubscription_WithEmptyOrgId_ShouldReturn400`
5. `GetSubscription_WhenNotMember_ShouldReturn403`
6. `GetSubscription_WhenMember_ShouldReturnFreeSubscription`

### POST /billing/checkout
7. `CreateCheckout_WhenNotAuthenticated_ShouldReturn401`
8. `CreateCheckout_WithoutCsrf_ShouldReturn403`
9. `CreateCheckout_WithAbsoluteUrl_ShouldReturnValidationError`
10. `CreateCheckout_WithProtocolRelativeUrl_ShouldReturnValidationError`
11. `CreateCheckout_WhenNotOrgMember_ShouldReturn403`
12. `CreateCheckout_WithValidRequest_ShouldReturn200`

### POST /billing/customer-portal
13. `CreateCustomerPortal_WhenNotAuthenticated_ShouldReturn401`
14. `CreateCustomerPortal_WithoutCsrf_ShouldReturn403`
15. `CreateCustomerPortal_WithAbsoluteReturnUrl_ShouldReturnValidationError`
16. `CreateCustomerPortal_WhenNotOrgMember_ShouldReturn403`
17. `CreateCustomerPortal_WithValidRequest_ShouldReturn200`

### POST /billing/webhook
18. `HandleWebhook_WithInvalidSignature_ShouldReturn400`

## Status
DONE
