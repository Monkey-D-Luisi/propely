# Audit Action: audit-0010-info-leakage-404-vs-403

## Metadata
- ID: audit-0010
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-10
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-004-executive-summary.md`
  - Action plan item: "#2 — 404 vs 403 info leakage"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0010-info-leakage-404-vs-403.md`

## Goal
Eliminate the information leakage where non-members receive 404 while members with insufficient role receive 403, allowing org membership enumeration.

## Acceptance Criteria
- [x] Non-members receive 403 (not 404) when attempting org operations
- [x] Members with insufficient role still receive 403 with specific messages
- [x] Target member not found in RemoveMember still returns 404 (legitimate entity lookup)
- [x] All existing and new tests pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
