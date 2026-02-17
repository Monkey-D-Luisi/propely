# Audit Action: audit-0011-rate-limit-delete-endpoints

## Metadata
- ID: audit-0011
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-10
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-004-executive-summary.md`
  - Action plan item: "#3 — Add rate limiting to DELETE endpoints"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0011-rate-limit-delete-endpoints.md`

## Goal
Add per-IP rate limiting to the destructive DELETE endpoints to prevent abuse and amplification of other attack vectors.

## Acceptance Criteria
- [x] DELETE endpoints on `/orgs/*` have a stricter rate limit than the global default
- [x] Configuration uses existing AspNetCoreRateLimit infrastructure
- [x] Build passes

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
