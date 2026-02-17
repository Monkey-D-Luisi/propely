# Audit Action: audit-0007-oauth-unverified-email-test

## Metadata
- ID: audit-0007
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-02-09
- Priority: P1
- Source:
  - Executive summary: `docs/audits/epic-002-executive-summary.md`
  - Action plan item: "#7 — OAuth unverified email integration test"
- Dependencies:
  - None
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0007-oauth-unverified-email-test.md`

## Goal
Add an integration test verifying that the OAuth callback endpoint rejects login attempts when the provider returns an unverified email address.

## Acceptance Criteria
- [x] Integration test forges a valid ExternalOAuth cookie with `email_verified=false`
- [x] Test verifies redirect to login page with `oauthError=email_not_verified`
- [x] All existing tests continue to pass

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass
- [x] No secrets committed
- [x] Walkthrough updated
