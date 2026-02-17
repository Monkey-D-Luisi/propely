# Walkthrough: audit-0034-consolidate-fixtures

## Summary
Made `authenticatedPage` fixture delegate to `registeredPage` instead of duplicating the `registerUser()` call. Both fixture names are preserved for semantic clarity.

## Key Decisions
- **Delegation over removal**: Both fixture names are kept because they communicate different test intent. `registeredPage` = "user exists, may test login". `authenticatedPage` = "user is logged in, ready for app actions".
- **Playwright fixture chaining**: `authenticatedPage` depends on `registeredPage`, which Playwright resolves by reusing the same page + user instance.

## Files Changed
- `apps/web/e2e/fixtures/auth.fixture.ts` — `authenticatedPage` now delegates to `registeredPage`

## Tests
- All E2E tests use the same fixture behavior (no functional change)

## Verification
- TypeScript syntax valid
- Fixture dependency chain is correct (Playwright resolves `registeredPage` before `authenticatedPage`)
