# Task: cr-0013-tests-components-review

## Metadata
- ID: cr-0013
- Type: CodeReview
- Status: DONE
- Owner: Agent
- Created: 2026-02-06
- PR: #152 (`test(web): add unit tests for UI components (#0014)`)
- Target branch: main
- CI: N/A (test-only PR)

## Changed Files
- `apps/web/src/components/auth/__tests__/LoginForm.test.tsx`
- `apps/web/src/components/auth/__tests__/RegisterForm.test.tsx`
- `apps/web/src/components/layout/__tests__/AppHeader.test.tsx`
- `apps/web/src/components/orgs/__tests__/InviteForm.test.tsx`
- `apps/web/src/components/orgs/__tests__/LeaveOrgButton.test.tsx`
- `apps/web/src/components/orgs/__tests__/MembersManager.test.tsx`
- `apps/web/src/components/orgs/__tests__/MembersTable.test.tsx`
- `apps/web/src/components/ui/__tests__/toast.test.tsx`
- `apps/web/src/components/ui/skeleton/__tests__/skeleton.test.tsx`
- `docs/backlog/epic-001-professional-saas-refinement.md`
- `docs/tasks/0014-tests-components.md`
- `docs/walkthroughs/0014-tests-components.md`

## Review Threads

### Source 1: Inline Review Comments (8 total)
1. **Copilot #2773849579** (LoginForm.test.tsx:81) — Double render: component rendered twice in "shows validation errors" test
2. **Copilot #2773849552** (LoginForm.test.tsx:84) — After fixing double render, use getBy instead of getAllBy with indexing
3. **Copilot #2773849601** (MembersManager.test.tsx:3) — Unused import `waitFor`
4. **Copilot #2773849614** (toast.test.tsx:2) — Unused import `renderWithProviders`
5. **Copilot #2773849633** (skeleton.test.tsx:2) — Unused import `screen`
6. **Gemini #2773853962** (LoginForm.test.tsx:84) — Same as #1/#2: duplicate render
7. **Gemini #2773853970** (MembersTable.test.tsx:109) — `selects[0]` targets current user, suggest targeting another member
8. **Gemini #2773853977** (toast.test.tsx:33) — `renderInProvider` duplicates `renderWithProviders`, use shared utility

### Source 2: General Reviews (2 total)
- Copilot: Summary overview — no additional actionable items beyond inline comments
- Gemini: General praise + references 3 inline issues — no additional actionable items

### Source 3: Issue Comments (1 total)
- Gemini: Summary of changes — no actionable feedback

## Comment Resolution Plan

### MUST_FIX
- [x] **LoginForm double render** (comments #1, #2, #6): Remove duplicate `renderWithProviders` call in "shows validation errors" test, revert to `getByLabelText`/`getByRole` without array indexing

### SHOULD_FIX
- [x] **Unused import `waitFor`** (comment #3): Remove from MembersManager.test.tsx
- [x] **Use `renderWithProviders` in toast.test** (comments #4, #8): Remove local `renderInProvider` helper, use shared `renderWithProviders` from test/utils. This addresses both Copilot's unused import note and Gemini's duplication concern.
- [x] **Unused import `screen`** (comment #5): Remove from skeleton.test.tsx
- [x] **MembersTable role change target** (comment #7): Change `selects[0]` to `selects[1]` to target a non-current-user member (Bob) for clearer test intent
