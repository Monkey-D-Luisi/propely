# Audit Action: audit-0015-domain-error-code-helper

## Metadata
- ID: audit-0015
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-10
- Priority: P3
- Source:
  - Executive summary: `docs/audits/epic-004-executive-summary.md`
  - Action plan item: "#7 — Extract domain error code helper"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0015-domain-error-code-helper.md`

## Goal
Extract the repeated `(error.body as { detail?: string }).detail` type assertion into a reusable `getDomainErrorCode()` helper.

## Acceptance Criteria
- [x] `getDomainErrorCode(error: ApiError): string | undefined` added to `lib/api.ts`
- [x] All three usages in MembersManager.tsx replaced with the helper
- [x] Unit tests added for the helper
- [x] All existing tests pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
