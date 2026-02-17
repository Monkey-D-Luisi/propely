# Task: 0013 - Tests for Custom Hooks

## Metadata
- ID: 0013
- Type: Standard
- Status: TODO
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0013-tests-hooks.md`

## Goal
Write tests for all custom hooks in `hooks/orgs.ts` using `renderHook` from React Testing Library with mocked API calls.

## Context
Custom hooks encapsulate all data fetching and mutation logic. Testing them ensures the API integration layer works correctly without rendering full components.

## Scope
### In scope
- Tests for useCurrentUser
- Tests for useMyOrgs
- Tests for useMembers
- Tests for useCreateOrg
- Tests for useInviteMember
- Tests for useUpdateRole
- Tests for useLeaveOrg

### Out of scope
- Component tests (task 0014)
- Schema/utility tests (task 0012)

## Requirements
- R1: Each hook has tests for loading state, success state, and error state
- R2: `apiFetch` is mocked to control responses
- R3: Mutation hooks (create, invite, update) test both success and failure paths
- R4: `useLeaveOrg` tests cover the last-owner protection logic

## Acceptance Criteria
- AC1: All hook tests pass
- AC2: Coverage for `hooks/` directory is > 80%
- AC3: `npm test` passes
- AC4: Tests use `renderHook` and `act` correctly

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `apps/web/src/hooks/__tests__/orgs.test.ts`
2. Mock `@/lib/api` module
3. Write tests for each hook
4. Run and verify

## Files to Create / Modify
- `apps/web/src/hooks/__tests__/orgs.test.ts` (create)

## Testing Plan
- This IS the testing task.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
