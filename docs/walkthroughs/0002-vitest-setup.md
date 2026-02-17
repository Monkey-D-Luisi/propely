# Walkthrough: 0002-vitest-setup

## Task Reference
- Task: `docs/tasks/0002-vitest-setup.md`
- Walkthrough: `docs/walkthroughs/0002-vitest-setup.md`
- Branch/PR: `chore/0002-vitest-setup`
- Date: 2026-02-05

## Summary
Installed and configured Vitest + React Testing Library + happy-dom for the web app. Created vitest config with path alias support, test setup with next/navigation mocks, shared test utilities with provider wrappers, and a sample Button component test (5 tests) to verify the setup works end-to-end.

## Context
- Background: The web app had zero test files and no test runner configured.
- Problem statement: Need a testing foundation before comprehensive tests in tasks 0012-0014.
- Constraints: Setup only, no comprehensive tests yet.

## Decisions & Trade-offs
- **Decision:** Used `happy-dom` instead of `jsdom`
  - Options considered: jsdom (task spec), happy-dom
  - Why this choice: jsdom v27 is ESM-only and has a `require()` of ES Module error with Vitest's forks pool (`parse5` cannot be required from CJS). happy-dom is the recommended Vitest environment, faster, and works out of the box.
  - Consequences / risks: None — happy-dom is well-maintained and widely used with Vitest. Minor API differences from jsdom are irrelevant for RTL tests.

- **Decision:** Excluded test files from main tsconfig, created tsconfig.test.json
  - Options considered: Single tsconfig with vitest types, separate tsconfig for tests
  - Why this choice: Next.js build runs TypeScript checking on all included files. Test files using `vi.mock()` and `vi.fn()` globals would fail the build. Separate tsconfig keeps concerns clean.
  - Consequences / risks: IDE may need to reference tsconfig.test.json for test files. Vitest uses its own config so this is transparent.

- **Decision:** Did NOT create mock for next-intl yet
  - Options considered: Create passthrough mock now, wait until next-intl is installed
  - Why this choice: next-intl is not installed yet (task 0003). Creating a mock for a non-existent dependency would be premature. The mock will be added when next-intl is set up.

## Implementation Notes
- Key changes: Vitest config, test setup, test utilities, sample test, tsconfig separation
- Edge cases handled: Next.js build excluding test files, Vitest globals for TypeScript
- Known limitations: CJS deprecation warning from Vite (cosmetic, does not affect functionality)

## Commands Run
```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event @vitejs/plugin-react happy-dom
npm test              # 5 tests pass
npm run lint          # 0 warnings
npm run build         # success
```

## Files Changed
- `apps/web/vitest.config.ts` (create) — Vitest config with happy-dom, path aliases, setup file
- `apps/web/test/setup.ts` (create) — jest-dom matchers, next/navigation mock
- `apps/web/test/utils.tsx` (create) — renderWithProviders with ToastProvider, re-exports
- `apps/web/src/components/ui/__tests__/button.test.tsx` (create) — 5 tests: render, className, disabled, onClick, no-click-when-disabled
- `apps/web/tsconfig.json` (modify) — Exclude test/, __tests__/, *.test.ts(x) from Next.js build
- `apps/web/tsconfig.test.json` (create) — Extends main tsconfig, adds vitest/globals types
- `apps/web/package.json` (modify) — Added test, test:watch, test:coverage scripts; added dev deps

## Tests
### Unit
- `button.test.tsx`: 5 tests covering render, styling, disabled state, click handling
- How to run: `cd apps/web && npm test`

### Manual
- `npm test` — 5 tests pass
- `npm run lint` — 0 warnings
- `npm run build` — success

## Follow-ups / Backlog
- Add next-intl mock to test/setup.ts when task 0003 is complete
- Consider adding coverage threshold configuration when writing comprehensive tests (tasks 0012-0014)
- The CJS deprecation warning from Vite is cosmetic and will resolve in a future Vite major

## Checklist
- [x] Task scope matches `docs/tasks/0002-vitest-setup.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
