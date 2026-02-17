# Code Review: cr-0023 - Org Settings PR Review

## Metadata
- PR: #175
- Branch: `feat/0023-org-settings`
- Target: `main`
- CI Status: All checks passing (Detect Changes, Orgs API Build & Test, Web Build & Test)

## Changed Files (31)
- Backend: 14 files (domain, application, infrastructure, API, tests)
- Frontend: 13 files (pages, components, hooks, schemas, i18n, tests)
- Docs: 4 files (task, epic, walkthrough)

## Review Sources
- Inline review comments: 9
- General reviews: 3 (Codex, Gemini, Copilot)
- Issue comments: 1 (Gemini summary)

## Review Threads

### Thread 1 — Whitespace-only org name validation (Codex #2777508122 + Copilot #2777511287)
- **File:** `UpdateOrgRequestValidator.cs:12-14`
- **Claim:** `NotEmpty()` allows whitespace-only input (e.g. `"   "`), and `Organization.Update()` trims it to empty string.
- **Verified:** Correct. FluentValidation's `NotEmpty()` rejects null/empty but not whitespace-only.
- **Classification:** MUST_FIX

### Thread 2 — DRY `isManager` helper (Gemini #2777508129)
- **File:** `OrgSettingsForm.tsx:24`
- **Claim:** `isManager` is duplicated in MembersManager.tsx and OrgSettingsForm.tsx.
- **Verified:** Correct. Both files define the same function.
- **Classification:** SHOULD_FIX

### Thread 3 — `values` prop performance in useForm (Gemini #2777508131)
- **File:** `OrgSettingsForm.tsx:48`
- **Claim:** Using `values` prop causes re-renders; should use `reset` in `useEffect`.
- **Verified:** Incorrect. The `values` prop is the idiomatic react-hook-form v7+ approach for async data. It only triggers a re-render when the reference changes (when org loads), not on each form change. The react-hook-form docs specifically added `values` as the recommended pattern.
- **Classification:** SUGGESTION — will respond with rationale, no code change.

### Thread 4 — Skeleton loader missing labels (Gemini #2777508132)
- **File:** `OrgSettingsForm.tsx:62`
- **Claim:** Skeleton is missing label placeholders, causing layout shift.
- **Verified:** Valid. The skeleton shows field-size blocks but no label-size blocks above them.
- **Classification:** SHOULD_FIX

### Thread 5 — Error toast uses generic message (Gemini #2777508133)
- **File:** `OrgSettingsForm.tsx:110`
- **Claim:** Toast should use specific API error message instead of generic i18n.
- **Verified:** The suggestion conflicts with the codebase pattern. All other forms (InviteForm, LeaveOrgButton, MembersManager) use generic i18n messages in toasts. Showing raw API errors in toasts can expose technical details. The specific error is already displayed in the form via `FormError`.
- **Classification:** SUGGESTION — will respond with rationale, no code change.

### Thread 6 — Type mismatch in `useOrg` (Copilot #2777511291)
- **File:** `hooks/orgs.ts:217`
- **Claim:** `apiFetch<Org>(..., OrgDetailSchema)` is unsound because `Org` has optional `role` but `OrgDetailSchema` requires it.
- **Verified:** Valid. Should use `OrgDetail` type inferred from `OrgDetailSchema`.
- **Classification:** SHOULD_FIX

### Thread 7 — `useUpdateOrg` response description typing (Copilot #2777511292)
- **File:** `hooks/orgs.ts:241`
- **Claim:** Response type `description?: string` doesn't allow null, but API can return null.
- **Verified:** Valid. The API returns `result.Description` which can be null.
- **Classification:** SHOULD_FIX

### Thread 8 — `clearErrors('root')` on retry (Copilot #2777511296)
- **File:** `OrgSettingsForm.tsx:111`
- **Claim:** Root error is never cleared on success or retry, leaving error banner visible.
- **Verified:** Correct. `setError('root', ...)` persists across retries.
- **Classification:** MUST_FIX

## Comment Resolution Plan

### MUST_FIX
- [x] Thread 1: Add whitespace validation to `UpdateOrgRequestValidator` (and `CreateOrgRequestValidator` for consistency)
- [x] Thread 8: Add `methods.clearErrors('root')` at start of `onSubmit`

### SHOULD_FIX
- [x] Thread 2: Extract `isManager` to `apps/web/src/lib/roles.ts` and import in both components
- [x] Thread 4: Add label skeleton placeholders to loading state
- [x] Thread 6: Create `OrgDetail` type from `OrgDetailSchema` and use in `useOrg`
- [x] Thread 7: Fix `useUpdateOrg` response type to allow `description: string | null`

### SUGGESTION (no code change)
- [x] Thread 3: Respond — `values` is the idiomatic react-hook-form v7+ pattern
- [x] Thread 5: Respond — generic toast is the codebase convention; specific errors shown via FormError
