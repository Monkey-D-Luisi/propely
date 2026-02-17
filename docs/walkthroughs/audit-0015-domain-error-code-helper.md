# Walkthrough: audit-0015-domain-error-code-helper

## Task Reference
- Task: `docs/tasks/audit-0015-domain-error-code-helper.md`
- Walkthrough: `docs/walkthroughs/audit-0015-domain-error-code-helper.md`
- Branch/PR: `fix/audit-epic-004-batch`
- Date: `2026-02-10`

## Summary
Added `getDomainErrorCode(error: ApiError): string | undefined` to `lib/api.ts` to safely extract the `detail` field from RFC 7807 ProblemDetails error bodies. Replaced three inline type assertions in `MembersManager.tsx` with the helper.

## Context
- Background: The backend returns RFC 7807 ProblemDetails responses with domain error codes in the `detail` field. The frontend code used raw type assertions `(error.body as { detail?: string }).detail` to extract them.
- Problem statement: The type assertion was repeated three times in `MembersManager.tsx`, was fragile, and a previous bug used `.error` instead of `.detail` (fixed in cr-0042). A helper function prevents this class of bugs.

## Files Changed
- `apps/web/src/lib/api.ts` — Added `getDomainErrorCode()` helper function
- `apps/web/src/components/orgs/MembersManager.tsx` — Replaced 3 inline type assertions with `getDomainErrorCode(error)`
- `apps/web/src/lib/__tests__/api.test.ts` — Added 4 unit tests for `getDomainErrorCode`

## Tests
### Unit (4 new tests)
1. `extracts detail from ProblemDetails body` — Happy path
2. `returns undefined when body has no detail` — Missing field
3. `returns undefined when body is undefined` — No body
4. `returns undefined when detail is not a string` — Wrong type

## Checklist
- [x] Task scope matches `docs/tasks/audit-0015-domain-error-code-helper.md`
- [x] Tests updated and passing
- [x] No secrets committed
