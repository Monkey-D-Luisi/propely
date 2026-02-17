# Audit Action: audit-0008-csrf-token-ttl

## Metadata
- ID: audit-0008
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P2
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#8 — CSRF token TTL"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0008-csrf-token-ttl.md`

## Goal
Add a TTL to CSRF tokens so they expire after 1 hour, limiting the window for token replay attacks.

## Acceptance Criteria
- [x] CSRF tokens include a timestamp
- [x] Tokens older than 1 hour are rejected
- [x] Existing tests pass with the new format
- [x] No disruption to active sessions within the 1-hour window

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
