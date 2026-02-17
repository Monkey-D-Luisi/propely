# Audit Action: audit-0014-integration-tests-delete-endpoints

## Metadata
- ID: audit-0014
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-10
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-004-executive-summary.md`
  - Action plan item: "#6 — Add integration tests for new endpoints"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0014-integration-tests-delete-endpoints.md`

## Goal
Add integration tests for the three DELETE endpoints (leave org, remove member, delete org) covering auth, authorization, happy path, and error responses.

## Acceptance Criteria
- [x] Tests cover all three DELETE endpoints
- [x] Tests verify authentication (401), authorization (403), happy path (200), and domain errors (400, 404)
- [x] Tests follow existing integration test patterns
- [x] Build passes

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] No secrets committed
- [x] Walkthrough updated
