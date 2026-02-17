# Walkthrough: 0052 - E2E Smoke Tests (Playwright)

## Task Reference
- Task: `docs/tasks/0052-e2e-smoke-tests.md`
- Walkthrough: `docs/walkthroughs/0052-e2e-smoke-tests.md`
- Branch/PR: `feat/0052-e2e-smoke-tests`
- Date: `2026-02-12`

## Summary
Installed and configured Playwright for end-to-end smoke testing of critical user flows. Four test specs cover registration, login, organization creation, and member invitation. The CI workflow now includes an E2E job that boots the full Docker Compose stack, waits for health checks, and runs the tests with Chromium.

## Context
- Background: The project had Vitest unit tests (305 tests, 60% coverage) and .NET integration tests, but no browser-level E2E tests exercising the full stack.
- Problem statement: No automated verification that the browser -> Next.js -> API -> database pipeline works end-to-end.
- Constraints: Chromium only for CI (task constraint). Tests must be deterministic and self-cleaning.

## Decisions & Trade-offs
- **Label-based selectors over test IDs**
  - Options considered: (1) data-testid attributes, (2) CSS selectors, (3) Label/role selectors
  - Why this choice: FormField already associates `<label htmlFor>` with `<input id>` via `useId()`. Using `getByLabel()` and `getByRole()` is resilient to styling changes and follows Playwright best practices.

- **Unique test data per run**
  - Options considered: (1) Shared seed data, (2) Per-test unique data, (3) API-based setup
  - Why this choice: Each test generates unique emails/org names with `Date.now()` + random suffix. Avoids collision between parallel runs and requires no cleanup logic. CI destroys Docker volumes after each run.

- **Sequential workers**
  - Options considered: (1) Parallel workers, (2) Single worker
  - Why this choice: `workers: 1` because tests are inherently sequential (login depends on registration). Parallelism would require separate browser contexts and more complex fixture management for 4 smoke tests.

- **E2E CI runs in parallel with unit tests**
  - Options considered: (1) Run after unit tests, (2) Run in parallel
  - Why this choice: E2E depends only on the `changes` job (path detection), not on unit test results. This avoids unnecessary waiting since E2E exercises the full stack independently. If unit tests fail, the E2E results are still useful for diagnosing whether the issue is unit-level or integration-level.

- **Login test clears cookies**
  - Why: Registration auto-logs-in the user. To test the actual login flow, we clear cookies after the `registeredPage` fixture runs, then navigate to `/en/login` with fresh session state.

## Implementation Notes
- Playwright config: Chromium only, 1 retry in CI, screenshots on failure, trace on first retry
- Auth fixture: `testUser`, `registeredPage`, `authenticatedPage` — all generate unique credentials
- i18n routing: All test URLs prefixed with `/en/` (default locale)
- CSRF handling: No special setup needed — Playwright controls a real browser, so CSRF token fetch works naturally
- Invite test scopes the email field to the `<section>` containing "Invite a member" to avoid ambiguity with other email fields on the members page

## Commands Run
```bash
cd apps/web && npm install --save-dev @playwright/test
cd apps/web && npm run build   # Verify build passes
cd apps/web && npm test        # Verify unit tests pass (305/305)
```

## Files Changed
- `apps/web/package.json` — Added `@playwright/test` dev dependency and `test:e2e`/`test:e2e:ui` scripts
- `apps/web/playwright.config.ts` — Playwright configuration (Chromium, baseURL, reporters, screenshots)
- `apps/web/e2e/fixtures/auth.fixture.ts` — Shared fixtures for test user generation and registration
- `apps/web/e2e/register.spec.ts` — Registration flow smoke tests (happy path + validation)
- `apps/web/e2e/login.spec.ts` — Login flow smoke tests (happy path + invalid credentials + validation)
- `apps/web/e2e/create-org.spec.ts` — Create organization smoke test
- `apps/web/e2e/invite-member.spec.ts` — Invite member smoke test
- `apps/web/tsconfig.json` — Added `e2e` to `exclude` array
- `apps/web/.gitignore` — Added Playwright artifact directories
- `.gitignore` — Added Playwright artifact directories at root level
- `.github/workflows/ci.yml` — Added E2E job with Docker Compose, health checks, and artifact upload
- `apps/web/package-lock.json` — Auto-updated

## Tests
### E2E (Playwright)
- `register.spec.ts`: 3 tests (happy path, empty field validation, password mismatch)
- `login.spec.ts`: 3 tests (happy path, invalid credentials, empty form validation)
- `create-org.spec.ts`: 1 test (create and verify org appears)
- `invite-member.spec.ts`: 1 test (create org, navigate, invite, verify toast)
- **Total: 8 E2E smoke tests**
- Run: `cd apps/web && npx playwright test`

### Unit (existing — verified still pass)
- 305 tests across 29 files — all passing

## Security
- No secrets in test code — unique test emails use `@test.local` domain
- CI uses Docker Compose with `.env.example` defaults (no real credentials)
- CSRF flow works naturally through browser automation

## Follow-ups / Backlog
- [ ] Add visual regression testing (screenshot comparison)
- [ ] Add mobile viewport smoke tests
- [ ] Consider API-based test setup for faster fixtures (bypass UI registration)
- [ ] Add Playwright to pre-merge branch protection rules

## Checklist
- [x] Task scope matches `docs/tasks/0052-e2e-smoke-tests.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
