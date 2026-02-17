# Task: 0052 - E2E Smoke Tests (Playwright)

## Metadata
- ID: 0052
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #204
- Epic: `docs/backlog/epic-008-cicd-release.md`
- Old Issue: #36
- Milestone: v1.0

## Goal
Set up Playwright for E2E testing and write smoke tests for critical user flows: register, login, create org, invite member.

## Context
The project has unit tests (Vitest for frontend, xUnit for backend) and integration tests (Testcontainers for backend). What's missing is end-to-end testing that exercises the full stack: browser -> Next.js -> APIs -> database. Playwright provides cross-browser testing with excellent CI support.

## Scope
### In scope
- Install and configure Playwright in the web app
- Write smoke tests for: registration, login, create org, invite member, notification bell
- Configure docker-compose for E2E test environment
- GitHub Actions integration (run E2E after deploy)
- Test report as CI artifact
- Screenshot on failure

### Out of scope
- Full regression test suite
- Visual regression testing
- Performance/load testing
- Mobile browser testing

## Requirements
- R1: Playwright installed and configured for the web app
- R2: Smoke tests cover the critical happy-path flows
- R3: Tests run against docker-compose (full stack)
- R4: CI runs E2E tests and stores report as artifact
- R5: Failed tests capture screenshots

## Acceptance Criteria
- AC1: `npx playwright test` runs all smoke tests
- AC2: Register flow test: fills form -> submits -> lands on dashboard
- AC3: Login flow test: enters credentials -> submits -> authenticated
- AC4: Create org test: fills org name -> creates -> sees org page
- AC5: CI workflow runs E2E tests
- AC6: Test report available as CI artifact

## Constraints (non-negotiable)
- Tests must be deterministic (seed data or test setup)
- Tests must clean up after themselves
- Use Chromium for CI (fastest)
- Update walkthrough

## Implementation Steps

1. **Install Playwright** in web app
   - `cd apps/web && npm init playwright@latest`
   - Configure `playwright.config.ts` with baseURL, retries, reporter

2. **Create test fixtures** (`apps/web/e2e/fixtures/`)
   - Auth fixture: register test user, login, store auth state
   - DB fixture: seed data or API calls for test setup

3. **Write smoke tests** (`apps/web/e2e/`)
   - `register.spec.ts`: Visit /register -> fill form -> submit -> expect dashboard
   - `login.spec.ts`: Visit /login -> fill form -> submit -> expect authenticated
   - `create-org.spec.ts`: Login -> create org -> expect org page
   - `invite-member.spec.ts`: Login as org owner -> invite -> check notification

4. **Create docker-compose.e2e.yml** (or use existing with test config)
   - All services running
   - Test database with seed data

5. **Add E2E script** to package.json
   - `"test:e2e": "playwright test"`

6. **Add CI workflow step** (`.github/workflows/ci.yml` or separate)
   - Start docker-compose
   - Wait for health checks
   - Run Playwright tests
   - Upload report artifact

## Files to Create / Modify

### Create
- `apps/web/playwright.config.ts`
- `apps/web/e2e/register.spec.ts`
- `apps/web/e2e/login.spec.ts`
- `apps/web/e2e/create-org.spec.ts`
- `apps/web/e2e/invite-member.spec.ts`
- `apps/web/e2e/fixtures/auth.ts`
- `docker-compose.e2e.yml` (if separate from dev)
- `docs/walkthroughs/0052-e2e-smoke-tests.md`

### Modify
- `apps/web/package.json` (add Playwright, e2e script)
- `.github/workflows/ci.yml` (add E2E step)
- `.gitignore` (add playwright artifacts)

## Testing Plan
- This task IS the E2E testing setup
- All smoke tests must pass locally and in CI

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Playwright configured
- [x] Smoke tests pass in CI *(8/8 pass on main)*
- [x] CI runs E2E tests
- [x] Test report as artifact *(playwright-report uploaded)*
- [x] Walkthrough updated
