# Audit Action: audit-0009-race-condition-owner-count

## Metadata
- ID: audit-0009
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-10
- Priority: P0
- Source:
  - Executive summary: `docs/audits/epic-004-executive-summary.md`
  - Action plan item: "#1 — Race condition on owner count"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0009-race-condition-owner-count.md`

## Goal
Eliminate the race condition where concurrent leave/remove operations can bypass the last-owner guard and orphan an organization.

## Acceptance Criteria
- [x] Leave and remove handlers execute inside an explicit database transaction
- [x] Membership rows are locked with `SELECT ... FOR UPDATE` during the transaction
- [x] Concurrent requests are serialized at the database level, preventing stale reads
- [x] Existing unit tests pass with the new transaction wrapping
- [x] New tests verify transaction usage

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
