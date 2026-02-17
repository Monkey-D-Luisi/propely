# Audit Action: audit-0016-csrf-billing-endpoints

## Metadata
- ID: audit-0016
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- Priority: P0
- Source:
  - Executive summary: `docs/audits/epic-006-executive-summary.md`
  - Action plan item: "Add CSRF validation to billing endpoints"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0016-csrf-billing-endpoints.md`

## Goal
Add CSRF token validation to BillingController POST endpoints to match AuthController's security posture.

## Context
The audit found that BillingController's POST endpoints (`/billing/checkout` and `/billing/customer-portal`) lacked CSRF validation, while AuthController validated CSRF on all 8 of its state-changing endpoints. Since the app uses cookie-based authentication, these endpoints are vulnerable to cross-site request forgery.

## Scope
### In scope
- Extract `ValidateCsrf()` from AuthController to shared `CsrfValidator` helper
- Add CSRF validation to BillingController `CreateCheckout` and `CreateCustomerPortal`
- Update AuthController to use the shared helper

### Out of scope
- CSRF token generation changes
- Frontend CSRF handling (already sends tokens correctly)

## Requirements
- R1: Both billing POST endpoints must validate CSRF tokens before processing
- R2: AuthController must continue to work identically using the shared helper
- R3: Webhook endpoint must NOT require CSRF (it uses Stripe signature verification)

## Acceptance Criteria
- [x] AC1: `POST /billing/checkout` returns 403 when CSRF token is missing/invalid
- [x] AC2: `POST /billing/customer-portal` returns 403 when CSRF token is missing/invalid
- [x] AC3: AuthController endpoints continue to validate CSRF correctly
- [x] AC4: `POST /billing/webhook` does not require CSRF

## Constraints
- C1: Must not break existing AuthController CSRF behavior

## Implementation Steps
1. Create `Services/CsrfValidator.cs` with static `Validate(HttpRequest)` method
2. Update AuthController to use `CsrfValidator.Validate(Request)` instead of private method
3. Remove private `ValidateCsrf()` and `CsrfTokenMaxAge` from AuthController
4. Add CSRF validation calls to BillingController `CreateCheckout` and `CreateCustomerPortal`

## Testing Plan
- Unit tests: Covered by existing AuthEndpointTests (CSRF rejection)
- Integration tests: To be added in audit-0021
- Manual checks: N/A

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
