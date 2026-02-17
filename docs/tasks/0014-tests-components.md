# Task: 0014 - Tests for UI Components

## Metadata
- ID: 0014
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0014-tests-components.md`

## Goal
Write tests for all major UI components: auth forms, org management components, toast system, error boundary, and layout components.

## Context
Tasks 0012-0013 covered utilities and hooks. This task covers the component layer, testing rendering, user interactions, and integration with hooks and i18n.

## Scope
### In scope
- LoginForm: renders, validates, submits, shows errors
- RegisterForm: renders, validates, submits
- InviteForm: renders, validates, submits, shows toast
- MembersTable: renders members, role change interaction
- MembersManager: integrates table + invite + leave
- LeaveOrgButton: dialog open/close, confirm action
- AppHeader: renders user info, logout, language switcher
- Toast: shows/dismisses, variants
- ErrorBoundary: catches errors, shows fallback
- Skeleton components: render correctly

### Out of scope
- E2E tests
- Visual regression tests

## Requirements
- R1: Component tests use `renderWithProviders` from test/utils.tsx
- R2: User interactions tested with `@testing-library/user-event`
- R3: Form submissions tested with mocked API calls
- R4: i18n mocked to return keys (from test/mocks/intl.ts)
- R5: Router mocked (from test/mocks/navigation.ts)

## Acceptance Criteria
- AC1: All component tests pass
- AC2: Coverage for `components/` directory is > 70%
- AC3: `npm test` passes
- AC4: No flaky tests

## Constraints (non-negotiable)
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create test files alongside components or in `__tests__/` directories
2. Write tests for each component category
3. Run and verify

## Files to Create / Modify
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx` (create)
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx` (create)
- `apps/web/src/components/orgs/__tests__/InviteForm.test.tsx` (create)
- `apps/web/src/components/orgs/__tests__/MembersTable.test.tsx` (create)
- `apps/web/src/components/orgs/__tests__/MembersManager.test.tsx` (create)
- `apps/web/src/components/orgs/__tests__/LeaveOrgButton.test.tsx` (create)
- `apps/web/src/components/layout/__tests__/AppHeader.test.tsx` (create)
- `apps/web/src/components/ui/__tests__/toast.test.tsx` (create)

## Testing Plan
- This IS the testing task.

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
