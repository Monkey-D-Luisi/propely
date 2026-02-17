# Task: 0002 - Setup Vitest and React Testing Library

## Metadata
- ID: 0002
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0002-vitest-setup.md`

## Goal
Install and configure Vitest + React Testing Library + jsdom for the web app. Create test utilities, shared mocks, and a sample test to verify the setup works end-to-end.

## Context
The web app currently has zero test files. The `npm test` script exists but has no test runner configured. We need a testing foundation before we can write actual tests in tasks 0012-0014.

## Scope
### In scope
- Install vitest, @testing-library/react, @testing-library/jest-dom, @testing-library/user-event, jsdom
- Configure vitest.config.ts with path aliases matching tsconfig
- Create test setup file (setup global mocks, jest-dom matchers)
- Create shared test utilities (renderWithProviders, mock factories)
- Create mock for next/navigation (useRouter, useSearchParams, etc.)
- Create mock for next-intl (useTranslations - returns key as-is)
- Write one sample test to verify the setup works
- Add `test` and `test:coverage` scripts to package.json

### Out of scope
- Writing comprehensive tests (tasks 0012-0014)
- E2E testing setup
- Backend testing changes

## Requirements
- R1: `npm test` runs Vitest and exits with code 0
- R2: `npm run test:coverage` generates a coverage report
- R3: Path aliases (`@/`) resolve correctly in tests
- R4: React components can be rendered in tests using RTL
- R5: Toast context, router, and i18n are available in test renders via a shared wrapper

## Acceptance Criteria
- AC1: `npm test` passes with at least 1 test
- AC2: Coverage report is generated under `coverage/`
- AC3: A sample component test renders a component and asserts text content
- AC4: `npm run build` still succeeds (no config conflicts)
- AC5: ESLint still passes

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1. Install dev dependencies
2. Create vitest.config.ts extending Vite config with path aliases
3. Create test/setup.ts with global mocks and matchers
4. Create test/utils.tsx with renderWithProviders
5. Create test/mocks/ with common mocks
6. Write sample test
7. Update package.json scripts

## Implementation Steps
1. `npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event @vitejs/plugin-react jsdom`
2. Create `apps/web/vitest.config.ts`
3. Create `apps/web/test/setup.ts` - import jest-dom, mock next/navigation, mock next-intl
4. Create `apps/web/test/utils.tsx` - renderWithProviders wrapping ToastProvider
5. Create `apps/web/test/mocks/navigation.ts` - mock useRouter, useSearchParams, usePathname
6. Create `apps/web/test/mocks/intl.ts` - mock useTranslations (passthrough)
7. Write `apps/web/src/components/ui/__tests__/button.test.tsx` as sample
8. Update `package.json` with `"test": "vitest run"` and `"test:coverage": "vitest run --coverage"`
9. Add `coverage/` to `.gitignore`
10. Run tests, verify everything works

## Files to Create / Modify
- `apps/web/vitest.config.ts` (create)
- `apps/web/test/setup.ts` (create)
- `apps/web/test/utils.tsx` (create)
- `apps/web/test/mocks/navigation.ts` (create)
- `apps/web/test/mocks/intl.ts` (create)
- `apps/web/src/components/ui/__tests__/button.test.tsx` (create)
- `apps/web/package.json` (modify - scripts)
- `.gitignore` (modify - add coverage/)

## Testing Plan
- Unit tests: Sample button test
- Manual verification: `npm test`, `npm run test:coverage`

## Security & Privacy
- No security impact

## Observability
- No changes to observability

## Rollback Plan
Remove installed packages and created files. Revert package.json changes.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
