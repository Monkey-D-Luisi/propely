# Walkthrough: 0012 - Tests for Schemas, Utilities, and API Client

## Task Reference
- Task: `docs/tasks/0012-tests-schemas-utils.md`
- Walkthrough: `docs/walkthroughs/0012-tests-schemas-utils.md`
- Branch/PR: PR #149 (`feat/0012-tests-schemas-utils`)
- Date: `2026-02-06`

## Summary
Added comprehensive unit tests for all Zod schemas (static and factory-generated), the `apiFetch` utility (success, errors, schema validation, network failures), and CSRF helpers (`readCsrfTokenFromCookie`, `ensureCsrfToken`). Total: 73 new tests across 3 test files, all passing.

## Context
- Background: Task 0002 set up Vitest + RTL. No tests existed for the lib/ utilities.
- Problem statement: Schemas and utilities validate all API communication — they need high confidence.
- Constraints: Must test with mocked `fetch` and `document.cookie` since utilities run in browser context.

## Decisions & Trade-offs
- **Decision: Mock `fetch` globally instead of using MSW**
  - Options considered: Mock Service Worker (MSW), vi.stubGlobal with vi.fn()
  - Why this choice: `apiFetch` is a thin wrapper around `fetch`. Mocking at the `fetch` level is simpler and faster for these unit tests. MSW would add overhead without benefit for this scope.

- **Decision: Mock `apiFetch` module in CSRF tests**
  - Options considered: Mock `fetch` and let `apiFetch` run naturally, mock `apiFetch` directly
  - Why this choice: CSRF tests should isolate `ensureCsrfToken` from `apiFetch` internals. Testing `apiFetch` is already covered in its own test file.

- **Decision: Use `safeParse` for custom error message assertions**
  - Options considered: Catch ZodError exceptions, use `safeParse` for non-throwing validation
  - Why this choice: `safeParse` provides a clean way to assert both that validation fails and that the correct custom i18n message is returned.

## Implementation Notes
- Key changes:
  - `schemas.test.ts` (49 tests): All static schemas (RoleSchema, InviteRoleSchema, OrgSchema, MemberSchema, MeSchema, response schemas), form schema factories (login, register, invite, org), custom error messages, edge cases (whitespace trimming, boundary values)
  - `api.test.ts` (17 tests): ApiError construction, isApiError type guard, success/error paths, schema validation, empty responses, network errors, header merging, base URL prefixing
  - `csrf.test.ts` (7 tests): Cookie parsing, URL decoding, missing cookies, cache behavior, API fallback, error handling
- Edge cases: Whitespace-only org names, PASSWORD_MIN_LENGTH boundary, empty response bodies, non-JSON error responses
- Known limitations: `@vitest/coverage-v8` not installed so coverage percentage was not measured numerically. All lib/ code has comprehensive test coverage based on code path analysis.

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
cd apps/web && npx vitest run
cd apps/web && npm run build
```

## Files Changed
- `apps/web/src/lib/__tests__/schemas.test.ts` — Created 49 tests for all Zod schemas and form schema factories
- `apps/web/src/lib/__tests__/api.test.ts` — Created 17 tests for apiFetch, ApiError, isApiError
- `apps/web/src/lib/__tests__/csrf.test.ts` — Created 7 tests for readCsrfTokenFromCookie and ensureCsrfToken
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0012 status
- `docs/tasks/0012-tests-schemas-utils.md` — Marked DoD checklist complete

## Tests
### Unit
- What was added: 73 new tests across 3 files (schemas: 49, api: 17, csrf: 7)
- How to run: `cd apps/web && npx vitest run`
- Result: All 78 tests pass (73 new + 5 existing button tests)

## Observability
- No changes

## Security
- No security impact — tests only

## Follow-ups / Backlog
- [ ] Install `@vitest/coverage-v8` and add coverage gates (task 0021)

## Checklist
- [x] Task scope matches `docs/tasks/0012-tests-schemas-utils.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
