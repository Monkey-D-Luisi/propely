# Task: 0045 - Pricing Page and Billing Management UI

## Metadata
- ID: 0045
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #197
- Epic: `docs/backlog/epic-006-billing.md`
- Old Issue: #28
- Dependencies: 0043 (subscription lifecycle), 0044 (webhooks)

## Goal
Create the frontend pricing page and billing management UI so users can subscribe to plans and manage their billing.

## Context
Tasks 0042-0044 set up the backend billing infrastructure. This task creates the frontend: a pricing page showing available plans with a checkout button, and a billing management page that redirects to Stripe Customer Portal for upgrades/downgrades/cancellation.

## Scope
### In scope
- Pricing page (`/pricing`) showing plan comparison
- Checkout button per plan (redirects to Stripe Checkout)
- Billing management page (`/orgs/{orgId}/billing`) for subscribed orgs
- Current plan display with status
- "Manage Billing" button (redirects to Stripe Customer Portal)
- Payment failure warning banner
- i18n strings (EN + ES)
- Hide billing UI when `BILLING_MODE=free`

### Out of scope
- Custom payment form (we use Stripe Checkout)
- Invoice history (handled by Stripe Customer Portal)
- Usage-based billing

## Requirements
- R1: Pricing page shows all plans with features and prices
- R2: Checkout button initiates Stripe Checkout for selected plan
- R3: Billing page shows current subscription status
- R4: "Manage Billing" button opens Stripe Customer Portal
- R5: Payment failure shows warning to org owners
- R6: All strings internationalized
- R7: When `BILLING_MODE=free`, billing UI is hidden

## Acceptance Criteria
- AC1: Pricing page renders with plan cards
- AC2: Clicking "Subscribe" redirects to Stripe Checkout
- AC3: Billing page shows current plan and status
- AC4: "Manage Billing" button works (Stripe portal opens)
- AC5: Billing UI hidden when BILLING_MODE=free
- AC6: `npm run build` and `npm test` pass
- AC7: i18n strings in EN + ES

## Constraints (non-negotiable)
- Use react-hook-form + zod for any forms
- i18n for all strings
- Responsive design
- Update walkthrough

## Implementation Steps

### Frontend (apps/web)

1. **Create pricing page** (`apps/web/src/app/[locale]/pricing/page.tsx`)
   - Fetch plans from API or use hardcoded plan config
   - Display plan cards with: name, price, features list, CTA button
   - Current plan highlighted if user is subscribed
   - "Subscribe" button calls `POST /billing/checkout` -> redirect to Stripe URL

2. **Create billing page** (`apps/web/src/app/[locale]/orgs/[orgId]/billing/page.tsx`)
   - Show current subscription status (plan name, status, renewal date)
   - "Manage Billing" button -> calls `POST /billing/portal` -> redirect to Stripe portal URL
   - Payment failure warning if status is PastDue

3. **Create useBilling hook** (`apps/web/src/hooks/billing.ts`)
   - `useSubscription(orgId)`: GET /billing/subscription
   - `useCheckout(orgId, planId)`: POST /billing/checkout -> returns URL
   - `useBillingPortal(orgId)`: POST /billing/portal -> returns URL

4. **Create PlanCard component** (`apps/web/src/components/billing/PlanCard.tsx`)
   - Plan name, price, feature list, action button
   - Highlight "current plan" or "popular" plan

5. **Create BillingStatusCard component** (`apps/web/src/components/billing/BillingStatusCard.tsx`)
   - Current plan, status badge, renewal date, manage button

6. **Add billing link to org navigation**
   - Add "Billing" link in org settings or sidebar (owners only)

7. **Conditionally show/hide billing UI**
   - Use feature flag or API response to detect BILLING_MODE
   - `GET /billing/config` endpoint returns `{ enabled: boolean, publishableKey: string }`

8. **Add Zod schemas** (`apps/web/src/lib/schemas.ts`)
   - `SubscriptionResponseSchema`, `CheckoutResponseSchema`, `BillingConfigSchema`

9. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
   - `billing.pricing`, `billing.subscribe`, `billing.currentPlan`, `billing.manageBilling`, `billing.paymentFailed`, `billing.free`, `billing.pro`, `billing.enterprise`, `billing.perMonth`, `billing.features`

### Backend additions

10. **Add billing config endpoint** to BillingController
    - `GET /billing/config` returns `{ enabled, plans }` (no secret keys)

11. **Add portal session endpoint** to BillingController
    - `POST /billing/portal` -> `IPaymentService.CreateCustomerPortalSessionAsync` -> return URL

### Testing

12. **Frontend tests**
    - Pricing page renders plans
    - Billing page shows subscription status
    - Billing UI hidden when billing disabled

## Files to Create / Modify

### Create
- `apps/web/src/app/[locale]/pricing/page.tsx`
- `apps/web/src/app/[locale]/orgs/[orgId]/billing/page.tsx`
- `apps/web/src/hooks/billing.ts`
- `apps/web/src/components/billing/PlanCard.tsx`
- `apps/web/src/components/billing/BillingStatusCard.tsx`
- `docs/walkthroughs/0045-pricing-billing-ui.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs`
- `apps/web/src/lib/schemas.ts`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Frontend tests: Component rendering, conditional display
- Integration tests: API calls for checkout and portal
- Manual: Full Stripe test mode checkout flow

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] i18n strings added (EN + ES)
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
