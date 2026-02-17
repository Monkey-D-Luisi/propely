# Walkthrough: audit-0023-frontend-redirect-validation

## Summary
Added `isAllowedRedirect()` helper to `billing.ts` that validates redirect URLs start with allowed Stripe domain prefixes before `window.location.href` assignment.

## Files Changed
| File | Change |
|------|--------|
| `apps/web/src/hooks/billing.ts` | Added `isAllowedRedirect()` check before redirecting in `useCreateCheckout` and `useCreateCustomerPortal` |
