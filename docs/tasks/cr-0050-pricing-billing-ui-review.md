# Code Review: cr-0050-pricing-billing-ui-review

## Metadata
- PR: #247
- Branch: feat/0045-pricing-billing-ui
- Target: main
- Reviewers: Copilot, Gemini

## Changed Files
- apps/web/messages/en.json
- apps/web/messages/es.json
- apps/web/src/app/[locale]/orgs/[orgId]/billing/page.tsx
- apps/web/src/app/[locale]/pricing/page.tsx
- apps/web/src/components/billing/BillingManagement.tsx
- apps/web/src/components/billing/PlanCard.tsx
- apps/web/src/components/billing/PricingContent.tsx
- apps/web/src/components/layout/AppHeader.tsx
- apps/web/src/hooks/billing.ts
- apps/web/src/lib/schemas.ts
- docs/backlog/epic-006-billing.md
- docs/tasks/0045-pricing-billing-ui.md
- docs/walkthroughs/0045-pricing-billing-ui.md
- services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/CustomerPortalRequest.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/CustomerPortalRequestValidator.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCustomerPortalSession/CreateCustomerPortalSessionCommand.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/CreateCustomerPortalSession/CreateCustomerPortalSessionCommandHandler.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IPlanProvider.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetPlans/GetPlansQuery.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Queries/GetPlans/GetPlansQueryHandler.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/PlanProvider.cs
- services/orgs-api/tests/.../CreateCustomerPortalSessionCommandHandlerTests.cs
- services/orgs-api/tests/.../GetPlansQueryHandlerTests.cs

## Comment Resolution Plan

| # | Source | File | Classification | Action |
|---|--------|------|---------------|--------|
| C1 | Copilot | BillingManagement.tsx:20 | SHOULD_FIX | Remove unused `useCurrentUser()` call |
| C2 | Copilot | PricingContent.tsx:22 | SHOULD_FIX | Remove unused `selectedOrg` state |
| C3 | Copilot | 0045-pricing-billing-ui.md:7 | OUT_OF_SCOPE | BILLING_MODE=free hiding was a scope stretch; task core is complete |
| C4 | Copilot | epic-006-billing.md:60 | OUT_OF_SCOPE | Same as C3 — status reflects core implementation done |
| C5 | Copilot | walkthrough:102 | SHOULD_FIX | Add BILLING_MODE=free hiding to Follow-ups |
| C6 | Copilot | BillingManagement.tsx:47 | MUST_FIX | Fix status key: API returns "pastdue", frontend expects "past_due" |
| C7 | Copilot | BillingManagement.tsx:58 | MUST_FIX | Same status mismatch in getStatusColor |

## Behavioral Parity Checks
- [x] Redirect parity checked: N/A — no auth/redirect flows in billing UI
- [x] Locale source correctness checked: locale derived from `useLocale()` hook (route locale)
- [x] API/UI contract parity checked: status key mismatch found (C6/C7) — fixing
- [x] Test parity checked: backend has unit tests; frontend has build verification
