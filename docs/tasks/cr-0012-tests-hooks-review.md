# Code Review: cr-0012-tests-hooks-review

## PR Metadata

- **PR:** [#151 - test(web): add unit tests for custom hooks (#0013)](https://github.com/Monkey-D-Luisi/saas-template/pull/151)
- **Branch:** `feat/0013-tests-hooks` → `main`
- **CI Status:** Pending (no check runs yet)
- **Author:** Monkey-D-Luisi
- **Created:** 2026-02-06

## Changed Files

| File | Status | +/- |
|------|--------|-----|
| `apps/web/src/hooks/__tests__/orgs.test.ts` | Added | +403 |
| `docs/backlog/epic-001-professional-saas-refinement.md` | Modified | +3/-3 |
| `docs/tasks/0013-tests-hooks.md` | Modified | +6/-6 |
| `docs/walkthroughs/0013-tests-hooks.md` | Added | +67 |

## Review Threads

### SHOULD_FIX

- [ ] **[S1] Use `vi.importActual` for mock strategy** (gemini-code-assist, line 14-27)
  - **Comment:** The mock re-implements `ApiError` and `isApiError`, which is brittle. Suggests using `vi.importActual` to only mock `apiFetch`.
  - **Analysis:** Valid suggestion. The current mock duplicates implementation details that could drift from the real module. Using `vi.importActual` keeps real exports and only mocks what's needed.
  - **Action:** Refactor mock to use `vi.importActual`.

- [ ] **[S2] Use schema-shaped fixtures for `useInviteMember`** (Copilot, line 249)
  - **Comment:** The mocked response `{ message: 'ok' }` doesn't match `InviteResponseSchema` which expects `{ ok: boolean, inviteUrl?: string }`.
  - **Analysis:** Valid. Using schema-shaped fixtures ensures tests validate the real contract and avoids false confidence.
  - **Action:** Change mock response to `{ ok: true, inviteUrl: 'https://example.com/invite' }`.

### SUGGESTION

(None)

### QUESTION

(None)

### OUT_OF_SCOPE

(None)

## Comment Resolution Plan

### SHOULD_FIX (2 items)

1. [S1] Refactor `vi.mock('@/lib/api', ...)` to use `vi.importActual`:
   ```typescript
   vi.mock('@/lib/api', async () => {
     const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
     return {
       ...actual,
       apiFetch: vi.fn(),
     };
   });
   ```
   This removes the duplicate `ApiError` class and `isApiError` function from the mock.

2. [S2] Update `useInviteMember` test fixture from `{ message: 'ok' }` to match schema:
   ```typescript
   const response = {
     ok: true,
     inviteUrl: 'https://example.com/invite',
   };
   ```

## Verification

- Run `cd apps/web && npm test` — all 102 tests should pass
- Run `cd apps/web && npm run build` — clean build

## Definition of Done

- [ ] All SHOULD_FIX items addressed
- [ ] Tests pass
- [ ] Build passes
- [ ] Changes committed and pushed
- [ ] Review threads replied to
