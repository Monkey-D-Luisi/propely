# Walkthrough: audit-0016-csrf-billing-endpoints

## Summary
Extracted CSRF validation logic from AuthController's private method into a shared `CsrfValidator` static helper class. Added CSRF validation to BillingController's `CreateCheckout` and `CreateCustomerPortal` POST endpoints. Updated AuthController to use the shared helper, eliminating code duplication.

## Key Decisions
- **Static helper over base class**: Chose `CsrfValidator.Validate(HttpRequest)` static method over a base controller class to avoid changing the inheritance hierarchy.
- **Validation before auth check**: CSRF validation runs before `GetUserId()` in billing endpoints, consistent with AuthController's pattern where CSRF is checked first.
- **Webhook excluded**: `HandleWebhook` endpoint correctly does not require CSRF — it uses Stripe signature verification instead.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/src/.../Api/Services/CsrfValidator.cs` | New shared CSRF validation helper |
| `services/orgs-api/src/.../Api/Controllers/BillingController.cs` | Added CSRF checks to checkout and customer-portal endpoints |
| `services/orgs-api/src/.../Api/Controllers/AuthController.cs` | Replaced private `ValidateCsrf()` with `CsrfValidator.Validate(Request)`, removed private method |

## Tests
- Existing `AuthEndpointTests` continue to pass (CSRF rejection scenarios).
- Integration tests for BillingController CSRF to be added in audit-0021.

## Verification
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors
```
