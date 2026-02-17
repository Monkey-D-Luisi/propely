# Walkthrough: audit-0012-delete-org-section-tests

## Task Reference
- Task: `docs/tasks/audit-0012-delete-org-section-tests.md`
- Walkthrough: `docs/walkthroughs/audit-0012-delete-org-section-tests.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Added 10 component tests for `DeleteOrgSection.tsx` covering the full user interaction flow: rendering, dialog lifecycle, name confirmation, success/error toasts, forbidden handling, redirect, and state reset.

## Context
- Background: `DeleteOrgSection.tsx` handles the dangerous org deletion flow with a name-confirmation dialog. It was the only component in Epic 004 without any test coverage.
- Problem statement: Zero test coverage on a destructive operation that requires careful UI validation.

## Decisions & Trade-offs
- **Decision: Use `getAllByRole('button', { name: 'Delete organization' })` for confirm button**
  - Why: Both the trigger button and the dialog confirm button have the same accessible name. Using `getAllByRole` and selecting the last element (the dialog button) avoids fragile selectors.

## Files Changed
- `apps/web/src/components/orgs/__tests__/DeleteOrgSection.test.tsx` (CREATED) — 10 tests

## Tests
### Component (10 tests)
1. Renders the danger zone section with delete button
2. Opens confirmation dialog when delete button is clicked
3. Closes dialog when cancel is clicked
4. Disables confirm button when org name is not typed
5. Enables confirm button when org name matches
6. Calls deleteOrg and shows success toast on confirm
7. Shows generic error toast on API failure
8. Shows forbidden error toast on 403 response
9. Resets confirm text when dialog is closed and reopened
10. Closes dialog after successful deletion

## Checklist
- [x] Task scope matches `docs/tasks/audit-0012-delete-org-section-tests.md`
- [x] Tests updated and passing
- [x] No secrets committed
