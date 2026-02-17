# Walkthrough: audit-0028-login-clear-storage

## Summary
Added `localStorage.clear()` and `sessionStorage.clear()` to the login E2E test to ensure complete session isolation before testing the login flow.

## Key Decisions
- **`page.evaluate()` for storage clearing**: Playwright doesn't have a built-in method for clearing web storage, so `page.evaluate()` is the standard approach.
- **Order**: Clear cookies first (via Playwright API), then clear web storage (via page context), then navigate to `about:blank` to kill client-side JS.

## Files Changed
- `apps/web/e2e/login.spec.ts` — Added localStorage and sessionStorage clearing

## Tests
- Existing login tests unaffected (clearing empty storage is a no-op)

## Verification
- TypeScript syntax valid
- Login test behavior unchanged (storage was already empty in practice)
