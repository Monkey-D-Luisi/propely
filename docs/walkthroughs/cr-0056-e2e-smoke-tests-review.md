# Walkthrough: CR-0056 — E2E Smoke Tests PR Review

- **Task**: `docs/tasks/cr-0056-e2e-smoke-tests-review.md`
- **PR**: [#256](https://github.com/Monkey-D-Luisi/saas-template/pull/256)

## What Changed

1. **Extracted `registerUser` helper** — Deduplicated the identical registration logic in `registeredPage` and `authenticatedPage` fixtures into a shared function.

2. **Added random suffix to org names** — `create-org.spec.ts` and `invite-member.spec.ts` now generate org names with `Date.now()` + random suffix, matching the pattern used in `testUser` fixture.

3. **Fixed web health check URL** — Changed CI health check from `/` to `/en` for a direct 200 response instead of relying on i18n middleware redirect behavior.

4. **Extracted `createOrg` helper** — Moved org creation logic from `invite-member.spec.ts` into a shared `e2e/helpers/org.helper.ts` file, reused in both org-related specs.

5. **Excluded `e2e/` from ESLint** — Added `e2e/**` to `globalIgnores` in `eslint.config.mjs`. Playwright's `use()` callback was triggering `react-hooks/rules-of-hooks` false positives in CI.

6. **Reordered E2E CI steps** — Moved Node.js setup, `npm ci`, and Playwright browser install before Docker Compose startup. Docker's bind mount creates `node_modules` as root, blocking subsequent `npm ci` with EACCES.

## Rejected
- DoD/status comments (3 comments) — consistent with established project pattern from tasks 0050/0051.

## Validation
- `npm run build` passes
- `npm test` passes (305/305)
