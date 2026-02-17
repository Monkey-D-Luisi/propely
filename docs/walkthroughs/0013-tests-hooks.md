# Walkthrough: 0013-tests-hooks

## Task Reference
- Task: `docs/tasks/0013-tests-hooks.md`
- Walkthrough: `docs/walkthroughs/0013-tests-hooks.md`
- Branch/PR: `feat/0013-tests-hooks`
- Date: `2026-02-06`

## Summary
Added 24 unit tests for all 7 custom hooks in `hooks/orgs.ts` using `renderHook` from React Testing Library with mocked `apiFetch`. Covers loading states, success/error paths, refetch, optimistic updates, and the last-owner protection logic in `useLeaveOrg`.

## Context
- Background: Custom hooks encapsulate all data fetching and mutation logic. Testing them ensures the API integration layer works correctly without rendering full components.
- Problem statement: No test coverage for the hooks layer.
- Constraints: Must mock `apiFetch` at the module level; hooks use `useEffect` for auto-fetching.

## Decisions & Trade-offs
- **Mock strategy:**
  - Options considered: (a) Mock `fetch` globally, (b) Mock `@/lib/api` module
  - Why this choice: Mocking `@/lib/api` at the module level isolates hook logic from the API client implementation (which has its own tests in `api.test.ts`).
  - Consequences: If `apiFetch` signature changes, tests need updating.

- **No wrapper needed:**
  - Hooks don't use React context (no i18n, no toast), so `renderHook` works without a custom wrapper.

## Implementation Notes
- Key changes: Single test file `src/hooks/__tests__/orgs.test.ts` with 24 tests across 7 describe blocks.
- Tests per hook:
  - `useCurrentUser`: 3 (loading, success, error)
  - `useMyOrgs`: 4 (loading, success, error, refetch)
  - `useMembers`: 5 (loading, success, error, refetch, optimistic setMembers)
  - `useCreateOrg`: 2 (success, error propagation)
  - `useInviteMember`: 2 (success, error propagation)
  - `useUpdateRole`: 2 (success, error propagation)
  - `useLeaveOrg`: 6 (null member, last owner block, non-last owner downgrade, member downgrade, already viewer skip, error propagation)

## Commands Run
```bash
cd apps/web && npx vitest run --reporter=verbose
cd apps/web && npx next build
```

## Files Changed
- `apps/web/src/hooks/__tests__/orgs.test.ts` — Created: 24 tests for all 7 hooks
- `docs/backlog/epic-001-professional-saas-refinement.md` — Updated task 0013 status and progress tracker
- `docs/tasks/0013-tests-hooks.md` — Updated Definition of Done checklist
- `docs/walkthroughs/0013-tests-hooks.md` — Created walkthrough

## Tests
### Unit
- 24 new hook tests, 102 total (78 existing + 24 new)
- Run: `cd apps/web && npm test`

## Observability
- No changes

## Security
- No security impact

## Follow-ups / Backlog
- [ ] Task 0014: Component tests (next in Phase 4)

## Checklist
- [x] Task scope matches `docs/tasks/0013-tests-hooks.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
