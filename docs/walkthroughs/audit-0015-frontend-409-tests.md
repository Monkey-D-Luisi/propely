# Walkthrough: audit-0015-frontend-409-tests

## Task Reference
- Task: `docs/audits/epic-003-executive-summary.md` (action #3)
- Walkthrough: `docs/walkthroughs/audit-0015-frontend-409-tests.md`
- Branch/PR: `fix/audit-0013-epic-003-remediations`
- Date: `2026-02-09`

## Summary
Added Vitest/RTL tests verifying that 409 conflict errors display the correct i18n error messages in both the organization creation page and the organization settings form.

## Context
- Background: Both org creation (`mine/page.tsx`) and org settings (`OrgSettingsForm.tsx`) handle 409 responses with i18n error messages, but had no frontend tests verifying this behavior.
- Problem statement: The 409 error path could silently break during frontend refactoring without test detection.
- Constraints: Must use the existing `renderWithProviders` test infrastructure and mock hooks via `vi.mock`.

## Decisions & Trade-offs
- **Decision: Use `getByRole('alert')` + `toHaveTextContent` for OrgSettingsForm test**
  - Options considered: (1) `getByText` exact match, (2) `getByRole('alert')` + `toHaveTextContent`
  - Why this choice: React Hook Form's `setError('root', ...)` triggers re-renders through the form state, which has timing characteristics that differ from direct DOM updates. Using `getByRole('alert')` targets the `FormError` component specifically and is more resilient to re-render timing.

## Files Changed
- `apps/web/src/components/orgs/__tests__/OrgSettingsForm.test.tsx` — Added `shows name-already-exists error on 409 conflict` test using `ApiError` with status 409
- `apps/web/src/app/[locale]/orgs/mine/__tests__/page.test.tsx` — New file with two tests: 409 conflict shows name-already-exists message, non-409 error shows generic error

## Tests
### Frontend (Vitest)
- `OrgSettingsForm > shows name-already-exists error on 409 conflict` — Mocks `useUpdateOrg` to reject with `ApiError(409)`, verifies toast title "Couldn't update settings" and FormError alert text "An organization with this name already exists."
- `MyOrgsPage > shows name-already-exists error on 409 conflict when creating org` — Mocks `useCreateOrg` to reject with `ApiError(409)`, verifies FormError displays "An organization with this name already exists."
- `MyOrgsPage > shows generic error on non-409 failure when creating org` — Mocks `useCreateOrg` to reject with generic Error, verifies FormError displays "Failed to create organization."

## Checklist
- [x] Task scope matches audit action #3 in `docs/audits/epic-003-executive-summary.md`
- [x] Tests updated and passing (271 frontend tests across 26 files)
- [x] No secrets committed
