# Walkthrough: 0045-pricing-billing-ui

## Task Reference
- Task: `docs/tasks/0045-pricing-billing-ui.md`
- Walkthrough: `docs/walkthroughs/0045-pricing-billing-ui.md`
- Branch/PR: `feat/0045-pricing-billing-ui`
- Date: 2026-02-11

## Summary
Added the pricing page and billing management UI. This includes backend endpoints (GET /billing/plans, POST /billing/customer-portal) with full CQRS implementation, and frontend components (PricingContent, BillingManagement, PlanCard) with page routes, i18n (EN + ES), and navigation integration.

## Context
- Background: Epic 006 (Billing & Subscriptions) tasks 0042-0044 established backend billing infrastructure. This task adds the frontend pricing and billing management UI.
- Problem statement: No UI exists for users to view plans, subscribe, or manage billing.
- Constraints: Must use existing backend patterns (CQRS, Clean Architecture) and frontend patterns (next-intl, custom hooks, Tailwind).

## Decisions & Trade-offs
- Used server-side plan provider (IPlanProvider.GetAllPlans) instead of hardcoding plans in the frontend, keeping plan configuration centralized in appsettings.json.
- Plans endpoint is AllowAnonymous since plan info is public; billing mutations require Owner/Admin role.
- Customer portal endpoint follows same validation pattern as checkout (relative URL, org membership check).
- PricingContent shows org selector dropdown when user has multiple orgs, simplifying the checkout flow.
- BillingManagement component fetches subscription data client-side to keep the page route as a simple server component.

## Implementation Notes
### Backend
- Added `GetAllPlans()` to `IPlanProvider` interface and implemented in `PlanProvider`
- Created `GetPlansQuery` + handler (CQRS read) mapping PlanInfo to PlanDto
- Created `CreateCustomerPortalSessionCommand` + handler with org membership/role/billing validation
- Added `GET /billing/plans` (AllowAnonymous) and `POST /billing/customer-portal` endpoints to BillingController
- Created `CustomerPortalRequest` DTO and `CustomerPortalRequestValidator` (FluentValidation)

### Frontend
- Added `Plan`, `Subscription` Zod schemas to `schemas.ts`
- Created 4 hooks in `billing.ts`: `usePlans`, `useSubscription`, `useCreateCheckout`, `useCreateCustomerPortal`
- Created `PlanCard` component with recommended/current plan styling
- Created `PricingContent` component with plan grid, org selector, login prompt
- Created `BillingManagement` component with subscription status, features, portal button
- Added pricing page route at `/[locale]/pricing`
- Added billing page route at `/[locale]/orgs/[orgId]/billing`
- Added "Pricing" link to `AppHeader`
- Added billing i18n strings to both EN and ES message files

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build
cd apps/web && npm test
```

## Files Changed

### Backend (services/orgs-api)
- `src/.../Application/Billing/Interfaces/IPlanProvider.cs` — Added `GetAllPlans()` method
- `src/.../Infrastructure/Billing/PlanProvider.cs` — Implemented `GetAllPlans()`
- `src/.../Application/Billing/Queries/GetPlans/GetPlansQuery.cs` — New query + DTO
- `src/.../Application/Billing/Queries/GetPlans/GetPlansQueryHandler.cs` — New handler
- `src/.../Application/Billing/Commands/CreateCustomerPortalSession/CreateCustomerPortalSessionCommand.cs` — New command + result
- `src/.../Application/Billing/Commands/CreateCustomerPortalSession/CreateCustomerPortalSessionCommandHandler.cs` — New handler
- `src/.../Api/Controllers/BillingController.cs` — Added plans + customer-portal endpoints
- `src/.../Api/Dtos/CustomerPortalRequest.cs` — New DTO
- `src/.../Api/Validators/CustomerPortalRequestValidator.cs` — New validator
- `tests/.../Billing/Queries/GetPlansQueryHandlerTests.cs` — 2 unit tests
- `tests/.../Billing/Commands/CreateCustomerPortalSessionCommandHandlerTests.cs` — 6 unit tests

### Frontend (apps/web)
- `src/lib/schemas.ts` — Added Plan + Subscription schemas
- `src/hooks/billing.ts` — 4 billing hooks
- `src/components/billing/PlanCard.tsx` — Plan card component
- `src/components/billing/PricingContent.tsx` — Pricing page content
- `src/components/billing/BillingManagement.tsx` — Billing management content
- `src/app/[locale]/pricing/page.tsx` — Pricing route
- `src/app/[locale]/orgs/[orgId]/billing/page.tsx` — Billing route
- `src/components/layout/AppHeader.tsx` — Added Pricing link
- `messages/en.json` — Added billing i18n strings
- `messages/es.json` — Added billing i18n strings (Spanish)

## Tests
### Unit
- `GetPlansQueryHandlerTests` — 2 tests (returns mapped plans, handles empty list)
- `CreateCustomerPortalSessionCommandHandlerTests` — 6 tests (success owner, success admin, org not found, not member, not admin, billing disabled)
- Backend: 445 tests pass (305 unit + 135 integration + 5 architecture)
- Frontend: 293 tests pass (27 test files)

### Integration
- N/A

### Manual
- Pricing page displays plans and checkout flow works
- Billing management shows subscription status
- Customer portal redirect works

## Observability
- Existing logging in StripePaymentService covers new flows

## Security
- ReturnUrl validation prevents open redirect
- Owner/Admin role check on billing mutations
- Plans endpoint is public (no sensitive data)

## Follow-ups / Backlog
- [ ] Add price amounts when Stripe plans are configured with real prices
- [ ] Implement hiding of billing UI when `BILLING_MODE=free` (deferred from this PR)

## Checklist
- [x] Task scope matches `docs/tasks/0045-pricing-billing-ui.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
