# Task: 0012 - Tests for Schemas, Utilities, and API Client

## Metadata
- ID: 0012
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0012-tests-schemas-utils.md`

## Goal
Write comprehensive tests for all Zod schemas, the apiFetch utility, CSRF helpers, and any other utility functions in the web app.

## Context
Task 0002 set up Vitest + RTL. Now we write actual tests for the non-component code. Schemas and utilities are the easiest to test and provide high value since they validate all API communication.

## Scope
### In scope
- Tests for all Zod schemas in `lib/schemas.ts`
- Tests for `apiFetch` in `lib/api.ts` (mock fetch)
- Tests for `ensureCsrfToken` in `lib/csrf.ts`
- Tests for any other utility functions

### Out of scope
- Component tests (task 0014)
- Hook tests (task 0013)

## Requirements
- R1: Every Zod schema has tests for valid input, invalid input, and edge cases
- R2: `apiFetch` tests cover: success, error responses, schema validation, network errors
- R3: CSRF tests cover: token fetch, cache, failure
- R4: Tests use descriptive names that explain the scenario

## Acceptance Criteria
- AC1: All schema tests pass
- AC2: All utility tests pass
- AC3: Coverage for `lib/` directory is > 90%
- AC4: `npm test` passes with zero failures
- AC5: `npm run build` still succeeds

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `apps/web/src/lib/__tests__/schemas.test.ts`
2. Create `apps/web/src/lib/__tests__/api.test.ts`
3. Create `apps/web/src/lib/__tests__/csrf.test.ts`
4. Run tests and verify coverage

## Files to Create / Modify
- `apps/web/src/lib/__tests__/schemas.test.ts` (create)
- `apps/web/src/lib/__tests__/api.test.ts` (create)
- `apps/web/src/lib/__tests__/csrf.test.ts` (create)

## Testing Plan
- This IS the testing task. Run `npm test` and `npm run test:coverage`.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
