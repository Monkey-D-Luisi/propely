# Walkthrough: audit-0035-duplicate-registration-test

## Summary
Added an E2E test that verifies duplicate email registration is properly rejected with the correct error message.

## Key Decisions
- **`registeredPage` fixture**: Reuses the existing fixture to get a user that's already registered, avoiding duplicate setup code.
- **Exact error text assertion**: Uses the exact message from `en.json` (`"An account with this email already exists."`) to catch any regressions in error messaging.
- **10s timeout**: Allows time for the server round-trip (registration attempt → 409 response → error display).

## Files Changed
- `apps/web/e2e/register.spec.ts` — Added "shows error when registering with an already-used email" test case

## Tests
- New test: `shows error when registering with an already-used email`
  1. Uses `registeredPage` to get a pre-registered user
  2. Navigates to `/en/register`
  3. Fills email, password, confirm password with the existing user's credentials
  4. Clicks "Create account"
  5. Asserts "An account with this email already exists." error is visible

## Verification
- TypeScript syntax valid
- Test uses correct error message from `apps/web/messages/en.json:111`
